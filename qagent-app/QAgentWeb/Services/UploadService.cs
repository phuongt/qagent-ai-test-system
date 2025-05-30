using QAgentWeb.Data;
using QAgentWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace QAgentWeb.Services
{
    public interface IUploadService
    {
        Task<UploadSession> CreateSessionAsync(int projectId, string? userId = null);
        Task<(bool Success, string Message, Screen? Screen)> UploadFileAsync(int sessionId, IFormFile file, string? description = null);
        Task<UploadSession?> GetSessionAsync(int sessionId);
        Task<UploadSession?> GetSessionByIdAsync(int sessionId);
        Task<bool> CompleteSessionAsync(int sessionId);
        Task<bool> CancelSessionAsync(int sessionId);
        Task<List<UploadSession>> GetUserSessionsAsync(string userId, int page = 1, int pageSize = 10);
        Task<bool> ValidateFileAsync(IFormFile file);
        Task<bool> DeleteScreenAsync(int screenId);
    }

    public class UploadService : IUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadService> _logger;
        
        // File validation constants
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private static readonly string[] AllowedDocumentTypes = { "application/pdf" };
        private static readonly long MaxFileSize = 10 * 1024 * 1024; // 10MB
        
        public UploadService(
            ApplicationDbContext context,
            IGoogleDriveService googleDriveService,
            IWebHostEnvironment environment,
            ILogger<UploadService> logger)
        {
            _context = context;
            _googleDriveService = googleDriveService;
            _environment = environment;
            _logger = logger;
        }

        public async Task<UploadSession> CreateSessionAsync(int projectId, string? userId = null)
        {
            try
            {
                var session = new UploadSession
                {
                    SessionId = new Random().Next(1000, 9999),
                    ProjectId = projectId,
                    UserId = userId,
                    Status = UploadSession.Statuses.Started,
                    CreatedBy = userId
                };

                _context.UploadSessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created upload session {SessionId} for project {ProjectId}", session.SessionId, projectId);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating upload session for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<(bool Success, string Message, Screen? Screen)> UploadFileAsync(int sessionId, IFormFile file, string? description = null)
        {
            try
            {
                // Get session
                var session = await _context.UploadSessions
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session == null)
                {
                    return (false, "Upload session not found", null);
                }

                if (!session.IsInProgress() && session.Status != UploadSession.Statuses.Started)
                {
                    return (false, "Upload session is not active", null);
                }

                // Validate file
                var validationResult = await ValidateFileAsync(file);
                if (!validationResult)
                {
                    return (false, "File validation failed", null);
                }

                // Start session if not started
                if (session.Status == UploadSession.Statuses.Started)
                {
                    session.Start();
                }

                // Create screen record
                var screen = new Screen
                {
                    Name = Path.GetFileNameWithoutExtension(file.FileName),
                    Description = description ?? "",
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    ProjectId = session.ProjectId,
                    UploadSessionId = session.Id,
                    AnalysisStatus = Screen.AnalysisStatuses.Pending,
                    CreatedBy = session.UserId
                };

                // Save file locally first
                var localPath = await SaveFileLocallyAsync(file, screen);
                screen.FilePath = localPath;

                // Upload to Google Drive
                try
                {
                    var fileId = await _googleDriveService.UploadFileAsync(
                        screen.OriginalFileName,
                        file.OpenReadStream(),
                        file.ContentType
                    );
                    if (!string.IsNullOrEmpty(fileId))
                    {
                        screen.GoogleDriveFileId = fileId;
                        screen.GoogleDriveFileUrl = $"https://drive.google.com/file/d/{fileId}/view";
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload to Google Drive: fileId is empty");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Google Drive upload failed for file {FileName}", file.FileName);
                }

                // Save screen to database
                _context.Screens.Add(screen);

                // Update session stats
                session.AddFile();
                session.TotalSizeBytes += file.Length;
                session.FileProcessed(true, file.Length);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Uploaded file {FileName} to session {SessionId}", file.FileName, session.SessionId);
                return (true, "File uploaded successfully", screen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to session {SessionId}", file.FileName, sessionId);
                
                // Update session with error
                var session = await _context.UploadSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (session != null)
                {
                    session.FileProcessed(false, file.Length);
                    await _context.SaveChangesAsync();
                }

                return (false, $"Upload failed: {ex.Message}", null);
            }
        }

        public Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Task.FromResult(false);
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("File {FileName} exceeds maximum size limit", file.FileName);
                return Task.FromResult(false);
            }

            // Check content type
            var allowedTypes = AllowedImageTypes.Concat(AllowedDocumentTypes).ToArray();
            if (!allowedTypes.Contains(file.ContentType?.ToLower()))
            {
                _logger.LogWarning("File {FileName} has unsupported content type {ContentType}", file.FileName, file.ContentType);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private async Task<string> SaveFileLocallyAsync(IFormFile file, Screen screen)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "screens");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{screen.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/screens/{fileName}";
        }

        public async Task<UploadSession?> GetSessionAsync(int sessionId)
        {
            return await _context.UploadSessions
                .Include(s => s.Project)
                .Include(s => s.Screens)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<UploadSession?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.UploadSessions
                .Include(s => s.Project)
                .Include(s => s.Screens)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<bool> CompleteSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.UploadSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (session == null) return false;

                session.Complete();
                
                // Update project status
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == session.ProjectId);
                if (project != null)
                {
                    project.MarkAsUploaded();
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing upload session {SessionId}", sessionId);
                return false;
            }
        }

        public async Task<bool> CancelSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.UploadSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (session == null) return false;

                session.Cancel();
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling upload session {SessionId}", sessionId);
                return false;
            }
        }

        public async Task<List<UploadSession>> GetUserSessionsAsync(string userId, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(userId)) return new List<UploadSession>();
            return await _context.UploadSessions
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .Include(s => s.Project)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> DeleteScreenAsync(int screenId)
        {
            try
            {
                var screen = await _context.Screens.FindAsync(screenId);
                if (screen == null) return false;

                // Soft delete
                screen.IsDeleted = true;
                screen.DeletedAt = DateTime.UtcNow;

                // Delete local file if exists
                if (!string.IsNullOrEmpty(screen.FilePath))
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, screen.FilePath.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting screen {ScreenId}", screenId);
                return false;
            }
        }
    }
} 