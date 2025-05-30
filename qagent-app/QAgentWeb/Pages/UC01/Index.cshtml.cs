using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;

namespace QAgentWeb.Pages.UC01
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Project> Projects { get; set; } = Enumerable.Empty<Project>();

        [BindProperty]
        public string ProjectName { get; set; } = "";

        [BindProperty]
        public string Domain { get; set; } = "";

        [BindProperty]
        public string Description { get; set; } = "";

        [BindProperty]
        public List<IFormFile> UploadFiles { get; set; } = new();

        public async Task OnGetAsync()
        {
            Projects = await _context.Projects
                .Include(p => p.Screens)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var project = new Project
                {
                    Name = ProjectName,
                    Domain = Domain,
                    Description = Description,
                    Status = "Uploaded",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = GetCurrentUserId()
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync(); // Save để có ProjectId

                // Process uploaded files
                if (UploadFiles.Any())
                {
                    foreach (var file in UploadFiles)
                    {
                        if (IsValidFile(file))
                        {
                            var screen = new Screen
                            {
                                ProjectId = project.Id,
                                Name = Path.GetFileNameWithoutExtension(file.FileName),
                                Description = $"Uploaded file: {file.FileName}",
                                FilePath = await SaveFileAsync(file, project.Id.ToString()),
                                OriginalFileName = file.FileName,
                                ContentType = file.ContentType,
                                FileSizeBytes = file.Length,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.Screens.Add(screen);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Project '{ProjectName}' được tạo thành công với {UploadFiles.Count} files!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo project: {ex.Message}";
                await OnGetAsync();
                return Page();
            }
        }

        private bool IsValidFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxSize = 10 * 1024 * 1024; // 10MB

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension) && file.Length <= maxSize;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string projectId)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", projectId);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{projectId}/{fileName}";
        }

        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? "anonymous";
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (int.TryParse(id, out int projectId))
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project != null)
                {
                    project.IsDeleted = true;
                    project.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage();
        }
    }
} 