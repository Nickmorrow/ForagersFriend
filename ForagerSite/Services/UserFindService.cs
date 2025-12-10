using Azure.Core;
using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

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

        public async Task<Dictionary<Guid, string>> GetCommentUserNames()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.UserSecurity)
                .AsNoTracking()
                .ToDictionaryAsync(u => u.UsrId, u => u.UserSecurity.UssUsername);
        }
        public async Task<UserFindsViewModel> GetUserFindsViewModel(Guid userId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var user = await context.Users
                .Include(u => u.UserSecurity)
                //.Include(u => u.UserImage)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsrId == userId);

            if (user == null)
            {
                return new UserFindsViewModel();
            }

            var userFinds = await context.UserFinds
                .Where(uf => uf.UsfUsrId == userId)
                .Include(uf => uf.UserFindLocation)
                .Include(uf => uf.UserImages)
                .Include(uf => uf.UserVotes)
                .Include(uf => uf.UserFindsCommentXrefs)
                    .ThenInclude(xref => xref.UserFindsComment)
                        .ThenInclude(comment => comment.UserVotes)
                .Include(uf => uf.UserFindsCommentXrefs)
                    .ThenInclude(xref => xref.User)
                        .ThenInclude(commentUser => commentUser.UserSecurity)
                
                .AsNoTracking()
                .ToListAsync();

            var userImage = context.UserImages
                    .Where(ui => ui.UsiUsrId == user.UsrId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                    .FirstOrDefault();

            var userViewModel = new UserFindsViewModel
            {
                userId = user.UsrId,
                profilePic = userImage?.UsiImageData ?? UserFindsViewModel.PlaceholderImageUrl,
                userName = user.UserSecurity.UssUsername,
                finds = userFinds.Select(uf => new FindDto(uf)).ToList(),
            };

            foreach (var find in userViewModel.finds)
            {
                find.findLocation = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .Select(uf => uf.UserFindLocation)
                    .Where(ufl => ufl != null)
                    .Select(ufl => new FindLocationDto(ufl))
                    .FirstOrDefault();
                find.findImages = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .SelectMany(uf => uf.UserImages)
                    .Select(ui => new ImageDto(ui))
                    .ToList();
                find.findsCommentXrefs = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .SelectMany(uf => uf.UserFindsCommentXrefs)
                    .Select(xref => new FindsCommentXrefDto(xref))
                    .ToList();
                find.findVotes = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .SelectMany(uf => uf.UserVotes)
                    .Select(uv => new UserVoteDto(uv))
                    .ToList();

                foreach (var xref in find.findsCommentXrefs)
                {
                    var commentProficPic = context.UserImages
                        .Where(ui => ui.UsiUsrId == xref.comxUserId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                        .FirstOrDefault();
                    xref.CommentUserProfilePic = commentProficPic?.UsiImageData ?? UserFindsViewModel.PlaceholderImageUrl;
                }
            }
            return userViewModel;
        }
        public async Task<List<UserFindsViewModel>> GetUserFindsViewModels(Guid userId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var user = await context.Users
                .Include(u => u.UserSecurity)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsrId == userId);

            if (user == null)
            {
                return new List<UserFindsViewModel>();
            }

            var userFinds = await context.UserFinds
                .Where(uf => uf.UsfUsrId == userId)
                .Include(uf => uf.UserFindLocation)
                .Include(uf => uf.UserImages)
                .Include(uf => uf.UserVotes)
                .Include(uf => uf.UserFindsCommentXrefs)
                    .ThenInclude(xref => xref.UserFindsComment)
                        .ThenInclude(comment => comment.UserVotes)
                .Include(uf => uf.UserFindsCommentXrefs)
                    .ThenInclude(xref => xref.User)
                        .ThenInclude(commentUser => commentUser.UserSecurity)
                .AsNoTracking()
                .ToListAsync();

            var userImage = context.UserImages
                    .Where(ui => ui.UsiUsrId == user.UsrId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                    .FirstOrDefault();

            var userViewModel = new UserFindsViewModel
            {
                userId = user.UsrId,
                profilePic = userImage?.UsiImageData ?? UserFindsViewModel.PlaceholderImageUrl,
                userName = user.UserSecurity.UssUsername,
                finds = userFinds.Select(uf => new FindDto(uf)).ToList(),
            };

            foreach (var find in userViewModel.finds)
            {
                find.findLocation = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .Select(uf => uf.UserFindLocation)
                    .Where(ufl => ufl != null)
                    .Select(ufl => new FindLocationDto(ufl))
                    .FirstOrDefault();
                find.findImages = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .SelectMany(uf => uf.UserImages)
                    .Select(ui => new ImageDto(ui))
                    .ToList();
                find.findsCommentXrefs = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .SelectMany(uf => uf.UserFindsCommentXrefs)
                    .Select(xref => new FindsCommentXrefDto(xref))
                    .ToList();
                find.findVotes = userFinds
                    .Where(uf => uf.UsfId == find.findId)
                    .SelectMany(uf => uf.UserVotes)
                    .Select(uv => new UserVoteDto(uv))
                    .ToList();

                foreach (var xref in find.findsCommentXrefs)
                {
                    var commentProficPic = context.UserImages
                        .Where(ui => ui.UsiUsrId == xref.comxUserId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                        .FirstOrDefault();
                    xref.CommentUserProfilePic = commentProficPic?.UsiImageData ?? UserFindsViewModel.PlaceholderImageUrl;
                }
            }


            return new List<UserFindsViewModel> { userViewModel };
        }
        public async Task<List<UserFindsViewModel>> GetUserFindsViewModels()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var users = await context.Users
                .Include(u => u.UserSecurity)
                .AsNoTracking()
                .ToListAsync();

            var userIds = users.Select(u => u.UsrId).ToList();

            var userFinds = await context.UserFinds
                .Where(uf => userIds.Contains(uf.UsfUsrId))
                .Include(uf => uf.UserFindLocation)
                .Include(uf => uf.UserImages)
                .Include(uf => uf.UserVotes)
                .Include(uf => uf.UserFindsCommentXrefs)
                    .ThenInclude(xref => xref.UserFindsComment)
                        .ThenInclude(comment => comment.UserVotes)
                .Include(uf => uf.UserFindsCommentXrefs)
                    .ThenInclude(xref => xref.User)
                        .ThenInclude(commentUser => commentUser.UserSecurity)
                .AsNoTracking()
                .ToListAsync();

            var userViewModelsList = new List<UserFindsViewModel>();    

            foreach (var user in users)
            {
                var userFindsForUser = userFinds.Where(uf => uf.UsfUsrId == user.UsrId).ToList();

                var userImage = context.UserImages
                    .Where(ui => ui.UsiUsrId == user.UsrId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                    .FirstOrDefault();

                var userViewModel = new UserFindsViewModel
                {
                    userId = user.UsrId,
                    profilePic = userImage?.UsiImageData ?? UserFindsViewModel.PlaceholderImageUrl,
                    userName = user.UserSecurity.UssUsername,
                    finds = userFindsForUser.Select(uf => new FindDto(uf)).ToList(),
                };

                foreach (var find in userViewModel.finds)
                {
                    find.findLocation = userFinds
                        .Where(uf => uf.UsfId == find.findId)
                        .Select(uf => uf.UserFindLocation)
                        .Where(ufl => ufl != null)
                        .Select(ufl => new FindLocationDto(ufl))
                        .FirstOrDefault();
                    find.findImages = userFinds
                        .Where(uf => uf.UsfId == find.findId)
                        .SelectMany(uf => uf.UserImages)
                        .Select(ui => new ImageDto(ui))
                        .ToList();
                    find.findsCommentXrefs = userFinds
                        .Where(uf => uf.UsfId == find.findId)
                        .SelectMany(uf => uf.UserFindsCommentXrefs)
                        .Select(xref => new FindsCommentXrefDto(xref))
                        .ToList();
                    find.findVotes = userFinds
                        .Where(uf => uf.UsfId == find.findId)
                        .SelectMany(uf => uf.UserVotes)
                        .Select(uv => new UserVoteDto(uv))
                        .ToList();

                    foreach (var xref in find.findsCommentXrefs)
                    {
                        var commentProficPic = context.UserImages
                            .Where(ui => ui.UsiUsrId == xref.comxUserId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                            .FirstOrDefault();
                        xref.CommentUserProfilePic = commentProficPic?.UsiImageData ?? UserFindsViewModel.PlaceholderImageUrl;
                    }
                }
                userViewModelsList.Add(userViewModel);
            }
                                                     
            return userViewModelsList;
        }
        public async Task<List<UserFindLocation>> GetUserFindLocations(Guid userId)
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
                .FirstOrDefaultAsync(uf => uf.UsfId == findId);
            if (userFind != null)
            {
                userFind.UserImages = await context.UserImages
                    .Where(ui => ui.UsiUsfId == findId)
                    .ToListAsync();
                userFind.UserFindLocation = await context.UserFindLocations
                    .Where(l => l.UslUsfId == findId).FirstOrDefaultAsync();
            }
            return userFind;
            //return await context.UserFinds
            //    .Include(uf => uf.UserImages) // Eagerly load UserImages
            //    .FirstOrDefaultAsync(uf => uf.findId == findId);

            //return await context.UserFinds.FirstOrDefaultAsync(uf => uf.findId == findId);
        }

        public async Task<FindsCommentXrefDto> AddComment(string comment, Guid findId, Guid userId, Guid? comId = null)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var userComment = new UserFindsComment
            {
                UscComment = comment,
                UscCommentDate = DateTime.Now,
                UscParentCommentId = comId.HasValue ? comId.Value : null
            };
            context.UserFindsComments.Add(userComment);
            await context.SaveChangesAsync();

            var userCommentXref = new UserFindsCommentXref
            {
                UcxUsrId = userId,
                UcxUscId = userComment.UscId,
                UcxUsfId = findId
            };
            context.UserFindsCommentXrefs.Add(userCommentXref);
            await context.SaveChangesAsync();

            var commentDto = new FindCommentDto(userComment);
            var xrefDto = new FindsCommentXrefDto
            {
                comXId = userCommentXref.UcxId,
                comxUserId = userCommentXref.UcxUsrId,
                comxFindId = userCommentXref.UcxUsfId,
                comxComId = userCommentXref.UcxUscId,
                findsComment = commentDto,
                CommentUserProfilePic = context.UserImages?
                    .Where(ui => ui.UsiUsrId == userCommentXref.UcxUsrId && ui.UsiUsfId == null && ui.UsiImageData.StartsWith("/UserProfileImages"))
                    .Select(ui => ui.UsiImageData)
                    .FirstOrDefault() ?? UserFindsViewModel.PlaceholderImageUrl
            };
            return xrefDto;
        }
        public async Task DeleteComment(Guid xrefId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var xref = await context.UserFindsCommentXrefs
                .Include(x => x.UserFindsComment)
                .FirstOrDefaultAsync(x => x.UcxId == xrefId);

            if (xref == null)
                return;

            var commentId = xref.UcxUscId;

            // 1) Find direct child comments (replies)
            var childComments = await context.UserFindsComments
                .Where(c => c.UscParentCommentId == commentId)
                .ToListAsync();

            if (childComments.Any())
            {
                var childIds = childComments.Select(c => c.UscId).ToList();

                // 1a) Delete their xrefs
                var childXrefs = await context.UserFindsCommentXrefs
                    .Where(xx => childIds.Contains(xx.UcxUscId))
                    .ToListAsync();

                context.UserFindsCommentXrefs.RemoveRange(childXrefs);

                // 1b) Delete the child comments themselves
                context.UserFindsComments.RemoveRange(childComments);
            }

            // 2) Delete the xref + the parent comment
            context.UserFindsCommentXrefs.Remove(xref);
            context.UserFindsComments.Remove(xref.UserFindsComment);

            await context.SaveChangesAsync();
        }

        public async Task<UserFindsViewModel> CreateFind(
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
        List<string> uploadedFileUrls,
        Guid userId,
        string userName,
        string AccessLevel)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var mapViewModel = new UserFindsViewModel();

            mapViewModel.userId = userId;
            mapViewModel.userName = userName;

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
                UsfAccessibility = AccessLevel
            };
            mapViewModel.finds.Add(new FindDto(userFind));

            context.UserFinds.Add(userFind);
            await context.SaveChangesAsync();

            var userFindLocation = new UserFindLocation
            {
                UslLatitude = lat,
                UslLongitude = lng,
                UslUsfId = userFind.UsfId
            };
            mapViewModel.finds[0].findLocation = new FindLocationDto(userFindLocation);

            context.UserFindLocations.Add(userFindLocation);
            await context.SaveChangesAsync();

            foreach (var image in uploadedFileUrls)
            {
                var userImage = new UserImage
                {
                    UsiUsrId = userId,
                    UsiUsfId = userFind.UsfId,
                    UsiImageData = image
                };
                mapViewModel.finds[0].findImages.Add(new ImageDto(userImage));

                context.UserImages.Add(userImage);
                await context.SaveChangesAsync();
            }
            mapViewModel.finds[0].findId = userFind.UsfId;
            return mapViewModel;
        }
        public async Task<UserFindsViewModel> UpdateFind(
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
            List<string>? deletedFileUrls,
            Guid userId,
            string userName,
            string accessLevel)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var mapViewModel = new UserFindsViewModel();

            mapViewModel.userId = userId;
            mapViewModel.userName = userName;

            var userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsfId == findId);
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
            userFind.UsfAccessibility = accessLevel;

            context.UserFinds.Update(userFind);
            mapViewModel.finds.Add(new FindDto(userFind));

            var userFindLocation = await context.UserFindLocations.FirstOrDefaultAsync(ufl => ufl.UslUsfId == findId);
            if (userFindLocation == null)
            {
                throw new Exception("User find location not found");
            }

            userFindLocation.UslLatitude = lat;
            userFindLocation.UslLongitude = lng;

            context.UserFindLocations.Update(userFindLocation);
            mapViewModel.finds[0].findLocation = new FindLocationDto(userFindLocation);

            // Manage image URLs
            var existingImages = await context.UserImages.Where(ui => ui.UsiUsfId == findId).ToListAsync();
            var existingImageUrls = existingImages.Select(ui => ui.UsiImageData).ToList();
            var existingImageDtos = existingImages.Select(ui => new ImageDto(ui)).ToList();
            mapViewModel.finds[0].findImages.AddRange(existingImageDtos);

            // Delete old image URLs from the database
            if (deletedFileUrls != null)
            {
                foreach (var urlToDelete in deletedFileUrls)
                {
                    var imageToDelete = existingImages.First(ui => ui.UsiImageData == urlToDelete);
                    context.UserImages.Remove(imageToDelete);
                    var imageDtoToDelete = existingImageDtos.First(ui => ui.imageData == urlToDelete);
                    mapViewModel.finds[0].findImages.Remove(imageDtoToDelete);
                }
            }           
            // Add new image URLs to the database
            if (uploadedFileUrls != null)
            {
                foreach (var urlToAdd in uploadedFileUrls)
                {
                    var addedImage = new UserImage
                    {
                        UsiUsfId = findId,
                        UsiImageData = urlToAdd
                    };
                    context.UserImages.Add(addedImage);                   
                    mapViewModel.finds[0].findImages.Add(new ImageDto(addedImage));                   
                }
            }           
            await context.SaveChangesAsync();
            mapViewModel.finds[0].findId = findId;
            return mapViewModel;
        }
        public async Task DeleteFind(Guid findId, Guid userId, string userName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsfId == findId);
            var userFindLocation = await context.UserFindLocations.FirstOrDefaultAsync(ufl => ufl.UslUsfId == userFind.UsfId);
            var images = await context.UserImages.Where(ui => ui.UsiUsfId == findId).ToListAsync();
            var userFindCommentXrefs = await context.UserFindsCommentXrefs.Where(xref => xref.UcxUsfId == findId).ToListAsync();
            var userFindComments = await context.UserFindsCommentXrefs.Where(xref => xref.UcxUsfId == findId).ToListAsync();

            foreach (var image in images)
            {
                var fileName = Path.GetFileName(image.UsiImageData);

                try
                {
                    string userDirectory = Path.Combine(_config.GetValue<string>("FileStorage"), userName);
                    string filePath = Path.Combine(userDirectory, fileName);

                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error deleting file {fileName}: {ex.Message}", ex);
                }
                finally
                {
                    context.UserImages.Remove(image);
                }
            }
            foreach (var xref in userFindCommentXrefs)
            {
                context.UserFindsCommentXrefs.Remove(xref);
                context.UserFindsComments.Remove(xref.UserFindsComment);
            }
            if (userFind != null)
            {
                context.UserFindLocations.Remove(userFindLocation);
                context.UserFinds.Remove(userFind);               
            }
            await context.SaveChangesAsync();
        } 

        public async Task<UserVoteDto> Vote(Guid findOrCommentId, Guid userId, string voteType, int voteValue)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var userVote = new UserVote
            {
                UsvUsrId = userId,
                UsvVoteValue = voteValue,
            };
            var existingVote = null as UserVote;

            if (voteType == "find")
            {
                userVote.UsvUsfId = findOrCommentId;

                var userFind = await context.UserFinds.FirstOrDefaultAsync(uf => uf.UsfId == findOrCommentId);
                existingVote = await context.UserVotes.FirstOrDefaultAsync(uv => uv.UsvUsrId == userId && uv.UsvUsfId == findOrCommentId);

                if (existingVote != null)
                {
                    userFind.UsfAccuracyScore -= existingVote.UsvVoteValue;
                    context.UserVotes.Remove(existingVote);

                    if (existingVote.UsvVoteValue != voteValue)
                    {
                        userFind.UsfAccuracyScore += voteValue;
                    }
                }
                else
                {
                    userFind.UsfAccuracyScore = (userFind.UsfAccuracyScore ?? 0) + voteValue;
                }
                context.UserFinds.Update(userFind);
            }
            else if (voteType == "comment")
            {
                userVote.UsvUscId = findOrCommentId;
                existingVote = await context.UserVotes.FirstOrDefaultAsync(uv => uv.UsvUsrId == userId && uv.UsvUscId == findOrCommentId);

                if (existingVote != null)
                {
                    context.UserVotes.Remove(existingVote);
                }
            }

            if (existingVote != null && existingVote.UsvVoteValue == voteValue)
            {
                await context.SaveChangesAsync();
                return new UserVoteDto();
            }

            context.UserVotes.Add(userVote);
            var userVoteDto = new UserVoteDto(userVote);

            await context.SaveChangesAsync();
            return userVoteDto;
        }
    }
}
