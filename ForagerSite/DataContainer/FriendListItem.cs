namespace ForagerSite.DataContainer
{
    public class FriendListItem
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string ProfilePicUrl { get; set; } = "UserProfileImages/Shared/PlaceHolder.jpeg";
    }
}
