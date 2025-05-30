using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAgentWeb.Models
{
    public class Project : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Domain { get; set; } = string.Empty; // Web, Mobile, Desktop, API
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Uploaded, Analyzing, Completed, Archived
        
        [StringLength(100)]
        public string? GoogleDriveFolderId { get; set; }
        
        [StringLength(500)]
        public string? GoogleDriveFolderUrl { get; set; }
        
        // Foreign key to User (will be added when Identity is implemented)
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        public bool IsArchived { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<Screen> Screens { get; set; } = new List<Screen>();
        public virtual ICollection<UploadSession> UploadSessions { get; set; } = new List<UploadSession>();
        
        // Domain constants
        public static class Domains
        {
            public const string Web = "Web";
            public const string Mobile = "Mobile";
            public const string Desktop = "Desktop";
            public const string API = "API";
            
            public static List<string> GetAll() => new() { Web, Mobile, Desktop, API };
        }
        
        // Status constants
        public static class Statuses
        {
            public const string Draft = "Draft";
            public const string Uploaded = "Uploaded";
            public const string Analyzing = "Analyzing";
            public const string Completed = "Completed";
            public const string Archived = "Archived";
            
            public static List<string> GetAll() => new() { Draft, Uploaded, Analyzing, Completed, Archived };
        }
        
        // Business logic methods
        public void MarkAsUploaded()
        {
            Status = Statuses.Uploaded;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void MarkAsAnalyzing()
        {
            Status = Statuses.Analyzing;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void MarkAsCompleted()
        {
            Status = Statuses.Completed;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public bool CanBeDeleted()
        {
            return Status == Statuses.Draft || Status == Statuses.Archived;
        }
        
        public int GetScreenCount()
        {
            return Screens?.Count(s => !s.IsDeleted) ?? 0;
        }
        
        public double GetProgressPercentage()
        {
            var totalScreens = GetScreenCount();
            if (totalScreens == 0) return 0;
            
            var analyzedScreens = Screens?.Count(s => !s.IsDeleted && s.AnalysisStatus == Screen.AnalysisStatuses.Completed) ?? 0;
            return Math.Round((double)analyzedScreens / totalScreens * 100, 2);
        }
    }
} 