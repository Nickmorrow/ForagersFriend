using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.DataContainer;
using DataAccess.Models;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ForagerSite.Services;

public class InboxService : IInboxService
{
    private readonly IDbContextFactory<ForagerDbContext> _db;

    public InboxService(IDbContextFactory<ForagerDbContext> db)
    {
        _db = db;
    }

    public async Task<List<ThreadListItemDc>> GetThreads(Guid userId, MailboxFolder folder, string? search, int take = 50)
    {
        using var ctx = _db.CreateDbContext();

        // Threads where user participates
        var threadsQ =
            ctx.UserMessageThreads
               .AsNoTracking()
               .Where(t => t.UmtUserAId == userId || t.UmtUserBId == userId);

        // Per-user thread state
        var statesQ =
            ctx.UserThreadStates
               .AsNoTracking()
               .Where(s => s.UtsUserId == userId);

        // Last message timestamp per thread
        var lastMsgQ =
            ctx.UserMessages
               .AsNoTracking()
               .GroupBy(m => m.UsmThreadId)
               .Select(g => new
               {
                   ThreadId = g.Key,
                   LastUtc = g.Max(x => x.UsmSendDate)
               });

        // Threads + last message time
        var joinedQ =
            from t in threadsQ
            join lm in lastMsgQ on t.UmtId equals lm.ThreadId
            select new { Thread = t, lm.LastUtc };

        // Folder-specific direction filtering
        if (folder == MailboxFolder.Inbox)
        {
            // Latest message must be inbound
            joinedQ =
                from x in joinedQ
                join m in ctx.UserMessages.AsNoTracking()
                    on new { Tid = x.Thread.UmtId, Utc = x.LastUtc }
                    equals new { Tid = m.UsmThreadId, Utc = m.UsmSendDate }
                where m.UsmRecipientId == userId
                select x;
        }
        else if (folder == MailboxFolder.Sent)
        {
            joinedQ =
                from x in joinedQ
                where ctx.UserMessages.Any(m =>
                    m.UsmThreadId == x.Thread.UmtId &&
                    m.UsmSenderId == userId)
                select x;
        }

        // Join in state
        var withStateQ =
            from x in joinedQ
            join s in statesQ on x.Thread.UmtId equals s.UtsThreadId into sgj
            from s in sgj.DefaultIfEmpty()
            select new { x.Thread, x.LastUtc, State = s };

        // Archive / Trash filtering (NO switch expression)
        if (folder == MailboxFolder.Archive)
        {
            withStateQ =
                withStateQ.Where(x =>
                    x.State != null &&
                    x.State.UtsArchivedUtc != null &&
                    x.State.UtsDeletedUtc == null);
        }
        else if (folder == MailboxFolder.Trash)
        {
            withStateQ =
                withStateQ.Where(x =>
                    x.State != null &&
                    x.State.UtsDeletedUtc != null);
        }
        else
        {
            // Inbox + Sent default view
            withStateQ =
                withStateQ.Where(x =>
                    x.State == null ||
                    (x.State.UtsArchivedUtc == null && x.State.UtsDeletedUtc == null));
        }

        // Search (subject, body, other user name)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            withStateQ =
                from x in withStateQ
                where
                    ctx.UserMessages.Any(m =>
                        m.UsmThreadId == x.Thread.UmtId &&
                        (m.UsmSubject.Contains(term) || m.UsmMessage.Contains(term)))
                    ||
                    (x.Thread.UmtUserAId == userId
                        ? x.Thread.UserB.UsrName.Contains(term)
                        : x.Thread.UserA.UsrName.Contains(term))
                select x;
        }

        // Final projection
        var results =
            await (from x in withStateQ
                   let other =
                       x.Thread.UmtUserAId == userId
                           ? x.Thread.UserB
                           : x.Thread.UserA

                   let lastMessage =
                       ctx.UserMessages.AsNoTracking()
                           .Where(m => m.UsmThreadId == x.Thread.UmtId)
                           .OrderByDescending(m => m.UsmSendDate)
                           .Select(m => new
                           {
                               m.UsmSubject,
                               m.UsmMessage,
                               m.UsmSendDate,
                               m.UsmRecipientId
                           })
                           .First()

                   let lastInboundUtc =
                       ctx.UserMessages.AsNoTracking()
                           .Where(m =>
                               m.UsmThreadId == x.Thread.UmtId &&
                               m.UsmRecipientId == userId)
                           .Max(m => (DateTime?)m.UsmSendDate)

                   select new ThreadListItemDc
                   {
                       ThreadId = x.Thread.UmtId,
                       OtherUserId = other.UsrId,
                       OtherUserName = other.UsrName,

                       Subject = lastMessage.UsmSubject,
                       Preview =
                           lastMessage.UsmMessage.Length > 80
                               ? lastMessage.UsmMessage.Substring(0, 80) + "…"
                               : lastMessage.UsmMessage,

                       LastMessageUtc = lastMessage.UsmSendDate,

                       IsUnread =
                           lastInboundUtc != null &&
                           (x.State == null ||
                            x.State.UtsLastReadUtc == null ||
                            lastInboundUtc > x.State.UtsLastReadUtc)
                   })
                  .OrderByDescending(x => x.LastMessageUtc)
                  .Take(take)
                  .ToListAsync();

