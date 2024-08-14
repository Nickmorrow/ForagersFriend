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
        public async Task<bool> UsernameExists(string username)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.UserSecurities.AnyAsync(us => us.UssUsername == username);
            }
        }

        public async Task<bool> EmailExists(string email)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Users.AnyAsync(u => u.UsrEmail == email);
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
        public void AddUser(User user, UserSecurity userSecurity)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Add<User>(user);
                context.Add<UserSecurity>(userSecurity);
                context.SaveChanges();
            }
        }

        public async Task AddUserFind(UserFind userFind, UserFindLocation userFindLocation)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                context.UserFinds.Add(userFind);
                await context.SaveChangesAsync();

                userFindLocation.UslUsfId = userFind.UsFId;

                context.UserFindLocations.Add(userFindLocation);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {              
                Console.WriteLine($"An error occurred while saving user find: {ex.Message}");
                throw; 
            }
        }

    }
}
