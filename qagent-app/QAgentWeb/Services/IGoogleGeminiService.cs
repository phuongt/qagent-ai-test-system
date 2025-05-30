using QAgentWeb.Models;

namespace QAgentWeb.Services
{
    public interface IGoogleGeminiService
    {
        Task<bool> IsServiceAvailableAsync();
        Task<TextExtractionResult> ExtractTextFromImageAsync(string imagePath);
        Task<ElementDetectionResult> AnalyzeUIElementsAsync(string imagePath, string? businessDescription = null);
        Task<string> GenerateScreenDescriptionAsync(string imagePath, string? context = null);
        Task<List<BusinessFunction>> InferBusinessFunctionsAsync(string imagePath, List<UIElement> elements, string? description = null);
    }

    public class GeminiAnalysisResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public List<UIElement> UIElements { get; set; } = new List<UIElement>();
        public List<BusinessFunction> BusinessFunctions { get; set; } = new List<BusinessFunction>();
        public string ScreenType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
    }
} 