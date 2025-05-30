using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QAgentWeb.Models
{
    public class AnalysisLog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string FunctionId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AnalysisStatus { get; set; } = string.Empty; // processing|completed|failed

        [StringLength(100)]
        public string AIModelUsed { get; set; } = string.Empty;

        public double ProcessingTime { get; set; } // in seconds

        [Range(0, 1)]
        public double ConfidenceScore { get; set; }

        [StringLength(1000)]
        public string ErrorMessage { get; set; } = string.Empty;

        public string RawAIResponse { get; set; } = string.Empty; // JSON string

        // Navigation properties
        public string? StandardizedScreenId { get; set; }
        public virtual StandardizedScreen? StandardizedScreen { get; set; }

        // Helper methods
        public T? GetRawAIResponseAsObject<T>() where T : class
        {
            if (string.IsNullOrEmpty(RawAIResponse))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(RawAIResponse);
            }
            catch
            {
                return null;
            }
        }

        public void SetRawAIResponse<T>(T obj) where T : class
        {
            RawAIResponse = JsonSerializer.Serialize(obj);
        }
    }
} 