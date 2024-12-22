using Azure.Core;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using ForagerSite.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace ForagerSite.Services
{
    public class VmService
    {
        public List<UserFindsViewModel> MyViewModels { get; set; } = new();
        public List<UserFindsViewModel> AllViewModels { get; set; } = new();
        public List<UserFindsViewModel> CurrentViewModels { get; set; } = new();
        public void AddOrUpdateViewModel(Guid userId, UserFindsViewModel viewModel)
        {
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.user.UsrId == userId);

            existingViewModel.userFinds.Add(viewModel.userFinds[0]);
            existingViewModel.userFindLocations.Add(viewModel.userFindLocations[0]);
            existingViewModel.userImages.AddRange(viewModel.userImages);

            var backupVmlMy = MyViewModels.FirstOrDefault(vm => vm.user.UsrId == userId); // Backup Vm My filter

            backupVmlMy.userFinds.Add(viewModel.userFinds[0]);
            backupVmlMy.userFindLocations.Add(viewModel.userFindLocations[0]);
            backupVmlMy.userImages.AddRange(viewModel.userImages);

            if (AllViewModels.Count > 0) // Backup Vm All filter
            {
                var backupVmlAll = AllViewModels.FirstOrDefault(vm => vm.user.UsrId == userId);

                backupVmlAll.userFinds.Add(viewModel.userFinds[0]);
                backupVmlAll.userFindLocations.Add(viewModel.userFindLocations[0]);
                backupVmlAll.userImages.AddRange(viewModel.userImages);
            }            
        }

        // Get a user's view model
        public UserFindsViewModel GetViewModel(Guid userId)
        {
            return CurrentViewModels.FirstOrDefault(vm => vm.user.UsrId == userId);
        }

        public void DeleteInfo(Guid userId, Guid findId)
        {            
            var existingViewModel = CurrentViewModels.FirstOrDefault(vm => vm.user.UsrId == userId);

            existingViewModel.userFinds.Remove(existingViewModel.userFinds.Where(f => f.UsFId == findId).FirstOrDefault());
            existingViewModel.userFindLocations.Remove(existingViewModel.userFindLocations.Where(fl => fl.UslUsfId == findId).FirstOrDefault());
            existingViewModel.userImages.RemoveAll(fi => fi.UsiUsfId == findId);

            var backupVmMy = MyViewModels.FirstOrDefault(vm => vm.user.UsrId == userId);

            backupVmMy.userFinds.Remove(backupVmMy.userFinds.Where(f => f.UsFId == findId).FirstOrDefault());
            backupVmMy.userFindLocations.Remove(backupVmMy.userFindLocations.Where(fl => fl.UslUsfId == findId).FirstOrDefault());
            backupVmMy.userImages.RemoveAll(fi => fi.UsiUsfId == findId);

            if (AllViewModels.Count > 0)
            {
                var backupVmAll = AllViewModels.FirstOrDefault(vm => vm.user.UsrId == userId);
                backupVmAll.userFinds.Remove(backupVmAll.userFinds.Where(f => f.UsFId == findId).FirstOrDefault());
                backupVmAll.userFindLocations.Remove(backupVmAll.userFindLocations.Where(fl => fl.UslUsfId == findId).FirstOrDefault());
                backupVmAll.userImages.RemoveAll(fi => fi.UsiUsfId == findId);
            }
        }
    }
    
}
