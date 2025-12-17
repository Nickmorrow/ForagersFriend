using ForagerSite.DataContainer;
using static ForagerSite.CommonRules.SearchRules;

namespace ForagerSite.CommonRules
{

    public static class SearchRules
    {
        public enum MapFilters
        {
            UserOnly,
            AllUsers,
            FriendUsers
        }

        public enum VisibilityFilters
        {
            Public,
            Private,
            Friends
        }
        public enum SortMode
        {
            Distance,
            DateDiscovered,
            Popularity
        }
    }
    
}
