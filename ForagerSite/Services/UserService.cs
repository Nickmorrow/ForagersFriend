using ForagerSite.Data;
using ForagerSite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ForagerSite.Services
{
    public class UserService
    {
        private IDbContextFactory<ForagerDbContext> _dbContextFactory;

        public UserService(IDbContextFactory<ForagerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void AddUser(User user, UserSecurity userSecurity)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Add<User>(user);
                context.Add<UserSecurity>(userSecurity);
                context.SaveChanges();
            }
        }
        public UserViewModel AuthenticateUser(string username, string password)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var userSecurity = context.UserSecurities
                    .Include(us => us.User)
                    .FirstOrDefault(us => us.UssUsername == username && us.UssPassword == password);

                if (userSecurity != null)
                {
                    return new UserViewModel
                    {
                        user = userSecurity.User,
                        userSecurity = userSecurity
                    };
                }

                return null;
            }
        }
        public void UpdateUserSecurity(UserSecurity userSecurity)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.UserSecurities.Update(userSecurity);
                context.SaveChanges();
            }
        }


    }
}
