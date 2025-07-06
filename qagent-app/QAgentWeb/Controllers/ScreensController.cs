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
    public class ScreensController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUploadService _uploadService;

        public ScreensController(ApplicationDbContext context, IUploadService uploadService)
        {
            _context = context;
            _uploadService = uploadService;
        }

        // GET: api/Screens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScreenDto>>> GetScreens([FromQuery] int? projectId = null)
        {
            var query = _context.Screens
                .Include(s => s.Project)
                .Where(s => !s.IsDeleted);

            if (projectId.HasValue)
            {
                query = query.Where(s => s.ProjectId == projectId.Value);
            }

            var screens = await query
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ScreenDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    FilePath = s.FilePath,
                    OriginalFileName = s.OriginalFileName,
                    ContentType = s.ContentType,
                    FileSizeBytes = s.FileSizeBytes,
                    AnalysisStatus = s.AnalysisStatus,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project!.Name,
                    GoogleDriveFileId = s.GoogleDriveFileId,
                    GoogleDriveFileUrl = s.GoogleDriveFileUrl,
                    Priority = s.Priority,
                    ScreenType = s.ScreenType,
                    UserId = s.UserId
                })
                .ToListAsync();

            return Ok(screens);
        }

        // GET: api/Screens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ScreenDto>> GetScreen(int id)
        {
            var screen = await _context.Screens
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (screen == null)
            {
                return NotFound();
            }

            var screenDto = new ScreenDto
            {
                Id = screen.Id,
                Name = screen.Name,
                Description = screen.Description,
                FilePath = screen.FilePath,
                OriginalFileName = screen.OriginalFileName,
                ContentType = screen.ContentType,
                FileSizeBytes = screen.FileSizeBytes,
                AnalysisStatus = screen.AnalysisStatus,
                CreatedAt = screen.CreatedAt,
                UpdatedAt = screen.UpdatedAt,
                ProjectId = screen.ProjectId,
                ProjectName = screen.Project?.Name,
                GoogleDriveFileId = screen.GoogleDriveFileId,
                GoogleDriveFileUrl = screen.GoogleDriveFileUrl,
                Priority = screen.Priority,
                ScreenType = screen.ScreenType,
                UserId = screen.UserId
            };

            return Ok(screenDto);
        }

        // POST: api/Screens
        [HttpPost]
        public async Task<ActionResult<ScreenDto>> CreateScreen([FromForm] CreateScreenDto createScreenDto)
        {
            // Verify project exists
            var project = await _context.Projects.FindAsync(createScreenDto.ProjectId);
            if (project == null || project.IsDeleted)
            {
                return BadRequest("Project not found");
            }

            // Validate file
            if (!IsValidFile(createScreenDto.File))
            {
                return BadRequest("Invalid file type or size");
            }

            // Save file
            var filePath = await SaveFileAsync(createScreenDto.File, createScreenDto.ProjectId.ToString());

            var screen = new Screen
            {
                Name = createScreenDto.Name,
                Description = createScreenDto.Description,
                ProjectId = createScreenDto.ProjectId,
                ScreenType = createScreenDto.ScreenType,
                Priority = createScreenDto.Priority,
                FilePath = filePath,
                OriginalFileName = createScreenDto.File.FileName,
                ContentType = createScreenDto.File.ContentType,
                FileSizeBytes = createScreenDto.File.Length,
                AnalysisStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = GetCurrentUserId()
            };

            _context.Screens.Add(screen);
            await _context.SaveChangesAsync();

            var screenDto = new ScreenDto
            {
                Id = screen.Id,
                Name = screen.Name,
                Description = screen.Description,
                FilePath = screen.FilePath,
                OriginalFileName = screen.OriginalFileName,
                ContentType = screen.ContentType,
                FileSizeBytes = screen.FileSizeBytes,
                AnalysisStatus = screen.AnalysisStatus,
                CreatedAt = screen.CreatedAt,
                UpdatedAt = screen.UpdatedAt,
                ProjectId = screen.ProjectId,
                ProjectName = project.Name,
                Priority = screen.Priority,
                ScreenType = screen.ScreenType,
                UserId = screen.UserId
            };

            return CreatedAtAction(nameof(GetScreen), new { id = screen.Id }, screenDto);
        }

        // POST: api/Screens/upload
        [HttpPost("upload")]
        public async Task<ActionResult<List<ScreenDto>>> UploadScreens([FromForm] ScreenUploadDto uploadDto)
        {
            // Verify project exists
            var project = await _context.Projects.FindAsync(uploadDto.ProjectId);
            if (project == null || project.IsDeleted)
            {
                return BadRequest("Project not found");
            }

            var screens = new List<Screen>();
            var errors = new List<string>();

            foreach (var file in uploadDto.Files)
            {
                if (!IsValidFile(file))
                {
                    errors.Add($"Invalid file: {file.FileName}");
                    continue;
                }

                try
                {
                    var filePath = await SaveFileAsync(file, uploadDto.ProjectId.ToString());
                    var screen = new Screen
                    {
                        Name = Path.GetFileNameWithoutExtension(file.FileName),
                        Description = uploadDto.Description ?? $"Uploaded file: {file.FileName}",
                        ProjectId = uploadDto.ProjectId,
                        ScreenType = "UI",
                        Priority = "Medium",
                        FilePath = filePath,
                        OriginalFileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSizeBytes = file.Length,
                        AnalysisStatus = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UserId = GetCurrentUserId()
                    };

                    screens.Add(screen);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error uploading {file.FileName}: {ex.Message}");
                }
            }

            if (screens.Any())
            {
                _context.Screens.AddRange(screens);
                await _context.SaveChangesAsync();
            }

            var screenDtos = screens.Select(s => new ScreenDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                FilePath = s.FilePath,
                OriginalFileName = s.OriginalFileName,
                ContentType = s.ContentType,
                FileSizeBytes = s.FileSizeBytes,
                AnalysisStatus = s.AnalysisStatus,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                ProjectId = s.ProjectId,
                ProjectName = project.Name,
                Priority = s.Priority,
                ScreenType = s.ScreenType,
                UserId = s.UserId
            }).ToList();

            var response = new
            {
                Screens = screenDtos,
                Errors = errors,
                TotalUploaded = screens.Count,
                TotalAttempted = uploadDto.Files.Count
            };

            return Ok(response);
        }

        // PUT: api/Screens/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScreen(int id, UpdateScreenDto updateScreenDto)
        {
            var screen = await _context.Screens.FindAsync(id);
            if (screen == null || screen.IsDeleted)
            {
                return NotFound();
            }

            screen.Name = updateScreenDto.Name;
            screen.Description = updateScreenDto.Description;
            screen.ScreenType = updateScreenDto.ScreenType;
            screen.Priority = updateScreenDto.Priority;
            screen.AnalysisStatus = updateScreenDto.AnalysisStatus ?? screen.AnalysisStatus;
            screen.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScreenExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Screens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScreen(int id)
        {
            var screen = await _context.Screens.FindAsync(id);
            if (screen == null || screen.IsDeleted)
            {
                return NotFound();
            }

            screen.IsDeleted = true;
            screen.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IsValidFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".bmp", ".gif", ".tiff" };
            var maxSize = 50 * 1024 * 1024; // 50MB

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

        private bool ScreenExists(int id)
        {
            return _context.Screens.Any(e => e.Id == id && !e.IsDeleted);
        }

        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? "anonymous";
        }
    }
}