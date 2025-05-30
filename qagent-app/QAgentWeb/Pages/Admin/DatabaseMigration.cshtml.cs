using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QAgentWeb.Services;
using QAgentWeb.Data;

namespace QAgentWeb.Pages.Admin
{
    public class DatabaseMigrationModel : PageModel
    {
        private readonly IDatabaseMigrationService _migrationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseMigrationModel> _logger;

        public DatabaseMigrationModel(
            IDatabaseMigrationService migrationService,
            ApplicationDbContext context,
            ILogger<DatabaseMigrationModel> logger)
        {
            _migrationService = migrationService;
            _context = context;
            _logger = logger;
        }

        public bool IsConnected { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public Dictionary<string, int>? TableCounts { get; set; }

        public async Task OnGetAsync()
        {
            await CheckDatabaseConnectionAsync();
            await LoadTableCountsAsync();
        }

        public async Task<IActionResult> OnPostRunMigrationAsync()
        {
            try
            {
                _logger.LogInformation("Starting database migration from web interface");

                var success = await _migrationService.RunMigrationAsync();

                if (success)
                {
                    Message = "Database migration completed successfully! Tables have been created.";
                    IsSuccess = true;
                    _logger.LogInformation("Database migration completed successfully");
                }
                else
                {
                    Message = "Database migration failed. Please check the logs for details.";
                    IsSuccess = false;
                    _logger.LogError("Database migration failed");
                }
            }
            catch (Exception ex)
            {
                Message = $"Error during migration: {ex.Message}";
                IsSuccess = false;
                _logger.LogError(ex, "Error during database migration");
            }

            await CheckDatabaseConnectionAsync();
            await LoadTableCountsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding from web interface");

                var success = await _migrationService.SeedDataAsync();

                if (success)
                {
                    Message = "Data seeding completed successfully! Sample data has been added to the database.";
                    IsSuccess = true;
                    _logger.LogInformation("Data seeding completed successfully");
                }
                else
                {
                    Message = "Data seeding failed. Please check the logs for details.";
                    IsSuccess = false;
                    _logger.LogError("Data seeding failed");
                }
            }
            catch (Exception ex)
            {
                Message = $"Error during data seeding: {ex.Message}";
                IsSuccess = false;
                _logger.LogError(ex, "Error during data seeding");
            }

            await CheckDatabaseConnectionAsync();
            await LoadTableCountsAsync();
            return Page();
        }

        private async Task CheckDatabaseConnectionAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                _logger.LogWarning(ex, "Database connection check failed");
            }
        }

        private async Task LoadTableCountsAsync()
        {
            try
            {
                if (IsConnected)
                {
                    TableCounts = await _migrationService.GetTableCountsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load table counts");
                TableCounts = new Dictionary<string, int>();
            }
        }
    }
} 