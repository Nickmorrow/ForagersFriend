using DataAccess.Models;
using ForagerSite.Utilities;
using ForagerSite.Services;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;


namespace ForagerSite.Services
{
    public class MapService : IMapService
    {       
        public static readonly List<string> MapFilters = new List<string>
        {
            "UserOnly", "AllUsers", "FriendUsers"
        };
        
        private readonly UserStateService _userStateService;
        private readonly IUserFindService _userFindService;

        public event Action OnChange;
        public event Action<bool> OnLoadingChange;
        public event Action? OnCreateFormRequested;
        public event Action<Guid>? OnMarkerSelected;
        public string mapFilter { get; set; } = MapFilters[0];
        public double? pendingLat { get; set; }
        public double? pendingLng { get; set; }
        public List<UserFindsViewModel> MyViewModels { get; set; } = new();
        public List<UserFindsViewModel> AllViewModels { get; set; } = new();
        public List<UserFindsViewModel> CurrentViewModels { get; set; } = new();
        public Dictionary<Guid, string> userNamesKvp { get; set; }

        public MapService(UserStateService userStateService, IUserFindService userFindService)
        {
            _userStateService = userStateService;
            _userFindService = userFindService;
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
        private void NotifyLoadingChanged(bool isLoading) => OnLoadingChange?.Invoke(isLoading);        
        public void UpdateViewModels(Guid userId, UserFindsViewModel viewModel)
        {
            var currentUserId = _userStateService.CurrentUser.user.UsrId;

            if (currentUserId == viewModel.userId)
            {               
                MyViewModels[0] = VmUtilities.Copy(viewModel);               
            }

            if (AllViewModels.Count > 0)
            {
                var backupVmlAllIndex = AllViewModels.FindIndex(vm => vm.userId == userId);
                if (backupVmlAllIndex != -1)
                {
                    AllViewModels[backupVmlAllIndex] = VmUtilities.Copy(viewModel);
                }
            }
        }
        public UserFindsViewModel GetViewModel(Guid userId)
        {
            return CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
        }

        [JSInvokable("OnMarkerClicked")]
        public Task OnMarkerClicked(string findId)
        {
            if (Guid.TryParse(findId, out var guid))
            {
                OnMarkerSelected?.Invoke(guid);
            }
            return Task.CompletedTask;
        }

        [JSInvokable("TriggerCreateForm")]
        public Task TriggerCreateForm(double lat, double lng)
        {
            pendingLat = lat;
            pendingLng = lng;

            OnCreateFormRequested?.Invoke();
            return Task.CompletedTask;
        }        

        public async Task CreateFindVm(
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
        string accessLevel)       
        {
            NotifyLoadingChanged(true);

            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;

            var newUserFindViewModel =
            await _userFindService.CreateFind(
                name,
                speciesName,
                speciesType,
                useCategory,
                features,
                lookalikes,
                harvestMethod,
                tastesLike,
                description,
                lat,
                lng,
                uploadedFileUrls,
                userId,
                userName,
                accessLevel
            );

            var find = newUserFindViewModel.finds[0];
            var currentViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
            currentViewModel.finds.Add(find);

            UpdateViewModels(userId, currentViewModel);

            NotifyStateChanged();
            NotifyLoadingChanged(false);

        }
        public async Task UpdateFindVm(
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
        string accessLevel)
        {            
            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;

            NotifyLoadingChanged(true);

            FindLocationDto location = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId).finds.FirstOrDefault(f => f.findId == upFindId).findLocation;

            var newUserFindViewModel =
            await _userFindService.UpdateFind(
                upFindId,
                name,
                speciesName,
                speciesType,
                useCategory,
                features,
                lookalikes,
                harvestMethod,
                tastesLike,
                description,
                location.latitude,
                location.longitude,
                uploadedFileUrls,
                deletedFileUrls,
                userId,
                userName,
                accessLevel
            );

            var find = newUserFindViewModel.finds[0];
            var currentViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);

            var originalIndex = currentViewModel.finds.FindIndex(f => f.findId == upFindId);

            int index = currentViewModel.finds.FindIndex(f => f.findId == find.findId);               
            currentViewModel.finds[index] = find; 
                
            UpdateViewModels(userId, currentViewModel);

            NotifyStateChanged();
            NotifyLoadingChanged(false);
        }

        public async Task DeleteFindVm(Guid delFindId)
        {
            NotifyLoadingChanged(true);

            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;
            var deletedFindVm = new UserFindsViewModel();

            _userFindService.DeleteFind(delFindId, userId, userName);

            var currentViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
            var find = currentViewModel.finds.FirstOrDefault(f => f.findId == delFindId);
            currentViewModel.finds.Remove(find);

            UpdateViewModels(userId, currentViewModel);

            NotifyStateChanged();
            NotifyLoadingChanged(false);
        }

        public async Task Vote(Guid vmId, Guid findOrCommentId, int voteValue, string voteType)
        {
            var userId = _userStateService.CurrentUser.user.UsrId;
            var currentVm = CurrentViewModels.FirstOrDefault(vm => vm.userId == vmId);
            var userVoteDto = await _userFindService.Vote(findOrCommentId, userId, voteType, voteValue);
            var find = new FindDto();

            UserVoteDto? existingVote = null;

            if (voteType == "find")
            {
                find = currentVm.finds.FirstOrDefault(f => f.findId == findOrCommentId);
                existingVote = find.findVotes.FirstOrDefault(v => v.voteUserId == userId);

                if (existingVote != null && existingVote.voteValue == voteValue)
                {
                    find.findVotes.Remove(existingVote);
                }
                else if (existingVote != null && existingVote.voteValue != voteValue)
                {
                    find.findVotes.Remove(existingVote);
                    find.findVotes.Add(userVoteDto);
                }
                else if (existingVote == null)
                {
                    find.findVotes.Add(userVoteDto);
                }
                                             
            }
            else if (voteType == "comment")
            {
                // Find the find that owns this comment
                find = currentVm.finds.FirstOrDefault(f => f.findsCommentXrefs.Any(x => x.comxComId == findOrCommentId));
                var comment = find.findsCommentXrefs.FirstOrDefault(x => x.comxComId == findOrCommentId).findsComment;

                existingVote = find.findsCommentXrefs
                    .Where(xref => xref.findsComment.comId == findOrCommentId)
                    .SelectMany(xref => xref.findsComment.commentVotes)
                    .FirstOrDefault(v => v.voteUserId == userId);

                if (existingVote != null && existingVote.voteValue == voteValue)
                {
                    comment.commentVotes.Remove(existingVote);
                }
                else if (existingVote != null && existingVote.voteValue != voteValue)
                {
                    comment.commentVotes.Remove(existingVote);
                    comment.commentVotes.Add(userVoteDto);
                }
                else if (existingVote == null)
                {
                    comment.commentVotes.Add(userVoteDto);
                }
            }   
            UpdateViewModels(vmId, currentVm);
        }

    }
}
