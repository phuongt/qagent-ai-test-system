using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;

namespace QAgentWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIAnalysisService _aiAnalysisService;
        private readonly IGoogleVisionService _googleVisionService;
        private readonly IGoogleGeminiService _googleGeminiService;
        private readonly IRuleEngineService _ruleEngineService;

        public AnalysisController(
            ApplicationDbContext context,
            IAIAnalysisService aiAnalysisService,
            IGoogleVisionService googleVisionService,
            IGoogleGeminiService googleGeminiService,
            IRuleEngineService ruleEngineService)
        {
            _context = context;
            _aiAnalysisService = aiAnalysisService;
            _googleVisionService = googleVisionService;
            _googleGeminiService = googleGeminiService;
            _ruleEngineService = ruleEngineService;
        }

        // POST: api/Analysis/analyze
        [HttpPost("analyze")]
        public async Task<ActionResult<AnalysisResultDto>> AnalyzeScreen(AnalysisRequestDto analysisRequest)
        {
            var screen = await _context.Screens
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == analysisRequest.ScreenId && !s.IsDeleted);

            if (screen == null)
            {
                return NotFound("Screen not found");
            }

            try
            {
                var analysisLog = new AnalysisLog
                {
                    FunctionId = Guid.NewGuid().ToString(),
                    ImageUrl = screen.FilePath ?? "",
                    AnalysisStatus = "Processing",
                    AIModelUsed = "Google Gemini",
                    StartedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AnalysisLogs.Add(analysisLog);
                await _context.SaveChangesAsync();

                // Update screen status
                screen.AnalysisStatus = "Processing";
                screen.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var result = new AnalysisResultDto
                {
                    Id = analysisLog.Id,
                    ScreenId = screen.Id,
                    ScreenName = screen.Name,
                    AnalysisStatus = "Processing",
                    StartedAt = analysisLog.StartedAt,
                    AIModelUsed = analysisLog.AIModelUsed
                };

                // Perform analysis asynchronously
                _ = Task.Run(async () => await PerformAnalysisAsync(screen, analysisRequest, analysisLog.Id));

                return Ok(result);
            }
            catch (Exception ex)
            {
                screen.AnalysisStatus = "Failed";
                screen.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return StatusCode(500, new { message = "Analysis failed", error = ex.Message });
            }
        }

        // GET: api/Analysis/result/{id}
        [HttpGet("result/{id}")]
        public async Task<ActionResult<AnalysisResultDto>> GetAnalysisResult(int id)
        {
            var analysisLog = await _context.AnalysisLogs
                .Include(a => a.StandardizedScreen)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (analysisLog == null)
            {
                return NotFound("Analysis result not found");
            }

            var result = new AnalysisResultDto
            {
                Id = analysisLog.Id,
                ScreenId = analysisLog.StandardizedScreenId != null ? 
                    int.Parse(analysisLog.StandardizedScreenId) : 0,
                AnalysisStatus = analysisLog.AnalysisStatus,
                StartedAt = analysisLog.StartedAt,
                CompletedAt = analysisLog.CompletedAt,
                AIModelUsed = analysisLog.AIModelUsed,
                ErrorMessage = analysisLog.ErrorMessage
            };

            if (analysisLog.StandardizedScreen != null)
            {
                result.UIElements = analysisLog.StandardizedScreen.UIElements.Select(e => new UIElementDto
                {
                    Type = e.Type,
                    Text = e.Text,
                    X = e.X,
                    Y = e.Y,
                    Width = e.Width,
                    Height = e.Height,
                    Properties = e.Properties
                }).ToList();
            }

            return Ok(result);
        }

        // GET: api/Analysis/screen/{screenId}
        [HttpGet("screen/{screenId}")]
        public async Task<ActionResult<List<AnalysisResultDto>>> GetScreenAnalyses(int screenId)
        {
            var analyses = await _context.AnalysisLogs
                .Where(a => a.StandardizedScreenId == screenId.ToString())
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new AnalysisResultDto
                {
                    Id = a.Id,
                    ScreenId = screenId,
                    AnalysisStatus = a.AnalysisStatus,
                    StartedAt = a.StartedAt,
                    CompletedAt = a.CompletedAt,
                    AIModelUsed = a.AIModelUsed,
                    ErrorMessage = a.ErrorMessage
                })
                .ToListAsync();

            return Ok(analyses);
        }

        // POST: api/Analysis/batch
        [HttpPost("batch")]
        public async Task<ActionResult<BatchAnalysisResultDto>> BatchAnalyze(BatchAnalysisRequestDto batchRequest)
        {
            var screens = await _context.Screens
                .Where(s => batchRequest.ScreenIds.Contains(s.Id) && !s.IsDeleted)
                .ToListAsync();

            if (!screens.Any())
            {
                return BadRequest("No valid screens found");
            }

            var batchResult = new BatchAnalysisResultDto
            {
                StartedAt = DateTime.UtcNow,
                TotalScreens = screens.Count
            };

            try
            {
                // Process each screen
                foreach (var screen in screens)
                {
                    try
                    {
                        var analysisRequest = new AnalysisRequestDto
                        {
                            ScreenId = screen.Id,
                            BusinessDescription = batchRequest.BusinessDescription,
                            AnalysisType = batchRequest.AnalysisType,
                            IncludeUIElements = batchRequest.IncludeUIElements,
                            IncludeTextExtraction = batchRequest.IncludeTextExtraction,
                            IncludeISTQBRules = batchRequest.IncludeISTQBRules,
                            GenerateChecklist = batchRequest.GenerateChecklist
                        };

                        var analysisLog = new AnalysisLog
                        {
                            FunctionId = Guid.NewGuid().ToString(),
                            ImageUrl = screen.FilePath ?? "",
                            AnalysisStatus = "Processing",
                            AIModelUsed = "Google Gemini",
                            StartedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.AnalysisLogs.Add(analysisLog);
                        await _context.SaveChangesAsync();

                        var result = new AnalysisResultDto
                        {
                            Id = analysisLog.Id,
                            ScreenId = screen.Id,
                            ScreenName = screen.Name,
                            AnalysisStatus = "Processing",
                            StartedAt = analysisLog.StartedAt,
                            AIModelUsed = analysisLog.AIModelUsed
                        };

                        batchResult.Results.Add(result);

                        // Process asynchronously
                        _ = Task.Run(async () => await PerformAnalysisAsync(screen, analysisRequest, analysisLog.Id));
                    }
                    catch (Exception ex)
                    {
                        batchResult.Errors.Add($"Screen {screen.Name}: {ex.Message}");
                        batchResult.FailedAnalyses++;
                    }
                }

                batchResult.SuccessfulAnalyses = batchResult.Results.Count;
                batchResult.CompletedAt = DateTime.UtcNow;

                return Ok(batchResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Batch analysis failed", error = ex.Message });
            }
        }

        // GET: api/Analysis/progress/{batchId}
        [HttpGet("progress/{batchId}")]
        public async Task<ActionResult<AnalysisProgressDto>> GetAnalysisProgress(string batchId)
        {
            // This would typically be implemented with a background job system
            // For now, return a simple progress response
            var progress = new AnalysisProgressDto
            {
                Status = "In Progress",
                CompletedScreens = 0,
                TotalScreens = 1,
                CurrentStep = "Analyzing UI elements",
                ProgressPercentage = 50,
                StartedAt = DateTime.UtcNow.AddMinutes(-2)
            };

            return Ok(progress);
        }

        // POST: api/Analysis/regenerate-checklist
        [HttpPost("regenerate-checklist")]
        public async Task<ActionResult<List<ChecklistItemDto>>> RegenerateChecklist(int screenId, string? businessDescription = null)
        {
            var screen = await _context.Screens
                .FirstOrDefaultAsync(s => s.Id == screenId && !s.IsDeleted);

            if (screen == null)
            {
                return NotFound("Screen not found");
            }

            try
            {
                // Get latest analysis results
                var latestAnalysis = await _context.AnalysisLogs
                    .Where(a => a.StandardizedScreenId == screenId.ToString())
                    .OrderByDescending(a => a.StartedAt)
                    .FirstOrDefaultAsync();

                if (latestAnalysis == null)
                {
                    return BadRequest("No analysis found for this screen. Please run analysis first.");
                }

                // Generate new checklist using AI service
                var checklistItems = await GenerateChecklistItemsAsync(screen, businessDescription);

                return Ok(checklistItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Checklist generation failed", error = ex.Message });
            }
        }

        private async Task PerformAnalysisAsync(Screen screen, AnalysisRequestDto analysisRequest, int analysisLogId)
        {
            try
            {
                var analysisLog = await _context.AnalysisLogs.FindAsync(analysisLogId);
                if (analysisLog == null) return;

                // Perform Google Vision analysis
                var visionResult = await _googleVisionService.AnalyzeImageAsync(screen.FilePath!);

                // Perform Google Gemini analysis
                var geminiResult = await _googleGeminiService.AnalyzeImageAsync(
                    screen.FilePath!, 
                    analysisRequest.BusinessDescription ?? "");

                // Apply ISTQB rules
                var appliedRules = await _ruleEngineService.ApplyRulesAsync(
                    screen.Id, 
                    analysisRequest.BusinessDescription ?? "");

                // Update analysis log
                analysisLog.AnalysisStatus = "Completed";
                analysisLog.CompletedAt = DateTime.UtcNow;
                analysisLog.UpdatedAt = DateTime.UtcNow;

                // Update screen status
                screen.AnalysisStatus = "Completed";
                screen.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var analysisLog = await _context.AnalysisLogs.FindAsync(analysisLogId);
                if (analysisLog != null)
                {
                    analysisLog.AnalysisStatus = "Failed";
                    analysisLog.ErrorMessage = ex.Message;
                    analysisLog.CompletedAt = DateTime.UtcNow;
                    analysisLog.UpdatedAt = DateTime.UtcNow;
                }

                screen.AnalysisStatus = "Failed";
                screen.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<ChecklistItemDto>> GenerateChecklistItemsAsync(Screen screen, string? businessDescription)
        {
            // This would integrate with your AI service to generate checklist items
            // For now, return sample data
            var checklistItems = new List<ChecklistItemDto>
            {
                new ChecklistItemDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Verify UI Element Visibility",
                    Description = "Check that all UI elements are visible and properly rendered",
                    Category = "UI Testing",
                    Priority = "High",
                    IsRequired = true,
                    TestSteps = new List<string> { "Open the screen", "Verify all elements are visible" },
                    ExpectedResults = new List<string> { "All UI elements should be visible and properly aligned" }
                }
            };

            return checklistItems;
        }
    }
}