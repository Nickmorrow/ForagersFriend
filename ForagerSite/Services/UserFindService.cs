using ForagerSite.Data;
using ForagerSite.Models;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Services
{
    public class UserFindService
    {

        private IDbContextFactory<ForagerDbContext> _dbContextFactory;

        public UserFindService(IDbContextFactory<ForagerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
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

        public async Task UpdateUserFind(UserFind updatedUserFind, UserFindLocation updatedUserFindLocation)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                // Retrieve the existing UserFind entity
                var existingUserFind = await context.UserFinds
                    .FirstOrDefaultAsync(uf => uf.UsFId == updatedUserFind.UsFId);

                if (existingUserFind == null)
                {
                    throw new InvalidOperationException("User find not found.");
                }

                // Update the properties of the existing UserFind entity
                existingUserFind.UsfName = updatedUserFind.UsfName;
                existingUserFind.UsfSpeciesName = updatedUserFind.UsfSpeciesName;
                existingUserFind.UsfSpeciesType = updatedUserFind.UsfSpeciesType;
                existingUserFind.UsfUseCategory = updatedUserFind.UsfUseCategory;
                existingUserFind.UsfFeatures = updatedUserFind.UsfFeatures;
                existingUserFind.UsfLookAlikes = updatedUserFind.UsfLookAlikes;
                existingUserFind.UsfHarvestMethod = updatedUserFind.UsfHarvestMethod;
                existingUserFind.UsfTastesLike = updatedUserFind.UsfTastesLike;
                existingUserFind.UsfDescription = updatedUserFind.UsfDescription;

                // Retrieve the existing UserFindLocation entity
                var existingUserFindLocation = await context.UserFindLocations
                    .FirstOrDefaultAsync(ufl => ufl.UslId == updatedUserFindLocation.UslId);

                if (existingUserFindLocation == null)
                {
                    throw new InvalidOperationException("User find location not found.");
                }

                // Update the properties of the existing UserFindLocation entity
                existingUserFindLocation.UslLatitude = updatedUserFindLocation.UslLatitude;
                existingUserFindLocation.UslLongitude = updatedUserFindLocation.UslLongitude;

                // Save the changes
                context.UserFinds.Update(existingUserFind);
                context.UserFindLocations.Update(existingUserFindLocation);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user find: {ex.Message}");
                throw;
            }
        }
       

    }
}
