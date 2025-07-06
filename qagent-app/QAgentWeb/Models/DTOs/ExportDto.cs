using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models.DTOs
{
    public class ExportRequestDto
    {
        [Required]
        public string ExportType { get; set; } = ""; // "excel", "csv", "pdf", "json"

        [Required]
        public string DataType { get; set; } = ""; // "project", "screen", "analysis", "checklist"

        [Required]
        public List<int> ItemIds { get; set; } = new();

        public string? Format { get; set; } // Additional format options

        public bool IncludeMetadata { get; set; } = true;
        public bool IncludeImages { get; set; } = false;
        public bool IncludeAnalysisResults { get; set; } = true;
        public bool IncludeChecklistItems { get; set; } = true;
        public bool IncludeAppliedRules { get; set; } = false;

        public Dictionary<string, object> CustomOptions { get; set; } = new();
    }

    public class ExportResultDto
    {
        public string ExportId { get; set; } = "";
        public string Status { get; set; } = ""; // "Processing", "Completed", "Failed"
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? DownloadUrl { get; set; }
        public long? FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public ExportMetadataDto? Metadata { get; set; }
    }

    public class ExportMetadataDto
    {
        public string ExportType { get; set; } = "";
        public string DataType { get; set; } = "";
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public List<string> IncludedFields { get; set; } = new();
        public Dictionary<string, object> Statistics { get; set; } = new();
    }

    public class GoogleSheetsExportDto
    {
        [Required]
        public string DataType { get; set; } = "";

        [Required]
        public List<int> ItemIds { get; set; } = new();

        public string? SpreadsheetId { get; set; }
        public string? WorksheetName { get; set; }
        public bool CreateNewSpreadsheet { get; set; } = true;
        public bool ShareWithUser { get; set; } = true;
        public string? UserEmail { get; set; }
        public bool IncludeFormatting { get; set; } = true;
        public bool IncludeCharts { get; set; } = false;
    }

    public class GoogleSheetsResultDto
    {
        public string SpreadsheetId { get; set; } = "";
        public string SpreadsheetUrl { get; set; } = "";
        public string WorksheetName { get; set; } = "";
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsShared { get; set; }
        public List<string> SharedWith { get; set; } = new();
    }

    public class ExportTemplateDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string DataType { get; set; } = "";
        public string ExportType { get; set; } = "";
        public List<ExportFieldDto> Fields { get; set; } = new();
        public Dictionary<string, object> Options { get; set; } = new();
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ExportFieldDto
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string DataType { get; set; } = "";
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; } = true;
        public int Order { get; set; }
        public string? Format { get; set; }
        public Dictionary<string, object> Options { get; set; } = new();
    }

    public class CreateExportTemplateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string Description { get; set; } = "";

        [Required]
        public string DataType { get; set; } = "";

        [Required]
        public string ExportType { get; set; } = "";

        [Required]
        public List<ExportFieldDto> Fields { get; set; } = new();

        public Dictionary<string, object> Options { get; set; } = new();
    }

    public class BulkExportRequestDto
    {
        [Required]
        public List<ExportRequestDto> Exports { get; set; } = new();

        public string? BatchName { get; set; }
        public bool ZipResults { get; set; } = true;
        public bool NotifyWhenComplete { get; set; } = true;
        public string? NotificationEmail { get; set; }
    }

    public class BulkExportResultDto
    {
        public string BatchId { get; set; } = "";
        public string Status { get; set; } = "";
        public List<ExportResultDto> Results { get; set; } = new();
        public string? ZipFilePath { get; set; }
        public string? ZipDownloadUrl { get; set; }
        public long? TotalSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalExports { get; set; }
        public int CompletedExports { get; set; }
        public int FailedExports { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ExportHistoryDto
    {
        public string ExportId { get; set; } = "";
        public string DataType { get; set; } = "";
        public string ExportType { get; set; } = "";
        public string Status { get; set; } = "";
        public string? FileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}