using DataAccess.Models;

namespace ForagerSite.Services
{
    public interface IMapService
    {
        string mapFilter { get; set; }
        double? pendingLat { get; set; }
        double? pendingLng { get; set; }

        List<UserFindsViewModel> MyViewModels { get; set; }
        List<UserFindsViewModel> AllViewModels { get; set; }
        List<UserFindsViewModel> CurrentViewModels { get; set; }

        Dictionary<Guid, string> userNamesKvp { get; set; }

        event Action OnChange;
        event Action<bool> OnLoadingChange;
        event Action? OnCreateFormRequested;
        event Action<Guid>? OnMarkerSelected;

        void UpdateViewModels(Guid userId, UserFindsViewModel viewModel);
        UserFindsViewModel? GetViewModel(Guid userId);

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

        Task Vote(Guid vmId, Guid findOrCommentId, int voteValue, string voteType);
    }
}
