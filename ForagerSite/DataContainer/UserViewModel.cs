using DataAccess.Models;
using ForagerSite;
using ForagerSite.Components;
using ForagerSite.DataContainer;

namespace ForagerSite.DataContainer
{
    public class UserViewModel
    {
        public User user { get; set; }
        public UserSecurity userSecurity { get; set; }

        public UserViewModel()
        {
            user = new User();
            userSecurity = new UserSecurity();
        }
    }
}
