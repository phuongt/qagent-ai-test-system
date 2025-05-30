using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAgentWeb.Models
{
    public class Screen : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ScreenType { get; set; } = "Other"; // Login, Dashboard, Form, List, Detail, etc.
        
        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        [StringLength(50)]
        public string AnalysisStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        
        [StringLength(200)]
        public string? OriginalFileName { get; set; }
        
        [StringLength(500)]
        public string? FilePath { get; set; }
        
        [StringLength(100)]
        public string? GoogleDriveFileId { get; set; }
        
        [StringLength(500)]
        public string? GoogleDriveFileUrl { get; set; }
        
        public long? FileSizeBytes { get; set; }
        
        [StringLength(50)]
        public string? ContentType { get; set; }
        
        // Analysis results (JSON format)
        public string? AnalysisResult { get; set; }
        
        public double? AnalysisConfidence { get; set; }
        
        public int? ComplexityScore { get; set; }
        
        // Foreign keys
        public int ProjectId { get; set; }
        
        public int? UploadSessionId { get; set; }
        
        [StringLength(50)]
        public string? UserId { get; set; }
        
        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
        
        [ForeignKey("UploadSessionId")]
        public virtual UploadSession? UploadSession { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        // Screen type constants
        public static class ScreenTypes
        {
            public const string Login = "Login";
            public const string Dashboard = "Dashboard";
            public const string Form = "Form";
            public const string List = "List";
            public const string Detail = "Detail";
            public const string Navigation = "Navigation";
            public const string Modal = "Modal";
            public const string Error = "Error";
            public const string Loading = "Loading";
            public const string Other = "Other";
            
            public static List<string> GetAll() => new() 
            { 
                Login, Dashboard, Form, List, Detail, Navigation, Modal, Error, Loading, Other 
            };
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
        
        // Analysis status constants
        public static class AnalysisStatuses
        {
            public const string Pending = "Pending";
            public const string Processing = "Processing";
            public const string Completed = "Completed";
            public const string Failed = "Failed";
            
            public static List<string> GetAll() => new() { Pending, Processing, Completed, Failed };
        }
        
        // Business logic methods
        public void MarkAsProcessing()
        {
            AnalysisStatus = AnalysisStatuses.Processing;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void MarkAsCompleted(string analysisResult, double confidence, int complexity)
        {
            AnalysisStatus = AnalysisStatuses.Completed;
            AnalysisResult = analysisResult;
            AnalysisConfidence = confidence;
            ComplexityScore = complexity;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void MarkAsFailed()
        {
            AnalysisStatus = AnalysisStatuses.Failed;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public bool IsImage()
        {
            return ContentType?.StartsWith("image/") == true;
        }
        
        public bool CanBeAnalyzed()
        {
            return IsImage() && !string.IsNullOrEmpty(FilePath) && AnalysisStatus != AnalysisStatuses.Processing;
        }
        
        public string GetFileSizeFormatted()
        {
            if (!FileSizeBytes.HasValue) return "Unknown";
            
            var bytes = FileSizeBytes.Value;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1048576) return $"{bytes / 1024:F1} KB";
            return $"{bytes / 1048576:F1} MB";
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
        
        public string GetStatusBadgeClass()
        {
            return AnalysisStatus switch
            {
                AnalysisStatuses.Completed => "bg-green-100 text-green-800",
                AnalysisStatuses.Processing => "bg-blue-100 text-blue-800",
                AnalysisStatuses.Failed => "bg-red-100 text-red-800",
                AnalysisStatuses.Pending => "bg-gray-100 text-gray-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
} 