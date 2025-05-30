namespace QAgentWeb.Services
{
    public interface IGoogleVisionService
    {
        Task<string> ExtractTextFromImageAsync(string imagePath);
        Task<List<string>> DetectLabelsAsync(string imagePath);
        Task<List<ObjectAnnotation>> DetectObjectsAsync(string imagePath);
        Task<GoogleVisionResult> AnalyzeImageAsync(string imageUrl);
        Task<GoogleVisionResult> AnalyzeImageAsync(byte[] imageData);
        Task<bool> IsServiceAvailableAsync();
    }

    public class GoogleVisionResult
    {
        public bool Success { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ObjectAnnotation
    {
        public string Name { get; set; } = string.Empty;
        public double Score { get; set; }
        public BoundingBox BoundingPoly { get; set; } = new();
    }

    public class BoundingBox
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
} 