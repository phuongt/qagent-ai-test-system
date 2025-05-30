using QAgentWeb.Models;

namespace QAgentWeb.Services
{
    public interface ITextExtractionService
    {
        Task<TextExtractionResult> ExtractTextFromImageAsync(string imageUrl);
        Task<TextExtractionResult> ExtractTextFromImageAsync(byte[] imageData);
        Task<bool> IsImageSupportedAsync(string fileName);
    }

    public class TextExtractionService : ITextExtractionService
    {
        private readonly IGoogleVisionService _googleVisionService;
        private readonly IGoogleGeminiService _googleGeminiService;
        private readonly ILogger<TextExtractionService> _logger;
        private readonly IConfiguration _configuration;

        public TextExtractionService(
            IGoogleVisionService googleVisionService,
            IGoogleGeminiService googleGeminiService,
            ILogger<TextExtractionService> logger,
            IConfiguration configuration)
        {
            _googleVisionService = googleVisionService;
            _googleGeminiService = googleGeminiService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<TextExtractionResult> ExtractTextFromImageAsync(string imageUrl)
        {
            try
            {
                _logger.LogInformation("Starting text extraction from image URL: {ImageUrl}", imageUrl);

                // Check preferred AI service
                var preferredService = _configuration["UIAnalysis:PreferredAIService"];
                
                TextExtractionResult extractionResult;

                if (preferredService == "GoogleGemini")
                {
                    _logger.LogInformation("Using Google Gemini for text extraction");
                    extractionResult = await _googleGeminiService.ExtractTextFromImageAsync(imageUrl);
                }
                else
                {
                    _logger.LogInformation("Using Google Vision for text extraction");
                    var result = await _googleVisionService.AnalyzeImageAsync(imageUrl);
                    
                    extractionResult = new TextExtractionResult
                    {
                        Success = result.Success,
                        ExtractedText = result.ExtractedText,
                        ConfidenceScore = result.Confidence,
                        LanguageDetected = "vi", // Default Vietnamese
                        ProcessingTime = TimeSpan.Zero, // Will be set by caller
                        ErrorMessage = result.ErrorMessage
                    };
                }

                _logger.LogInformation("Text extraction completed. Success: {Success}, Text length: {Length}", 
                    extractionResult.Success, extractionResult.ExtractedText?.Length ?? 0);

                return extractionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during text extraction from image URL: {ImageUrl}", imageUrl);
                return new TextExtractionResult
                {
                    Success = false,
                    ErrorMessage = $"Text extraction failed: {ex.Message}"
                };
            }
        }

        public async Task<TextExtractionResult> ExtractTextFromImageAsync(byte[] imageData)
        {
            try
            {
                _logger.LogInformation("Starting text extraction from image data. Size: {Size} bytes", imageData.Length);

                // Check preferred AI service
                var preferredService = _configuration["UIAnalysis:PreferredAIService"];
                
                TextExtractionResult extractionResult;

                if (preferredService == "GoogleGemini")
                {
                    _logger.LogInformation("Using Google Gemini for text extraction");
                    // Convert byte array to base64 string for Gemini API
                    var base64Image = Convert.ToBase64String(imageData);
                    var tempImagePath = $"data:image/jpeg;base64,{base64Image}";
                    extractionResult = await _googleGeminiService.ExtractTextFromImageAsync(tempImagePath);
                }
                else
                {
                    _logger.LogInformation("Using Google Vision for text extraction");
                    var result = await _googleVisionService.AnalyzeImageAsync(imageData);
                    
                    extractionResult = new TextExtractionResult
                    {
                        Success = result.Success,
                        ExtractedText = result.ExtractedText,
                        ConfidenceScore = result.Confidence,
                        LanguageDetected = "vi", // Default Vietnamese
                        ProcessingTime = TimeSpan.Zero, // Will be set by caller
                        ErrorMessage = result.ErrorMessage
                    };
                }

                _logger.LogInformation("Text extraction completed. Success: {Success}, Text length: {Length}", 
                    extractionResult.Success, extractionResult.ExtractedText?.Length ?? 0);

                return extractionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during text extraction from image data");
                return new TextExtractionResult
                {
                    Success = false,
                    ErrorMessage = $"Text extraction failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> IsImageSupportedAsync(string fileName)
        {
            var supportedFormats = _configuration.GetSection("UIAnalysis:SupportedFormats").Get<string[]>() 
                                   ?? new[] { "jpg", "jpeg", "png", "bmp" };

            var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            return await Task.FromResult(supportedFormats.Contains(extension));
        }
    }

    public class TextExtractionResult
    {
        public bool Success { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string LanguageDetected { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        // Backward compatibility properties
        public double Confidence => ConfidenceScore;
    }
} 