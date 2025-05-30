using QAgentWeb.Models;

namespace QAgentWeb.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(string id);
        Task<Project> CreateProjectAsync(Project project);
        Task UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(string id);
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId);
        Task<Project?> GetProjectWithDetailsAsync(string id);
        Task<bool> ArchiveProjectAsync(string id, string? userId = null);
        Task<bool> RestoreProjectAsync(string id, string? userId = null);

    }
} 