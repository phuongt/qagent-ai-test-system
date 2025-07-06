using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models.DTOs
{
    public class TestingRuleDto
    {
        public int Id { get; set; }
        public string RuleId { get; set; } = "";
        public string RuleName { get; set; } = "";
        public string RuleCategory { get; set; } = "";
        public string RuleType { get; set; } = "";
        public string Description { get; set; } = "";
        public string? DescriptionVi { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public string? RuleTemplate { get; set; }
        public string? Applicability { get; set; }
        public string? Examples { get; set; }
        public string? Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int RuleCategoryId { get; set; }
        public string? CategoryName { get; set; }
    }

    public class CreateTestingRuleDto
    {
        [Required]
        [MaxLength(50)]
        public string RuleId { get; set; } = "";

        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string RuleCategory { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string RuleType { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        [MaxLength(1000)]
        public string? DescriptionVi { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? RuleTemplate { get; set; }

        public string? Applicability { get; set; }

        public string? Examples { get; set; }

        [MaxLength(20)]
        public string? Version { get; set; }

        [Required]
        public int RuleCategoryId { get; set; }
    }

    public class UpdateTestingRuleDto
    {
        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string RuleCategory { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string RuleType { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        [MaxLength(1000)]
        public string? DescriptionVi { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? RuleTemplate { get; set; }

        public string? Applicability { get; set; }

        public string? Examples { get; set; }

        [MaxLength(20)]
        public string? Version { get; set; }

        [Required]
        public int RuleCategoryId { get; set; }
    }

    public class RuleCategoryDto
    {
        public int Id { get; set; }
        public string CategoryId { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string? Description { get; set; }
        public string? ISTQBReference { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int RuleCount { get; set; }
    }

    public class CreateRuleCategoryDto
    {
        [Required]
        [MaxLength(50)]
        public string CategoryId { get; set; } = "";

        [Required]
        [MaxLength(200)]
        public string CategoryName { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? ISTQBReference { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class RuleApplicationDto
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = "";
        public int TestingRuleId { get; set; }
        public string? RuleName { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? ScreenId { get; set; }
        public string? ScreenName { get; set; }
        public bool WasSuccessful { get; set; }
        public string? AppliedContext { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? UserId { get; set; }
        public string? Notes { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ApplyRulesRequestDto
    {
        [Required]
        public int ScreenId { get; set; }

        [MaxLength(1000)]
        public string? BusinessDescription { get; set; }

        public List<string>? SpecificRuleIds { get; set; }

        public string? RuleCategory { get; set; }

        public bool ForceReapply { get; set; } = false;
    }

    public class ApplyRulesResultDto
    {
        public string SessionId { get; set; } = "";
        public int ScreenId { get; set; }
        public string? ScreenName { get; set; }
        public List<RuleApplicationDto> AppliedRules { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalRulesApplied { get; set; }
        public int SuccessfulApplications { get; set; }
        public int FailedApplications { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<ChecklistItemDto> GeneratedChecklistItems { get; set; } = new();
    }
}