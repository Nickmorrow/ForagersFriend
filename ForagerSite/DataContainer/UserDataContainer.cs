using DataAccess.Models;
using ForagerSite;
using ForagerSite.Components;
using ForagerSite.DataContainer;

namespace ForagerSite.DataContainer
{
    public class UserDataContainer
    {
        public User user { get; set; }
        public UserSecurity userSecurity { get; set; }

        public UserDataContainer()
        {
            user = new User();
            userSecurity = new UserSecurity();
        }
    }
}
