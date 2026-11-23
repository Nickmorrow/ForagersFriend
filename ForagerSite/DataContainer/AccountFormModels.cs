using System.ComponentModel.DataAnnotations;

namespace ForagerSite.DataContainer
{
    public sealed class LoginInputModel
    {
        [Required]
        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class RegistrationInputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Country { get; set; }
        [StringLength(100)]
        public string? StateOrProvince { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(256, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public sealed class AccountRecoveryInputModel
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("password|username|confirmation")]
        public string RequestType { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordInputModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(256, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public sealed class EmailChangeInputModel
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string NewEmail { get; set; } = string.Empty;
    }

    public sealed class ConfirmEmailChangeInputModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
