
using ForagerSite;
using ForagerSite.Components;
using ForagerSite.DataContainer;


namespace ForagerSite.DataContainer
{
    public class SessionUserState
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }

}
