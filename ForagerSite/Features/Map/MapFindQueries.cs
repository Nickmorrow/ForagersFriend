using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Features.Finds;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Features.Map;

public sealed class MapFindQueries
{
    private const int MaxPinsPerViewport = 500;

    private readonly IDbContextFactory<ForagerDbContext> _dbContextFactory;
    private readonly IUserSessionService _session;

    public MapFindQueries(
        IDbContextFactory<ForagerDbContext> dbContextFactory,
        IUserSessionService session)
    {
        _dbContextFactory = dbContextFactory;
        _session = session;
    }

    public async Task<MapPinPage> GetViewportPinsAsync(
        MapPinRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateViewport(request.Viewport);

        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var finds = context.UserFinds
            .AsNoTracking()
            .Where(find => find.UserFindLocation != null);

        finds = FindVisibility.VisibleToViewer(finds, viewerId);
        if (request.Scope.IncludeMine || request.Scope.IncludeFriends)
        {
            var friendIds = context.UserRelationships
                .AsNoTracking()
                .Where(relationship =>
                    relationship.UrlStatus == RelationshipStatus.Friends &&
                    (relationship.UrlUserAId == viewerId || relationship.UrlUserBId == viewerId))
                .Select(relationship =>
                    relationship.UrlUserAId == viewerId
                        ? relationship.UrlUserBId
                        : relationship.UrlUserAId);

            finds = finds.Where(find =>
                (request.Scope.IncludeMine && find.UsfUsrId == viewerId) ||
                (request.Scope.IncludeFriends && friendIds.Contains(find.UsfUsrId)));
        }

        var viewport = request.Viewport;
        finds = finds.Where(find =>
            find.UserFindLocation!.UslLatitude >= viewport.South &&
            find.UserFindLocation.UslLatitude <= viewport.North);

        finds = viewport.West <= viewport.East
            ? finds.Where(find =>
                find.UserFindLocation!.UslLongitude >= viewport.West &&
                find.UserFindLocation.UslLongitude <= viewport.East)
            : finds.Where(find =>
                find.UserFindLocation!.UslLongitude >= viewport.West ||
                find.UserFindLocation.UslLongitude <= viewport.East);

        var rows = await finds
            .OrderByDescending(find => find.UsfFindDate)
            .ThenByDescending(find => find.UsfId)
            .Select(find => new MapPin(
                find.UsfId,
                find.UsfUsrId,
                find.User.UserSecurity.UssUsername,
                context.UserImages
                    .Where(image => image.UsiUsfId == find.UsfId)
                    .OrderBy(image => image.UsiId)
                    .Select(image => image.UsiImageData)
                    .FirstOrDefault(),
                find.UsfName,
                find.UsfSpeciesName,
                find.UsfSpeciesType,
                find.UsfUseCategory,
                find.UsfFindDate,
                find.UserFindLocation!.UslLatitude,
                find.UserFindLocation.UslLongitude))
            .Take(MaxPinsPerViewport + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > MaxPinsPerViewport;
        if (hasMore)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        return new MapPinPage(rows, hasMore);
    }

    private Guid GetViewerId()
    {
        var sessionUser = _session.SessionUser;
        if (!_session.IsAuthenticated || sessionUser is null || sessionUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("A signed-in user is required for map find queries.");
        }

        return sessionUser.UserId;
    }

    private static void ValidateViewport(MapViewport viewport)
    {
        ArgumentNullException.ThrowIfNull(viewport);

        if (!double.IsFinite(viewport.South) ||
            !double.IsFinite(viewport.West) ||
            !double.IsFinite(viewport.North) ||
            !double.IsFinite(viewport.East) ||
            viewport.South < -90 ||
            viewport.North > 90 ||
            viewport.South >= viewport.North ||
            viewport.West < -180 ||
            viewport.West > 180 ||
            viewport.East < -180 ||
            viewport.East > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(viewport), "Viewport coordinates are invalid.");
        }
    }
}
