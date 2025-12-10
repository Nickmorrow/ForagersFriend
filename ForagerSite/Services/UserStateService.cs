using DataAccess.Models;

namespace ForagerSite.Services
{
    public class UserStateService
    {
        public bool IsAuthenticated { get; set; }
        public UserViewModel CurrentUser { get; set; }

        public event Action? OnChange;

        public void SetUserState(bool isAuthenticated, UserViewModel currentUser)
        {
            IsAuthenticated = isAuthenticated;
            CurrentUser = currentUser;
            Console.WriteLine($"User authenticated: {IsAuthenticated}, User: {CurrentUser?.user?.UsrName}");
            OnChange?.Invoke();
        }

        public void Logout()
        {
            SetUserState(false, null);
        }
    }
}
