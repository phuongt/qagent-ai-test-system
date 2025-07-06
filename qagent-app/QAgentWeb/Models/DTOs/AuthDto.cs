using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; } = false;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = "";

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Organization { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }
    }

    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public UserDto? User { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}";
        public string? Phone { get; set; }
        public string? Organization { get; set; }
        public string? JobTitle { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = "";

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Organization { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        [MinLength(6)]
        public string CurrentPassword { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = "";
    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = "";
    }

    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; } = "";

        [Required]
        public string RefreshToken { get; set; } = "";
    }

    public class UserStatsDto
    {
        public int TotalProjects { get; set; }
        public int TotalScreens { get; set; }
        public int CompletedAnalyses { get; set; }
        public int PendingAnalyses { get; set; }
        public int GeneratedChecklists { get; set; }
        public DateTime? LastActivity { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int? ProjectId { get; set; }
        public int? ScreenId { get; set; }
    }
}