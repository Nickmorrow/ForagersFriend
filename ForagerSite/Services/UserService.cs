using ForagerSite.Data;
using ForagerSite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;

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
        public async Task<User> GetUserAsync(Guid userId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Users.FirstOrDefaultAsync(u => u.UsrId == userId);               
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Users.ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            using var context = _dbContextFactory.CreateDbContext(); 
            var existingUser = await context.Users.FindAsync(user.UsrId);

            if (existingUser != null)
            {
                existingUser.UsrName = user.UsrName; 
                existingUser.UsrBio = user.UsrBio; 
                existingUser.UsrCountry = user.UsrCountry; 
                existingUser.UsrStateorProvince = user.UsrStateorProvince; 
                existingUser.UsrZipCode = user.UsrZipCode; 
                existingUser.UsrEmail = user.UsrEmail;

                context.Users.Update(existingUser); 
                await context.SaveChangesAsync();
            }
            else 
            { 
                throw new Exception("User not found"); 
            }
        }
        public async Task UploadProfilePicUrl(User user, string fileUrl)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var existingUrls = context.UserImages
                .Where(ui => ui.UsiUsrId == user.UsrId 
                    && ui.UsiUsfId == Guid.Empty 
                    && ui.UsiImageData.Contains("UserProfileImages"))
                .ToList();
            if (existingUrls.Any())
            {
                foreach (var existingUrl in existingUrls)
                {
                    context.UserImages.Remove(existingUrl);
                }
            }
            var imageUrl = new UserImage
            {
                UsiUsrId = user.UsrId,
                UsiImageData = fileUrl
            };
            context.UserImages.Add(imageUrl);
            context.SaveChanges();
        }





    }
}
