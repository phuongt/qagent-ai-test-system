using QAgentWeb.Models;

namespace QAgentWeb.Services
{
    public interface IAIAnalysisService
    {
        Task<AnalysisResult> AnalyzeScreenAsync(int screenId, string? businessDescription = null);
        Task<AnalysisResult> AnalyzeImageAsync(string imagePath, string? businessDescription = null);
        Task<List<UIElement>> DetectUIElementsAsync(string imagePath);
        Task<string> ExtractTextFromImageAsync(string imagePath);
        Task<StandardizedScreen> StandardizeScreenAsync(AnalysisResult analysisResult);
        Task<bool> IsServiceAvailableAsync();
    }

    public class AnalysisResult
    {
        public string ImagePath { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
        public List<UIElement> UIElements { get; set; } = new();
        public List<BusinessFunction> BusinessFunctions { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public int ComplexityScore { get; set; }
        public string ScreenType { get; set; } = "Other";
        public DateTime AnalyzedAt { get; set; }
        public string AIModelUsed { get; set; } = "GPT-4-Vision";
        public TimeSpan ProcessingTime { get; set; }
        public string? ErrorMessage { get; set; }
        public StandardizedScreen? StandardizedScreen { get; set; }
    }
} 