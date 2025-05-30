using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAgentWeb.Models
{
    public class QAgentTask : BaseEntity
    {
        [StringLength(50)]
        public string? TaskId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";
        
        [StringLength(50)]
        public string? TaskType { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(50)]
        public string? AssignedTo { get; set; }
        
        [StringLength(100)]
        public string? Category { get; set; }
        
        public string? Notes { get; set; }
        
        public int Progress { get; set; } = 0;
        
        public decimal? EstimatedHours { get; set; }
        
        public decimal? ActualHours { get; set; }
        
        public string? Tags { get; set; } // JSON format in database
        
        // Foreign Keys
        public int? ProjectId { get; set; }
        
        public int? ScreenId { get; set; }
        
        public int? ParentTaskId { get; set; }
        
        // User relationship
        [StringLength(50)]
        public string? UserId { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
        
        [ForeignKey("ScreenId")]
        public virtual Screen? Screen { get; set; }
        
        [ForeignKey("ParentTaskId")]
        public virtual QAgentTask? ParentTask { get; set; }
        
        public virtual ICollection<QAgentTask> SubTasks { get; set; } = new List<QAgentTask>();
        
        // Status constants
        public static class Statuses
        {
            public const string Pending = "Pending";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string OnHold = "OnHold";
            
            public static List<string> GetAll() => new() { Pending, InProgress, Completed, Cancelled, OnHold };
        }
        
        // Priority constants
        public static class Priorities
        {
            public const string Low = "Low";
            public const string Medium = "Medium";
            public const string High = "High";
            public const string Critical = "Critical";
            
            public static List<string> GetAll() => new() { Low, Medium, High, Critical };
        }
        
        // Business logic methods
        public void MarkAsInProgress()
        {
            Status = Statuses.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void MarkAsCompleted()
        {
            Status = Statuses.Completed;
            CompletedAt = DateTime.UtcNow;
            Progress = 100;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void SetProgress(int progressValue)
        {
            Progress = Math.Max(0, Math.Min(100, progressValue));
            UpdatedAt = DateTime.UtcNow;
            
            if (Progress == 100 && Status != Statuses.Completed)
            {
                MarkAsCompleted();
            }
        }
        
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                Statuses.Completed => "bg-green-100 text-green-800",
                Statuses.InProgress => "bg-blue-100 text-blue-800",
                Statuses.OnHold => "bg-yellow-100 text-yellow-800",
                Statuses.Cancelled => "bg-red-100 text-red-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
        
        public string GetPriorityBadgeClass()
        {
            return Priority switch
            {
                Priorities.Critical => "bg-red-100 text-red-800",
                Priorities.High => "bg-orange-100 text-orange-800",
                Priorities.Medium => "bg-yellow-100 text-yellow-800",
                Priorities.Low => "bg-green-100 text-green-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
}