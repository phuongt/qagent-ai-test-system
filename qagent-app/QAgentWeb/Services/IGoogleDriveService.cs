namespace QAgentWeb.Services
{
    public interface IGoogleDriveService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileContent, string mimeType);
        Task<Stream> DownloadFileAsync(string fileId);
        Task<bool> DeleteFileAsync(string fileId);
        Task<IEnumerable<string>> ListFilesAsync(string folderName = "");
        Task<bool> IsServiceAvailableAsync();
    }
} 