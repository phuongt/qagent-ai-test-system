using QAgentWeb.Models;
using System.Text.Json;

namespace QAgentWeb.Services
{
    public interface IUIElementDetectionService
    {
        Task<ElementDetectionResult> DetectUIElementsAsync(string imageUrl, string extractedText);
        Task<List<UIElement>> ClassifyElementsAsync(List<DetectedElement> rawElements);
        Task<double> CalculateConfidenceScore(List<UIElement> elements);
    }

    public class UIElementDetectionService : IUIElementDetectionService
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<UIElementDetectionService> _logger;
        private readonly IConfiguration _configuration;

        public UIElementDetectionService(
            IOpenAIService openAIService,
            ILogger<UIElementDetectionService> logger,
            IConfiguration configuration)
        {
            _openAIService = openAIService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ElementDetectionResult> DetectUIElementsAsync(string imageUrl, string extractedText)
        {
            try
            {
                _logger.LogInformation("Starting UI element detection for image: {ImageUrl}", imageUrl);

                var prompt = GenerateElementDetectionPrompt(extractedText);
                var response = await _openAIService.AnalyzeImageAsync(imageUrl, prompt);

                if (!response.Success)
                {
                    return new ElementDetectionResult
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage
                    };
                }

                var detectedElements = ParseDetectionResponse(response.Content);
                var classifiedElements = await ClassifyElementsAsync(detectedElements);
                var confidenceScore = await CalculateConfidenceScore(classifiedElements);

                return new ElementDetectionResult
                {
                    Success = true,
                    DetectedElements = classifiedElements,
                    ConfidenceScore = confidenceScore,
                    ProcessingTime = TimeSpan.Zero // Will be set by caller
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UI element detection");
                return new ElementDetectionResult
                {
                    Success = false,
                    ErrorMessage = $"Element detection failed: {ex.Message}"
                };
            }
        }

        public async Task<List<UIElement>> ClassifyElementsAsync(List<DetectedElement> rawElements)
        {
            var classifiedElements = new List<UIElement>();

            foreach (var element in rawElements)
            {
                var uiElement = new UIElement
                {
                    ElementId = Guid.NewGuid().ToString(),
                    ElementType = ClassifyElementType(element.Type, element.Text),
                    ElementName = GenerateElementName(element.Text, element.Type),
                    ElementLabel = element.Text,
                    IsRequired = DetermineIfRequired(element.Text),
                    ValidationRules = GenerateValidationRules(element.Type, element.Text),
                    Position = element.Position
                };

                classifiedElements.Add(uiElement);
            }

            return await Task.FromResult(classifiedElements);
        }

        public async Task<double> CalculateConfidenceScore(List<UIElement> elements)
        {
            if (!elements.Any())
                return 0.0;

            double totalScore = 0;
            foreach (var element in elements)
            {
                double elementScore = 0.8; // Base confidence

                // Increase confidence for well-labeled elements
                if (!string.IsNullOrEmpty(element.ElementLabel))
                    elementScore += 0.1;

                // Increase confidence for positioned elements
                if (element.Position != null)
                    elementScore += 0.1;

                totalScore += Math.Min(elementScore, 1.0);
            }

            return await Task.FromResult(Math.Min(totalScore / elements.Count, 1.0));
        }

        private string GenerateElementDetectionPrompt(string extractedText)
        {
            return $@"
Analyze this UI screenshot and extract all UI elements. Pay attention to:
1. Form inputs (text fields, dropdowns, checkboxes, radio buttons)
2. Buttons (submit, cancel, action buttons)
3. Tables and grids
4. Labels and text displays
5. Navigation elements

Extracted text from OCR: {extractedText}

Please respond with a JSON array of detected elements in this format:
[
  {{
    ""type"": ""input|button|dropdown|checkbox|radio|table|label"",
    ""text"": ""visible text or label"",
    ""position"": {{""x"": 0, ""y"": 0, ""width"": 0, ""height"": 0}}
  }}
]

Focus on interactive elements and their relationships. Use Vietnamese labels when appropriate.
";
        }

        private List<DetectedElement> ParseDetectionResponse(string response)
        {
            try
            {
                var elements = JsonSerializer.Deserialize<List<DetectedElement>>(response) ?? new List<DetectedElement>();
                return elements;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse detection response, returning empty list");
                return new List<DetectedElement>();
            }
        }

        private string ClassifyElementType(string rawType, string text)
        {
            var lowerType = rawType.ToLowerInvariant();
            var lowerText = text.ToLowerInvariant();

            // Map raw types to standardized types
            return lowerType switch
            {
                "input" or "textbox" or "text_field" => "input",
                "button" or "btn" => "button",
                "dropdown" or "select" or "combobox" => "dropdown",
                "checkbox" or "check" => "checkbox",
                "radio" or "radiobutton" => "radio",
                "table" or "grid" or "datagrid" => "table",
                "label" or "text" or "static" => "label",
                _ => DetermineTypeFromText(lowerText)
            };
        }

        private string DetermineTypeFromText(string text)
        {
            if (text.Contains("lưu") || text.Contains("save") || text.Contains("submit"))
                return "button";
            
            if (text.Contains("chọn") || text.Contains("select"))
                return "dropdown";
            
            if (text.Contains("nhập") || text.Contains("input"))
                return "input";

            return "label";
        }

        private string GenerateElementName(string text, string type)
        {
            if (string.IsNullOrEmpty(text))
                return $"{type}_{Guid.NewGuid().ToString()[..8]}";

            // Convert Vietnamese text to camelCase variable name
            var name = text.ToLowerInvariant()
                          .Replace(" ", "_")
                          .Replace("ă", "a")
                          .Replace("â", "a")
                          .Replace("đ", "d")
                          .Replace("ê", "e")
                          .Replace("ô", "o")
                          .Replace("ơ", "o")
                          .Replace("ư", "u");

            return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-z0-9_]", "");
        }

        private bool DetermineIfRequired(string text)
        {
            var lowerText = text.ToLowerInvariant();
            return lowerText.Contains("*") || 
                   lowerText.Contains("bắt buộc") || 
                   lowerText.Contains("required") ||
                   lowerText.Contains("tên") ||
                   lowerText.Contains("mã");
        }

        private List<string> GenerateValidationRules(string type, string text)
        {
            var rules = new List<string>();

            if (DetermineIfRequired(text))
                rules.Add("required");

            switch (type.ToLowerInvariant())
            {
                case "input":
                    if (text.ToLowerInvariant().Contains("email"))
                        rules.Add("email");
                    else if (text.ToLowerInvariant().Contains("số") || text.ToLowerInvariant().Contains("number"))
                        rules.Add("numeric");
                    else
                        rules.Add("max_length:255");
                    break;
            }

            return rules;
        }
    }

    public class ElementDetectionResult
    {
        public bool Success { get; set; }
        public List<UIElement> DetectedElements { get; set; } = new List<UIElement>();
        public double ConfidenceScore { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ScreenType { get; set; } = string.Empty;
    }

    public class DetectedElement
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public ElementPosition? Position { get; set; }
    }
} 