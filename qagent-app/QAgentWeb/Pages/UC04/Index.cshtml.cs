using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;

namespace QAgentWeb.Pages.UC04
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Screen> Screens { get; set; } = new List<Screen>();

        public async Task OnGetAsync()
        {
            Screens = await _context.Screens
                .Include(s => s.Project)
                .Include(s => s.UploadSession)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
} 