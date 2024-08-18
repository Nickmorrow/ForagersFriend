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

        public async Task<List<UserFindLocation>> GetUserFindsAndLocationsAsync(Guid userId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.UserFindLocations
                .Include(ufl => ufl.UserFind)
                .Where(ufl => ufl.UserFind.UsfUsrId == userId)
                .ToListAsync();
        }

        public async Task<UserFind> GetFindById(Guid findId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == findId);
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

        public async Task CreateFind(
        Guid userId,
        string name,
        string speciesName,
        string speciesType,
        string useCategory,
        string features,
        string lookalikes,
        string harvestMethod,
        string tastesLike,
        string description,
        double lat,
        double lng)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var userFind = new UserFind
            {
                UsfName = name,
                UsfUsrId = userId,
                UsfSpeciesName = speciesName,
                UsfSpeciesType = speciesType,
                UsfUseCategory = useCategory,
                UsfFeatures = features,
                UsfLookAlikes = lookalikes,
                UsfHarvestMethod = harvestMethod,
                UsfTastesLike = tastesLike,
                UsfDescription = description,
                UsfFindDate = DateTime.Now,
            };

            context.UserFinds.Add(userFind);
            await context.SaveChangesAsync();

            var userFindLocation = new UserFindLocation
            {
                UslLatitude = lat,
                UslLongitude = lng,
                UslUsfId = userFind.UsFId
            };

            context.UserFindLocations.Add(userFindLocation);
            await context.SaveChangesAsync();

        }

        public async Task UpdateFind(
        Guid findId,
        string name,
        string speciesName,
        string speciesType,
        string useCategory,
        string features,
        string lookalikes,
        string harvestMethod,
        string tastesLike,
        string description,
        double lat,
        double lng)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == findId);
            if (userFind == null)
            {
                throw new Exception("User find not found");
            }

            userFind.UsfName = name;
            userFind.UsfSpeciesName = speciesName;
            userFind.UsfSpeciesType = speciesType;
            userFind.UsfUseCategory = useCategory;
            userFind.UsfFeatures = features;
            userFind.UsfLookAlikes = lookalikes;
            userFind.UsfHarvestMethod = harvestMethod;
            userFind.UsfTastesLike = tastesLike;
            userFind.UsfDescription = description;

            context.UserFinds.Update(userFind);

            var userFindLocation = await context.UserFindLocations.FirstOrDefaultAsync(ufl => ufl.UslUsfId == findId);
            if (userFindLocation == null)
            {
                throw new Exception("User find location not found");
            }

            userFindLocation.UslLatitude = lat;
            userFindLocation.UslLongitude = lng;

            context.UserFindLocations.Update(userFindLocation);
            await context.SaveChangesAsync();

            //return (userFind.UsFId, userFindLocation.UslId);
        }

        //public async Task<(Guid userFindId, Guid userFindLocationId)> SaveFind(
        //    string name,
        //    string speciesName,
        //    string speciesType,
        //    string useCategory,
        //    string features,
        //    string lookalikes,
        //    string harvestMethod,
        //    string tastesLike,
        //    string description,
        //    double lat,
        //    double lng,
        //    Guid? editFindId)
        //{
        //    using var context = _dbContextFactory.CreateDbContext();

        //    UserFind userFind;
        //    UserFindLocation userFindLocation;

        //    if (editFindId.HasValue)
        //    {
        //        // Update existing find
        //        userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == editFindId.Value);
        //        if (userFind == null)
        //        {
        //            throw new Exception("User find not found");
        //        }

        //        userFind.UsfName = name;
        //        userFind.UsfSpeciesName = speciesName;
        //        userFind.UsfSpeciesType = speciesType;
        //        userFind.UsfUseCategory = useCategory;
        //        userFind.UsfFeatures = features;
        //        userFind.UsfLookAlikes = lookalikes;
        //        userFind.UsfHarvestMethod = harvestMethod;
        //        userFind.UsfTastesLike = tastesLike;
        //        userFind.UsfDescription = description;

        //        context.UserFinds.Update(userFind);

        //        userFindLocation = await context.UserFindLocations.FirstOrDefaultAsync(ufl => ufl.UslUsfId == userFind.UsFId);
        //        userFindLocation.UslLatitude = lat;
        //        userFindLocation.UslLongitude = lng;

        //        context.UserFindLocations.Update(userFindLocation);
        //    }
        //    else
        //    {
        //        // Add new find
        //        userFind = new UserFind
        //        {
        //            UsfName = name,
        //            UsfSpeciesName = speciesName,
        //            UsfSpeciesType = speciesType,
        //            UsfUseCategory = useCategory,
        //            UsfFeatures = features,
        //            UsfLookAlikes = lookalikes,
        //            UsfHarvestMethod = harvestMethod,
        //            UsfTastesLike = tastesLike,
        //            UsfDescription = description
        //        };

        //        context.UserFinds.Add(userFind);
        //        await context.SaveChangesAsync();

        //        userFindLocation = new UserFindLocation
        //        {
        //            UslLatitude = lat,
        //            UslLongitude = lng,
        //            UslUsfId = userFind.UsFId
        //        };

        //        context.UserFindLocations.Add(userFindLocation);
        //    }

        //    await context.SaveChangesAsync();

        //    return (userFind.UsFId, userFindLocation.UslId);
        //}



    }
}
