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

        public async Task<List<UserFindsViewModel>> GetAllUserFindsViewModels()
        {           
            using var context = _dbContextFactory.CreateDbContext();

            var userViewModelsList = new List<UserFindsViewModel>();
            var userFindsViewModel = new UserFindsViewModel();
            
            foreach (var user in context.Users)
            {
                userFindsViewModel.user = user;
                userFindsViewModel.userSecurity = context.UserSecurities.FirstOrDefault(us => us.UssUsrId == userFindsViewModel.user.UsrId);

                userFindsViewModel.userFinds = context.UserFinds.Where(uf => uf.UsfUsrId == userFindsViewModel.user.UsrId).ToList();
                foreach (UserFind find in userFindsViewModel.userFinds)
                {
                    UserFindLocation userFindLocation = new UserFindLocation();
                    userFindLocation = context.UserFindLocations.FirstOrDefault(uf => uf.UslUsfId == find.UsFId);
;                   userFindsViewModel.userFindLocations.Add(userFindLocation); 
                        
                    userFindsViewModel.userImages = context.UserImages.Where(usi => usi.UsiUsfId == find.UsFId).ToList();
                    userFindsViewModel.userFindsCommentXrefs = context.UserFindsCommentXrefs.Where(usx => usx.UcxUsfId == find.UsFId).ToList();

                    foreach (UserFindsCommentXref usx in userFindsViewModel.userFindsCommentXrefs)
                    {
                        userFindsViewModel.userFindsComments = context.UserFindsComments.Where(usc => usc.UscId == usx.UcxUscId).ToList();
                        userFindsViewModel.CommentUsers = context.Users.Where(cu => cu.UsrId == usx.UcxUsrId).ToList();

                        foreach (User usr in userFindsViewModel.CommentUsers)
                        {
                            UserSecurity commentUserSecurity = new UserSecurity();
                            commentUserSecurity = context.UserSecurities.FirstOrDefault(cus => cus.UssUsrId == usr.UsrId);
                            userFindsViewModel.CommentUserSecurities.Add(commentUserSecurity);
                        }
                               
                    }
                }               

                userViewModelsList.Add(userFindsViewModel);
            }

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
            return await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == findId);
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

        public async Task DeleteFind(Guid findId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsFId == findId);
            var userFindLocation = await context.UserFindLocations.FirstOrDefaultAsync(ufl => ufl.UslUsfId == userFind.UsFId);
            
            if (userFind != null)
            {
                context.UserFindLocations.Remove(userFindLocation);
                context.UserFinds.Remove(userFind);
                await context.SaveChangesAsync();
            }
        }



    }
}
