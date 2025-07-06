using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models.DTOs
{
    public class AnalysisRequestDto
    {
        [Required]
        public int ScreenId { get; set; }

        [MaxLength(1000)]
        public string? BusinessDescription { get; set; }

        [MaxLength(50)]
        public string? AnalysisType { get; set; } = "Full";

        public bool IncludeUIElements { get; set; } = true;
        public bool IncludeTextExtraction { get; set; } = true;
        public bool IncludeISTQBRules { get; set; } = true;
        public bool GenerateChecklist { get; set; } = true;
    }

    public class AnalysisResultDto
    {
        public int Id { get; set; }
        public int ScreenId { get; set; }
        public string? ScreenName { get; set; }
        public string AnalysisStatus { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? AIModelUsed { get; set; }
        public string? ErrorMessage { get; set; }
        public List<UIElementDto> UIElements { get; set; } = new();
        public List<string> ExtractedText { get; set; } = new();
        public List<AppliedRuleDto> AppliedRules { get; set; } = new();
        public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
        public string? Summary { get; set; }
        public double? ConfidenceScore { get; set; }
    }

    public class UIElementDto
    {
        public string Type { get; set; } = "";
        public string Text { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class AppliedRuleDto
    {
        public string RuleId { get; set; } = "";
        public string RuleName { get; set; } = "";
        public string RuleCategory { get; set; } = "";
        public string Priority { get; set; } = "";
        public bool WasApplied { get; set; }
        public string? AppliedContext { get; set; }
        public List<string> GeneratedChecklistItems { get; set; } = new();
    }

    public class ChecklistItemDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string Priority { get; set; } = "";
        public bool IsRequired { get; set; }
        public List<string> TestSteps { get; set; } = new();
        public List<string> ExpectedResults { get; set; } = new();
        public string? RuleId { get; set; }
        public string? UIElementType { get; set; }
    }

    public class BatchAnalysisRequestDto
    {
        [Required]
        public List<int> ScreenIds { get; set; } = new();

        [MaxLength(1000)]
        public string? BusinessDescription { get; set; }

        [MaxLength(50)]
        public string? AnalysisType { get; set; } = "Full";

        public bool IncludeUIElements { get; set; } = true;
        public bool IncludeTextExtraction { get; set; } = true;
        public bool IncludeISTQBRules { get; set; } = true;
        public bool GenerateChecklist { get; set; } = true;
    }

    public class BatchAnalysisResultDto
    {
        public List<AnalysisResultDto> Results { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalScreens { get; set; }
        public int SuccessfulAnalyses { get; set; }
        public int FailedAnalyses { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class AnalysisProgressDto
    {
        public string Status { get; set; } = "";
        public int CompletedScreens { get; set; }
        public int TotalScreens { get; set; }
        public string? CurrentScreen { get; set; }
        public string? CurrentStep { get; set; }
        public double ProgressPercentage { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime StartedAt { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
    }
}