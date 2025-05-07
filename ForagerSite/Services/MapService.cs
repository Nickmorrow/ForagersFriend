using DataAccess.Models;
using ForagerSite.Utilities;
using ForagerSite.Services;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;



namespace ForagerSite.Services
{
    public class MapService
    {       

        public static readonly List<string> MapFilters = new List<string>
        {
            "UserOnly", "AllUsers", "FriendUsers"
        };
        public string mapFilter = MapFilters[0];
        private readonly UserStateService _userStateService;
        private readonly UserFindService _userFindService;
        public event Action OnChange;
        public event Action<bool> OnLoadingChange;
        public List<UserFindsViewModel> MyViewModels { get; set; } = new();
        public List<UserFindsViewModel> AllViewModels { get; set; } = new();
        public List<UserFindsViewModel> CurrentViewModels { get; set; } = new();
        public Dictionary<Guid, string> userNamesKvp { get; set; }
        public MapService(UserStateService userStateService, UserFindService userFindService)
        {
            _userStateService = userStateService;
            _userFindService = userFindService;
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
        private void NotifyLoadingChanged(bool isLoading) => OnLoadingChange?.Invoke(isLoading);        
        public void UpdateViewModels(Guid userId, UserFindsViewModel viewModel)
        {
            var currentUserId = _userStateService.CurrentUser.user.UsrId;

            //var existingVmIndex = CurrentViewModels.FindIndex(vm => vm.userId == userId);
            //if (existingVmIndex != -1)
            //{
            //    CurrentViewModels[existingVmIndex] = VmUtilities.Copy(viewModel);
            //}

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

        [JSInvokable("GetDetails")]
        public async Task<string> GetDetails(string findId, string mapFilter, string findUserId, string findUserName)
        {
            NotifyLoadingChanged(true);

            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;

            Guid findGuid;
            Guid findUserGuid;
            
            var selectedVm = new UserFindsViewModel();
            var selectedVms = new List<UserFindsViewModel>();

            if (Guid.TryParse(findId, out findGuid) && Guid.TryParse(findUserId, out findUserGuid))
            {
                var selectedFind = CurrentViewModels
                .FirstOrDefault(vm => vm.userId == findUserGuid)
                .finds
                .FirstOrDefault(f => f.findId == findGuid);

                selectedVm.finds.Add(selectedFind);
                selectedVm.userId = findUserGuid;
                selectedVm.userName = findUserName;
                
                //foreach (var kvp in CurrentViewModels.FirstOrDefault(vm => vm.userId == findUserGuid).userNamesKvp)
                //{
                //    selectedVm.userNamesKvp[kvp.Key] = kvp.Value;
                //}
                selectedVms.Add(selectedVm);
            }
            CurrentViewModels = VmUtilities.Copy(selectedVms);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            NotifyStateChanged(); 
            NotifyLoadingChanged(false);

            return JsonConvert.SerializeObject(CurrentViewModels, settings);
        }

        [JSInvokable("CreateFind")]
        public async Task<string> CreateFind(
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
        string mapFilter,
        List<string> uploadedFileUrls)
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
                userName
            );

            var find = newUserFindViewModel.finds[0];
            var currentViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
            currentViewModel.finds.Add(find);

            UpdateViewModels(userId, currentViewModel);

            List<UserFindsViewModel> updatedViewModels = mapFilter switch
            {
                "UserOnly" => CurrentViewModels.Where(vm => vm.userId == userId).ToList(),
                "AllUsers" => CurrentViewModels.ToList(),
                _ => new List<UserFindsViewModel>()
            };
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            NotifyStateChanged();
            NotifyLoadingChanged(false);

            return JsonConvert.SerializeObject(updatedViewModels, settings);
        }


        [JSInvokable("UpdateFind")]
        public async Task<string> UpdateFind(
        string findId,
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
        string mapFilter,
        List<string>? uploadedFileUrls,
        List<string>? deletedFileUrls)
        {            
            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;

            Guid findGuid;
            if (Guid.TryParse(findId, out findGuid))
            {
                NotifyLoadingChanged(true);

                var newUserFindViewModel =
                await _userFindService.UpdateFind(
                    findGuid,
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
                    deletedFileUrls,
                    userId,
                    userName
                );

                var find = newUserFindViewModel.finds[0];
                var currentViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);

                var originalIndex = currentViewModel.finds.FindIndex(f => f.findId == findGuid);

                int index = currentViewModel.finds.FindIndex(f => f.findId == find.findId);               
                currentViewModel.finds[index] = find; 
                
                UpdateViewModels(userId, currentViewModel);

                List<UserFindsViewModel> updatedViewModels = mapFilter switch
                {
                    "UserOnly" => CurrentViewModels.Where(vm => vm.userId == userId).ToList(),
                    "AllUsers" => CurrentViewModels.ToList(),
                    _ => new List<UserFindsViewModel>()
                };
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                NotifyStateChanged();
                NotifyLoadingChanged(false);
                return JsonConvert.SerializeObject(updatedViewModels, settings);
            }
            else
            {
                throw new ArgumentException("Invalid GUID format", nameof(findId));
            }
        }


        [JSInvokable("DeleteFind")]
        public async Task<string> DeleteFind(string findId, string mapFilter)
        {
            NotifyLoadingChanged(true);

            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;
            var deletedFindVm = new UserFindsViewModel();

            Guid findGuid;
            if (Guid.TryParse(findId, out findGuid))
                _userFindService.DeleteFind(findGuid, userId, userName);
           
            var currentViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
            var find = currentViewModel.finds.FirstOrDefault(f => f.findId == findGuid);
            currentViewModel.finds.Remove(find);

            UpdateViewModels(userId, currentViewModel);

            List<UserFindsViewModel> updatedViewModels = mapFilter switch
            {
                "UserOnly" => CurrentViewModels.Where(vm => vm.userId == userId).ToList(),
                "AllUsers" => CurrentViewModels.ToList(),
                _ => new List<UserFindsViewModel>()
            };
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            NotifyStateChanged();
            NotifyLoadingChanged(false);
            return JsonConvert.SerializeObject(updatedViewModels, settings);
        }
    }
}
