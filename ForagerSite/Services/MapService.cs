using DataAccess.Models;
using ForagerSite.Utilities;
using ForagerSite.Services;
using Microsoft.JSInterop;
using Newtonsoft.Json;


namespace ForagerSite.Services
{
    public class MapService
    {
        private readonly UserStateService _userStateService;
        private readonly UserFindService _userFindService;
        public List<UserFindsViewModel> MyViewModels { get; set; } = new();
        public List<UserFindsViewModel> AllViewModels { get; set; } = new();
        public List<UserFindsViewModel> CurrentViewModels { get; set; } = new();
        public MapService(UserStateService userStateService, UserFindService userFindService)
        {
            _userStateService = userStateService;
            _userFindService = userFindService;
        }

        public void AddViewModel(Guid userId, UserFindsViewModel viewModel)
        {
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);

            existingViewModel.userFinds.Add(viewModel.userFinds[0]);
            existingViewModel.userFindLocations.Add(viewModel.userFindLocations[0]);
            existingViewModel.userImages.AddRange(viewModel.userImages);

            var backupVmlMy = MyViewModels.FirstOrDefault(vm => vm.userId == userId); // Backup Vm My filter

            backupVmlMy.userFinds.Add(viewModel.userFinds[0]);
            backupVmlMy.userFindLocations.Add(viewModel.userFindLocations[0]);
            backupVmlMy.userImages.AddRange(viewModel.userImages);

