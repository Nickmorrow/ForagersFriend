using DataAccess.Models;

namespace ForagerSite.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotifications(Guid userId, bool unreadOnly = false, int take = 50);
        Task<int> GetUnreadCount(Guid userId);

        Task CreateFriendRequestNotification(Guid addresseeUserId, Guid requesterUserId, Guid friendRequestId);

        Task MarkRead(Guid notificationId, Guid userId);

        Task AcceptFriendRequestFromNotification(Guid notificationId, Guid currentUserId);
        Task DeclineFriendRequestFromNotification(Guid notificationId, Guid currentUserId);
    }
}
