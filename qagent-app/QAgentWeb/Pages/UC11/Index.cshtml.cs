using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;

namespace QAgentWeb.Pages.UC11
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<QAgentTask> Tasks { get; set; } = Enumerable.Empty<QAgentTask>();

        public async Task OnGetAsync()
        {
            // Use direct DbContext to avoid missing column issues
            Tasks = await _context.QAgentTasks
                .Where(t => !t.IsDeleted)
                .Select(t => new QAgentTask
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    Category = t.Category,
                    AssignedTo = t.AssignedTo,
                    DueDate = t.DueDate,
                    Progress = t.Progress,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    UserId = t.UserId
                })
                .ToListAsync();
        }
    }
} 