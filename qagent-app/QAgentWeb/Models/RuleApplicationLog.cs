using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAgentWeb.Models
{
    public class RuleApplicationLog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = string.Empty;
        
        // Foreign keys
        public int? ProjectId { get; set; }
        public int? ScreenId { get; set; }
        public int TestingRuleId { get; set; }
        
        [StringLength(50)]
        public string? UserId { get; set; }
        
        // Application context (JSON format)
        public string? AppliedContext { get; set; }
        
        public int GeneratedChecklistItems { get; set; } = 0;
        
        [Range(0.0, 10.0)]
        public double? EffectivenessScore { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public bool WasSuccessful { get; set; } = true;
        
        [StringLength(500)]
        public string? ErrorMessage { get; set; }
        
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
        
        [ForeignKey("ScreenId")]
        public virtual Screen? Screen { get; set; }
        
        [ForeignKey("TestingRuleId")]
        public virtual TestingRule TestingRule { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        // Business logic methods
        public void MarkAsSuccessful(int checklistItems, double effectiveness, string? context = null)
        {
            WasSuccessful = true;
            GeneratedChecklistItems = checklistItems;
            EffectivenessScore = effectiveness;
            AppliedContext = context;
            ErrorMessage = null;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void MarkAsFailed(string errorMessage)
        {
            WasSuccessful = false;
            ErrorMessage = errorMessage;
            EffectivenessScore = 0;
            GeneratedChecklistItems = 0;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public string GetEffectivenessDisplayText()
        {
            if (!EffectivenessScore.HasValue) return "N/A";
            
            return EffectivenessScore.Value switch
            {
                >= 8.0 => "Excellent",
                >= 6.0 => "Good",
                >= 4.0 => "Fair",
                >= 2.0 => "Poor",
                _ => "Very Poor"
            };
        }
        
        public string GetEffectivenessBadgeClass()
        {
            if (!EffectivenessScore.HasValue) return "bg-gray-100 text-gray-800";
            
            return EffectivenessScore.Value switch
            {
                >= 8.0 => "bg-green-100 text-green-800",
                >= 6.0 => "bg-blue-100 text-blue-800", 
                >= 4.0 => "bg-yellow-100 text-yellow-800",
                >= 2.0 => "bg-orange-100 text-orange-800",
                _ => "bg-red-100 text-red-800"
            };
        }
        
        public bool IsRecentApplication()
        {
            return AppliedAt > DateTime.UtcNow.AddHours(-24);
        }
    }
    
    // Helper class for JSON serialization of application context
    public class ApplicationContext
    {
        public string? ScreenType { get; set; }
        public List<string>? UIElements { get; set; }
        public List<string>? BusinessFunctions { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
} 