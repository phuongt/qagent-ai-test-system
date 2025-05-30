using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;

namespace QAgentWeb.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly ILogger<GoogleDriveService> _logger;
        private readonly IConfiguration _configuration;

        public GoogleDriveService(ILogger<GoogleDriveService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            var credentialPath = _configuration["GoogleDrive:CredentialPath"];
            if (string.IsNullOrEmpty(credentialPath))
            {
                throw new ArgumentException("Google Drive credential path not configured");
            }

            GoogleCredential credential;
            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.Scope.Drive);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "QAgent"
            });
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileContent, string mimeType)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName
                };

                var request = _driveService.Files.Create(fileMetadata, fileContent, mimeType);
                request.Fields = "id";
                
                var file = await request.UploadAsync();
                return request.ResponseBody?.Id ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Google Drive");
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileId)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                var stream = new MemoryStream();
                await request.DownloadAsync(stream);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from Google Drive");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                var request = _driveService.Files.Delete(fileId);
                await request.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Google Drive");
                return false;
            }
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string folderName = "")
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = string.IsNullOrEmpty(folderName) ? "" : $"parents in '{folderName}'";
                request.Fields = "files(id, name)";
                
                var result = await request.ExecuteAsync();
                return result.Files?.Select(f => f.Name) ?? Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files from Google Drive");
                throw;
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                var request = _driveService.Files.List();
                request.PageSize = 1;
                await request.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Drive service is not available");
                return false;
            }
        }
    }
} 