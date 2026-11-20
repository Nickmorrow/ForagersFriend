namespace ForagerSite.Features.Media;

public enum PhotoKind
{
    Find,
    Profile
}

public sealed record PhotoStorageReference(string StorageKey, string PublicUrl);

public interface IPhotoStorage
{
    Task<PhotoStorageReference> PublishAsync(
        Stream content,
        PhotoKind kind,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(PhotoStorageReference reference, CancellationToken cancellationToken = default);

    Task DeleteByPublicUrlAsync(string? publicUrl, CancellationToken cancellationToken = default);
}

public sealed class LocalPhotoStorage : IPhotoStorage
{
    private readonly string _webRootPath;
    private readonly IConfiguration _configuration;

    public LocalPhotoStorage(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _webRootPath = Path.GetFullPath(environment.WebRootPath);
        _configuration = configuration;
    }

    public async Task<PhotoStorageReference> PublishAsync(
        Stream content,
        PhotoKind kind,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (ownerId == Guid.Empty)
            throw new ArgumentException("A valid owner ID is required.", nameof(ownerId));

        var storageRoot = GetStorageRoot(kind);
        var ownerDirectory = Path.Combine(storageRoot, ownerId.ToString("N"));
        Directory.CreateDirectory(ownerDirectory);

        var fileName = $"{Path.GetRandomFileName()}.jpg";
        var filePath = Path.GetFullPath(Path.Combine(ownerDirectory, fileName));
        EnsurePathWithin(storageRoot, filePath);

        try
        {
            await using var output = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await content.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            throw;
        }

        var storageKey = Path.GetRelativePath(_webRootPath, filePath)
            .Replace(Path.DirectorySeparatorChar, '/');
        var publicUrl = "/" + string.Join('/', storageKey.Split('/').Select(Uri.EscapeDataString));
        return new PhotoStorageReference(storageKey, publicUrl);
    }

    public Task DeleteAsync(PhotoStorageReference reference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = ResolveStorageKey(reference.StorageKey);
        if (filePath is not null && File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task DeleteByPublicUrlAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(publicUrl) || !publicUrl.StartsWith("/", StringComparison.Ordinal) || publicUrl.StartsWith("//", StringComparison.Ordinal))
            return Task.CompletedTask;

        var path = publicUrl.Split(new[] { '?', '#' }, 2)[0];
        string storageKey;
        try
        {
            storageKey = string.Join('/', path.TrimStart('/').Split('/').Select(Uri.UnescapeDataString));
        }
        catch (UriFormatException)
        {
            return Task.CompletedTask;
        }

        var filePath = ResolveStorageKey(storageKey);
        if (filePath is not null && File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private string? ResolveStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey))
            return null;

        var segments = storageKey.Replace('\\', '/').Split('/');
        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or ".."))
            return null;

        var filePath = Path.GetFullPath(Path.Combine(_webRootPath, Path.Combine(segments)));
        if (!IsWithin(_webRootPath, filePath))
            return null;

        foreach (var kind in Enum.GetValues<PhotoKind>())
        {
            if (IsWithin(GetStorageRoot(kind), filePath))
                return filePath;
        }

        return null;
    }

    private string GetStorageRoot(PhotoKind kind)
    {
        var settingName = kind switch
        {
            PhotoKind.Find => "FileStorageFind_Images",
            PhotoKind.Profile => "FileStoragePf_Pics",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var configuredFolder = _configuration[settingName];
        if (string.IsNullOrWhiteSpace(configuredFolder))
            throw new InvalidOperationException($"{settingName} is not configured.");

        var storageRoot = Path.GetFullPath(Path.Combine(_webRootPath, configuredFolder));
        if (!IsWithin(_webRootPath, storageRoot))
            throw new InvalidOperationException($"{settingName} must be a folder inside the web root.");

        return storageRoot;
    }

    private static void EnsurePathWithin(string root, string path)
    {
        if (!IsWithin(root, path))
            throw new InvalidOperationException("The photo path is outside the configured storage folder.");
    }

    private static bool IsWithin(string root, string path)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(fullRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
