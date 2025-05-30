using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Services;
using System.Text.Json;

namespace QAgentWeb.Pages.UC02
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIAnalysisService _aiAnalysisService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ApplicationDbContext context,
            IAIAnalysisService aiAnalysisService,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _aiAnalysisService = aiAnalysisService;
            _logger = logger;
        }

        public List<Screen> Screens { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public string? SelectedProjectId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SearchTerm { get; set; }
        public int TotalScreens { get; set; }
        public int PendingScreens { get; set; }
        public int ProcessingScreens { get; set; }
        public int CompletedScreens { get; set; }
        public int FailedScreens { get; set; }
        public bool IsAIServiceAvailable { get; set; }

        [BindProperty]
        public int SelectedScreenId { get; set; }

        [BindProperty]
        public string BusinessDescription { get; set; } = string.Empty;

        public async Task OnGetAsync(string? projectId = null, string? status = null, string? search = null)
        {
            SelectedProjectId = projectId;
            SelectedStatus = status;
            SearchTerm = search;

            // Load projects for filter
            Projects = await _context.Projects
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Build query for screens
            var query = _context.Screens
                .Include(s => s.Project)
                .Where(s => !s.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(projectId) && int.TryParse(projectId, out var projId))
            {
                query = query.Where(s => s.ProjectId == projId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.AnalysisStatus == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.Contains(search) || 
                                        s.Description.Contains(search) ||
                                        (s.Project != null && s.Project.Name.Contains(search)));
            }

            Screens = await query
                .OrderByDescending(s => s.CreatedAt)
                .Take(50) // Limit for performance
                .ToListAsync();

            // Calculate statistics
            var allScreens = await _context.Screens
                .Where(s => !s.IsDeleted)
                .ToListAsync();

            TotalScreens = allScreens.Count;
            PendingScreens = allScreens.Count(s => s.AnalysisStatus == Screen.AnalysisStatuses.Pending);
            ProcessingScreens = allScreens.Count(s => s.AnalysisStatus == Screen.AnalysisStatuses.Processing);
            CompletedScreens = allScreens.Count(s => s.AnalysisStatus == Screen.AnalysisStatuses.Completed);
            FailedScreens = allScreens.Count(s => s.AnalysisStatus == Screen.AnalysisStatuses.Failed);

            // Check AI service availability
            IsAIServiceAvailable = await _aiAnalysisService.IsServiceAvailableAsync();
        }

        public async Task<IActionResult> OnPostAnalyzeScreenAsync()
        {
            try
            {
                var result = await _aiAnalysisService.AnalyzeScreenAsync(SelectedScreenId, BusinessDescription);
                
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    TempData["Error"] = $"Lỗi phân tích: {result.ErrorMessage}";
                }
                else
                {
                    TempData["Success"] = $"Phân tích thành công! Độ tin cậy: {result.ConfidenceScore:P1}, " +
                                         $"Thời gian xử lý: {result.ProcessingTime.TotalSeconds:F1}s";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing screen {ScreenId}", SelectedScreenId);
                TempData["Error"] = "Có lỗi xảy ra khi phân tích màn hình.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBatchAnalyzeAsync(string projectId)
        {
            try
            {
                if (!int.TryParse(projectId, out var projId))
                {
                    TempData["Error"] = "Project ID không hợp lệ.";
                    return RedirectToPage();
                }

                var pendingScreens = await _context.Screens
                    .Where(s => s.ProjectId == projId && 
                               s.AnalysisStatus == Screen.AnalysisStatuses.Pending &&
                               !s.IsDeleted)
                    .ToListAsync();

                if (!pendingScreens.Any())
                {
                    TempData["Warning"] = "Không có màn hình nào cần phân tích trong project này.";
                    return RedirectToPage();
                }

                // Start batch analysis (in background)
                _ = Task.Run(async () =>
                {
                    foreach (var screen in pendingScreens)
                    {
                        try
                        {
                            await _aiAnalysisService.AnalyzeScreenAsync(screen.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in batch analysis for screen {ScreenId}", screen.Id);
                        }
                    }
                });

                TempData["Success"] = $"Bắt đầu phân tích {pendingScreens.Count} màn hình. Quá trình sẽ chạy trong background.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting batch analysis for project {ProjectId}", projectId);
                TempData["Error"] = "Có lỗi xảy ra khi bắt đầu phân tích hàng loạt.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetAnalysisResultAsync(int screenId)
        {
            try
            {
                var screen = await _context.Screens
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == screenId);

                if (screen == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(screen.AnalysisResult))
                {
                    return new JsonResult(new { error = "Chưa có kết quả phân tích" });
                }

                var analysisResult = JsonSerializer.Deserialize<AnalysisResult>(screen.AnalysisResult);
                
                return new JsonResult(new
                {
                    screenName = screen.Name,
                    screenType = screen.ScreenType,
                    confidence = screen.AnalysisConfidence,
                    complexity = screen.ComplexityScore,
                    extractedText = analysisResult?.ExtractedText,
                    uiElements = analysisResult?.UIElements,
                    businessFunctions = analysisResult?.BusinessFunctions,
                    processingTime = analysisResult?.ProcessingTime.TotalSeconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analysis result for screen {ScreenId}", screenId);
                return new JsonResult(new { error = "Có lỗi xảy ra khi lấy kết quả phân tích" });
            }
        }
    }
} 