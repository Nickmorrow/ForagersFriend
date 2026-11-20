using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Services.Interfaces;
using ForagerSite.Utilities;
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
    IReadOnlyList<string> ImageUrls,
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
    IReadOnlyList<string>? ImageUrlsToAdd,
    IReadOnlyList<string>? ImageUrlsToDelete,
    string Accessibility);

public sealed class FindCommands
{
    private readonly IDbContextFactory<ForagerDbContext> _dbContextFactory;
    private readonly IUserSessionService _session;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public FindCommands(
        IDbContextFactory<ForagerDbContext> dbContextFactory,
        IUserSessionService session,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _dbContextFactory = dbContextFactory;
        _session = session;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<Guid> CreateAsync(
        CreateFindRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var viewerId = GetViewerId();
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

        foreach (var imageUrl in request.ImageUrls)
        {
            context.UserImages.Add(new UserImage
            {
                UsiUsrId = viewerId,
                UsiUsfId = find.UsfId,
                UsiImageData = imageUrl
            });
        }

        await RecalculateExperienceScoreAsync(context, viewerId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return find.UsfId;
    }

    public async Task<Guid> UpdateAsync(
        UpdateFindRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var viewerId = GetViewerId();
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
        var imageUrlsToDelete = request.ImageUrlsToDelete?.ToHashSet(StringComparer.Ordinal);
        if (imageUrlsToDelete is { Count: > 0 })
        {
            var imagesToDelete = existingImages
                .Where(image => imageUrlsToDelete.Contains(image.UsiImageData))
                .ToList();
            if (imagesToDelete.Count != imageUrlsToDelete.Count)
                throw new InvalidOperationException("One or more images do not belong to this find.");

            context.UserImages.RemoveRange(imagesToDelete);
        }

        foreach (var imageUrl in request.ImageUrlsToAdd ?? Array.Empty<string>())
        {
            context.UserImages.Add(new UserImage
            {
                UsiUsrId = viewerId,
                UsiUsfId = find.UsfId,
                UsiImageData = imageUrl
            });
        }

        await RecalculateExperienceScoreAsync(context, viewerId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return find.UsfId;
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

        await DeleteFindImagesAsync(images);

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
        return findId;
    }

    private async Task DeleteFindImagesAsync(IEnumerable<UserImage> images)
    {
        var storageMode = _configuration["ImageStorage"] ?? "Local";
        var blobConnectionString = _configuration["BlobStorage:ConnectionString"];

        foreach (var image in images)
        {
            var imagePathOrUrl = image.UsiImageData;
            if (storageMode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(blobConnectionString))
                    throw new InvalidOperationException("Blob storage connection string missing.");

                await BlobDeleteHelper.DeleteByUrlIfExistsAsync(imagePathOrUrl, blobConnectionString);
                continue;
            }

            var relativeUrl = imagePathOrUrl?.TrimStart('/');
            if (string.IsNullOrWhiteSpace(relativeUrl))
                continue;

            var filePath = Path.Combine(
                _environment.WebRootPath,
                relativeUrl.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath))
                File.Delete(filePath);
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
