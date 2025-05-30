using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;

namespace QAgentWeb.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, IAuthService authService, ILogger<IndexModel> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        public UserInfo? CurrentUser { get; set; }
        public List<QAgentTask> Tasks { get; set; } = new();
        public int TotalTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalProjects { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                // Get current user info
                var user = await _authService.GetUserByIdAsync(userIdString);
                if (user == null)
                {
                    return RedirectToPage("/Auth/Login");
                }

                CurrentUser = new UserInfo
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    IsEmailConfirmed = user.IsEmailConfirmed
                };

                // Get tasks (for now, get all tasks since we don't have user-specific tasks yet)
                Tasks = await _context.QAgentTasks
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Calculate statistics
                var allTasks = await _context.QAgentTasks
                    .Where(t => !t.IsDeleted)
                    .ToListAsync();

                TotalTasks = allTasks.Count;
                InProgressTasks = allTasks.Count(t => t.Status == QAgentTask.Statuses.InProgress);
                CompletedTasks = allTasks.Count(t => t.Status == QAgentTask.Statuses.Completed);

                // Get projects count for current user
                TotalProjects = await _context.Projects
                    .Where(p => !p.IsDeleted && p.UserId == userIdString)
                    .CountAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserId}", userIdString);
                return RedirectToPage("/Error");
            }
        }
    }
} 