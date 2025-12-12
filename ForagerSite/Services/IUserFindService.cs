using DataAccess.Models;
using ForagerSite.DataContainer;

namespace ForagerSite.Services
{
    public interface IUserFindService
    {
        Task RecalculateUserExpScore(Guid userId);
        Task<Dictionary<Guid, string>> GetCommentUserNames();

        Task<UserFindsDataContainer> GetUserFindsDC(Guid userId);

        Task<List<UserFindsDataContainer>> GetUserFindsDCs(Guid userId);

        Task<List<UserFindsDataContainer>> GetUserFindsDCs();

        Task<List<UserFindLocation>> GetUserFindLocations(Guid userId);

        Task<UserFind> GetFindById(Guid findId);

        Task<FindsCommentXrefDC> AddComment(
            string comment,
            Guid findId,
            Guid userId,
            Guid? comId = null);

        Task DeleteComment(Guid xrefId);

        Task<UserFindsDataContainer> CreateFind(
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
            string AccessLevel);

        Task<UserFindsDataContainer> UpdateFind(
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
            string accessLevel);

        Task DeleteFind(Guid findId, Guid userId, string userName);

        Task<UserVoteDC> Vote(
            Guid findOrCommentId,
            Guid userId,
            string voteType,
            int voteValue);
    }
}
