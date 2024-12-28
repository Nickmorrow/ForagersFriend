using DataAccess.Models;
using ForagerSite.Utilities;
using ForagerSite.Services;
using Microsoft.JSInterop;
using Newtonsoft.Json;


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
        public MapService(UserStateService userStateService, UserFindService userFindService)
        {
            _userStateService = userStateService;
            _userFindService = userFindService;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
        private void NotifyLoadingChanged(bool isLoading) => OnLoadingChange?.Invoke(isLoading);       
        public void AddViewModel(Guid userId, UserFindsViewModel viewModel)
        {
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);

            existingViewModel.finds.Add(viewModel.finds[0]);
            existingViewModel.findLocations.Add(viewModel.findLocations[0]);
            existingViewModel.images.AddRange(viewModel.images);

            var backupVmlMy = MyViewModels.FirstOrDefault(vm => vm.userId == userId); // Backup Vm My filter

            backupVmlMy.finds.Add(viewModel.finds[0]);
            backupVmlMy.findLocations.Add(viewModel.findLocations[0]);
            backupVmlMy.images.AddRange(viewModel.images);

            if (AllViewModels.Count > 0) // Backup Vm All filter
            {
                var backupVmlAll = AllViewModels.FirstOrDefault(vm => vm.userId == userId);

                backupVmlAll.finds.Add(viewModel.finds[0]);
                backupVmlAll.findLocations.Add(viewModel.findLocations[0]);
                backupVmlAll.images.AddRange(viewModel.images);
            }
        }
        public void UpdateViewModel(Guid userId, UserFindsViewModel viewModel)
        {
            // Update Vm
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
            if (existingViewModel != null)
            {
                existingViewModel.finds.Remove(existingViewModel.finds.Where(f => f.findId == viewModel.finds[0].findId).FirstOrDefault());
                existingViewModel.finds.Add(viewModel.finds[0]);

                existingViewModel.findLocations.Remove(existingViewModel.findLocations.Where(fl => fl.locFindId == viewModel.finds[0].findId).FirstOrDefault());
                existingViewModel.findLocations.Add(viewModel.findLocations[0]);

                existingViewModel.images.RemoveAll(fi => fi.imgFindId == viewModel.finds[0].findId);
                existingViewModel.images.AddRange(viewModel.images);
            }
            // Backup Vm My filter
            var backupVmlMy = MyViewModels.FirstOrDefault(vm => vm.userId == userId);
            if (existingViewModel != null)
            {
                backupVmlMy.finds.Remove(backupVmlMy.finds.Where(f => f.findId == viewModel.finds[0].findId).FirstOrDefault());
                backupVmlMy.finds.Add(viewModel.finds[0]);

                backupVmlMy.findLocations.Remove(backupVmlMy.findLocations.Where(fl => fl.locFindId == viewModel.finds[0].findId).FirstOrDefault());
                backupVmlMy.findLocations.Add(viewModel.findLocations[0]);

                backupVmlMy.images.RemoveAll(fi => fi.imgFindId == viewModel.finds[0].findId);
                backupVmlMy.images.AddRange(viewModel.images);
            }
            // Backup Vm All filter
            if (existingViewModel != null)
            {
                if (AllViewModels.Count > 0)
                {
                    var backupVmlAll = AllViewModels.FirstOrDefault(vm => vm.userId == userId);

                    backupVmlAll.finds.Remove(backupVmlAll.finds.Where(f => f.findId == viewModel.finds[0].findId).FirstOrDefault());
                    backupVmlAll.finds.Add(viewModel.finds[0]);

                    backupVmlAll.findLocations.Remove(backupVmlAll.findLocations.Where(fl => fl.locFindId == viewModel.finds[0].findId).FirstOrDefault());
                    backupVmlAll.findLocations.Add(viewModel.findLocations[0]);

                    backupVmlAll.images.RemoveAll(fi => fi.imgFindId == viewModel.finds[0].findId);
                    backupVmlAll.images.AddRange(viewModel.images);
                }
            }
        }
        public UserFindsViewModel GetViewModel(Guid userId)
        {
            return CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
        }
        public void DeleteInfo(Guid userId, Guid findId)
        {
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);

            existingViewModel.finds.Remove(existingViewModel.finds.Where(f => f.findId == findId).FirstOrDefault());
            existingViewModel.findLocations.Remove(existingViewModel.findLocations.Where(fl => fl.locFindId == findId).FirstOrDefault());
            existingViewModel.images.RemoveAll(fi => fi.imgFindId == findId);

            var backupVmMy = MyViewModels.FirstOrDefault(vm => vm.userId == userId);

            backupVmMy.finds.Remove(backupVmMy.finds.Where(f => f.findId == findId).FirstOrDefault());
            backupVmMy.findLocations.Remove(backupVmMy.findLocations.Where(fl => fl.locFindId == findId).FirstOrDefault());
            backupVmMy.images.RemoveAll(fi => fi.imgFindId == findId);

            if (AllViewModels.Count > 0)
            {
                var backupVmAll = AllViewModels.FirstOrDefault(vm => vm.userId == userId);
                backupVmAll.finds.Remove(backupVmAll.finds.Where(f => f.findId == findId).FirstOrDefault());
                backupVmAll.findLocations.Remove(backupVmAll.findLocations.Where(fl => fl.locFindId == findId).FirstOrDefault());
                backupVmAll.images.RemoveAll(fi => fi.imgFindId == findId);
            }
        }

        [JSInvokable("GetDetails")]
        public async Task<string> GetDetails(string findId, string mapFilter, string findUserId, string findUserName)
        {
            NotifyLoadingChanged(true);

            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;

            Guid findGuid;
            Guid findUserGuid;
            var selectedFind = new UserFind();
            var selectedVm = new UserFindsViewModel();
            var selectedVms = new List<UserFindsViewModel>();

            if (Guid.TryParse(findId, out findGuid) && Guid.TryParse(findUserId, out findUserGuid))
            {
                selectedFind = CurrentViewModels
                .FirstOrDefault(vm => vm.userId == findUserGuid)
                .finds
                .FirstOrDefault(f => f.findId == findGuid);

                selectedVm.finds.Add(selectedFind);
                selectedVm.userId = findUserGuid;
                selectedVm.userName = findUserName;
                selectedVm.findLocations = CurrentViewModels
                    .FirstOrDefault(vm => vm.userId == findUserGuid)
                    .findLocations
                    .Where(l => l.locFindId == findGuid)
                    .ToList();
                selectedVm.images = CurrentViewModels
                    .FirstOrDefault(vm => vm.userId == findUserGuid)
                    .images
                    .Where(i => i.imgFindId == findGuid)
                    .ToList();
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
            //var userStateService = ServiceLocator.ServiceProvider.GetService<UserStateService>();
            //var userFindService = ServiceLocator.ServiceProvider.GetService<UserFindService>();
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
            AddViewModel(userId, newUserFindViewModel);

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
            //var userStateService = ServiceLocator.ServiceProvider.GetService<UserStateService>();
            //var userFindService = ServiceLocator.ServiceProvider.GetService<UserFindService>();
            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;

            Guid findGuid;
            if (Guid.TryParse(findId, out findGuid))
            {
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
                UpdateViewModel(userId, newUserFindViewModel);

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
            //var userStateService = ServiceLocator.ServiceProvider.GetService<UserStateService>();
            //var userFindService = ServiceLocator.ServiceProvider.GetService<UserFindService>();
            var userId = _userStateService.CurrentUser.user.UsrId;
            var userName = _userStateService.CurrentUser.userSecurity.UssUsername;
            var deletedFindVm = new UserFindsViewModel();

            Guid findGuid;
            if (Guid.TryParse(findId, out findGuid))
                deletedFindVm = await _userFindService.DeleteFind(findGuid, userId, userName);

            DeleteInfo(userId, findGuid);

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
            return JsonConvert.SerializeObject(updatedViewModels, settings);
        }
    }
}
