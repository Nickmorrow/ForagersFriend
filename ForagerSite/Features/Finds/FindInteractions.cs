using System.Data;
using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Features.Finds;

public sealed class FindInteractions
{
    private readonly IDbContextFactory<ForagerDbContext> _dbContextFactory;
    private readonly IUserSessionService _session;

    public FindInteractions(
        IDbContextFactory<ForagerDbContext> dbContextFactory,
        IUserSessionService session)
    {
        _dbContextFactory = dbContextFactory;
        _session = session;
    }

    public async Task<Guid> AddCommentAsync(
        string text,
        Guid findId,
        Guid? parentCommentId = null,
        CancellationToken cancellationToken = default)
    {
        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var findExists = await FindVisibility.VisibleToViewer(context.UserFinds, viewerId)
            .AnyAsync(find => find.UsfId == findId, cancellationToken);
        if (!findExists)
            throw new KeyNotFoundException("Find was not found or is not visible to the signed-in user.");

        if (parentCommentId.HasValue)
        {
            var parentExists = await context.UserFindsCommentXrefs
                .AnyAsync(xref => xref.UcxUscId == parentCommentId.Value && xref.UcxUsfId == findId, cancellationToken);
            if (!parentExists)
                throw new KeyNotFoundException("Parent comment was not found for this find.");
        }

        var comment = new UserFindsComment
        {
            UscComment = text,
            UscCommentDate = DateTime.Now,
            UscParentCommentId = parentCommentId,
            UscCommentScore = 0
        };
        context.UserFindsComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        context.UserFindsCommentXrefs.Add(new UserFindsCommentXref
        {
            UcxUsrId = viewerId,
            UcxUscId = comment.UscId,
            UcxUsfId = findId
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return findId;
    }

    public async Task<Guid> DeleteCommentAsync(
        Guid commentXrefId,
        CancellationToken cancellationToken = default)
    {
        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var target = await context.UserFindsCommentXrefs
            .Where(xref => xref.UcxId == commentXrefId)
            .Join(FindVisibility.VisibleToViewer(context.UserFinds, viewerId),
                xref => xref.UcxUsfId,
                find => find.UsfId,
                (xref, find) => new { Xref = xref, Find = find })
            .Select(item => new
            {
                item.Xref,
                item.Find.UsfId,
                item.Find.UsfUsrId,
                CommentId = item.Xref.UcxUscId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null)
            throw new KeyNotFoundException("Comment was not found or is not visible to the signed-in user.");
        if (target.Xref.UcxUsrId != viewerId && target.UsfUsrId != viewerId)
            throw new UnauthorizedAccessException("The signed-in user cannot delete this comment.");

        var xrefs = await context.UserFindsCommentXrefs
            .Where(xref => xref.UcxUsfId == target.UsfId)
            .Include(xref => xref.UserFindsComment)
            .ToListAsync(cancellationToken);
        var comments = xrefs
            .Where(xref => xref.UserFindsComment is not null)
            .Select(xref => xref.UserFindsComment)
            .ToList();
        var childrenByParent = comments
            .Where(comment => comment.UscParentCommentId.HasValue)
            .GroupBy(comment => comment.UscParentCommentId!.Value)
            .ToDictionary(group => group.Key, group => group.Select(comment => comment.UscId).ToArray());
        var commentIdsToDelete = new HashSet<Guid> { target.CommentId };
        var pendingIds = new Queue<Guid>();
        pendingIds.Enqueue(target.CommentId);
        while (pendingIds.TryDequeue(out var parentId) && childrenByParent.TryGetValue(parentId, out var children))
        {
            foreach (var childId in children)
            {
                if (commentIdsToDelete.Add(childId))
                    pendingIds.Enqueue(childId);
            }
        }

        var xrefsToDelete = xrefs.Where(xref => commentIdsToDelete.Contains(xref.UcxUscId)).ToList();
        var commentsToDelete = comments.Where(comment => commentIdsToDelete.Contains(comment.UscId)).ToList();
        var votesToDelete = await context.UserVotes
            .Where(vote => vote.UsvUscId.HasValue && commentIdsToDelete.Contains(vote.UsvUscId.Value))
            .ToListAsync(cancellationToken);

        foreach (var comment in commentsToDelete)
            comment.UscParentCommentId = null;

        context.UserVotes.RemoveRange(votesToDelete);
        context.UserFindsCommentXrefs.RemoveRange(xrefsToDelete);
        context.UserFindsComments.RemoveRange(commentsToDelete);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return target.UsfId;
    }

    public async Task<Guid> ToggleVoteAsync(
        Guid targetId,
        string targetType,
        int voteValue,
        CancellationToken cancellationToken = default)
    {
        if (voteValue is not (-1 or 1))
            throw new ArgumentOutOfRangeException(nameof(voteValue), "A vote must be -1 or 1.");

        var viewerId = GetViewerId();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        UserFind? find;
        UserFindsComment? comment = null;
        Guid findId;
        Guid? commentId = null;
        Guid? ownerIdToRecalculate = null;

        if (string.Equals(targetType, "find", StringComparison.OrdinalIgnoreCase))
        {
            find = await FindVisibility.VisibleToViewer(context.UserFinds, viewerId)
                .FirstOrDefaultAsync(item => item.UsfId == targetId, cancellationToken)
                ?? throw new KeyNotFoundException("Find was not found or is not visible to the signed-in user.");
            findId = find.UsfId;
        }
        else if (string.Equals(targetType, "comment", StringComparison.OrdinalIgnoreCase))
        {
            var target = await context.UserFindsCommentXrefs
                .Where(xref => xref.UcxUscId == targetId)
                .Join(FindVisibility.VisibleToViewer(context.UserFinds, viewerId),
                    xref => xref.UcxUsfId,
                    ownerFind => ownerFind.UsfId,
                    (xref, ownerFind) => new { xref.UserFindsComment, ownerFind.UsfId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Comment was not found or is not visible to the signed-in user.");
            find = null;
            comment = target.UserFindsComment;
            findId = target.UsfId;
            commentId = targetId;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(targetType), "Vote target must be 'find' or 'comment'.");
        }

        var existingVote = await context.UserVotes
            .FirstOrDefaultAsync(vote =>
                vote.UsvUsrId == viewerId &&
                (commentId.HasValue
                    ? vote.UsvUscId == commentId
                    : vote.UsvUsfId == targetId),
                cancellationToken);

        if (commentId.HasValue)
        {
            var currentScore = comment!.UscCommentScore ?? 0;
            if (existingVote is null)
            {
                comment.UscCommentScore = currentScore + voteValue;
                context.UserVotes.Add(new UserVote
                {
                    UsvUsrId = viewerId,
                    UsvUscId = commentId,
                    UsvVoteValue = voteValue
                });
            }
            else if (existingVote.UsvVoteValue == voteValue)
            {
                comment.UscCommentScore = currentScore - existingVote.UsvVoteValue;
                context.UserVotes.Remove(existingVote);
            }
            else
            {
                comment.UscCommentScore = currentScore + voteValue - existingVote.UsvVoteValue;
                existingVote.UsvVoteValue = voteValue;
            }
        }
        else
        {
            var currentScore = find!.UsfAccuracyScore ?? 0;
            if (existingVote is null)
            {
                find.UsfAccuracyScore = currentScore + voteValue;
                context.UserVotes.Add(new UserVote
                {
                    UsvUsrId = viewerId,
                    UsvUsfId = find.UsfId,
                    UsvVoteValue = voteValue
                });
            }
            else if (existingVote.UsvVoteValue == voteValue)
            {
                find.UsfAccuracyScore = currentScore - existingVote.UsvVoteValue;
                context.UserVotes.Remove(existingVote);
            }
            else
            {
                find.UsfAccuracyScore = currentScore + voteValue - existingVote.UsvVoteValue;
                existingVote.UsvVoteValue = voteValue;
            }

            ownerIdToRecalculate = find.UsfUsrId;
        }

        await context.SaveChangesAsync(cancellationToken);
        if (ownerIdToRecalculate.HasValue)
        {
            await FindCommands.RecalculateExperienceScoreAsync(
                context,
                ownerIdToRecalculate.Value,
                cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return findId;
    }

    private Guid GetViewerId()
    {
        var sessionUser = _session.SessionUser;
        if (!_session.IsAuthenticated || sessionUser is null || sessionUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("A signed-in user is required for find interactions.");

        return sessionUser.UserId;
    }
}
