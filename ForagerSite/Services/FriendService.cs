using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.DataContainer;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace ForagerSite.Services
{
    public class FriendService : IFriendService
    {
        private readonly IDbContextFactory<ForagerDbContext> _db;

        private readonly INotificationService _notificationService;

        public FriendService(IDbContextFactory<ForagerDbContext> db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        //private static (Guid A, Guid B) NormalizePair(Guid u1, Guid u2)
        //    => u1.CompareTo(u2) < 0 ? (u1, u2) : (u2, u1);
        private static (Guid A, Guid B) NormalizePair(Guid u1, Guid u2)
        {
            var s1 = new SqlGuid(u1);
            var s2 = new SqlGuid(u2);

            return s1.CompareTo(s2) < 0 ? (u1, u2) : (u2, u1);
        }

        public async Task<FriendUiStatus> GetStatus(Guid me, Guid other)
        {
            if (me == other) return FriendUiStatus.None;

            using var ctx = _db.CreateDbContext();

            // Relationship row? (friends/blocked)
            var (a, b) = NormalizePair(me, other);
            var rel = await ctx.UserRelationships
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UrlUserAId == a && r.UrlUserBId == b);

            if (rel != null)
            {
                if (rel.UrlStatus == RelationshipStatus.Friends)
                    return FriendUiStatus.Friends;

                if (rel.UrlStatus == RelationshipStatus.Blocked)
                    return FriendUiStatus.Blocked;
            }

            // Pending request from me -> them?
            var outgoing = await ctx.FriendRequests
                .AsNoTracking()
                .AnyAsync(fr =>
                    fr.FrqRequesterUserId == me &&
                    fr.FrqAddresseeUserId == other &&
                    fr.FrqStatus == FriendRequestStatus.Pending);

            if (outgoing) return FriendUiStatus.RequestSent;

            // Pending request from them -> me?
            var incoming = await ctx.FriendRequests
                .AsNoTracking()
                .AnyAsync(fr =>
                    fr.FrqRequesterUserId == other &&
                    fr.FrqAddresseeUserId == me &&
                    fr.FrqStatus == FriendRequestStatus.Pending);

            if (incoming) return FriendUiStatus.RequestReceived;

            return FriendUiStatus.None;
        }

        public async Task SendRequest(Guid me, Guid other)
        {
            if (me == other) return;

            using var ctx = _db.CreateDbContext();

            // If relationship says blocked, don't allow requests
            var (a, b) = NormalizePair(me, other);
            var rel = await ctx.UserRelationships
                .FirstOrDefaultAsync(r => r.UrlUserAId == a && r.UrlUserBId == b);

            if (rel?.UrlStatus == RelationshipStatus.Blocked)
                return;

            // If they already requested me, do nothing (for now)
            var reversePending = await ctx.FriendRequests.AnyAsync(fr =>
                fr.FrqRequesterUserId == other &&
                fr.FrqAddresseeUserId == me &&
                fr.FrqStatus == FriendRequestStatus.Pending);

            if (reversePending)
                return;

            var existing = await ctx.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    fr.FrqRequesterUserId == me &&
                    fr.FrqAddresseeUserId == other);

            if (existing != null)
            {
                if (existing.FrqStatus == FriendRequestStatus.Pending)
                    return;

                if (existing.FrqStatus == FriendRequestStatus.Declined)
                {
                    // Re-open the request
                    existing.FrqStatus = FriendRequestStatus.Pending;
                    existing.FrqCreatedDate = DateTime.UtcNow;
                    existing.FrqAcceptedDate = null;

                    await ctx.SaveChangesAsync();

                    // ✅ ADD THIS: create notification for the addressee
                    await _notificationService.CreateFriendRequestNotification(
                        addresseeUserId: other,
                        requesterUserId: me,
                        friendRequestId: existing.FrqId);

                    return;
                }

                if (existing.FrqStatus == FriendRequestStatus.Accepted)
                    return;
            }

            // Create new request
            var fr = new FriendRequest
            {
                FrqId = Guid.NewGuid(),
                FrqRequesterUserId = me,
                FrqAddresseeUserId = other,
                FrqStatus = FriendRequestStatus.Pending,
                FrqCreatedDate = DateTime.UtcNow,
                FrqAcceptedDate = null
            };

            ctx.FriendRequests.Add(fr);
            await ctx.SaveChangesAsync();

            // ✅ ADD THIS: create notification for the addressee
            await _notificationService.CreateFriendRequestNotification(
                addresseeUserId: other,
                requesterUserId: me,
                friendRequestId: fr.FrqId);
        }


        public async Task AcceptRequest(Guid me, Guid other)
        {
            if (me == other) return;

            using var ctx = _db.CreateDbContext();

            // other -> me must be pending
            var req = await ctx.FriendRequests.FirstOrDefaultAsync(fr =>
                fr.FrqRequesterUserId == other &&
                fr.FrqAddresseeUserId == me &&
                fr.FrqStatus == FriendRequestStatus.Pending);

            if (req == null) return;

            req.FrqStatus = FriendRequestStatus.Accepted;
            req.FrqAcceptedDate = DateTime.UtcNow;

            // Upsert relationship (one row per pair)
            var (a, b) = NormalizePair(me, other);
            var rel = await ctx.UserRelationships
                .FirstOrDefaultAsync(r => r.UrlUserAId == a && r.UrlUserBId == b);

            if (rel == null)
            {
                ctx.UserRelationships.Add(new UserRelationship
                {
                    UrlId = Guid.NewGuid(),
                    UrlUserAId = a,
                    UrlUserBId = b,
                    UrlStatus = RelationshipStatus.Friends,
                    UrlActionUserId = me,
                    UrlCreatedDate = DateTime.UtcNow
                });
            }
            else
            {
                rel.UrlStatus = RelationshipStatus.Friends;
                rel.UrlActionUserId = me;
                rel.UrlUpdatedDate = DateTime.UtcNow;
            }

            await ctx.SaveChangesAsync();
        }

        public async Task DeclineRequest(Guid me, Guid other)
        {
            if (me == other) return;

            using var ctx = _db.CreateDbContext();

            // other -> me pending
            var req = await ctx.FriendRequests.FirstOrDefaultAsync(fr =>
                fr.FrqRequesterUserId == other &&
                fr.FrqAddresseeUserId == me &&
                fr.FrqStatus == FriendRequestStatus.Pending);

            if (req == null) return;

            req.FrqStatus = FriendRequestStatus.Declined;
            await ctx.SaveChangesAsync();
        }

        public async Task Unfriend(Guid me, Guid other)
        {
            if (me == other) return;

            using var ctx = _db.CreateDbContext();

            var (a, b) = NormalizePair(me, other);

            // Delete relationship row (Friends/Blocked lives here)
            var rel = await ctx.UserRelationships
                .FirstOrDefaultAsync(r => r.UrlUserAId == a && r.UrlUserBId == b);

            if (rel != null && rel.UrlStatus == RelationshipStatus.Friends)
            {
                ctx.UserRelationships.Remove(rel);
            }

            // Optional cleanup:
            // keep FriendRequests history, OR reset last accepted to Declined, OR leave alone.
            // I recommend leaving it, but if you want to clear "accepted" so UI is simpler:
            var accepted = await ctx.FriendRequests
                .Where(fr =>
                    ((fr.FrqRequesterUserId == me && fr.FrqAddresseeUserId == other) ||
                     (fr.FrqRequesterUserId == other && fr.FrqAddresseeUserId == me))
                    && fr.FrqStatus == FriendRequestStatus.Accepted)
                .ToListAsync();

            foreach (var fr in accepted)
            {
                fr.FrqStatus = FriendRequestStatus.Declined; // effectively "not friends anymore"
                                                             // fr.FrqAcceptedDate = null; // optional
            }

            await ctx.SaveChangesAsync();
        }
        public async Task<List<FriendListItem>> GetFriends(Guid userId)
        {
            using var ctx = _db.CreateDbContext();

            // Get friend IDs
            var friendIds = await ctx.UserRelationships
                .AsNoTracking()
                .Where(r =>
                    r.UrlStatus == RelationshipStatus.Friends &&
                    (r.UrlUserAId == userId || r.UrlUserBId == userId))
                .Select(r => r.UrlUserAId == userId ? r.UrlUserBId : r.UrlUserAId)
                .Distinct()
                .ToListAsync();

            if (!friendIds.Any())
                return new();

            // Fetch usernames
            var users = await ctx.Users
                .AsNoTracking()
                .Where(u => friendIds.Contains(u.UsrId))
                .Select(u => new
                {
                    u.UsrId,
                    UserName = u.UserSecurity.UssUsername
                })
                .ToListAsync();

            // Fetch profile images ONLY
            var profileImages = await ctx.UserImages
                .AsNoTracking()
                .Where(img =>
                    img.UsiUsrId != null &&
                    img.UsiUsfId == null &&
                    friendIds.Contains(img.UsiUsrId.Value))
                .ToListAsync();

            return users
                .Select(u => new FriendListItem
                {
                    UserId = u.UsrId,
                    UserName = u.UserName,
                    ProfilePicUrl =
                        profileImages.FirstOrDefault(p => p.UsiUsrId == u.UsrId)?.UsiImageData
                        ?? "UserProfileImages/Shared/PlaceHolder.jpeg"
                })
                .OrderBy(x => x.UserName)
                .ToList();
        }


    }
}