            if (AllViewModels.Count > 0) // Backup Vm All filter
            {
                var backupVmlAll = AllViewModels.FirstOrDefault(vm => vm.userId == userId);

                backupVmlAll.userFinds.Add(viewModel.userFinds[0]);
                backupVmlAll.userFindLocations.Add(viewModel.userFindLocations[0]);
                backupVmlAll.userImages.AddRange(viewModel.userImages);
            }
        }
        public void UpdateViewModel(Guid userId, UserFindsViewModel viewModel)
        {
            // Update Vm
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.userId == userId);
            if (existingViewModel != null)
            {
                existingViewModel.userFinds.Remove(existingViewModel.userFinds.Where(f => f.UsFId == viewModel.userFinds[0].UsFId).FirstOrDefault());
                existingViewModel.userFinds.Add(viewModel.userFinds[0]);

                existingViewModel.userFindLocations.Remove(existingViewModel.userFindLocations.Where(fl => fl.UslUsfId == viewModel.userFinds[0].UsFId).FirstOrDefault());
                existingViewModel.userFindLocations.Add(viewModel.userFindLocations[0]);

                existingViewModel.userImages.RemoveAll(fi => fi.UsiUsfId == viewModel.userFinds[0].UsFId);
                existingViewModel.userImages.AddRange(viewModel.userImages);
            }
            // Backup Vm My filter
            var backupVmlMy = MyViewModels.FirstOrDefault(vm => vm.userId == userId);
            if (existingViewModel != null)
            {
                backupVmlMy.userFinds.Remove(backupVmlMy.userFinds.Where(f => f.UsFId == viewModel.userFinds[0].UsFId).FirstOrDefault());
                backupVmlMy.userFinds.Add(viewModel.userFinds[0]);

                backupVmlMy.userFindLocations.Remove(backupVmlMy.userFindLocations.Where(fl => fl.UslUsfId == viewModel.userFinds[0].UsFId).FirstOrDefault());
                backupVmlMy.userFindLocations.Add(viewModel.userFindLocations[0]);

                backupVmlMy.userImages.RemoveAll(fi => fi.UsiUsfId == viewModel.userFinds[0].UsFId);
                backupVmlMy.userImages.AddRange(viewModel.userImages);
            }
            // Backup Vm All filter
            if (existingViewModel != null)
            {
                if (AllViewModels.Count > 0)
                {
                    var backupVmlAll = AllViewModels.FirstOrDefault(vm => vm.userId == userId);

                    backupVmlAll.userFinds.Remove(backupVmlAll.userFinds.Where(f => f.UsFId == viewModel.userFinds[0].UsFId).FirstOrDefault());
                    backupVmlAll.userFinds.Add(viewModel.userFinds[0]);

                    backupVmlAll.userFindLocations.Remove(backupVmlAll.userFindLocations.Where(fl => fl.UslUsfId == viewModel.userFinds[0].UsFId).FirstOrDefault());
                    backupVmlAll.userFindLocations.Add(viewModel.userFindLocations[0]);

                    backupVmlAll.userImages.RemoveAll(fi => fi.UsiUsfId == viewModel.userFinds[0].UsFId);
                    backupVmlAll.userImages.AddRange(viewModel.userImages);
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

            existingViewModel.userFinds.Remove(existingViewModel.userFinds.Where(f => f.UsFId == findId).FirstOrDefault());
            existingViewModel.userFindLocations.Remove(existingViewModel.userFindLocations.Where(fl => fl.UslUsfId == findId).FirstOrDefault());
            existingViewModel.userImages.RemoveAll(fi => fi.UsiUsfId == findId);

            var backupVmMy = MyViewModels.FirstOrDefault(vm => vm.userId == userId);

            backupVmMy.userFinds.Remove(backupVmMy.userFinds.Where(f => f.UsFId == findId).FirstOrDefault());
            backupVmMy.userFindLocations.Remove(backupVmMy.userFindLocations.Where(fl => fl.UslUsfId == findId).FirstOrDefault());
            backupVmMy.userImages.RemoveAll(fi => fi.UsiUsfId == findId);

            if (AllViewModels.Count > 0)
            {
                var backupVmAll = AllViewModels.FirstOrDefault(vm => vm.userId == userId);
                backupVmAll.userFinds.Remove(backupVmAll.userFinds.Where(f => f.UsFId == findId).FirstOrDefault());
                backupVmAll.userFindLocations.Remove(backupVmAll.userFindLocations.Where(fl => fl.UslUsfId == findId).FirstOrDefault());
                backupVmAll.userImages.RemoveAll(fi => fi.UsiUsfId == findId);
            }
        }

        [JSInvokable("GetDetails")]
        public async Task<string> GetDetails(string findId, string mapFilter, string findUserId, string findUserName)
        {
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
                .userFinds
                .FirstOrDefault(f => f.UsFId == findGuid);

                selectedVm.userFinds.Add(selectedFind);
                selectedVm.userId = findUserGuid;
                selectedVm.userName = findUserName;
                selectedVm.userFindLocations = CurrentViewModels
                    .FirstOrDefault(vm => vm.userId == findUserGuid)
                    .userFindLocations
                    .Where(l => l.UslUsfId == findGuid)
                    .ToList();
                selectedVm.userImages = CurrentViewModels
                    .FirstOrDefault(vm => vm.userId == findUserGuid)
                    .userImages
                    .Where(i => i.UsiUsfId == findGuid)
                    .ToList();
                selectedVms.Add(selectedVm);
            }
            CurrentViewModels = VmUtilities.Copy(selectedVms);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
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
            var userStateService = ServiceLocator.ServiceProvider.GetService<UserStateService>();
            var userFindService = ServiceLocator.ServiceProvider.GetService<UserFindService>();
            var userId = userStateService.CurrentUser.user.UsrId;
            var userName = userStateService.CurrentUser.userSecurity.UssUsername;

            var newUserFindViewModel =
            await userFindService.CreateFind(
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
            var userStateService = ServiceLocator.ServiceProvider.GetService<UserStateService>();
            var userFindService = ServiceLocator.ServiceProvider.GetService<UserFindService>();
            var userId = userStateService.CurrentUser.user.UsrId;
            var userName = userStateService.CurrentUser.userSecurity.UssUsername;

            Guid findGuid;
            if (Guid.TryParse(findId, out findGuid))
            {
                var newUserFindViewModel =
                await userFindService.UpdateFind(
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
            var userStateService = ServiceLocator.ServiceProvider.GetService<UserStateService>();
            var userFindService = ServiceLocator.ServiceProvider.GetService<UserFindService>();
            var userId = userStateService.CurrentUser.user.UsrId;
            var userName = userStateService.CurrentUser.userSecurity.UssUsername;
            var deletedFindVm = new UserFindsViewModel();

            Guid findGuid;
            if (Guid.TryParse(findId, out findGuid))
                deletedFindVm = await userFindService.DeleteFind(findGuid, userId, userName);

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
