using Azure.Core;
using ForagerSite.Data;
using ForagerSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Services
{
    public class UserFindService
    {

        private IDbContextFactory<ForagerDbContext> _dbContextFactory;

        private readonly IConfiguration _config;

        public UserFindService(IDbContextFactory<ForagerDbContext> dbContextFactory, IConfiguration config)
        {
            _dbContextFactory = dbContextFactory;
            _config = config;
        }

        public async Task<List<UserFindsViewModel>> GetUserFindsViewModels(Guid userId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var userWithFinds = await context.Users
                .Include(u => u.UserSecurity)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserFindLocation)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserImages)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserFindsCommentXrefs)
                        .ThenInclude(xref => xref.UserFindsComment)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserFindsCommentXrefs)
                        .ThenInclude(xref => xref.User)
                            .ThenInclude(commentUser => commentUser.UserSecurity)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsrId == userId);

            if (userWithFinds == null)
            {
                return new List<UserFindsViewModel>();
            }

            var userViewModel = new UserFindsViewModel
            {
                user = userWithFinds,
                userSecurity = userWithFinds.UserSecurity,
                userFinds = userWithFinds.UserFinds.ToList(),
                userFindLocations = userWithFinds.UserFinds
                                        .Select(uf => uf.UserFindLocation)
                                        .Where(ufl => ufl != null)
                                        .ToList(),
                userImages = userWithFinds.UserFinds
                                 .SelectMany(uf => uf.UserImages)
                                 .ToList(),
                userFindsCommentXrefs = userWithFinds.UserFinds
                                            .SelectMany(uf => uf.UserFindsCommentXrefs)
                                            .ToList(),
                userFindsComments = userWithFinds.UserFinds
                                        .SelectMany(uf => uf.UserFindsCommentXrefs)
                                        .Select(xref => xref.UserFindsComment)
                                        .Where(comment => comment != null)
                                        .ToList(),
                CommentUsers = userWithFinds.UserFinds
                                   .SelectMany(uf => uf.UserFindsCommentXrefs)
                                   .Select(xref => xref.User)
                                   .Where(commentUser => commentUser != null)
                                   .ToList(),
                CommentUserSecurities = userWithFinds.UserFinds
                                            .SelectMany(uf => uf.UserFindsCommentXrefs)
                                            .Select(xref => xref.User.UserSecurity)
                                            .Where(us => us != null)
                                            .ToList()
            };

            return new List<UserFindsViewModel> { userViewModel };
        }

        public async Task<List<UserFindsViewModel>> GetUserFindsViewModels()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var usersWithFinds = await context.Users
                .Include(u => u.UserSecurity)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserFindLocation) 
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserImages)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserFindsCommentXrefs)
                        .ThenInclude(xref => xref.UserFindsComment)
                .Include(u => u.UserFinds)
                    .ThenInclude(uf => uf.UserFindsCommentXrefs)
                        .ThenInclude(xref => xref.User)
                            .ThenInclude(commentUser => commentUser.UserSecurity)
                .AsNoTracking()
                .ToListAsync();

            var userViewModelsList = usersWithFinds.Select(user => new UserFindsViewModel
            {
                user = user,
                userSecurity = user.UserSecurity,
                userFinds = user.UserFinds.ToList(),
                userFindLocations = user.UserFinds
                                        .Select(uf => uf.UserFindLocation)
                                        .Where(ufl => ufl != null)
                                        .ToList(),
                userImages = user.UserFinds
                                 .SelectMany(uf => uf.UserImages)
                                 .ToList(),
                userFindsCommentXrefs = user.UserFinds
                                            .SelectMany(uf => uf.UserFindsCommentXrefs)
                                            .ToList(),
                userFindsComments = user.UserFinds
                                        .SelectMany(uf => uf.UserFindsCommentXrefs)
                                        .Select(xref => xref.UserFindsComment)
                                        .Where(comment => comment != null)
                                        .ToList(),
                CommentUsers = user.UserFinds
                                   .SelectMany(uf => uf.UserFindsCommentXrefs)
                                   .Select(xref => xref.User)
                                   .Where(commentUser => commentUser != null)
                                   .ToList(),
                CommentUserSecurities = user.UserFinds
                                            .SelectMany(uf => uf.UserFindsCommentXrefs)
                                            .Select(xref => xref.User.UserSecurity)
                                            .Where(us => us != null)
                                            .ToList()
            }).ToList();

            return userViewModelsList;
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

            var userFind = await context.UserFinds
                .FirstOrDefaultAsync(uf => uf.UsFId == findId);
            if (userFind != null)
            {
                userFind.UserImages = await context.UserImages
                    .Where(ui => ui.UsiUsfId == findId)
                    .ToListAsync();
            }
            return userFind;
            //return await context.UserFinds
            //    .Include(uf => uf.UserImages) // Eagerly load UserImages
            //    .FirstOrDefaultAsync(uf => uf.UsFId == findId);

            //return await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == findId);
        }

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
        double lng,
        List<string> uploadedFileUrls)
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

            foreach (var image in uploadedFileUrls)
            {
                var userImage = new UserImage
                {
                    UsiUsrId = userId,
                    UsiUsfId = userFind.UsFId,
                    UsiImageData = image
                };

                context.UserImages.Add(userImage);
                await context.SaveChangesAsync();
            }

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
            double lng,
            List<string>? uploadedFileUrls,
            List<string>? deletedFileUrls)
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

            // Manage image URLs
            var existingImages = await context.UserImages.Where(ui => ui.UsiUsfId == findId).ToListAsync();
            var existingImageUrls = existingImages.Select(ui => ui.UsiImageData).ToList();

            // Delete old image URLs from the database
            if (deletedFileUrls != null)
            {
                foreach (var urlToDelete in deletedFileUrls)
                {
                    var imageToDelete = existingImages.First(ui => ui.UsiImageData == urlToDelete);
                    context.UserImages.Remove(imageToDelete);
                }
            }           
            // Add new image URLs to the database
            if (uploadedFileUrls != null)
            {
                foreach (var urlToAdd in uploadedFileUrls)
                {
                    context.UserImages.Add(new UserImage
                    {
                        UsiUsfId = findId,
                        UsiImageData = urlToAdd
                    });
                }
            }           
            await context.SaveChangesAsync();
        }

        public async Task DeleteFind(Guid findId, string userName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == findId);
            var userFindLocation = await context.UserFindLocations.FirstOrDefaultAsync(ufl => ufl.UslUsfId == userFind.UsFId);
            var images = await context.UserImages.Where(ui => ui.UsiUsfId == findId).ToListAsync();

            foreach (var image in images) 
            {
                var fileName = Path.GetFileName(image.UsiImageData);
                string userDirectory = Path.Combine(_config.GetValue<string>("FileStorage"), userName);
                string filePath = Path.Combine(userDirectory, fileName);
                try
                {                    
                    System.IO.File.Delete(filePath);                    
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error deleting file {fileName}: {ex.Message}", ex);
                }
                context.UserImages.Remove(image);
            }
            if (userFind != null)
            {
                context.UserFindLocations.Remove(userFindLocation);
                context.UserFinds.Remove(userFind);
                await context.SaveChangesAsync();
            }
        }



    }
}
