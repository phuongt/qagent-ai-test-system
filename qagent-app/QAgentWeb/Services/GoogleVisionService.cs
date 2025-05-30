namespace QAgentWeb.Services
{
    public class GoogleVisionService : IGoogleVisionService
    {
        private readonly ILogger<GoogleVisionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOpenAIService _openAIService;

        public GoogleVisionService(
            ILogger<GoogleVisionService> logger, 
            IConfiguration configuration,
            IOpenAIService openAIService)
        {
            _logger = logger;
            _configuration = configuration;
            _openAIService = openAIService;
        }

        public async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            try
            {
                // For now, use OpenAI Vision as fallback since Google Vision package is not installed
                var prompt = "Extract all visible text from this image. Return only the text content, line by line.";
                var base64Image = await ConvertImageToBase64(imagePath);
                var message = $"{prompt}\n\nImage: data:image/jpeg;base64,{base64Image}";
                return await _openAIService.ChatCompletionAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image {ImagePath}", imagePath);
                return string.Empty;
            }
        }

        public Task<List<string>> DetectLabelsAsync(string imagePath)
        {
            try
            {
                // Mock implementation - return some basic UI labels
                return Task.FromResult(new List<string> { "UI", "Interface", "Screen", "Form", "Button", "Input" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting labels from image {ImagePath}", imagePath);
                return Task.FromResult(new List<string>());
            }
        }

        public Task<List<ObjectAnnotation>> DetectObjectsAsync(string imagePath)
        {
            try
            {
                // Mock implementation - return some basic UI objects
                var result = new List<ObjectAnnotation>
                {
                    new ObjectAnnotation
                    {
                        Name = "Button",
                        Score = 0.9,
                        BoundingPoly = new BoundingBox { X = 100, Y = 100, Width = 80, Height = 30 }
                    },
                    new ObjectAnnotation
                    {
                        Name = "Input Field",
                        Score = 0.85,
                        BoundingPoly = new BoundingBox { X = 50, Y = 50, Width = 200, Height = 25 }
                    }
                };
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting objects from image {ImagePath}", imagePath);
                return Task.FromResult(new List<ObjectAnnotation>());
            }
        }

        public async Task<GoogleVisionResult> AnalyzeImageAsync(string imageUrl)
        {
            try
            {
                var extractedText = await ExtractTextFromImageAsync(imageUrl);
                return new GoogleVisionResult
                {
                    Success = true,
                    ExtractedText = extractedText,
                    Confidence = 0.85
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image {ImageUrl}", imageUrl);
                return new GoogleVisionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GoogleVisionResult> AnalyzeImageAsync(byte[] imageData)
        {
            try
            {
                // Save temp file and analyze
                var tempPath = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tempPath, imageData);
                
                var result = await AnalyzeImageAsync(tempPath);
                
                // Clean up temp file
                File.Delete(tempPath);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image data");
                return new GoogleVisionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                // Check if OpenAI service is available as fallback
                return await _openAIService.IsServiceAvailableAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Vision service is not available");
                return false;
            }
        }

        private async Task<string> ConvertImageToBase64(string imagePath)
        {
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            return Convert.ToBase64String(imageBytes);
        }
    }
} 