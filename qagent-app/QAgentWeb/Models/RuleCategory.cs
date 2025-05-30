using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models
{
    public class RuleCategory : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string CategoryId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string CategoryName { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ISTQBReference { get; set; }
        
        public int SortOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<TestingRule> TestingRules { get; set; } = new List<TestingRule>();
        
        // ISTQB Categories constants
        public static class ISTQBCategories
        {
            public const string FunctionalSuitability = "functional_suitability";
            public const string PerformanceEfficiency = "performance_efficiency";
            public const string Usability = "usability";
            public const string Reliability = "reliability";
            public const string Security = "security";
            public const string Maintainability = "maintainability";
            public const string Portability = "portability";
            
            public static List<string> GetAll() => new() 
            { 
                FunctionalSuitability, PerformanceEfficiency, Usability,
                Reliability, Security, Maintainability, Portability
            };
        }
        
        // Test levels constants
        public static class TestLevels
        {
            public const string Unit = "unit";
            public const string Integration = "integration";
            public const string System = "system";
            public const string Acceptance = "acceptance";
            
            public static List<string> GetAll() => new() { Unit, Integration, System, Acceptance };
        }
        
        // Test types constants
        public static class TestTypes
        {
            public const string Functional = "functional";
            public const string NonFunctional = "non_functional";
            public const string Structural = "structural";
            public const string ChangeRelated = "change_related";
            
            public static List<string> GetAll() => new() { Functional, NonFunctional, Structural, ChangeRelated };
        }
        
        // Business logic methods
        public int GetActiveRulesCount()
        {
            return TestingRules?.Count(r => r.IsActive && !r.IsDeleted) ?? 0;
        }
        
        public string GetDisplayName()
        {
            return CategoryId switch
            {
                TestingRule.Categories.EquivalencePartitioning => "Equivalence Partitioning",
                TestingRule.Categories.BoundaryValueAnalysis => "Boundary Value Analysis",
                TestingRule.Categories.DecisionTable => "Decision Table Testing",
                TestingRule.Categories.StateTransition => "State Transition Testing",
                TestingRule.Categories.ErrorGuessing => "Error Guessing",
                TestingRule.Categories.UseCase => "Use Case Testing",
                _ => CategoryName
            };
        }
    }
} 