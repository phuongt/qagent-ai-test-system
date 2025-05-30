using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAgentWeb.Models
{
    public class TestingRule : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string RuleId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string RuleName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string RuleCategory { get; set; } = string.Empty; // EP, BVA, DecisionTable, StateTransition, ErrorGuessing, UseCase
        
        [Required]
        [StringLength(50)]
        public string RuleType { get; set; } = string.Empty; // input_validation, boundary_check, logic_flow, error_handling
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string DescriptionVi { get; set; } = string.Empty;
        
        // JSON field for applicability configuration
        public string? Applicability { get; set; }
        
        [StringLength(2000)]
        public string RuleTemplate { get; set; } = string.Empty;
        
        // JSON field for examples
        public string? Examples { get; set; }
        
        [Range(1, 10)]
        public int Priority { get; set; } = 5;
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(20)]
        public string Version { get; set; } = "1.0";
        
        // Foreign key to RuleCategory
        public int RuleCategoryId { get; set; }
        [ForeignKey("RuleCategoryId")]
        public virtual RuleCategory Category { get; set; } = null!;
        
        // Navigation properties
        public virtual ICollection<RuleApplicationLog> ApplicationLogs { get; set; } = new List<RuleApplicationLog>();
        
        // Rule categories constants
        public static class Categories
        {
            public const string EquivalencePartitioning = "EP";
            public const string BoundaryValueAnalysis = "BVA";
            public const string DecisionTable = "DecisionTable";
            public const string StateTransition = "StateTransition";
            public const string ErrorGuessing = "ErrorGuessing";
            public const string UseCase = "UseCase";
            
            public static List<string> GetAll() => new() 
            { 
                EquivalencePartitioning, BoundaryValueAnalysis, DecisionTable, 
                StateTransition, ErrorGuessing, UseCase 
            };
        }
        
        // Rule types constants
        public static class Types
        {
            public const string InputValidation = "input_validation";
            public const string BoundaryCheck = "boundary_check";
            public const string LogicFlow = "logic_flow";
            public const string ErrorHandling = "error_handling";
            
            public static List<string> GetAll() => new() 
            { 
                InputValidation, BoundaryCheck, LogicFlow, ErrorHandling 
            };
        }
        
        // Business logic methods
        public bool IsApplicableToScreenType(string screenType)
        {
            if (string.IsNullOrEmpty(Applicability)) return true;
            
            try
            {
                var config = System.Text.Json.JsonSerializer.Deserialize<ApplicabilityConfig>(Applicability);
                return config?.ScreenTypes?.Contains(screenType) == true;
            }
            catch
            {
                return true; // Default to true if JSON parsing fails
            }
        }
        
        public bool IsApplicableToUIElement(string elementType)
        {
            if (string.IsNullOrEmpty(Applicability)) return true;
            
            try
            {
                var config = System.Text.Json.JsonSerializer.Deserialize<ApplicabilityConfig>(Applicability);
                return config?.UIElementTypes?.Contains(elementType) == true;
            }
            catch
            {
                return true;
            }
        }
        
        public string GetCategoryDisplayName()
        {
            return RuleCategory switch
            {
                Categories.EquivalencePartitioning => "Equivalence Partitioning",
                Categories.BoundaryValueAnalysis => "Boundary Value Analysis",
                Categories.DecisionTable => "Decision Table Testing",
                Categories.StateTransition => "State Transition Testing",
                Categories.ErrorGuessing => "Error Guessing",
                Categories.UseCase => "Use Case Testing",
                _ => RuleCategory
            };
        }
        
        public string GetPriorityBadgeClass()
        {
            return Priority switch
            {
                >= 8 => "bg-red-100 text-red-800",
                >= 6 => "bg-orange-100 text-orange-800",
                >= 4 => "bg-yellow-100 text-yellow-800",
                _ => "bg-green-100 text-green-800"
            };
        }
    }
    
    // Helper class for JSON serialization
    public class ApplicabilityConfig
    {
        public List<string>? UIElementTypes { get; set; }
        public List<string>? DataTypes { get; set; }
        public List<string>? BusinessContexts { get; set; }
        public List<string>? ScreenTypes { get; set; }
    }
    
    // Example class for JSON serialization
    public class RuleExample
    {
        public string Scenario { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
    }
} 