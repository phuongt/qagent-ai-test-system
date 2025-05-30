using ClosedXML.Excel;
using System.Reflection;

namespace QAgentWeb.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            try
            {
                // Use Task.Run to make the CPU-bound work async
                return await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    
                    // Add headers
                    var properties = typeof(T).GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = properties[i].Name;
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Add data
                    var dataList = data.ToList();
                    for (int row = 0; row < dataList.Count; row++)
                    {
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var value = properties[col].GetValue(dataList[row]);
                            worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
                        }
                    }

                    // Auto-fit columns
                    worksheet.ColumnsUsed().AdjustToContents();

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to Excel");
                throw;
            }
        }

        public async Task<Stream> ExportToExcelStreamAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            var bytes = await ExportToExcelAsync(data, sheetName);
            return new MemoryStream(bytes);
        }

        public async Task<bool> SaveExcelFileAsync<T>(IEnumerable<T> data, string filePath, string sheetName = "Sheet1")
        {
            try
            {
                var bytes = await ExportToExcelAsync(data, sheetName);
                await File.WriteAllBytesAsync(filePath, bytes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Excel file to {FilePath}", filePath);
                return false;
            }
        }
    }
} 