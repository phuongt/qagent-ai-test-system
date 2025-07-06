using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models.DTOs;

namespace QAgentWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard/overview
        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
        {
            try
            {
                var userId = GetCurrentUserId();
                var now = DateTime.UtcNow;
                var thirtyDaysAgo = now.AddDays(-30);

                // Get basic counts
                var totalProjects = await _context.Projects.CountAsync(p => p.UserId == userId && !p.IsDeleted);
                var totalScreens = await _context.Screens.CountAsync(s => s.UserId == userId && !s.IsDeleted);
                var totalAnalyses = await _context.AnalysisLogs.CountAsync(a => a.CreatedAt >= thirtyDaysAgo);

                // Get analysis status counts
                var completedAnalyses = await _context.AnalysisLogs
                    .CountAsync(a => a.AnalysisStatus == "Completed" && a.CreatedAt >= thirtyDaysAgo);
                var processingAnalyses = await _context.AnalysisLogs
                    .CountAsync(a => a.AnalysisStatus == "Processing");
                var failedAnalyses = await _context.AnalysisLogs
                    .CountAsync(a => a.AnalysisStatus == "Failed" && a.CreatedAt >= thirtyDaysAgo);

                // Get project status breakdown
                var projectsByStatus = await _context.Projects
                    .Where(p => p.UserId == userId && !p.IsDeleted)
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Get recent activity
                var recentProjects = await _context.Projects
                    .Where(p => p.UserId == userId && !p.IsDeleted)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Take(5)
                    .Select(p => new RecentActivityDto
                    {
                        Type = "Project",
                        Description = $"Updated project: {p.Name}",
                        Timestamp = p.UpdatedAt,
                        ProjectId = p.Id
                    })
                    .ToListAsync();

                var recentScreens = await _context.Screens
                    .Where(s => s.UserId == userId && !s.IsDeleted)
                    .OrderByDescending(s => s.UpdatedAt)
                    .Take(5)
                    .Select(s => new RecentActivityDto
                    {
                        Type = "Screen",
                        Description = $"Updated screen: {s.Name}",
                        Timestamp = s.UpdatedAt,
                        ScreenId = s.Id,
                        ProjectId = s.ProjectId
                    })
                    .ToListAsync();

                var recentActivities = recentProjects.Concat(recentScreens)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList();

                var overview = new DashboardOverviewDto
                {
                    TotalProjects = totalProjects,
                    TotalScreens = totalScreens,
                    TotalAnalyses = totalAnalyses,
                    CompletedAnalyses = completedAnalyses,
                    ProcessingAnalyses = processingAnalyses,
                    FailedAnalyses = failedAnalyses,
                    ProjectsByStatus = projectsByStatus.ToDictionary(x => x.Status, x => x.Count),
                    RecentActivities = recentActivities,
                    LastUpdated = now
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get dashboard overview", error = ex.Message });
            }
        }

        // GET: api/Dashboard/analytics
        [HttpGet("analytics")]
        public async Task<ActionResult<DashboardAnalyticsDto>> GetAnalytics(
            [FromQuery] int days = 30)
        {
            try
            {
                var userId = GetCurrentUserId();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                // Daily activity data
                var dailyData = new List<DailyActivityDto>();
                for (int i = 0; i < days; i++)
                {
                    var date = startDate.AddDays(i);
                    var nextDate = date.AddDays(1);

                    var projectsCreated = await _context.Projects
                        .CountAsync(p => p.UserId == userId && p.CreatedAt >= date && p.CreatedAt < nextDate);

                    var screensUploaded = await _context.Screens
                        .CountAsync(s => s.UserId == userId && s.CreatedAt >= date && s.CreatedAt < nextDate);

                    var analysesCompleted = await _context.AnalysisLogs
                        .CountAsync(a => a.AnalysisStatus == "Completed" && 
                                        a.CompletedAt >= date && a.CompletedAt < nextDate);

                    dailyData.Add(new DailyActivityDto
                    {
                        Date = date,
                        ProjectsCreated = projectsCreated,
                        ScreensUploaded = screensUploaded,
                        AnalysesCompleted = analysesCompleted
                    });
                }

                // Rule category usage
                var ruleCategoryUsage = await _context.RuleApplicationLogs
                    .Include(r => r.TestingRule)
                    .Where(r => r.AppliedAt >= startDate && r.WasSuccessful)
                    .GroupBy(r => r.TestingRule!.RuleCategory)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync();

                // Analysis performance
                var analysisPerformance = await _context.AnalysisLogs
                    .Where(a => a.StartedAt >= startDate)
                    .GroupBy(a => a.AnalysisStatus)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var analytics = new DashboardAnalyticsDto
                {
                    DateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                    DailyActivity = dailyData,
                    RuleCategoryUsage = ruleCategoryUsage.ToDictionary(x => x.Category, x => x.Count),
                    AnalysisPerformance = analysisPerformance.ToDictionary(x => x.Status, x => x.Count),
                    TotalActivityScore = dailyData.Sum(d => d.ProjectsCreated + d.ScreensUploaded + d.AnalysesCompleted)
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get dashboard analytics", error = ex.Message });
            }
        }

        // GET: api/Dashboard/projects/recent
        [HttpGet("projects/recent")]
        public async Task<ActionResult<List<ProjectDto>>> GetRecentProjects([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var recentProjects = await _context.Projects
                    .Include(p => p.Screens)
                    .Where(p => p.UserId == userId && !p.IsDeleted)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Take(limit)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Domain = p.Domain,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        UserId = p.UserId,
                        ScreenCount = p.Screens.Count(s => !s.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(recentProjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get recent projects", error = ex.Message });
            }
        }

        // GET: api/Dashboard/screens/pending-analysis
        [HttpGet("screens/pending-analysis")]
        public async Task<ActionResult<List<ScreenDto>>> GetPendingAnalysisScreens([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var pendingScreens = await _context.Screens
                    .Include(s => s.Project)
                    .Where(s => s.UserId == userId && !s.IsDeleted && 
                              (s.AnalysisStatus == null || s.AnalysisStatus == "Pending"))
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(limit)
                    .Select(s => new ScreenDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        ProjectId = s.ProjectId,
                        ProjectName = s.Project!.Name,
                        ScreenType = s.ScreenType,
                        Priority = s.Priority,
                        AnalysisStatus = s.AnalysisStatus,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(pendingScreens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get pending analysis screens", error = ex.Message });
            }
        }

        // GET: api/Dashboard/quick-stats
        [HttpGet("quick-stats")]
        public async Task<ActionResult<QuickStatsDto>> GetQuickStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var today = DateTime.UtcNow.Date;
                var thisWeek = today.AddDays(-(int)today.DayOfWeek);
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var quickStats = new QuickStatsDto
                {
                    Today = new PeriodStatsDto
                    {
                        ProjectsCreated = await _context.Projects.CountAsync(p => 
                            p.UserId == userId && p.CreatedAt >= today),
                        ScreensUploaded = await _context.Screens.CountAsync(s => 
                            s.UserId == userId && s.CreatedAt >= today),
                        AnalysesCompleted = await _context.AnalysisLogs.CountAsync(a => 
                            a.AnalysisStatus == "Completed" && a.CompletedAt >= today)
                    },
                    ThisWeek = new PeriodStatsDto
                    {
                        ProjectsCreated = await _context.Projects.CountAsync(p => 
                            p.UserId == userId && p.CreatedAt >= thisWeek),
                        ScreensUploaded = await _context.Screens.CountAsync(s => 
                            s.UserId == userId && s.CreatedAt >= thisWeek),
                        AnalysesCompleted = await _context.AnalysisLogs.CountAsync(a => 
                            a.AnalysisStatus == "Completed" && a.CompletedAt >= thisWeek)
                    },
                    ThisMonth = new PeriodStatsDto
                    {
                        ProjectsCreated = await _context.Projects.CountAsync(p => 
                            p.UserId == userId && p.CreatedAt >= thisMonth),
                        ScreensUploaded = await _context.Screens.CountAsync(s => 
                            s.UserId == userId && s.CreatedAt >= thisMonth),
                        AnalysesCompleted = await _context.AnalysisLogs.CountAsync(a => 
                            a.AnalysisStatus == "Completed" && a.CompletedAt >= thisMonth)
                    }
                };

                return Ok(quickStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get quick stats", error = ex.Message });
            }
        }

        // GET: api/Dashboard/system-health
        [HttpGet("system-health")]
        public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
        {
            try
            {
                var health = new SystemHealthDto
                {
                    DatabaseStatus = "Healthy",
                    AIServicesStatus = "Healthy",
                    StorageStatus = "Healthy",
                    LastChecked = DateTime.UtcNow
                };

                // Check database connectivity
                try
                {
                    await _context.Database.CanConnectAsync();
                    health.DatabaseStatus = "Healthy";
                }
                catch
                {
                    health.DatabaseStatus = "Unhealthy";
                }

                // Check AI services (mock implementation)
                health.AIServicesStatus = "Healthy";
                health.ActiveProcesses = await _context.AnalysisLogs
                    .CountAsync(a => a.AnalysisStatus == "Processing");

                // Check storage
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (Directory.Exists(uploadsPath))
                {
                    var dirInfo = new DirectoryInfo(uploadsPath);
                    var totalSize = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                        .Sum(file => file.Length);
                    health.StorageUsed = totalSize;
                    health.StorageStatus = totalSize > 1024 * 1024 * 1024 ? "Warning" : "Healthy"; // 1GB threshold
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get system health", error = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? "anonymous";
        }
    }

    // Dashboard DTOs
    public class DashboardOverviewDto
    {
        public int TotalProjects { get; set; }
        public int TotalScreens { get; set; }
        public int TotalAnalyses { get; set; }
        public int CompletedAnalyses { get; set; }
        public int ProcessingAnalyses { get; set; }
        public int FailedAnalyses { get; set; }
        public Dictionary<string, int> ProjectsByStatus { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class DashboardAnalyticsDto
    {
        public DateRangeDto DateRange { get; set; } = new();
        public List<DailyActivityDto> DailyActivity { get; set; } = new();
        public Dictionary<string, int> RuleCategoryUsage { get; set; } = new();
        public Dictionary<string, int> AnalysisPerformance { get; set; } = new();
        public int TotalActivityScore { get; set; }
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DailyActivityDto
    {
        public DateTime Date { get; set; }
        public int ProjectsCreated { get; set; }
        public int ScreensUploaded { get; set; }
        public int AnalysesCompleted { get; set; }
    }

    public class QuickStatsDto
    {
        public PeriodStatsDto Today { get; set; } = new();
        public PeriodStatsDto ThisWeek { get; set; } = new();
        public PeriodStatsDto ThisMonth { get; set; } = new();
    }

    public class PeriodStatsDto
    {
        public int ProjectsCreated { get; set; }
        public int ScreensUploaded { get; set; }
        public int AnalysesCompleted { get; set; }
    }

    public class SystemHealthDto
    {
        public string DatabaseStatus { get; set; } = "";
        public string AIServicesStatus { get; set; } = "";
        public string StorageStatus { get; set; } = "";
        public long StorageUsed { get; set; }
        public int ActiveProcesses { get; set; }
        public DateTime LastChecked { get; set; }
    }
}