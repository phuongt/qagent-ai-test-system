using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;

namespace QAgentWeb.Pages.UC05
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<UploadSession> UploadSessions { get; set; } = new List<UploadSession>();

        public async Task OnGetAsync()
        {
            UploadSessions = await _context.UploadSessions
                .Include(u => u.Project)
                .Include(u => u.Screens)
                .OrderByDescending(u => u.StartedAt)
                .ToListAsync();
        }
    }
} 