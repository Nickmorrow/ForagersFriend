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

        //public async Task AddUserFind(UserFind userFind, UserFindLocation userFindLocation)
        //{
        //    try
        //    {
        //        using var context = _dbContextFactory.CreateDbContext();

        //        context.UserFinds.Add(userFind);
        //        await context.SaveChangesAsync();

        //        userFindLocation.UslUsfId = userFind.UsFId;

        //        context.UserFindLocations.Add(userFindLocation);
        //        await context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {              
        //        Console.WriteLine($"An error occurred while saving user find: {ex.Message}");
        //        throw; 
        //    }
        //}

        //public async Task UpdateUserFind(UserFind updatedUserFind, UserFindLocation updatedUserFindLocation)
        //{
        //    try
        //    {
        //        using var context = _dbContextFactory.CreateDbContext();

        //        // Retrieve the existing UserFind entity
        //        var existingUserFind = await context.UserFinds
        //            .FirstOrDefaultAsync(uf => uf.UsFId == updatedUserFind.UsFId);

        //        if (existingUserFind == null)
        //        {
        //            throw new InvalidOperationException("User find not found.");
        //        }

        //        // Update the properties of the existing UserFind entity
        //        existingUserFind.UsfName = updatedUserFind.UsfName;
        //        existingUserFind.UsfSpeciesName = updatedUserFind.UsfSpeciesName;
        //        existingUserFind.UsfSpeciesType = updatedUserFind.UsfSpeciesType;
        //        existingUserFind.UsfUseCategory = updatedUserFind.UsfUseCategory;
        //        existingUserFind.UsfFeatures = updatedUserFind.UsfFeatures;
        //        existingUserFind.UsfLookAlikes = updatedUserFind.UsfLookAlikes;
        //        existingUserFind.UsfHarvestMethod = updatedUserFind.UsfHarvestMethod;
        //        existingUserFind.UsfTastesLike = updatedUserFind.UsfTastesLike;
        //        existingUserFind.UsfDescription = updatedUserFind.UsfDescription;

        //        // Retrieve the existing UserFindLocation entity
        //        var existingUserFindLocation = await context.UserFindLocations
        //            .FirstOrDefaultAsync(ufl => ufl.UslId == updatedUserFindLocation.UslId);

        //        if (existingUserFindLocation == null)
        //        {
        //            throw new InvalidOperationException("User find location not found.");
        //        }

        //        // Update the properties of the existing UserFindLocation entity
        //        existingUserFindLocation.UslLatitude = updatedUserFindLocation.UslLatitude;
        //        existingUserFindLocation.UslLongitude = updatedUserFindLocation.UslLongitude;

        //        // Save the changes
        //        context.UserFinds.Update(existingUserFind);
        //        context.UserFindLocations.Update(existingUserFindLocation);
        //        await context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while updating user find: {ex.Message}");
        //        throw;
        //    }
        //}
        //public async Task<List<UserFind>> GetAllUserFindsAsync()
        //{
        //    using var context = _dbContextFactory.CreateDbContext();
        //    return await context.UserFinds.ToListAsync();
        //}

        //public async Task<List<UserFindLocation>> GetAllUserFindLocationsAsync()
        //{
        //    using var context = _dbContextFactory.CreateDbContext();
        //    return await context.UserFindLocations.ToListAsync();
        //}

        //public async Task<List<UserFindWithLocation>> GetAllUserFindsWithLocationsAsync()
        //{
        //    using var context = _dbContextFactory.CreateDbContext();
        //    return await context.UserFinds
        //        .Join(context.UserFindLocations,
        //              find => find.UsFId,
        //              location => location.UslUsfId,
        //              (find, location) => new UserFindWithLocation
        //              {
        //                  UserFind = find,
        //                  UserFindLocation = location
        //              })
        //        .ToListAsync();
        //}




    }
}
