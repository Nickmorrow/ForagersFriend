using ForagerSite.Models;

namespace ForagerSite.Services
{
    public class UserStateService
    {
        public bool IsAuthenticated { get; set; }
        public UserViewModel CurrentUser { get; set; }

        public void SetUserState(bool isAuthenticated, UserViewModel currentUser)
        {
            IsAuthenticated = isAuthenticated;
            CurrentUser = currentUser;
            Console.WriteLine($"User authenticated: {IsAuthenticated}, User: {CurrentUser?.user?.UsrName}");
        }
    }
}
