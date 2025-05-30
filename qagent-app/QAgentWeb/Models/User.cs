using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; } = false;

        public string? EmailConfirmationToken { get; set; }

        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordTokenExpiry { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<UploadSession> UploadSessions { get; set; } = new List<UploadSession>();
        public virtual ICollection<Screen> Screens { get; set; } = new List<Screen>();
        public virtual ICollection<QAgentTask> QAgentTasks { get; set; } = new List<QAgentTask>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
    }
} 