using QAgentWeb.Models;
using System.Text.Json;

namespace QAgentWeb.Services
{
    public interface IScreenStandardizationService
    {
        Task<StandardizationResult> StandardizeScreenAsync(string functionId, List<UIElement> elements, string businessDescription, List<string> sourceImages);
        Task<BusinessFunction> InferBusinessFunctionAsync(List<UIElement> elements, string description);
        Task<List<Workflow>> InferWorkflowsAsync(List<UIElement> elements, string description);
        Task<string> DetermineScreenTypeAsync(List<UIElement> elements);
    }

    public class ScreenStandardizationService : IScreenStandardizationService
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<ScreenStandardizationService> _logger;
        private readonly IConfiguration _configuration;

        public ScreenStandardizationService(
            IOpenAIService openAIService,
            ILogger<ScreenStandardizationService> logger,
            IConfiguration configuration)
        {
            _openAIService = openAIService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<StandardizationResult> StandardizeScreenAsync(string functionId, List<UIElement> elements, string businessDescription, List<string> sourceImages)
        {
            try
            {
                _logger.LogInformation("Starting screen standardization for function: {FunctionId}", functionId);

                var screenType = await DetermineScreenTypeAsync(elements);
                var businessFunction = await InferBusinessFunctionAsync(elements, businessDescription);
                var workflows = await InferWorkflowsAsync(elements, businessDescription);
                var screenName = await GenerateScreenNameAsync(elements, businessDescription);

                var standardizedScreen = new StandardizedScreen
                {
                    ScreenId = GenerateScreenId(),
                    FunctionId = functionId,
                    ScreenName = screenName,
                    ScreenType = screenType,
                    Description = businessDescription,
                    ConfidenceScore = CalculateOverallConfidence(elements, businessFunction, workflows),
                    UIElements = elements,
                    BusinessFunctions = new List<BusinessFunction> { businessFunction },
                    Workflows = workflows,
                    SourceImages = sourceImages,
                    CreatedAt = DateTime.UtcNow
                };

                return new StandardizationResult
                {
                    Success = true,
                    StandardizedScreen = standardizedScreen,
                    ProcessingTime = 0 // Will be set by caller
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during screen standardization");
                return new StandardizationResult
                {
                    Success = false,
                    ErrorMessage = $"Standardization failed: {ex.Message}"
                };
            }
        }

        public async Task<BusinessFunction> InferBusinessFunctionAsync(List<UIElement> elements, string description)
        {
            var prompt = $@"
Analyze these UI elements and business description to determine the main business function:

UI Elements:
{JsonSerializer.Serialize(elements.Select(e => new { e.ElementType, e.ElementLabel, e.IsRequired }), new JsonSerializerOptions { WriteIndented = true })}

Business Description: {description}

Please determine:
1. Function name (in Vietnamese)
2. Function type (create|read|update|delete|search|export)
3. Function description
4. Which elements are related to this function

Respond in JSON format:
{{
  ""function_name"": ""string"",
  ""function_type"": ""create|read|update|delete|search|export"",
  ""description"": ""string"",
  ""related_elements"": [""element_id1"", ""element_id2""]
}}
";

            try
            {
                var response = await _openAIService.GenerateTextAsync(prompt);
                if (response.Success)
                {
                    var businessFunction = JsonSerializer.Deserialize<BusinessFunction>(response.Content);
                    if (businessFunction != null)
                    {
                        businessFunction.RelatedElements = elements.Select(e => e.ElementId).ToList();
                        return businessFunction;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to infer business function from AI, using fallback");
            }

            // Fallback business function
            return new BusinessFunction
            {
                FunctionName = "Chức năng nghiệp vụ",
                FunctionType = DetermineFunctionTypeFromElements(elements),
                Description = description ?? "Chức năng nghiệp vụ từ phân tích UI",
                RelatedElements = elements.Select(e => e.ElementId).ToList()
            };
        }

        public async Task<List<Workflow>> InferWorkflowsAsync(List<UIElement> elements, string description)
        {
            var workflows = new List<Workflow>();

            try
            {
                var prompt = $@"
Analyze these UI elements to determine the workflow steps:

UI Elements:
{JsonSerializer.Serialize(elements.Select(e => new { e.ElementType, e.ElementLabel, e.IsRequired }), new JsonSerializerOptions { WriteIndented = true })}

Business Description: {description}

Please identify the main workflow and its steps based on the UI elements.
Consider the logical order of user interactions.

Respond in JSON format:
{{
  ""workflow_name"": ""string"",
  ""steps"": [""step1"", ""step2"", ""step3""],
  ""conditions"": [""condition1"", ""condition2""]
}}
";

                var response = await _openAIService.GenerateTextAsync(prompt);
                if (response.Success)
                {
                    var workflow = JsonSerializer.Deserialize<Workflow>(response.Content);
                    if (workflow != null)
                    {
                        workflows.Add(workflow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to infer workflows from AI, using fallback");
            }

            // Fallback workflow if AI fails
            if (!workflows.Any())
            {
                workflows.Add(new Workflow
                {
                    WorkflowName = "Quy trình cơ bản",
                    Steps = GenerateBasicWorkflowSteps(elements),
                    Conditions = new List<string> { "Người dùng đã đăng nhập", "Có quyền truy cập chức năng" }
                });
            }

            return workflows;
        }

        public async Task<string> DetermineScreenTypeAsync(List<UIElement> elements)
        {
            var inputElements = elements.Count(e => e.ElementType == "input");
            var buttonElements = elements.Count(e => e.ElementType == "button");
            var tableElements = elements.Count(e => e.ElementType == "table");

            // Logic to determine screen type based on element counts and types
            if (tableElements > 0 && inputElements <= 2)
                return "grid";
            
            if (inputElements > 3)
                return "form";
            
            if (inputElements <= 2 && buttonElements >= 1)
                return "search";

            if (elements.Any(e => e.ElementLabel.ToLowerInvariant().Contains("dashboard") || 
                                 e.ElementLabel.ToLowerInvariant().Contains("tổng quan")))
                return "dashboard";

            if (elements.Any(e => e.ElementLabel.ToLowerInvariant().Contains("báo cáo") || 
                                 e.ElementLabel.ToLowerInvariant().Contains("report")))
                return "report";

            if (elements.Count > 5)
                return "workflow";

            return await Task.FromResult("form");
        }

        private async Task<string> GenerateScreenNameAsync(List<UIElement> elements, string description)
        {
            // Try to generate a meaningful name from description first
            if (!string.IsNullOrEmpty(description))
            {
                var firstSentence = description.Split('.').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(firstSentence) && firstSentence.Length <= 200)
                {
                    return firstSentence;
                }
            }

            // Generate name from UI elements
            var buttonElement = elements.FirstOrDefault(e => e.ElementType == "button");
            if (buttonElement != null && !string.IsNullOrEmpty(buttonElement.ElementLabel))
            {
                return $"Màn hình {buttonElement.ElementLabel}";
            }

            // Fallback
            return await Task.FromResult("Màn hình chức năng nghiệp vụ");
        }

        private string GenerateScreenId()
        {
            return $"scr_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}";
        }

        private double CalculateOverallConfidence(List<UIElement> elements, BusinessFunction businessFunction, List<Workflow> workflows)
        {
            double elementConfidence = elements.Any() ? 0.8 : 0.3;
            double functionConfidence = !string.IsNullOrEmpty(businessFunction.FunctionName) ? 0.9 : 0.5;
            double workflowConfidence = workflows.Any() ? 0.8 : 0.6;

            return (elementConfidence + functionConfidence + workflowConfidence) / 3.0;
        }

        private string DetermineFunctionTypeFromElements(List<UIElement> elements)
        {
            var hasSubmitButton = elements.Any(e => e.ElementType == "button" && 
                (e.ElementLabel.ToLowerInvariant().Contains("lưu") || 
                 e.ElementLabel.ToLowerInvariant().Contains("thêm") ||
                 e.ElementLabel.ToLowerInvariant().Contains("tạo")));

            var hasSearchElements = elements.Any(e => e.ElementLabel.ToLowerInvariant().Contains("tìm kiếm") ||
                                                     e.ElementLabel.ToLowerInvariant().Contains("search"));

            var hasDeleteButton = elements.Any(e => e.ElementType == "button" && 
                (e.ElementLabel.ToLowerInvariant().Contains("xóa") || 
                 e.ElementLabel.ToLowerInvariant().Contains("delete")));

            var hasEditElements = elements.Any(e => e.ElementLabel.ToLowerInvariant().Contains("sửa") ||
                                               e.ElementLabel.ToLowerInvariant().Contains("edit"));

            if (hasSubmitButton) return "create";
            if (hasDeleteButton) return "delete";
            if (hasEditElements) return "update";
            if (hasSearchElements) return "search";
            
            return "read";
        }

        private List<string> GenerateBasicWorkflowSteps(List<UIElement> elements)
        {
            var steps = new List<string> { "Truy cập màn hình" };

            if (elements.Any(e => e.ElementType == "input"))
            {
                steps.Add("Nhập thông tin vào các trường");
            }

            if (elements.Any(e => e.ElementType == "dropdown"))
            {
                steps.Add("Chọn giá trị từ dropdown");
            }

            if (elements.Any(e => e.ElementType == "button"))
            {
                steps.Add("Nhấn nút để thực hiện hành động");
            }

            steps.Add("Xác nhận kết quả");

            return steps;
        }
    }

    public class StandardizationResult
    {
        public bool Success { get; set; }
        public StandardizedScreen? StandardizedScreen { get; set; }
        public double ProcessingTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
} 