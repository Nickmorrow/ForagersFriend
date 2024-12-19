
//using System;
//using System.Threading.Tasks;
//using DataAccess.Models;
//using Microsoft.AspNetCore.Identity;

//namespace ForagerSite.Services
//{
//    public class PasswordResetService : IPasswordResetService
//    {
//        private readonly UserManager<ApplicationUser> _userManager;

//        public PasswordResetService(UserManager<ApplicationUser> userManager)
//        {
//            _userManager = userManager;
//        }

//        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
//        {
//            return await _userManager.FindByEmailAsync(email);
//        }

//        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
//        {
//            return await _userManager.GeneratePasswordResetTokenAsync(user);
//        }
//    }
//}
