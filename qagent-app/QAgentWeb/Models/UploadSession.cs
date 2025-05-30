using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAgentWeb.Models
{
    public class UploadSession : BaseEntity
    {
        [Required]
        public int SessionId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Started"; // Started, InProgress, Completed, Failed, Cancelled
        
        public int TotalFiles { get; set; } = 0;
        
        public int ProcessedFiles { get; set; } = 0;
        
        public int SuccessfulFiles { get; set; } = 0;
        
        public int FailedFiles { get; set; } = 0;
        
        public long TotalSizeBytes { get; set; } = 0;
        
        public long ProcessedSizeBytes { get; set; } = 0;
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        // Foreign keys
        public int ProjectId { get; set; }
        
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
        
        public virtual ICollection<Screen> Screens { get; set; } = new List<Screen>();
        
        // Status constants
        public static class Statuses
        {
            public const string Started = "Started";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
            
            public static List<string> GetAll() => new() { Started, InProgress, Completed, Failed, Cancelled };
        }
        
        // Business logic methods
        public void Start()
        {
            Status = Statuses.InProgress;
            StartedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void AddFile()
        {
            TotalFiles++;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void FileProcessed(bool success, long fileSize)
        {
            ProcessedFiles++;
            ProcessedSizeBytes += fileSize;
            
            if (success)
                SuccessfulFiles++;
            else
                FailedFiles++;
                
            UpdatedAt = DateTime.UtcNow;
            
            // Auto-complete if all files processed
            if (ProcessedFiles >= TotalFiles)
            {
                Complete();
            }
        }
        
        public void Complete()
        {
            Status = Statuses.Completed;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Fail(string errorMessage)
        {
            Status = Statuses.Failed;
            ErrorMessage = errorMessage;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Cancel()
        {
            Status = Statuses.Cancelled;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public double GetProgressPercentage()
        {
            if (TotalFiles == 0) return 0;
            return Math.Round((double)ProcessedFiles / TotalFiles * 100, 2);
        }
        
        public double GetSizeProgressPercentage()
        {
            if (TotalSizeBytes == 0) return 0;
            return Math.Round((double)ProcessedSizeBytes / TotalSizeBytes * 100, 2);
        }
        
        public TimeSpan? GetDuration()
        {
            if (!StartedAt.HasValue) return null;
            
            var endTime = CompletedAt ?? DateTime.UtcNow;
            return endTime - StartedAt.Value;
        }
        
        public string GetDurationFormatted()
        {
            var duration = GetDuration();
            if (!duration.HasValue) return "Unknown";
            
            var d = duration.Value;
            if (d.TotalMinutes < 1) return $"{d.Seconds}s";
            if (d.TotalHours < 1) return $"{d.Minutes}m {d.Seconds}s";
            return $"{(int)d.TotalHours}h {d.Minutes}m";
        }
        
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                Statuses.Completed => "bg-green-100 text-green-800",
                Statuses.InProgress => "bg-blue-100 text-blue-800",
                Statuses.Failed => "bg-red-100 text-red-800",
                Statuses.Cancelled => "bg-gray-100 text-gray-800",
                Statuses.Started => "bg-yellow-100 text-yellow-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
        
        public bool IsCompleted()
        {
            return Status == Statuses.Completed;
        }
        
        public bool IsInProgress()
        {
            return Status == Statuses.InProgress || Status == Statuses.Started;
        }
        
        public bool HasErrors()
        {
            return FailedFiles > 0 || Status == Statuses.Failed;
        }
        
        public string GetTotalSizeFormatted()
        {
            var bytes = TotalSizeBytes;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1048576) return $"{bytes / 1024:F1} KB";
            return $"{bytes / 1048576:F1} MB";
        }
        
        public string GetProcessedSizeFormatted()
        {
            var bytes = ProcessedSizeBytes;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1048576) return $"{bytes / 1024:F1} KB";
            return $"{bytes / 1048576:F1} MB";
        }
    }
} 