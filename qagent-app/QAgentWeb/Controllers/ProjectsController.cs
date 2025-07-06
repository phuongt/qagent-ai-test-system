using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;

namespace QAgentWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Screens)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
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
                    GoogleDriveFolderId = p.GoogleDriveFolderId,
                    GoogleDriveFolderUrl = p.GoogleDriveFolderUrl,
                    ScreenCount = p.Screens.Count(s => !s.IsDeleted)
                })
                .ToListAsync();

            return Ok(projects);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Screens.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (project == null)
            {
                return NotFound();
            }

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Domain = project.Domain,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                UserId = project.UserId,
                GoogleDriveFolderId = project.GoogleDriveFolderId,
                GoogleDriveFolderUrl = project.GoogleDriveFolderUrl,
                ScreenCount = project.Screens.Count,
                Screens = project.Screens.Select(s => new ScreenDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    FilePath = s.FilePath,
                    OriginalFileName = s.OriginalFileName,
                    ContentType = s.ContentType,
                    FileSizeBytes = s.FileSizeBytes,
                    AnalysisStatus = s.AnalysisStatus,
                    CreatedAt = s.CreatedAt
                }).ToList()
            };

            return Ok(projectDto);
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto createProjectDto)
        {
            var project = new Project
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description,
                Domain = createProjectDto.Domain,
                Status = "Created",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = GetCurrentUserId()
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Domain = project.Domain,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                UserId = project.UserId,
                ScreenCount = 0
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, projectDto);
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto updateProjectDto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.IsDeleted)
            {
                return NotFound();
            }

            project.Name = updateProjectDto.Name;
            project.Description = updateProjectDto.Description;
            project.Domain = updateProjectDto.Domain;
            project.Status = updateProjectDto.Status;
            project.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.IsDeleted)
            {
                return NotFound();
            }

            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id && !e.IsDeleted);
        }

        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? "anonymous";
        }
    }
}