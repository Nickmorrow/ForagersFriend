namespace DataAccess.Models;

public enum AccountEmailJobType
{
    PasswordReset,
    UserNameRecovery,
    ConfirmationResend,
    PasswordChangedNotice
}

public sealed class AccountEmailJob
{
    public int Id { get; set; }
    public AccountEmailJobType Type { get; set; }
    public string? Email { get; set; }
    public Guid? AccountId { get; set; }
    public string? NewEmail { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime NextAttemptUtc { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public int Attempts { get; set; }
}
