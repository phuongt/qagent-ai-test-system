using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;

namespace QAgentWeb.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(string id)
        {
            if (int.TryParse(id, out int projectId))
            {
                return await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            }
            return null;
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;
            
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            
            return project;
        }

        public async Task UpdateProjectAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(string id)
        {
            var project = await GetProjectByIdAsync(id);
            if (project != null)
            {
                project.IsDeleted = true;
                project.DeletedAt = DateTime.UtcNow;
                await UpdateProjectAsync(project);
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<Project>();
            return await _context.Projects
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithDetailsAsync(string id)
        {
            if (int.TryParse(id, out int projectId))
            {
                return await _context.Projects
                    .Include(p => p.Screens)
                    .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            }
            return null;
        }

        public async Task<bool> ArchiveProjectAsync(string id, string? userId = null)
        {
            var project = await GetProjectByIdAsync(id);
            if (project == null) return false;
            if (userId != null && project.UserId != userId)
                return false;

            project.IsArchived = true;
            project.UpdatedAt = DateTime.UtcNow;
            await UpdateProjectAsync(project);
            
            return true;
        }

        public async Task<bool> RestoreProjectAsync(string id, string? userId = null)
        {
            var project = await GetProjectByIdAsync(id);
            if (project == null) return false;
            if (userId != null && project.UserId != userId)
                return false;

            project.IsArchived = false;
            project.UpdatedAt = DateTime.UtcNow;
            await UpdateProjectAsync(project);
            
            return true;
        }
    }
} 