using DataAccess.Models;
using ForagerSite.DataContainer;

namespace ForagerSite.Services.Interfaces
{
    public enum FriendUiStatus
    {
        None = 0,
        RequestSent = 1,
        RequestReceived = 2,
        Friends = 3,
        Blocked = 4
    }

    public interface IFriendService
    {
        Task<FriendUiStatus> GetStatus(Guid me, Guid other);
        Task SendRequest(Guid me, Guid other);
        Task AcceptRequest(Guid me, Guid other);
        Task DeclineRequest(Guid me, Guid other);
        Task Unfriend(Guid me, Guid other);
        Task<List<FriendListItem>> GetFriends(Guid userId);


    }
}
