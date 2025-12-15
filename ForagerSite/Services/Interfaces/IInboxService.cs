using ForagerSite.DataContainer;
using DataAccess.Models;
using System.Threading;

namespace ForagerSite.Services.Interfaces;

public interface IInboxService
{
    Task<List<ThreadListItemDc>> GetThreads(Guid userId, MailboxFolder folder, string? search, int take = 50);
    //Task<List<UserPickItemDc>> SearchUsers(string term, Guid excludeUserId, int take = 10);
    Task<List<UserPickItemDc>> SearchUsers(string term, Guid currentUserId, int take = 8);


    Task<ThreadDc> GetThread(Guid userId, Guid threadId);
    Task SendReply(Guid senderId, Guid threadId, string body);
    Task MarkRead(Guid userId, Guid threadId);
    Task ArchiveThread(Guid userId, Guid threadId);
    Task TrashThread(Guid userId, Guid threadId);
    Task RestoreThread(Guid userId, Guid threadId);

    Task<Guid> SendMessage(Guid senderId, Guid recipientId, string subject, string body, Guid? threadId = null);
}
