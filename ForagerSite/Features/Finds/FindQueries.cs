using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.DataContainer;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Features.Finds;

public sealed class FindQueries
{
    private const string ProfileImagePrefix = "/UserProfileImages";

    private readonly IDbContextFactory<ForagerDbContext> _dbContextFactory;
    private readonly IUserSessionService _session;

    public FindQueries(
        IDbContextFactory<ForagerDbContext> dbContextFactory,
        IUserSessionService session)
    {
        _dbContextFactory = dbContextFactory;
        _session = session;
    }

    public async Task<IReadOnlyList<ProfileFindSummary>> GetProfileFindSummariesAsync(
        Guid profileUserId,
        CancellationToken cancellationToken = default)
    {
        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await FindVisibility.VisibleToViewer(context.UserFinds.AsNoTracking(), viewerId)
            .Where(find => find.UsfUsrId == profileUserId && find.UserFindLocation != null)
            .OrderByDescending(find => find.UsfFindDate)
            .ThenByDescending(find => find.UsfId)
            .Select(find => new ProfileFindSummary(
                find.UsfId,
                find.UsfUsrId,
                find.UsfName,
                find.UsfSpeciesName,
                find.UsfSpeciesType,
                find.UsfFindDate,
                find.UsfUseCategory,
                find.UsfAccessibility,
                find.UserFindLocation!.UslLatitude,
                find.UserFindLocation.UslLongitude,
                context.UserImages
                    .Where(image => image.UsiUsfId == find.UsfId)
                    .OrderBy(image => image.UsiId)
                    .Select(image => image.UsiImageData)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfileFindSummary?> GetProfileFindSummaryByIdAsync(
        Guid findId,
        CancellationToken cancellationToken = default)
    {
        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await FindVisibility.VisibleToViewer(context.UserFinds.AsNoTracking(), viewerId)
            .Where(find => find.UsfId == findId && find.UserFindLocation != null)
            .Select(find => new ProfileFindSummary(
                find.UsfId,
                find.UsfUsrId,
                find.UsfName,
                find.UsfSpeciesName,
                find.UsfSpeciesType,
                find.UsfFindDate,
                find.UsfUseCategory,
                find.UsfAccessibility,
                find.UserFindLocation!.UslLatitude,
                find.UserFindLocation.UslLongitude,
                context.UserImages
                    .Where(image => image.UsiUsfId == find.UsfId)
                    .OrderBy(image => image.UsiId)
                    .Select(image => image.UsiImageData)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, string>> GetUserNamesAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<Guid, string>();

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.UsrId))
            .Select(user => new
            {
                user.UsrId,
                Username = user.UserSecurity.UssUsername
            })
            .ToDictionaryAsync(row => row.UsrId, row => row.Username, cancellationToken);
    }

    public async Task<UserFindsDataContainer?> GetFindDetailByIdAsync(
        Guid findId,
        CancellationToken cancellationToken = default)
    {
        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var detail = await FindVisibility.VisibleToViewer(context.UserFinds.AsNoTracking(), viewerId)
            .Where(find => find.UsfId == findId && find.UserFindLocation != null)
            .Select(find => new
            {
                OwnerId = find.UsfUsrId,
                OwnerName = find.User.UserSecurity.UssUsername,
                OwnerProfilePic = context.UserImages
                    .Where(image => image.UsiUsrId == find.UsfUsrId &&
                                    image.UsiUsfId == null &&
                                    image.UsiImageData.StartsWith(ProfileImagePrefix))
                    .OrderBy(image => image.UsiId)
                    .Select(image => image.UsiImageData)
                    .FirstOrDefault(),
                FindId = find.UsfId,
                FindName = find.UsfName,
                SpeciesName = find.UsfSpeciesName,
                SpeciesType = find.UsfSpeciesType,
                FindDate = find.UsfFindDate,
                UseCategory = find.UsfUseCategory,
                Features = find.UsfFeatures,
                LookAlikes = find.UsfLookAlikes,
                HarvestMethod = find.UsfHarvestMethod,
                TastesLike = find.UsfTastesLike,
                Description = find.UsfDescription,
                FindScore = find.UsfAccuracyScore,
                Accessibility = find.UsfAccessibility,
                LocationId = find.UserFindLocation!.UslId,
                Latitude = find.UserFindLocation.UslLatitude,
                Longitude = find.UserFindLocation.UslLongitude
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null)
            return null;

        var findImages = await context.UserImages
            .AsNoTracking()
            .Where(image => image.UsiUsfId == findId)
            .OrderBy(image => image.UsiId)
            .Select(image => new ImageDC
            {
                imageId = image.UsiId,
                imgUserId = image.UsiUsrId,
                imgFindId = image.UsiUsfId,
                imageData = image.UsiImageData
            })
            .ToListAsync(cancellationToken);

        var comments = await context.UserFindsCommentXrefs
            .AsNoTracking()
            .Where(xref => xref.UcxUsfId == findId)
            .OrderBy(xref => xref.UserFindsComment.UscCommentDate)
            .Select(xref => new
            {
                XrefId = xref.UcxId,
                UserId = xref.UcxUsrId,
                CommentId = xref.UserFindsComment.UscId,
                Comment = xref.UserFindsComment.UscComment,
                Score = xref.UserFindsComment.UscCommentScore,
                Date = xref.UserFindsComment.UscCommentDate,
                ParentId = xref.UserFindsComment.UscParentCommentId,
                UserProfilePic = context.UserImages
                    .Where(image => image.UsiUsrId == xref.UcxUsrId &&
                                    image.UsiUsfId == null &&
                                    image.UsiImageData.StartsWith(ProfileImagePrefix))
                    .OrderBy(image => image.UsiId)
                    .Select(image => image.UsiImageData)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var commentIds = comments.Select(comment => comment.CommentId).ToArray();
        var votes = await context.UserVotes
            .AsNoTracking()
            .Where(vote => vote.UsvUsfId == findId ||
                (vote.UsvUscId.HasValue && commentIds.Contains(vote.UsvUscId.Value)))
            .Select(vote => new UserVoteDC
            {
                voteId = vote.UsvId,
                voteUserId = vote.UsvUsrId,
                voteFindId = vote.UsvUsfId,
                voteCommentId = vote.UsvUscId,
                voteValue = vote.UsvVoteValue
            })
            .ToListAsync(cancellationToken);

        var find = new FindDC
        {
            findId = detail.FindId,
            findUserId = detail.OwnerId,
            findName = detail.FindName,
            speciesName = detail.SpeciesName,
            speciesType = detail.SpeciesType,
            findDate = detail.FindDate,
            useCategory = detail.UseCategory,
            features = detail.Features,
            lookAlikes = detail.LookAlikes,
            harvestMethod = detail.HarvestMethod,
            tastesLike = detail.TastesLike,
            description = detail.Description,
            findScore = detail.FindScore,
            accessibility = detail.Accessibility,
            findLocation = new FindLocationDC
            {
                locId = detail.LocationId,
                locFindId = detail.FindId,
                latitude = detail.Latitude,
                longitude = detail.Longitude
            },
            findImages = findImages,
            findVotes = votes.Where(vote => vote.voteFindId == findId).ToList(),
            findsCommentXrefs = comments.Select(comment => new FindsCommentXrefDC
            {
                comXId = comment.XrefId,
                comxUserId = comment.UserId,
                comxComId = comment.CommentId,
                comxFindId = findId,
                CommentUserProfilePic = comment.UserProfilePic ?? UserFindsDataContainer.PlaceholderImageUrl,
                findsComment = new FindCommentDC
                {
                    comId = comment.CommentId,
                    comment = comment.Comment,
                    commentScore = comment.Score,
                    commentDate = comment.Date,
                    parentCommentId = comment.ParentId,
                    commentVotes = votes.Where(vote => vote.voteCommentId == comment.CommentId).ToList()
                }
            }).ToList()
        };

        return new UserFindsDataContainer
        {
            userId = detail.OwnerId,
            profilePic = detail.OwnerProfilePic ?? UserFindsDataContainer.PlaceholderImageUrl,
            userName = detail.OwnerName,
            finds = new List<FindDC> { find }
        };
    }

    private Guid GetViewerId()
    {
        var sessionUser = _session.SessionUser;
        if (!_session.IsAuthenticated || sessionUser is null || sessionUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("A signed-in user is required for find queries.");
        }

        return sessionUser.UserId;
    }
}
