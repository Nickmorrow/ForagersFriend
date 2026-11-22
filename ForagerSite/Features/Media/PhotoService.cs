using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace ForagerSite.Features.Media;

public sealed class PhotoService
{
    private const long MaximumFileSize = 10 * 1024 * 1024;
    private const int MaximumFindImages = 4;
    private const int MaximumDimension = 8000;
    private const long MaximumPixelCount = 16_000_000;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private readonly IPhotoStorage _storage;
    private readonly ICurrentUserService _currentUser;
    private readonly IDbContextFactory<ForagerDbContext> _dbContextFactory;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(
        IPhotoStorage storage,
        ICurrentUserService currentUser,
        IDbContextFactory<ForagerDbContext> dbContextFactory,
        ILogger<PhotoService> logger)
    {
        _storage = storage;
        _currentUser = currentUser;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public Task<IReadOnlyList<PhotoStorageReference>> UploadFindImagesAsync(
        IReadOnlyList<IBrowserFile> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        if (files.Count > MaximumFindImages)
            throw new InvalidDataException($"You can upload a maximum of {MaximumFindImages} images.");

        return UploadAsync(files, PhotoKind.Find, maxWidth: 1280, quality: 75, cancellationToken);
    }

    public async Task<PhotoStorageReference> UploadProfileImageAsync(
        Guid targetUserId,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var ownerId = await GetSignedInUserIdAsync(targetUserId);
        var uploaded = await UploadAsync(new[] { file }, PhotoKind.Profile, maxWidth: 256, quality: 70, cancellationToken);
        var reference = uploaded[0];

        List<UserImage> previousImages;
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            previousImages = await context.UserImages
                .Where(image => image.UsiUsrId == ownerId && image.UsiUsfId == null)
                .ToListAsync(cancellationToken);
            context.UserImages.RemoveRange(previousImages);
            context.UserImages.Add(new UserImage
            {
                UsiUsrId = ownerId,
                UsiImageData = reference.PublicUrl
            });
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await CleanupNewImageAsync(reference);
            throw;
        }

        await CleanupStoredImagesAsync(previousImages);
        return reference;
    }

    public async Task DeleteProfileImageAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var ownerId = await GetSignedInUserIdAsync(targetUserId);
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var images = await context.UserImages
            .Where(image => image.UsiUsrId == ownerId && image.UsiUsfId == null)
            .ToListAsync(cancellationToken);
        context.UserImages.RemoveRange(images);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await CleanupStoredImagesAsync(images);
    }

    public Task DeleteAsync(PhotoStorageReference reference, CancellationToken cancellationToken = default) =>
        _storage.DeleteAsync(reference, cancellationToken);

    public Task DeleteByPublicUrlAsync(string? publicUrl, CancellationToken cancellationToken = default) =>
        _storage.DeleteByPublicUrlAsync(publicUrl, cancellationToken);

    private async Task<IReadOnlyList<PhotoStorageReference>> UploadAsync(
        IReadOnlyList<IBrowserFile> files,
        PhotoKind kind,
        int maxWidth,
        int quality,
        CancellationToken cancellationToken)
    {
        var ownerId = await GetSignedInUserIdAsync();
        var references = new List<PhotoStorageReference>(files.Count);

        try
        {
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateFile(file);

                await using var input = file.OpenReadStream(MaximumFileSize, cancellationToken);
                using var buffer = new MemoryStream((int)file.Size);
                await input.CopyToAsync(buffer, cancellationToken);
                var jpeg = ResizeAndEncode(buffer.ToArray(), maxWidth, quality);
                await using (jpeg)
                    references.Add(await _storage.PublishAsync(jpeg, kind, ownerId, cancellationToken));
            }

            return references;
        }
        catch
        {
            foreach (var reference in references)
            {
                try
                {
                    await _storage.DeleteAsync(reference, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Failed to clean up newly published photo {StorageKey}.", reference.StorageKey);
                }
            }

            throw;
        }
    }

    private static void ValidateFile(IBrowserFile file)
    {
        if (file.Size <= 0 || file.Size > MaximumFileSize)
            throw new InvalidDataException($"File '{file.Name}' must be between 1 byte and {MaximumFileSize / (1024 * 1024)} MB.");

        if (!AllowedExtensions.Contains(Path.GetExtension(file.Name)))
            throw new InvalidDataException($"File '{file.Name}' must be a JPG, JPEG, or PNG image.");
    }

    private static MemoryStream ResizeAndEncode(byte[] bytes, int maxWidth, int quality)
    {
        using (var probeStream = new MemoryStream(bytes, writable: false))
        using (var codec = SKCodec.Create(probeStream))
        {
            if (codec is null)
                throw new InvalidDataException("The uploaded file is not a supported image.");

            var info = codec.Info;
            if (info.Width <= 0 || info.Height <= 0 || info.Width > MaximumDimension || info.Height > MaximumDimension ||
                (long)info.Width * info.Height > MaximumPixelCount)
                throw new InvalidDataException("The image dimensions exceed the allowed limit.");
        }

        using var bitmap = SKBitmap.Decode(bytes);
        if (bitmap is null)
            throw new InvalidDataException("The uploaded file is not a supported image.");

        var width = bitmap.Width;
        var height = bitmap.Height;
        if (width > maxWidth)
        {
            var ratio = (double)maxWidth / width;
            width = maxWidth;
            height = Math.Max(1, (int)(height * ratio));
        }

        var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var resizedBitmap = new SKBitmap(imageInfo);
        using (var canvas = new SKCanvas(resizedBitmap))
        using (var paint = new SKPaint { FilterQuality = SKFilterQuality.High })
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(bitmap, new SKRect(0, 0, width, height), paint);
        }

        using var image = SKImage.FromBitmap(resizedBitmap);
        using var encodedImage = image.Encode(SKEncodedImageFormat.Jpeg, quality)
            ?? throw new InvalidOperationException("Could not encode the image as JPEG.");

        var output = new MemoryStream();
        encodedImage.SaveTo(output);
        output.Position = 0;
        return output;
    }

    private Task<Guid> GetSignedInUserIdAsync() => _currentUser.GetRequiredUserIdAsync();

    private async Task<Guid> GetSignedInUserIdAsync(Guid targetUserId)
    {
        var signedInUserId = await GetSignedInUserIdAsync();
        if (targetUserId != signedInUserId)
            throw new UnauthorizedAccessException("You can only change your own profile photo.");

        return signedInUserId;
    }

    private async Task CleanupNewImageAsync(PhotoStorageReference reference)
    {
        try
        {
            await _storage.DeleteAsync(reference, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to clean up newly published profile photo {StorageKey}.", reference.StorageKey);
        }
    }

    private async Task CleanupStoredImagesAsync(IEnumerable<UserImage> images)
    {
        foreach (var image in images)
        {
            try
            {
                await _storage.DeleteByPublicUrlAsync(image.UsiImageData, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to clean up profile photo URL {PublicUrl} after its database reference was removed.", image.UsiImageData);
            }
        }
    }
}
