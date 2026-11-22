namespace ForagerSite.Services.Interfaces;

public interface ICurrentUserService
{
    Task<Guid> GetRequiredUserIdAsync();
}
