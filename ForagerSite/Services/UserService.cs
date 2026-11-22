using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.DataContainer;
using ForagerSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace ForagerSite.Services
{
    public class UserService : IUserService
    {
        private IDbContextFactory<ForagerDbContext> _dbContextFactory;
        private readonly ILookupNormalizer _lookupNormalizer;

        public UserService(
            IDbContextFactory<ForagerDbContext> dbContextFactory,
            ILookupNormalizer lookupNormalizer)
        {
            _dbContextFactory = dbContextFactory;
            _lookupNormalizer = lookupNormalizer;
        }  
        public async Task<UserProfileDto> GetUserViewModelById(Guid userId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            return await context.Profiles
                .AsNoTracking()
                .Where(profile => profile.UsrId == userId)
                .Select(profile => new UserProfileDto
                {
                    UsrId = profile.UsrId,
                    Username = profile.Account.UserName ?? string.Empty,
                    UsrName = profile.UsrName,
                    UsrBio = profile.UsrBio,
                    UsrFindsNum = profile.UsrFindsNum,
                    UsrExpScore = profile.UsrExpScore,
                    UsrJoinedDate = profile.UsrJoinedDate,
                    UsrCountry = profile.UsrCountry,
                    UsrStateorProvince = profile.UsrStateorProvince,
                    UsrZipCode = profile.UsrZipCode
                })
                .FirstOrDefaultAsync() ?? new UserProfileDto();
        }

        public async Task<bool> UsernameExists(string username)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var normalizedUsername = _lookupNormalizer.NormalizeName(username.Trim());
            return await context.Users.AnyAsync(account => account.NormalizedUserName == normalizedUsername);
        }      
        public async Task<bool> EmailExists(string email)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var normalizedEmail = _lookupNormalizer.NormalizeEmail(email.Trim());
            return await context.Users.AnyAsync(account => account.NormalizedEmail == normalizedEmail);
        }           
        public void UpdateLastLogin(Guid userId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var userSecurity = context.UserSecurities.FirstOrDefault(us => us.UssUsrId == userId);
                if (userSecurity is null)
                    return;

                userSecurity.UssLastLoginDate = DateTime.UtcNow;
                context.SaveChanges();
            }
        }
        public void AddUser(RegistrationInputModel input)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = new User
                {
                    UsrId = Guid.NewGuid(),
                    UsrName = input.Name.Trim(),
                    UsrEmail = input.Email.Trim(),
                    UsrCountry = input.Country,
                    UsrStateorProvince = input.StateOrProvince,
                    UsrJoinedDate = DateTime.UtcNow
                };
                var userSecurity = new UserSecurity
                {
                    UssId = Guid.NewGuid(),
                    UssUsrId = user.UsrId,
                    UssUsername = input.Username.Trim(),
                    UssPassword = input.Password,
                    User = user
                };

                context.Profiles.Add(user);
                context.UserSecurities.Add(userSecurity);
                context.SaveChanges();
            }
        }
        public async Task<List<string>> GetAllUserNames()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Users
                .AsNoTracking()
                .Where(account => account.UserName != null)
                .Select(account => account.UserName!)
                .ToListAsync();
        }

        public async Task UpdateUser(UserProfileDto user)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingUser = await context.Profiles.FindAsync(user.UsrId);

            if (existingUser != null)
            {
                existingUser.UsrName = user.UsrName; 
                existingUser.UsrBio = user.UsrBio; 
                existingUser.UsrCountry = user.UsrCountry; 
                existingUser.UsrStateorProvince = user.UsrStateorProvince; 
                existingUser.UsrZipCode = user.UsrZipCode; 

                context.Profiles.Update(existingUser); 
                await context.SaveChangesAsync();
            }
            else 
            { 
                throw new Exception("User not found"); 
            }
        }
        public async Task<string> GetUserProfilePic(Guid userId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var image = await context.UserImages
                .AsNoTracking()
                .Where(ui => ui.UsiUsrId == userId && ui.UsiImageData.Contains("UserProfileImages") && ui.UsiUsfId == null)
                .FirstOrDefaultAsync();

            return image?.UsiImageData ?? string.Empty;
        }
    }
}
