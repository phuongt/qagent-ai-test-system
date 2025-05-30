namespace QAgentWeb.Services
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1");
        Task<Stream> ExportToExcelStreamAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1");
        Task<bool> SaveExcelFileAsync<T>(IEnumerable<T> data, string filePath, string sheetName = "Sheet1");
    }
} 