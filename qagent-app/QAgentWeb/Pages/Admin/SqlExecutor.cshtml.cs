using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Text;

namespace QAgentWeb.Pages.Admin
{
    public class SqlExecutorModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlExecutorModel> _logger;

        public SqlExecutorModel(IConfiguration configuration, ILogger<SqlExecutorModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public string SqlQuery { get; set; } = "";
        
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }
        public string QueryResult { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateTablesAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                var createTablesScript = @"
-- Drop tables if they exist (in correct order due to foreign keys)
DROP TABLE IF EXISTS QAgentTasks;
DROP TABLE IF EXISTS Screens;
DROP TABLE IF EXISTS Projects;
DROP TABLE IF EXISTS UploadSessions;
DROP TABLE IF EXISTS Users;

-- Create Users table
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(50) NOT NULL UNIQUE,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_users_userid (UserId),
    INDEX idx_users_username (Username),
    INDEX idx_users_email (Email)
);

-- Create Projects table
CREATE TABLE Projects (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    UserId VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_projects_projectid (ProjectId),
    INDEX idx_projects_userid (UserId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- Create UploadSessions table
CREATE TABLE UploadSessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SessionId INT NOT NULL UNIQUE,
    UserId VARCHAR(50) NOT NULL,
    ProjectId VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Active',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_uploadsessions_sessionid (SessionId),
    INDEX idx_uploadsessions_userid (UserId),
    INDEX idx_uploadsessions_projectid (ProjectId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId) ON DELETE CASCADE
);

-- Create Screens table
CREATE TABLE Screens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ScreenId VARCHAR(50) NOT NULL UNIQUE,
    SessionId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    UploadedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_screens_screenid (ScreenId),
    INDEX idx_screens_sessionid (SessionId),
    FOREIGN KEY (SessionId) REFERENCES UploadSessions(SessionId) ON DELETE CASCADE
);

-- Create QAgentTasks table
CREATE TABLE QAgentTasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TaskId VARCHAR(50) NOT NULL UNIQUE,
    SessionId INT NOT NULL,
    TaskType VARCHAR(100) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    Input TEXT,
    Output TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_qagenttasks_taskid (TaskId),
    INDEX idx_qagenttasks_sessionid (SessionId),
    INDEX idx_qagenttasks_status (Status),
    FOREIGN KEY (SessionId) REFERENCES UploadSessions(SessionId) ON DELETE CASCADE
);";

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(createTablesScript, connection);
                await command.ExecuteNonQueryAsync();
                
                Message = "Tables created successfully!";
                IsSuccess = true;
                _logger.LogInformation("Database tables created successfully");
            }
            catch (Exception ex)
            {
                Message = $"Error creating tables: {ex.Message}";
                IsSuccess = false;
                _logger.LogError(ex, "Error creating database tables");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSeedDataAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                var seedDataScript = @"
-- Insert test user
INSERT IGNORE INTO Users (UserId, Username, Email, PasswordHash) VALUES 
('USR001', 'admin', 'admin@qagent.com', '$2a$11$rQZOKvKxNvGWJmFvEZFzKOYxF8yN5rQZOKvKxNvGWJmFvEZFzKOYxF');

-- Insert test project
INSERT IGNORE INTO Projects (ProjectId, Name, Description, UserId) VALUES 
('PRJ001', 'Test Project', 'A test project for QAgent', 'USR001');

-- Insert test upload session
INSERT IGNORE INTO UploadSessions (SessionId, UserId, ProjectId, Status) VALUES 
(1001, 'USR001', 'PRJ001', 'Active');

-- Insert test screen
INSERT IGNORE INTO Screens (ScreenId, SessionId, FileName, FilePath) VALUES 
('SCR001', 1001, 'test_screen.png', '/uploads/test_screen.png');

-- Insert test task
INSERT IGNORE INTO QAgentTasks (TaskId, SessionId, TaskType, Status, Input) VALUES 
('TSK001', 1001, 'Analysis', 'Pending', 'Analyze uploaded screens');";

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(seedDataScript, connection);
                await command.ExecuteNonQueryAsync();
                
                Message = "Seed data inserted successfully!";
                IsSuccess = true;
                _logger.LogInformation("Seed data inserted successfully");
            }
            catch (Exception ex)
            {
                Message = $"Error seeding data: {ex.Message}";
                IsSuccess = false;
                _logger.LogError(ex, "Error seeding database data");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDropTablesAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                var dropTablesScript = @"
DROP TABLE IF EXISTS QAgentTasks;
DROP TABLE IF EXISTS Screens;
DROP TABLE IF EXISTS Projects;
DROP TABLE IF EXISTS UploadSessions;
DROP TABLE IF EXISTS Users;";

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(dropTablesScript, connection);
                await command.ExecuteNonQueryAsync();
                
                Message = "All tables dropped successfully!";
                IsSuccess = true;
                _logger.LogInformation("Database tables dropped successfully");
            }
            catch (Exception ex)
            {
                Message = $"Error dropping tables: {ex.Message}";
                IsSuccess = false;
                _logger.LogError(ex, "Error dropping database tables");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostExecuteSqlAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SqlQuery))
                {
                    Message = "Please enter a SQL query";
                    IsSuccess = false;
                    return Page();
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var result = new StringBuilder();

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(SqlQuery, connection);
                
                if (SqlQuery.Trim().ToUpper().StartsWith("SELECT"))
                {
                    using var reader = await command.ExecuteReaderAsync();
                    
                    // Get column names
                    var columnCount = reader.FieldCount;
                    var columns = new string[columnCount];
                    for (int i = 0; i < columnCount; i++)
                    {
                        columns[i] = reader.GetName(i);
                    }
                    result.AppendLine(string.Join("\t", columns));
                    result.AppendLine(new string('-', 50));
                    
                    // Get data
                    while (await reader.ReadAsync())
                    {
                        var values = new string[columnCount];
                        for (int i = 0; i < columnCount; i++)
                        {
                            values[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "NULL";
                        }
                        result.AppendLine(string.Join("\t", values));
                    }
                }
                else
                {
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    result.AppendLine($"Query executed successfully. Rows affected: {rowsAffected}");
                }
                
                QueryResult = result.ToString();
                Message = "Query executed successfully!";
                IsSuccess = true;
                _logger.LogInformation("SQL query executed successfully");
            }
            catch (Exception ex)
            {
                Message = $"Error executing query: {ex.Message}";
                IsSuccess = false;
                QueryResult = "";
                _logger.LogError(ex, "Error executing SQL query");
            }

            return Page();
        }
    }
} 