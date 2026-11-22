using Microsoft.AspNetCore.Identity;

namespace DataAccess.Models
{
    public sealed class ForagerAccount : IdentityUser<Guid>
    {
        public User Profile { get; set; } = null!;
    }
}
