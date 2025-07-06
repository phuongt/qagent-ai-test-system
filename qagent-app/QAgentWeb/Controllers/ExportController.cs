using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;

namespace QAgentWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExcelExportService _excelExportService;
        private readonly IGoogleDriveService _googleDriveService;

        public ExportController(
            ApplicationDbContext context,
            IExcelExportService excelExportService,
            IGoogleDriveService googleDriveService)
        {
            _context = context;
            _excelExportService = excelExportService;
            _googleDriveService = googleDriveService;
        }

        // POST: api/Export/excel
        [HttpPost("excel")]
        public async Task<ActionResult<ExportResultDto>> ExportToExcel(ExportRequestDto exportRequest)
        {
            try
            {
                var exportId = Guid.NewGuid().ToString();
                var result = new ExportResultDto
                {
                    ExportId = exportId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow
                };

                // Process export asynchronously
                _ = Task.Run(async () => await ProcessExcelExportAsync(exportRequest, exportId));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Export failed", error = ex.Message });
            }
        }

        // POST: api/Export/csv
        [HttpPost("csv")]
        public async Task<ActionResult<ExportResultDto>> ExportToCsv(ExportRequestDto exportRequest)
        {
            try
            {
                var exportId = Guid.NewGuid().ToString();
                var result = new ExportResultDto
                {
                    ExportId = exportId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow
                };

                // Process export asynchronously
                _ = Task.Run(async () => await ProcessCsvExportAsync(exportRequest, exportId));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Export failed", error = ex.Message });
            }
        }

        // POST: api/Export/google-sheets
        [HttpPost("google-sheets")]
        public async Task<ActionResult<GoogleSheetsResultDto>> ExportToGoogleSheets(GoogleSheetsExportDto exportRequest)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = new GoogleSheetsResultDto
                {
                    CreatedAt = DateTime.UtcNow
                };

                // Get data based on type
                var exportData = await GetExportDataAsync(exportRequest.DataType, exportRequest.ItemIds);

                if (exportData == null || !exportData.Any())
                {
                    return BadRequest("No data found for export");
                }

                // Create or update Google Sheets
                if (exportRequest.CreateNewSpreadsheet)
                {
                    var spreadsheetResult = await CreateGoogleSpreadsheetAsync(exportData, exportRequest);
                    result.SpreadsheetId = spreadsheetResult.SpreadsheetId;
                    result.SpreadsheetUrl = spreadsheetResult.SpreadsheetUrl;
                    result.WorksheetName = spreadsheetResult.WorksheetName;
                    result.TotalRows = spreadsheetResult.TotalRows;
                    result.TotalColumns = spreadsheetResult.TotalColumns;
                }
                else
                {
                    // Update existing spreadsheet
                    await UpdateGoogleSpreadsheetAsync(exportRequest.SpreadsheetId!, exportData, exportRequest);
                    result.SpreadsheetId = exportRequest.SpreadsheetId!;
                    result.SpreadsheetUrl = $"https://docs.google.com/spreadsheets/d/{exportRequest.SpreadsheetId}";
                    result.WorksheetName = exportRequest.WorksheetName ?? "Sheet1";
                }

                // Share with user if requested
                if (exportRequest.ShareWithUser && !string.IsNullOrEmpty(exportRequest.UserEmail))
                {
                    await ShareGoogleSpreadsheetAsync(result.SpreadsheetId, exportRequest.UserEmail);
                    result.IsShared = true;
                    result.SharedWith.Add(exportRequest.UserEmail);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Google Sheets export failed", error = ex.Message });
            }
        }

        // GET: api/Export/result/{exportId}
        [HttpGet("result/{exportId}")]
        public async Task<ActionResult<ExportResultDto>> GetExportResult(string exportId)
        {
            try
            {
                // In a real implementation, this would check a database or cache
                // For now, return a mock result
                var result = new ExportResultDto
                {
                    ExportId = exportId,
                    Status = "Completed",
                    FileName = $"export_{exportId}.xlsx",
                    FilePath = $"/exports/{exportId}.xlsx",
                    DownloadUrl = $"/api/Export/download/{exportId}",
                    FileSizeBytes = 1024 * 50, // 50KB
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    CompletedAt = DateTime.UtcNow.AddMinutes(-2),
                    Metadata = new ExportMetadataDto
                    {
                        ExportType = "excel",
                        DataType = "project",
                        TotalItems = 10,
                        ProcessedItems = 10,
                        IncludedFields = new List<string> { "Name", "Description", "Status", "CreatedAt" },
                        Statistics = new Dictionary<string, object>
                        {
                            ["TotalProjects"] = 10,
                            ["ActiveProjects"] = 8,
                            ["CompletedProjects"] = 2
                        }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get export result", error = ex.Message });
            }
        }

        // GET: api/Export/download/{exportId}
        [HttpGet("download/{exportId}")]
        public async Task<ActionResult> DownloadExport(string exportId)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", $"{exportId}.xlsx");
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Export file not found");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = $"export_{exportId}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Download failed", error = ex.Message });
            }
        }

        // POST: api/Export/bulk
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkExportResultDto>> BulkExport(BulkExportRequestDto bulkRequest)
        {
            try
            {
                var batchId = Guid.NewGuid().ToString();
                var result = new BulkExportResultDto
                {
                    BatchId = batchId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    TotalExports = bulkRequest.Exports.Count
                };

                // Process bulk export asynchronously
                _ = Task.Run(async () => await ProcessBulkExportAsync(bulkRequest, batchId));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bulk export failed", error = ex.Message });
            }
        }

        // GET: api/Export/templates
        [HttpGet("templates")]
        public async Task<ActionResult<List<ExportTemplateDto>>> GetExportTemplates(
            [FromQuery] string? dataType = null,
            [FromQuery] string? exportType = null)
        {
            try
            {
                // Return default templates
                var templates = new List<ExportTemplateDto>
                {
                    new ExportTemplateDto
                    {
                        Id = "project-basic",
                        Name = "Basic Project Export",
                        Description = "Export project basic information",
                        DataType = "project",
                        ExportType = "excel",
                        IsSystem = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Fields = new List<ExportFieldDto>
                        {
                            new ExportFieldDto { Name = "Name", DisplayName = "Project Name", DataType = "string", IsRequired = true, Order = 1 },
                            new ExportFieldDto { Name = "Description", DisplayName = "Description", DataType = "string", IsRequired = false, Order = 2 },
                            new ExportFieldDto { Name = "Status", DisplayName = "Status", DataType = "string", IsRequired = true, Order = 3 },
                            new ExportFieldDto { Name = "CreatedAt", DisplayName = "Created Date", DataType = "datetime", IsRequired = true, Order = 4 }
                        }
                    },
                    new ExportTemplateDto
                    {
                        Id = "screen-detailed",
                        Name = "Detailed Screen Export",
                        Description = "Export screen information with analysis results",
                        DataType = "screen",
                        ExportType = "excel",
                        IsSystem = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Fields = new List<ExportFieldDto>
                        {
                            new ExportFieldDto { Name = "Name", DisplayName = "Screen Name", DataType = "string", IsRequired = true, Order = 1 },
                            new ExportFieldDto { Name = "ProjectName", DisplayName = "Project", DataType = "string", IsRequired = true, Order = 2 },
                            new ExportFieldDto { Name = "ScreenType", DisplayName = "Screen Type", DataType = "string", IsRequired = false, Order = 3 },
                            new ExportFieldDto { Name = "AnalysisStatus", DisplayName = "Analysis Status", DataType = "string", IsRequired = false, Order = 4 },
                            new ExportFieldDto { Name = "CreatedAt", DisplayName = "Created Date", DataType = "datetime", IsRequired = true, Order = 5 }
                        }
                    }
                };

                if (!string.IsNullOrEmpty(dataType))
                {
                    templates = templates.Where(t => t.DataType == dataType).ToList();
                }

                if (!string.IsNullOrEmpty(exportType))
                {
                    templates = templates.Where(t => t.ExportType == exportType).ToList();
                }

                return Ok(templates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get export templates", error = ex.Message });
            }
        }

        // GET: api/Export/history
        [HttpGet("history")]
        public async Task<ActionResult<List<ExportHistoryDto>>> GetExportHistory(
            [FromQuery] int? limit = 50,
            [FromQuery] string? dataType = null,
            [FromQuery] string? exportType = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // In a real implementation, this would query a database
                // For now, return mock data
                var history = new List<ExportHistoryDto>
                {
                    new ExportHistoryDto
                    {
                        ExportId = Guid.NewGuid().ToString(),
                        DataType = "project",
                        ExportType = "excel",
                        Status = "Completed",
                        FileName = "projects_export.xlsx",
                        FileSizeBytes = 1024 * 75,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        CompletedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(2),
                        UserId = userId,
                        UserName = "Current User",
                        IsAvailable = true,
                        ExpiresAt = DateTime.UtcNow.AddDays(6)
                    },
                    new ExportHistoryDto
                    {
                        ExportId = Guid.NewGuid().ToString(),
                        DataType = "screen",
                        ExportType = "csv",
                        Status = "Completed",
                        FileName = "screens_export.csv",
                        FileSizeBytes = 1024 * 25,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        CompletedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(1),
                        UserId = userId,
                        UserName = "Current User",
                        IsAvailable = true,
                        ExpiresAt = DateTime.UtcNow.AddDays(4)
                    }
                };

                if (!string.IsNullOrEmpty(dataType))
                {
                    history = history.Where(h => h.DataType == dataType).ToList();
                }

                if (!string.IsNullOrEmpty(exportType))
                {
                    history = history.Where(h => h.ExportType == exportType).ToList();
                }

                return Ok(history.Take(limit.Value).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get export history", error = ex.Message });
            }
        }

        private async Task ProcessExcelExportAsync(ExportRequestDto exportRequest, string exportId)
        {
            try
            {
                var data = await GetExportDataAsync(exportRequest.DataType, exportRequest.ItemIds);
                
                if (data != null && data.Any())
                {
                    var excelBytes = await _excelExportService.ExportToExcelAsync(data);
                    var fileName = $"{exportId}.xlsx";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    await System.IO.File.WriteAllBytesAsync(filePath, excelBytes);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Excel export failed: {ex.Message}");
            }
        }

        private async Task ProcessCsvExportAsync(ExportRequestDto exportRequest, string exportId)
        {
            try
            {
                var data = await GetExportDataAsync(exportRequest.DataType, exportRequest.ItemIds);
                
                if (data != null && data.Any())
                {
                    var csvContent = GenerateCsvContent(data);
                    var fileName = $"{exportId}.csv";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    await System.IO.File.WriteAllTextAsync(filePath, csvContent);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"CSV export failed: {ex.Message}");
            }
        }

        private async Task ProcessBulkExportAsync(BulkExportRequestDto bulkRequest, string batchId)
        {
            try
            {
                var results = new List<ExportResultDto>();
                
                foreach (var exportRequest in bulkRequest.Exports)
                {
                    var exportId = Guid.NewGuid().ToString();
                    await ProcessExcelExportAsync(exportRequest, exportId);
                    
                    results.Add(new ExportResultDto
                    {
                        ExportId = exportId,
                        Status = "Completed",
                        FileName = $"{exportId}.xlsx",
                        CreatedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    });
                }
                
                // Create zip file if requested
                if (bulkRequest.ZipResults)
                {
                    await CreateZipFileAsync(results, batchId);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Bulk export failed: {ex.Message}");
            }
        }

        private async Task<List<Dictionary<string, object>>> GetExportDataAsync(string dataType, List<int> itemIds)
        {
            var data = new List<Dictionary<string, object>>();
            
            switch (dataType.ToLower())
            {
                case "project":
                    var projects = await _context.Projects
                        .Where(p => itemIds.Contains(p.Id) && !p.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var project in projects)
                    {
                        data.Add(new Dictionary<string, object>
                        {
                            ["Name"] = project.Name,
                            ["Description"] = project.Description ?? "",
                            ["Domain"] = project.Domain,
                            ["Status"] = project.Status,
                            ["CreatedAt"] = project.CreatedAt,
                            ["UpdatedAt"] = project.UpdatedAt
                        });
                    }
                    break;
                    
                case "screen":
                    var screens = await _context.Screens
                        .Include(s => s.Project)
                        .Where(s => itemIds.Contains(s.Id) && !s.IsDeleted)
                        .ToListAsync();
                    
                    foreach (var screen in screens)
                    {
                        data.Add(new Dictionary<string, object>
                        {
                            ["Name"] = screen.Name,
                            ["Description"] = screen.Description ?? "",
                            ["ProjectName"] = screen.Project?.Name ?? "",
                            ["ScreenType"] = screen.ScreenType ?? "",
                            ["AnalysisStatus"] = screen.AnalysisStatus ?? "",
                            ["CreatedAt"] = screen.CreatedAt,
                            ["UpdatedAt"] = screen.UpdatedAt
                        });
                    }
                    break;
            }
            
            return data;
        }

        private string GenerateCsvContent(List<Dictionary<string, object>> data)
        {
            if (!data.Any()) return "";
            
            var headers = string.Join(",", data.First().Keys);
            var rows = data.Select(row => string.Join(",", row.Values.Select(v => $"\"{v}\"")));
            
            return headers + "\n" + string.Join("\n", rows);
        }

        private async Task<GoogleSheetsResultDto> CreateGoogleSpreadsheetAsync(
            List<Dictionary<string, object>> data, 
            GoogleSheetsExportDto exportRequest)
        {
            // This would integrate with Google Sheets API
            // For now, return a mock result
            return new GoogleSheetsResultDto
            {
                SpreadsheetId = Guid.NewGuid().ToString(),
                SpreadsheetUrl = $"https://docs.google.com/spreadsheets/d/{Guid.NewGuid()}",
                WorksheetName = exportRequest.WorksheetName ?? "Export",
                TotalRows = data.Count + 1, // +1 for header
                TotalColumns = data.FirstOrDefault()?.Keys.Count ?? 0
            };
        }

        private async Task UpdateGoogleSpreadsheetAsync(
            string spreadsheetId, 
            List<Dictionary<string, object>> data, 
            GoogleSheetsExportDto exportRequest)
        {
            // This would update existing Google Sheets
            await Task.CompletedTask;
        }

        private async Task ShareGoogleSpreadsheetAsync(string spreadsheetId, string email)
        {
            // This would share the spreadsheet with the user
            await Task.CompletedTask;
        }

        private async Task CreateZipFileAsync(List<ExportResultDto> results, string batchId)
        {
            // This would create a zip file containing all export results
            await Task.CompletedTask;
        }

        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? "anonymous";
        }
    }
}