using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using Microsoft.Extensions.Localization;

namespace QAgentWeb.Pages.UC06
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

        public IEnumerable<QAgentTask> CompletedTasks { get; set; } = new List<QAgentTask>();
        public IEnumerable<Project> Projects { get; set; } = new List<Project>();
        public IEnumerable<Screen> Screens { get; set; } = new List<Screen>();
        public IEnumerable<UploadSession> UploadSessions { get; set; } = new List<UploadSession>();
        public IEnumerable<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            ViewData["Title"] = _localizer["UC06Title"];

            CompletedTasks = await _context.QAgentTasks
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();

            Projects = await _context.Projects
                .Include(p => p.Screens)
                .Include(p => p.UploadSessions)
                .ToListAsync();

            Screens = await _context.Screens
                .Include(s => s.Project)
                .ToListAsync();

            UploadSessions = await _context.UploadSessions
                .Include(u => u.Project)
                .OrderByDescending(u => u.StartedAt)
                .ToListAsync();

            Users = await _context.Users.ToListAsync();
        }
    }
} 