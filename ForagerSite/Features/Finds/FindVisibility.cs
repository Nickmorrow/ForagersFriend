using DataAccess.Models;

namespace ForagerSite.Features.Finds;

internal static class FindVisibility
{
    private const string PrivateAccessibility = "Private";

    public static IQueryable<UserFind> VisibleToViewer(IQueryable<UserFind> finds, Guid viewerId) =>
        finds.Where(find =>
            find.UsfUsrId == viewerId ||
            find.UsfAccessibility != PrivateAccessibility);
}
