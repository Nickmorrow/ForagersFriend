using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        private static (Guid A, Guid B) NormalizePair(Guid u1, Guid u2)
            => u1.CompareTo(u2) < 0 ? (u1, u2) : (u2, u1);

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
    }
}
