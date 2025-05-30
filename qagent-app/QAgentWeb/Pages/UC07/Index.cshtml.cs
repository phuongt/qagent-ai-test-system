using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using Microsoft.Extensions.Localization;

namespace QAgentWeb.Pages.UC07
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<IndexModel> _localizer;

        public IndexModel(ApplicationDbContext context, IStringLocalizer<IndexModel> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public List<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();

        public void OnGet()
        {
            ViewData["Title"] = _localizer["UC07Title"];

            // Generate mock notifications from existing data
            Notifications = new List<NotificationViewModel>();

            // Add task completion notifications
            var completedTasks = _context.QAgentTasks
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => t.UpdatedAt)
                .Take(5)
                .ToList();

            foreach (var task in completedTasks)
            {
                Notifications.Add(new NotificationViewModel
                {
                    Id = task.Id,
                    Title = "Task Completed",
                    Message = $"Task '{task.Title}' has been completed successfully",
                    Type = "success",
                    CreatedAt = task.UpdatedAt ?? DateTime.UtcNow,
                    IsRead = false,
                    Icon = "fa-check-circle"
                });
            }

            // Add upload session notifications
            var recentSessions = _context.UploadSessions
                .Include(u => u.Project)
                .OrderByDescending(u => u.StartedAt)
                .Take(3)
                .ToList();

            foreach (var session in recentSessions)
            {
                Notifications.Add(new NotificationViewModel
                {
                    Id = session.Id + 1000,
                    Title = "Upload Session Update",
                    Message = $"Upload session for '{session.Project?.Name ?? "Unknown Project"}' is {session.Status}",
                    Type = session.Status == "Completed" ? "success" : session.Status == "Failed" ? "error" : "info",
                    CreatedAt = session.StartedAt ?? DateTime.UtcNow,
                    IsRead = session.Status == "Completed",
                    Icon = session.Status == "Completed" ? "fa-cloud-upload-alt" : "fa-exclamation-triangle"
                });
            }

            // Sort by created date descending
            Notifications = Notifications.OrderByDescending(n => n.CreatedAt).ToList();
        }
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // success, error, warning, info
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Icon { get; set; } = "fa-bell";
    }
} 