using DataAccess.Models;
using ForagerSite;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;

namespace ForagerSite.Services.Interfaces
{
    public interface IMapService
    {
        string mapFilter { get; set; }
        double? pendingLat { get; set; }
        double? pendingLng { get; set; }

        List<UserFindsDataContainer> MyViewModels { get; set; }
        List<UserFindsDataContainer> AllViewModels { get; set; }
        List<UserFindsDataContainer> CurrentViewModels { get; set; }

        Dictionary<Guid, string> userNamesKvp { get; set; }

        event Action OnChange;
        event Action<bool> OnLoadingChange;
        event Action? OnCreateFormRequested;
        event Action<Guid>? OnMarkerSelected;

        void UpdateViewModels(Guid userId, UserFindsDataContainer viewModel);
        UserFindsDataContainer? GetViewModel(Guid userId);

        Task OnMarkerClicked(string findId);
        Task TriggerCreateForm(double lat, double lng);

        Task CreateFindVm(
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
            string accessLevel);

        Task UpdateFindVm(
            Guid upFindId,
            string name,
            string speciesName,
            string speciesType,
            string useCategory,
            string features,
            string lookalikes,
            string harvestMethod,
            string tastesLike,
            string description,
            List<string>? uploadedFileUrls,
            List<string>? deletedFileUrls,
            string accessLevel);

        Task DeleteFindVm(Guid delFindId);

        Task VoteVm(Guid vmId, Guid findOrCommentId, int voteValue, string voteType);
    }
}
