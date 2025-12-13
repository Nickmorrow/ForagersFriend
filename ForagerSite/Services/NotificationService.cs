using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDbContextFactory<ForagerDbContext> _db;

        public NotificationService(IDbContextFactory<ForagerDbContext> db)
        {
            _db = db;
        }

        public async Task<List<Notification>> GetNotifications(Guid userId, bool unreadOnly = false, int take = 50)
        {
            using var ctx = _db.CreateDbContext();

            var q = ctx.Notifications
                .AsNoTracking()
                .Include(n => n.ActorUser)
                .Where(n => n.NotUserId == userId);

            if (unreadOnly)
                q = q.Where(n => !n.NotIsRead);

            return await q.OrderByDescending(n => n.NotCreatedDate)
                          .Take(take)
                          .ToListAsync();
        }

        public async Task<int> GetUnreadCount(Guid userId)
        {
            using var ctx = _db.CreateDbContext();
            return await ctx.Notifications.CountAsync(n => n.NotUserId == userId && !n.NotIsRead);
        }

        public async Task CreateFriendRequestNotification(Guid addresseeUserId, Guid requesterUserId, Guid friendRequestId)
        {
            using var ctx = _db.CreateDbContext();

            ctx.Notifications.Add(new Notification
            {
                NotId = Guid.NewGuid(),
                NotUserId = addresseeUserId,
                NotActorUserId = requesterUserId,
                NotType = "FriendRequest",
                NotEntityType = "FriendRequest",
                NotEntityId = friendRequestId,
                NotMessage = "sent you a friend request",
                NotIsRead = false,
                NotCreatedDate = DateTime.UtcNow
            });

            await ctx.SaveChangesAsync();
        }

        public async Task MarkRead(Guid notificationId, Guid userId)
        {
            using var ctx = _db.CreateDbContext();

            var n = await ctx.Notifications
                .FirstOrDefaultAsync(x => x.NotId == notificationId && x.NotUserId == userId);

            if (n == null) return;

            n.NotIsRead = true;
            n.NotReadDate = DateTime.UtcNow;

            await ctx.SaveChangesAsync();
        }

        public async Task AcceptFriendRequestFromNotification(Guid notificationId, Guid me)
        {
            using var ctx = _db.CreateDbContext();

            var note = await ctx.Notifications.FirstOrDefaultAsync(n => n.NotId == notificationId && n.NotUserId == me);
            if (note == null) return;

            if (note.NotType != "FriendRequest" || note.NotEntityId == null)
                return;

            var requestId = note.NotEntityId.Value;

            var req = await ctx.FriendRequests.FirstOrDefaultAsync(fr => fr.FrqId == requestId);
            if (req == null) return;

            // Only addressee can accept
            if (req.FrqAddresseeUserId != me) return;
            if (req.FrqStatus != FriendRequestStatus.Pending) return;

            // Accept request
            req.FrqStatus = FriendRequestStatus.Accepted;
            req.FrqAcceptedDate = DateTime.UtcNow;

            // Upsert relationship
            var (a, b) = NormalizePair(req.FrqRequesterUserId, req.FrqAddresseeUserId);

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

            // Mark the friend request notification read
            note.NotIsRead = true;
            note.NotReadDate = DateTime.UtcNow;

            // Create "accepted" notification for requester
            ctx.Notifications.Add(new Notification
            {
                NotId = Guid.NewGuid(),
                NotUserId = req.FrqRequesterUserId,
                NotActorUserId = me,
                NotType = "FriendAccepted",
                NotEntityId = req.FrqId,
                NotEntityType = "FriendRequest",
                NotMessage = "accepted your friend request.",
                NotIsRead = false,
                NotCreatedDate = DateTime.UtcNow
            });

            await ctx.SaveChangesAsync();
        }

        public async Task DeclineFriendRequestFromNotification(Guid notificationId, Guid me)
        {
            using var ctx = _db.CreateDbContext();

            var note = await ctx.Notifications.FirstOrDefaultAsync(n => n.NotId == notificationId && n.NotUserId == me);
            if (note == null) return;

            if (note.NotType != "FriendRequest" || note.NotEntityId == null)
                return;

            var requestId = note.NotEntityId.Value;

            var req = await ctx.FriendRequests.FirstOrDefaultAsync(fr => fr.FrqId == requestId);
            if (req == null) return;

            // Only addressee can decline
            if (req.FrqAddresseeUserId != me) return;
            if (req.FrqStatus != FriendRequestStatus.Pending) return;

            req.FrqStatus = FriendRequestStatus.Declined;

            // Mark notification read
            note.NotIsRead = true;
            note.NotReadDate = DateTime.UtcNow;

            // Optional: notify requester of decline
            ctx.Notifications.Add(new Notification
            {
                NotId = Guid.NewGuid(),
                NotUserId = req.FrqRequesterUserId,
                NotActorUserId = me,
                NotType = "FriendDeclined",
                NotEntityId = req.FrqId,
                NotEntityType = "FriendRequest",
                NotMessage = "declined your friend request.",
                NotIsRead = false,
                NotCreatedDate = DateTime.UtcNow
            });

            await ctx.SaveChangesAsync();
        }

        private static (Guid A, Guid B) NormalizePair(Guid u1, Guid u2)
            => u1.CompareTo(u2) < 0 ? (u1, u2) : (u2, u1);


        
    }
}
