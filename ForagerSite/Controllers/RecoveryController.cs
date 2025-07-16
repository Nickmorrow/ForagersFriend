//using ForagerSite.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;
//using ForagerSite.Services;
//using ForagerSite.Models;

//namespace ForagerSite.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class RecoverPasswordController : ControllerBase
//    {
//        private readonly IPasswordResetService _passwordResetService;
//        private readonly IEmailService _emailService;

//        public RecoverPasswordController(IPasswordResetService userService, IEmailService emailService)
//        {
//            _passwordResetService = userService;
//            _emailService = emailService;
//        }

//        [HttpPost]
//        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordRequest request)
//        {
//            var user = await _passwordResetService.GetUserByEmailAsync(request.Email);
//            if (user == null)
//            {
//                return NotFound("User not found");
//            }

//            var token = await _passwordResetService.GeneratePasswordResetTokenAsync(user);
//            var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

//            await _emailService.SendPasswordResetEmail(user.Email, resetLink);

//            return Ok("Recovery email sent");
//        }
//    }
//}