        return results;
    }


    public async Task<ThreadDc> GetThread(Guid userId, Guid threadId)
    {
        using var ctx = _db.CreateDbContext();

        var thread = await ctx.UserMessageThreads
            .AsNoTracking()
            .Include(t => t.UserA)
            .Include(t => t.UserB)
            .FirstOrDefaultAsync(t => t.UmtId == threadId);

        if (thread is null || (thread.UmtUserAId != userId && thread.UmtUserBId != userId))
            throw new InvalidOperationException("Thread not found or not accessible.");

        var other = thread.UmtUserAId == userId ? thread.UserB : thread.UserA;

        var messages =
            await (from m in ctx.UserMessages.AsNoTracking()
                   join u in ctx.Users.AsNoTracking() on m.UsmSenderId equals u.UsrId
                   where m.UsmThreadId == threadId
                   orderby m.UsmSendDate
                   select new MessageItemDc
                   {
                       MessageId = m.UsmId,
                       SenderId = m.UsmSenderId,
                       SenderName = u.UsrName,
                       Subject = m.UsmSubject,
                       Body = m.UsmMessage,
                       SentUtc = m.UsmSendDate
                   })
                  .ToListAsync();

        return new ThreadDc
        {
            ThreadId = threadId,
            OtherUserId = other.UsrId,
            OtherUserName = other.UsrName,
            Messages = messages
        };
    }
    //public async Task<List<UserPickItemDc>> SearchUsers(string term, Guid excludeUserId, int take = 10)
    //{
    //    using var ctx = _db.CreateDbContext();

    //    term = term?.Trim() ?? "";
    //    if (term.Length < 1) return new();

    //    return await ctx.Users
    //        .AsNoTracking()
    //        .Where(u => u.UsrId != excludeUserId &&
    //                    (u.UsrName.Contains(term) || u.UserSecurity.UssUsername.Contains(term)))
    //        .OrderBy(u => u.UsrName)
    //        .Select(u => new UserPickItemDc
    //        {
    //            UserId = u.UsrId,
    //            DisplayName = u.UsrName,
    //            Username = u.UserSecurity.UssUsername
    //        })
    //        .Take(take)
    //        .ToListAsync();
    //}
    public async Task<List<UserPickItemDc>> SearchUsers(string term, Guid currentUserId, int take = 8)
    {
        using var ctx = _db.CreateDbContext();
        term = term.Trim();

        if (term.Length < 2) return new();

        // Search by username OR display name.
        // IMPORTANT: UserId must be Users.UsrId
        var results =
            await (from us in ctx.UserSecurities.AsNoTracking()
                   join u in ctx.Users.AsNoTracking() on us.UssUsrId equals u.UsrId
                   where u.UsrId != currentUserId
                   where us.UssUsername.Contains(term) || u.UsrName.Contains(term)
                   orderby u.UsrName
                   select new UserPickItemDc
                   {
                       UserId = u.UsrId,          // ✅ this is what fixes your FK error
                       DisplayName = u.UsrName,   // or whatever your display field is
                       Username = us.UssUsername
                   })
                  .Take(take)
                  .ToListAsync();

        return results;
    }


    public async Task MarkRead(Guid userId, Guid threadId)
    {
        using var ctx = _db.CreateDbContext();

        var state = await ctx.UserThreadStates
            .FirstOrDefaultAsync(s => s.UtsUserId == userId && s.UtsThreadId == threadId);

        if (state is null)
        {
            state = new UserThreadState
            {
                UtsUserId = userId,
                UtsThreadId = threadId,
                UtsLastReadUtc = DateTime.UtcNow,
                UtsUpdatedUtc = DateTime.UtcNow
            };
            ctx.UserThreadStates.Add(state);
        }
        else
        {
            state.UtsLastReadUtc = DateTime.UtcNow;
            state.UtsUpdatedUtc = DateTime.UtcNow;
        }

        await ctx.SaveChangesAsync();
    }

    public Task ArchiveThread(Guid userId, Guid threadId) => SetState(userId, threadId, archived: true, trashed: false);
    public Task TrashThread(Guid userId, Guid threadId) => SetState(userId, threadId, archived: false, trashed: true);
    public Task RestoreThread(Guid userId, Guid threadId) => SetState(userId, threadId, archived: false, trashed: false);

    private async Task SetState(Guid userId, Guid threadId, bool archived, bool trashed)
    {
        using var ctx = _db.CreateDbContext();

        var state = await ctx.UserThreadStates
            .FirstOrDefaultAsync(s => s.UtsUserId == userId && s.UtsThreadId == threadId);

        if (state is null)
        {
            state = new UserThreadState
            {
                UtsUserId = userId,
                UtsThreadId = threadId
            };
            ctx.UserThreadStates.Add(state);
        }

        state.UtsArchivedUtc = archived ? DateTime.UtcNow : null;
        state.UtsDeletedUtc = trashed ? DateTime.UtcNow : null;
        state.UtsUpdatedUtc = DateTime.UtcNow;

        await ctx.SaveChangesAsync();
    }

    public async Task<Guid> SendMessage(Guid senderId, Guid recipientId, string subject, string body, Guid? threadId = null)
    {
        using var ctx = _db.CreateDbContext();

        UserMessageThread thread;

        if (threadId is not null)
        {
            thread = await ctx.UserMessageThreads.FirstAsync(t => t.UmtId == threadId.Value);
        }
        else
        {
            // Canonical ordering so (A,B) and (B,A) map to same thread
            var a = senderId.CompareTo(recipientId) < 0 ? senderId : recipientId;
            var b = senderId.CompareTo(recipientId) < 0 ? recipientId : senderId;

            thread = await ctx.UserMessageThreads.FirstOrDefaultAsync(t => t.UmtUserAId == a && t.UmtUserBId == b)
                     ?? new UserMessageThread { UmtUserAId = a, UmtUserBId = b, UmtCreatedUtc = DateTime.UtcNow };
            if (thread.UmtId == Guid.Empty)
                thread.UmtId = Guid.NewGuid();

            if (ctx.Entry(thread).State == EntityState.Detached)
                ctx.UserMessageThreads.Add(thread);
        }

        var now = DateTime.UtcNow;

        var msg = new UserMessage
        {
            UsmId = Guid.NewGuid(),
            UsmThreadId = thread.UmtId,
            UsmSenderId = senderId,
            UsmRecipientId = recipientId,
            UsmSubject = subject,
            UsmMessage = body,
            UsmSendDate = now,
            UsmStatus = "unread"
        };

        ctx.UserMessages.Add(msg);

        // Ensure per-user states exist
        await EnsureThreadState(ctx, senderId, thread.UmtId);
        await EnsureThreadState(ctx, recipientId, thread.UmtId);

        // Updating UtsUpdatedUtc is enough for now; unread is computed by last inbound > last read.
        await ctx.SaveChangesAsync();
        return thread.UmtId;
    }
    public async Task SendReply(Guid senderId, Guid threadId, string body)
    {
        using var ctx = _db.CreateDbContext();

        var thread = await ctx.UserMessageThreads
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UmtId == threadId);

        if (thread is null)
            throw new InvalidOperationException("Thread not found.");

        var recipientId =
            thread.UmtUserAId == senderId
                ? thread.UmtUserBId
                : thread.UmtUserAId;

        var now = DateTime.UtcNow;

        var msg = new UserMessage
        {
            UsmId = Guid.NewGuid(),
            UsmThreadId = threadId,
            UsmSenderId = senderId,
            UsmRecipientId = recipientId,
            UsmSubject = "", // subject already known from first message
            UsmMessage = body.Trim(),
            UsmSendDate = now,
            UsmStatus = "unread"
        };

        ctx.UserMessages.Add(msg);

        await EnsureThreadState(ctx, senderId, threadId);
        await EnsureThreadState(ctx, recipientId, threadId);

        await ctx.SaveChangesAsync();
    }

    private static async Task EnsureThreadState(ForagerDbContext ctx, Guid userId, Guid threadId)
    {
        var state = await ctx.UserThreadStates.FirstOrDefaultAsync(s => s.UtsUserId == userId && s.UtsThreadId == threadId);
        if (state is null)
        {
            ctx.UserThreadStates.Add(new UserThreadState
            {
                UtsUserId = userId,
                UtsThreadId = threadId,
                UtsUpdatedUtc = DateTime.UtcNow
            });
        }
        else
        {
            state.UtsUpdatedUtc = DateTime.UtcNow;
        }
    }
}
