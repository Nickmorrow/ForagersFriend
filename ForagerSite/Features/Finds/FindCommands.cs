using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Features.Media;
using ForagerSite.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Features.Finds;

public sealed record CreateFindRequest(
    string Name,
    string SpeciesName,
    string SpeciesType,
    string UseCategory,
    string Features,
    string LookAlikes,
    string HarvestMethod,
    string TastesLike,
    string Description,
    double Latitude,
    double Longitude,
    IReadOnlyList<IBrowserFile> ImageFiles,
    string Accessibility);

public sealed record UpdateFindRequest(
    Guid FindId,
    string Name,
    string SpeciesName,
    string SpeciesType,
    string UseCategory,
    string Features,
    string LookAlikes,
    string HarvestMethod,
    string TastesLike,
    string Description,
    double Latitude,
    double Longitude,
    IReadOnlyList<IBrowserFile>? ImageFilesToAdd,
    IReadOnlyList<string>? ImageUrlsToDelete,
    string Accessibility);

public sealed class FindCommands
{
    private readonly IDbContextFactory<ForagerDbContext> _dbContextFactory;
    private readonly IUserSessionService _session;
    private readonly PhotoService _photoService;
    private readonly ILogger<FindCommands> _logger;

    public FindCommands(
        IDbContextFactory<ForagerDbContext> dbContextFactory,
        IUserSessionService session,
        PhotoService photoService,
        ILogger<FindCommands> logger)
    {
        _dbContextFactory = dbContextFactory;
        _session = session;
        _photoService = photoService;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(
        CreateFindRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var viewerId = GetViewerId();
        var uploadedImages = await _photoService.UploadFindImagesAsync(request.ImageFiles, cancellationToken);

        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var find = new UserFind
            {
                UsfUsrId = viewerId,
                UsfName = request.Name,
                UsfSpeciesName = request.SpeciesName,
                UsfSpeciesType = request.SpeciesType,
                UsfUseCategory = request.UseCategory,
                UsfFeatures = request.Features,
                UsfLookAlikes = request.LookAlikes,
                UsfHarvestMethod = request.HarvestMethod,
                UsfTastesLike = request.TastesLike,
                UsfDescription = request.Description,
                UsfFindDate = DateTime.Now,
                UsfAccessibility = request.Accessibility
            };

            context.UserFinds.Add(find);
            await context.SaveChangesAsync(cancellationToken);

            context.UserFindLocations.Add(new UserFindLocation
            {
                UslUsfId = find.UsfId,
                UslLatitude = request.Latitude,
                UslLongitude = request.Longitude
            });

            foreach (var image in uploadedImages)
            {
                context.UserImages.Add(new UserImage
                {
                    UsiUsrId = viewerId,
                    UsiUsfId = find.UsfId,
                    UsiImageData = image.PublicUrl
                });
            }

            await RecalculateExperienceScoreAsync(context, viewerId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return find.UsfId;
        }
        catch
        {
            await CleanupNewImagesAsync(uploadedImages);
            throw;
        }
    }

    public async Task<Guid> UpdateAsync(
        UpdateFindRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var viewerId = GetViewerId();
        var imageUrlsToDelete = request.ImageUrlsToDelete?.ToHashSet(StringComparer.Ordinal) ?? new HashSet<string>();

        await using (var validationContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            var ownsFind = await validationContext.UserFinds
                .AnyAsync(item => item.UsfId == request.FindId && item.UsfUsrId == viewerId, cancellationToken);
            if (!ownsFind)
                throw new KeyNotFoundException("Find was not found or is not owned by the signed-in user.");

            var existingUrls = await validationContext.UserImages
                .Where(image => image.UsiUsfId == request.FindId)
                .Select(image => image.UsiImageData)
                .ToListAsync(cancellationToken);
            if (imageUrlsToDelete.Count > 0 && imageUrlsToDelete.Except(existingUrls).Any())
                throw new InvalidOperationException("One or more images do not belong to this find.");
        }

        var uploadedImages = await _photoService.UploadFindImagesAsync(
            request.ImageFilesToAdd ?? Array.Empty<IBrowserFile>(), cancellationToken);
        List<UserImage> imagesToDelete;

        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var find = await context.UserFinds
                .Include(item => item.UserFindLocation)
                .Where(item => item.UsfId == request.FindId && item.UsfUsrId == viewerId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Find was not found or is not owned by the signed-in user.");

            find.UsfName = request.Name;
            find.UsfSpeciesName = request.SpeciesName;
            find.UsfSpeciesType = request.SpeciesType;
            find.UsfUseCategory = request.UseCategory;
            find.UsfFeatures = request.Features;
            find.UsfLookAlikes = request.LookAlikes;
            find.UsfHarvestMethod = request.HarvestMethod;
            find.UsfTastesLike = request.TastesLike;
            find.UsfDescription = request.Description;
            find.UsfAccessibility = request.Accessibility;

            if (find.UserFindLocation is null)
            {
                find.UserFindLocation = new UserFindLocation
                {
                    UslUsfId = find.UsfId,
                    UslLatitude = request.Latitude,
                    UslLongitude = request.Longitude
                };
            }
            else
            {
                find.UserFindLocation.UslLatitude = request.Latitude;
                find.UserFindLocation.UslLongitude = request.Longitude;
            }

            var existingImages = await context.UserImages
                .Where(image => image.UsiUsfId == find.UsfId)
                .ToListAsync(cancellationToken);
            imagesToDelete = existingImages
                .Where(image => imageUrlsToDelete.Contains(image.UsiImageData))
                .ToList();
            if (imagesToDelete.Count != imageUrlsToDelete.Count)
                throw new InvalidOperationException("One or more images do not belong to this find.");

            context.UserImages.RemoveRange(imagesToDelete);

            foreach (var image in uploadedImages)
            {
                context.UserImages.Add(new UserImage
                {
                    UsiUsrId = viewerId,
                    UsiUsfId = find.UsfId,
                    UsiImageData = image.PublicUrl
                });
            }

            await RecalculateExperienceScoreAsync(context, viewerId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await CleanupNewImagesAsync(uploadedImages);
            throw;
        }

        await CleanupStoredImagesAsync(imagesToDelete);
        return request.FindId;
    }

    public async Task<Guid> DeleteAsync(
        Guid findId,
        CancellationToken cancellationToken = default)
    {
        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var find = await context.UserFinds
            .Include(item => item.UserFindLocation)
            .Where(item => item.UsfId == findId && item.UsfUsrId == viewerId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Find was not found or is not owned by the signed-in user.");

        var images = await context.UserImages
            .Where(image => image.UsiUsfId == findId)
            .ToListAsync(cancellationToken);
        var commentXrefs = await context.UserFindsCommentXrefs
            .Where(xref => xref.UcxUsfId == findId)
            .Include(xref => xref.UserFindsComment)
            .ToListAsync(cancellationToken);
        var commentIds = commentXrefs.Select(xref => xref.UcxUscId).ToArray();
        var votes = await context.UserVotes
            .Where(vote => vote.UsvUsfId == findId ||
                (vote.UsvUscId.HasValue && commentIds.Contains(vote.UsvUscId.Value)))
            .ToListAsync(cancellationToken);

        context.UserVotes.RemoveRange(votes);
        foreach (var xref in commentXrefs)
        {
            if (xref.UserFindsComment is not null)
                xref.UserFindsComment.UscParentCommentId = null;
        }
        context.UserFindsCommentXrefs.RemoveRange(commentXrefs);
        context.UserFindsComments.RemoveRange(commentXrefs
            .Where(xref => xref.UserFindsComment is not null)
            .Select(xref => xref.UserFindsComment));
        context.UserImages.RemoveRange(images);
        if (find.UserFindLocation is not null)
            context.UserFindLocations.Remove(find.UserFindLocation);
        context.UserFinds.Remove(find);

        await context.SaveChangesAsync(cancellationToken);
        await RecalculateExperienceScoreAsync(context, viewerId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await CleanupStoredImagesAsync(images);
        return findId;
    }

    private async Task CleanupNewImagesAsync(IEnumerable<PhotoStorageReference> images)
    {
        foreach (var image in images)
        {
            try
            {
                await _photoService.DeleteAsync(image, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to clean up newly published photo {StorageKey}.", image.StorageKey);
            }
        }
    }

    private async Task CleanupStoredImagesAsync(IEnumerable<UserImage> images)
    {
        foreach (var image in images)
        {
            try
            {
                await _photoService.DeleteByPublicUrlAsync(image.UsiImageData, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to clean up photo URL {PublicUrl} after its database reference was removed.", image.UsiImageData);
            }
        }
    }

    internal static async Task RecalculateExperienceScoreAsync(
        ForagerDbContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var scores = await context.UserFinds
            .Where(find => find.UsfUsrId == userId)
            .Select(find => find.UsfAccuracyScore ?? 0)
            .ToListAsync(cancellationToken);
        var user = await context.Users.FirstOrDefaultAsync(user => user.UsrId == userId, cancellationToken);
        if (user is not null)
            user.UsrExpScore = scores.Sum() * scores.Count;
    }

    private Guid GetViewerId()
    {
        var sessionUser = _session.SessionUser;
        if (!_session.IsAuthenticated || sessionUser is null || sessionUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("A signed-in user is required for find commands.");

        return sessionUser.UserId;
    }
}
