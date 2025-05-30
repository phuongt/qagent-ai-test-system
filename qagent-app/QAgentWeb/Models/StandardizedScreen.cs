using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QAgentWeb.Models
{
    public class StandardizedScreen : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ScreenId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FunctionId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ScreenName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ScreenType { get; set; } = string.Empty; // form|grid|search|dashboard|workflow|report

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1)]
        public double ConfidenceScore { get; set; }

        public List<UIElement> UIElements { get; set; } = new List<UIElement>();

        public List<BusinessFunction> BusinessFunctions { get; set; } = new List<BusinessFunction>();

        public List<Workflow> Workflows { get; set; } = new List<Workflow>();

        public List<string> SourceImages { get; set; } = new List<string>();

        // Navigation properties
        public virtual ICollection<AnalysisLog> AnalysisLogs { get; set; } = new List<AnalysisLog>();
    }

    public class UIElement
    {
        [Required]
        [StringLength(50)]
        public string ElementId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ElementType { get; set; } = string.Empty; // input|button|dropdown|checkbox|radio|table|label

        [Required]
        [StringLength(100)]
        public string ElementName { get; set; } = string.Empty;

        [StringLength(200)]
        public string ElementLabel { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public List<string> ValidationRules { get; set; } = new List<string>();

        public ElementPosition? Position { get; set; }
    }

    public class ElementPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class BusinessFunction
    {
        [Required]
        [StringLength(200)]
        public string FunctionName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string FunctionType { get; set; } = string.Empty; // create|read|update|delete|search|export

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public List<string> RelatedElements { get; set; } = new List<string>();
    }

    public class Workflow
    {
        [Required]
        [StringLength(200)]
        public string WorkflowName { get; set; } = string.Empty;

        public List<string> Steps { get; set; } = new List<string>();

        public List<string> Conditions { get; set; } = new List<string>();
    }
} 