using QAgentWeb.Data;
using QAgentWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;

namespace QAgentWeb.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITextExtractionService _textExtractionService;
        private readonly IUIElementDetectionService _elementDetectionService;
        private readonly IScreenStandardizationService _standardizationService;
        private readonly ILogger<AIAnalysisService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public AIAnalysisService(
            ApplicationDbContext context,
            ITextExtractionService textExtractionService,
            IUIElementDetectionService elementDetectionService,
            IScreenStandardizationService standardizationService,
            ILogger<AIAnalysisService> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _context = context;
            _textExtractionService = textExtractionService;
            _elementDetectionService = elementDetectionService;
            _standardizationService = standardizationService;
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<AnalysisResult> AnalyzeScreenAsync(int screenId, string? businessDescription = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting analysis for screen ID: {ScreenId}", screenId);

                var screen = await _context.Screens
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == screenId);

                if (screen == null)
                {
                    throw new ArgumentException($"Screen with ID {screenId} not found");
                }

                // Create analysis log
                var analysisLog = new AnalysisLog
                {
                    FunctionId = screen.ProjectId.ToString(),
                    ImageUrl = screen.FilePath!,
                    AnalysisStatus = "processing",
                    AIModelUsed = "Combined AI Pipeline",
                    CreatedAt = DateTime.UtcNow
                };

                _context.AnalysisLogs.Add(analysisLog);

                // Update screen status to Processing
                screen.MarkAsProcessing();
                await _context.SaveChangesAsync();

                var result = await AnalyzeImageAsync(screen.FilePath!, businessDescription ?? screen.Description);
                
                // Create standardized screen
                if (result.StandardizedScreen != null)
                {
                    _context.StandardizedScreens.Add(result.StandardizedScreen);
                    analysisLog.StandardizedScreenId = result.StandardizedScreen.ScreenId;
                }

                // Update analysis log
                analysisLog.AnalysisStatus = result.ErrorMessage == null ? "completed" : "failed";
                analysisLog.ProcessingTime = stopwatch.Elapsed.TotalSeconds;
                analysisLog.ConfidenceScore = result.ConfidenceScore;
                analysisLog.ErrorMessage = result.ErrorMessage ?? string.Empty;
                analysisLog.SetRawAIResponse(result);

                // Update screen with analysis results
                screen.AnalysisResult = JsonSerializer.Serialize(result);
                screen.AnalysisConfidence = result.ConfidenceScore;
                screen.ComplexityScore = result.ComplexityScore;
                screen.ScreenType = result.ScreenType;
                screen.MarkAsCompleted(JsonSerializer.Serialize(result), result.ConfidenceScore, result.ComplexityScore);
                
                await _context.SaveChangesAsync();

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Analysis completed for screen ID: {ScreenId}, Duration: {Duration}ms", 
                    screenId, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing screen {ScreenId}", screenId);
                
                // Update screen status to Failed
                var screen = await _context.Screens.FindAsync(screenId);
                if (screen != null)
                {
                    screen.MarkAsFailed();
                    await _context.SaveChangesAsync();
                }

                stopwatch.Stop();
                return new AnalysisResult
                {
                    ErrorMessage = ex.Message,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<AnalysisResult> AnalyzeImageAsync(string imagePath, string? businessDescription = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting image analysis for: {ImagePath}", imagePath);

                // Step 1: Text Extraction
                var textExtractionResult = await _textExtractionService.ExtractTextFromImageAsync(imagePath);
                if (!textExtractionResult.Success)
                {
                    _logger.LogWarning("Text extraction failed: {Error}", textExtractionResult.ErrorMessage);
                }

                // Step 2: UI Element Detection
                var elementDetectionResult = await _elementDetectionService.DetectUIElementsAsync(imagePath, textExtractionResult.ExtractedText);
                if (!elementDetectionResult.Success)
                {
                    _logger.LogWarning("Element detection failed: {Error}", elementDetectionResult.ErrorMessage);
                    elementDetectionResult.DetectedElements = new List<UIElement>();
                }

                // Step 3: Screen Standardization
                var standardizationResult = await _standardizationService.StandardizeScreenAsync(
                    Guid.NewGuid().ToString(),
                    elementDetectionResult.DetectedElements,
                    businessDescription ?? "Phân tích từ UI",
                    new List<string> { imagePath }
                );

                if (!standardizationResult.Success)
                {
                    _logger.LogWarning("Screen standardization failed: {Error}", standardizationResult.ErrorMessage);
                }

                // Calculate final metrics
                var confidenceScore = CalculateFinalConfidenceScore(textExtractionResult, elementDetectionResult, standardizationResult);
                var complexityScore = CalculateComplexityScore(elementDetectionResult.DetectedElements);
                var screenType = standardizationResult.StandardizedScreen?.ScreenType ?? "unknown";

                stopwatch.Stop();

                var analysisResult = new AnalysisResult
                {
                    ImagePath = imagePath,
                    ExtractedText = textExtractionResult.ExtractedText,
                    UIElements = elementDetectionResult.DetectedElements,
                    BusinessFunctions = standardizationResult.StandardizedScreen?.BusinessFunctions ?? new List<BusinessFunction>(),
                    ConfidenceScore = confidenceScore,
                    ComplexityScore = complexityScore,
                    ScreenType = screenType,
                    AnalyzedAt = DateTime.UtcNow,
                    ProcessingTime = stopwatch.Elapsed,
                    StandardizedScreen = standardizationResult.StandardizedScreen
                };

                _logger.LogInformation("Image analysis completed for: {ImagePath}, Confidence: {Confidence}, Elements: {ElementCount}", 
                    imagePath, confidenceScore, elementDetectionResult.DetectedElements.Count);

                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image {ImagePath}", imagePath);
                stopwatch.Stop();
                
                return new AnalysisResult
                {
                    ImagePath = imagePath,
                    ErrorMessage = ex.Message,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<List<UIElement>> DetectUIElementsAsync(string imagePath)
        {
            try
            {
                var textResult = await _textExtractionService.ExtractTextFromImageAsync(imagePath);
                var detectionResult = await _elementDetectionService.DetectUIElementsAsync(imagePath, textResult.ExtractedText);
                
                return detectionResult.Success ? detectionResult.DetectedElements : new List<UIElement>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting UI elements from {ImagePath}", imagePath);
                return new List<UIElement>();
            }
        }

        public async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            try
            {
                var result = await _textExtractionService.ExtractTextFromImageAsync(imagePath);
                return result.Success ? result.ExtractedText : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from {ImagePath}", imagePath);
                return string.Empty;
            }
        }

        public async Task<StandardizedScreen> StandardizeScreenAsync(AnalysisResult analysisResult)
        {
            try
            {
                if (analysisResult.StandardizedScreen != null)
                {
                    return analysisResult.StandardizedScreen;
                }

                var result = await _standardizationService.StandardizeScreenAsync(
                    Guid.NewGuid().ToString(),
                    analysisResult.UIElements,
                    "Generated from analysis result",
                    new List<string> { analysisResult.ImagePath }
                );

                return result.StandardizedScreen ?? new StandardizedScreen
                {
                    ScreenId = Guid.NewGuid().ToString(),
                    FunctionId = Guid.NewGuid().ToString(),
                    ScreenName = "Fallback Screen",
                    ScreenType = analysisResult.ScreenType,
                    Description = "Generated fallback screen",
                    ConfidenceScore = analysisResult.ConfidenceScore,
                    UIElements = analysisResult.UIElements,
                    BusinessFunctions = analysisResult.BusinessFunctions,
                    Workflows = new List<Workflow>(),
                    SourceImages = new List<string> { analysisResult.ImagePath },
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error standardizing screen from analysis result");
                return new StandardizedScreen
                {
                    ScreenId = Guid.NewGuid().ToString(),
                    FunctionId = Guid.NewGuid().ToString(),
                    ScreenName = "Error Screen",
                    ScreenType = "unknown",
                    Description = $"Error during standardization: {ex.Message}",
                    ConfidenceScore = 0.0,
                    UIElements = new List<UIElement>(),
                    BusinessFunctions = new List<BusinessFunction>(),
                    Workflows = new List<Workflow>(),
                    SourceImages = new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                // Check if all required services are available
                var testImage = "test.jpg";
                var isImageSupported = await _textExtractionService.IsImageSupportedAsync(testImage);
                return true; // Basic availability check
            }
            catch
            {
                return false;
            }
        }

        private double CalculateFinalConfidenceScore(TextExtractionResult textResult, ElementDetectionResult elementResult, StandardizationResult standardizationResult)
        {
            var scores = new List<double>();

            if (textResult.Success)
                scores.Add(textResult.Confidence);

            if (elementResult.Success)
                scores.Add(elementResult.ConfidenceScore);

            if (standardizationResult.Success && standardizationResult.StandardizedScreen != null)
                scores.Add(standardizationResult.StandardizedScreen.ConfidenceScore);

            return scores.Any() ? scores.Average() : 0.3; // Fallback confidence
        }

        private int CalculateComplexityScore(List<UIElement> elements)
        {
            if (!elements.Any()) return 1;

            var baseScore = Math.Min(elements.Count, 20); // Cap at 20 for element count
            
            // Add complexity for different element types
            var elementTypes = elements.Select(e => e.ElementType).Distinct().Count();
            var typeComplexity = Math.Min(elementTypes * 2, 15);

            // Add complexity for required fields
            var requiredFields = elements.Count(e => e.IsRequired);
            var requiredComplexity = Math.Min(requiredFields, 10);

            var totalScore = baseScore + typeComplexity + requiredComplexity;
            return Math.Min(Math.Max(totalScore, 1), 100); // Ensure score is between 1-100
        }
    }
} 