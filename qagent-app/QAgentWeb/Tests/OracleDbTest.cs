using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace QAgentWeb.Tests
{
    public class OracleDbTest
    {
        private readonly OracleDbTestConfig _config;
        private readonly StringBuilder _testLog;
        private readonly List<string> _errors;
        private readonly List<string> _warnings;
        private readonly Dictionary<string, int> _actualCounts;
        private readonly Dictionary<string, int> _expectedCounts;
        
        public OracleDbTest()
        {
            _config = new OracleDbTestConfig();
            _testLog = new StringBuilder();
            _errors = new List<string>();
            _warnings = new List<string>();
            _actualCounts = new Dictionary<string, int>();
            _expectedCounts = new Dictionary<string, int>
            {
                { "Users", _config.ExpectedUserCount },
                { "Projects", _config.ExpectedProjectCount },
                { "Screens", _config.ExpectedScreenCount },
                { "QAgentTasks", _config.ExpectedTaskCount },
                { "TestingRules", _config.ExpectedRuleCount },
                { "RuleCategories", 10 }, // Fixed categories
                { "UploadSessions", _config.ExpectedProjectCount / 2 } // 50% of projects have sessions
            };
        }
        
        public async Task<bool> RunCompleteTest()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                LogInfo("=== ORACLE DATABASE TEST STARTED ===");
                LogInfo($"Test Configuration: {JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true })}");
                
                // Step 1: Test Connection
                LogInfo("Step 1: Testing Oracle Connection...");
                if (!await TestConnection())
                {
                    LogError("Oracle connection failed. Test cannot continue.");
                    return false;
                }
                LogInfo("✓ Oracle connection successful");
                
                // Step 2: Setup Database
                LogInfo("Step 2: Setting up database schema...");
                if (!await SetupDatabase())
                {
                    LogError("Database setup failed. Test cannot continue.");
                    return false;
                }
                LogInfo("✓ Database schema setup successful");
                
                // Step 3: Create Test Data
                LogInfo("Step 3: Creating test data...");
                if (!await CreateTestData())
                {
                    LogError("Test data creation failed. Test cannot continue.");
                    return false;
                }
                LogInfo("✓ Test data creation successful");
                
                // Step 4: Verify Record Counts
                LogInfo("Step 4: Verifying record counts...");
                if (!await VerifyRecordCounts())
                {
                    LogError("Record count verification failed.");
                    return false;
                }
                LogInfo("✓ Record count verification successful");
                
                // Step 5: Test Data Relationships
                LogInfo("Step 5: Testing data relationships...");
                if (!await TestDataRelationships())
                {
                    LogError("Data relationship tests failed.");
                    return false;
                }
                LogInfo("✓ Data relationship tests successful");
                
                // Step 6: Performance Tests
                LogInfo("Step 6: Running performance tests...");
                if (!await TestPerformance())
                {
                    LogWarning("Performance tests completed with warnings.");
                }
                else
                {
                    LogInfo("✓ Performance tests successful");
                }
                
                stopwatch.Stop();
                LogInfo($"=== ORACLE DATABASE TEST COMPLETED ===");
                LogInfo($"Total execution time: {stopwatch.ElapsedMilliseconds}ms");
                LogInfo($"Success: {_errors.Count == 0}");
                LogInfo($"Warnings: {_warnings.Count}");
                LogInfo($"Errors: {_errors.Count}");
                
                return _errors.Count == 0;
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error during test execution: {ex.Message}");
                LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                await Cleanup();
                await GenerateTestReport();
            }
        }
        
        private async Task<bool> TestConnection()
        {
            try
            {
                using var context = new OracleTestDbContext(_config);
                await context.Database.OpenConnectionAsync();
                var connectionState = context.Database.GetDbConnection().State;
                
                if (connectionState == System.Data.ConnectionState.Open)
                {
                    // Test a simple query
                    var result = await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM DUAL");
                    return true;
                }
                
                LogError($"Connection state is {connectionState}, expected Open");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Connection test failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> SetupDatabase()
        {
            try
            {
                using var context = new OracleTestDbContext(_config);
                
                // Drop existing tables if they exist (for clean test)
                try
                {
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS RuleApplicationLogs CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS TestingRules CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS RuleCategories CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS AnalysisLogs CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS StandardizedScreens CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS QAgentTasks CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS Screens CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS UploadSessions CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS Projects CASCADE");
                    await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS Users CASCADE");
                }
                catch
                {
                    // Ignore drop table errors - tables might not exist
                }
                
                // Create new schema
                await context.Database.EnsureCreatedAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Database setup failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> CreateTestData()
        {
            try
            {
                using var context = new OracleTestDbContext(_config);
                
                // Create Users
                var users = await CreateUsers(context);
                LogInfo($"Created {users.Count} users");
                
                // Create Rule Categories
                var categories = await CreateRuleCategories(context);
                LogInfo($"Created {categories.Count} rule categories");
                
                // Create Testing Rules
                var rules = await CreateTestingRules(context, categories);
                LogInfo($"Created {rules.Count} testing rules");
                
                // Create Projects
                var projects = await CreateProjects(context, users);
                LogInfo($"Created {projects.Count} projects");
                
                // Create Upload Sessions
                var sessions = await CreateUploadSessions(context, projects, users);
                LogInfo($"Created {sessions.Count} upload sessions");
                
                // Create Screens
                var screens = await CreateScreens(context, projects, sessions, users);
                LogInfo($"Created {screens.Count} screens");
                
                // Create QAgent Tasks
                var tasks = await CreateQAgentTasks(context, users, projects, screens);
                LogInfo($"Created {tasks.Count} QAgent tasks");
                
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Test data creation failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task<List<User>> CreateUsers(OracleTestDbContext context)
        {
            var users = new List<User>();
            
            for (int i = 1; i <= _config.ExpectedUserCount; i++)
            {
                var user = new User
                {
                    UserId = $"USER_{i:D3}",
                    Email = $"user{i}@qagent.test",
                    FirstName = $"User{i}",
                    LastName = $"Test{i}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword($"Password{i}"),
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                    CreatedBy = "TestSystem"
                };
                
                users.Add(user);
                context.Users.Add(user);
            }
            
            await context.SaveChangesAsync();
            return users;
        }
        
        private async Task<List<RuleCategory>> CreateRuleCategories(OracleTestDbContext context)
        {
            var categories = new List<RuleCategory>
            {
                new() { CategoryId = "UI_COMP", CategoryName = "UI Component Testing", Description = "Rules for UI component testing", SortOrder = 1 },
                new() { CategoryId = "FUNC_TEST", CategoryName = "Functional Testing", Description = "Rules for functional testing", SortOrder = 2 },
                new() { CategoryId = "USABILITY", CategoryName = "Usability Testing", Description = "Rules for usability testing", SortOrder = 3 },
                new() { CategoryId = "SECURITY", CategoryName = "Security Testing", Description = "Rules for security testing", SortOrder = 4 },
                new() { CategoryId = "PERF", CategoryName = "Performance Testing", Description = "Rules for performance testing", SortOrder = 5 },
                new() { CategoryId = "COMPAT", CategoryName = "Compatibility Testing", Description = "Rules for compatibility testing", SortOrder = 6 },
                new() { CategoryId = "ACCESS", CategoryName = "Accessibility Testing", Description = "Rules for accessibility testing", SortOrder = 7 },
                new() { CategoryId = "INTEGR", CategoryName = "Integration Testing", Description = "Rules for integration testing", SortOrder = 8 },
                new() { CategoryId = "REGRESS", CategoryName = "Regression Testing", Description = "Rules for regression testing", SortOrder = 9 },
                new() { CategoryId = "ACCEPT", CategoryName = "Acceptance Testing", Description = "Rules for acceptance testing", SortOrder = 10 }
            };
            
            foreach (var category in categories)
            {
                category.CreatedAt = DateTime.UtcNow;
                category.CreatedBy = "TestSystem";
                context.RuleCategories.Add(category);
            }
            
            await context.SaveChangesAsync();
            return categories;
        }
        
        private async Task<List<TestingRule>> CreateTestingRules(OracleTestDbContext context, List<RuleCategory> categories)
        {
            var rules = new List<TestingRule>();
            var ruleTypes = new[] { "Validation", "Verification", "Inspection", "Review" };
            
            for (int i = 1; i <= _config.ExpectedRuleCount; i++)
            {
                var category = categories[i % categories.Count];
                var rule = new TestingRule
                {
                    RuleId = $"RULE_{i:D3}",
                    RuleName = $"Testing Rule {i}",
                    RuleCategory = category.CategoryId,
                    RuleCategoryId = category.Id,
                    RuleType = ruleTypes[i % ruleTypes.Length],
                    Description = $"This is testing rule {i} for {category.CategoryName}",
                    DescriptionVi = $"Đây là quy tắc kiểm thử {i} cho {category.CategoryName}",
                    RuleTemplate = $"Template for rule {i}: Check {{element}} for {{condition}}",
                    Priority = (i % 4) + 1,
                    IsActive = true,
                    Version = "1.0",
                    CreatedAt = DateTime.UtcNow.AddDays(-20 + (i % 20)),
                    CreatedBy = "TestSystem"
                };
                
                rules.Add(rule);
                context.TestingRules.Add(rule);
            }
            
            await context.SaveChangesAsync();
            return rules;
        }
        
        private async Task<List<Project>> CreateProjects(OracleTestDbContext context, List<User> users)
        {
            var projects = new List<Project>();
            var domains = Project.Domains.GetAll();
            var statuses = Project.Statuses.GetAll();
            
            for (int i = 1; i <= _config.ExpectedProjectCount; i++)
            {
                var user = users[i % users.Count];
                var project = new Project
                {
                    Name = $"Test Project {i}",
                    Description = $"Description for test project {i}",
                    Domain = domains[i % domains.Count],
                    Status = statuses[i % statuses.Count],
                    UserId = user.UserId,
                    CreatedAt = DateTime.UtcNow.AddDays(-15 + (i % 15)),
                    CreatedBy = "TestSystem"
                };
                
                projects.Add(project);
                context.Projects.Add(project);
            }
            
            await context.SaveChangesAsync();
            return projects;
        }
        
        private async Task<List<UploadSession>> CreateUploadSessions(OracleTestDbContext context, List<Project> projects, List<User> users)
        {
            var sessions = new List<UploadSession>();
            var sessionCount = _expectedCounts["UploadSessions"];
            
            for (int i = 1; i <= sessionCount; i++)
            {
                var project = projects[i % projects.Count];
                var user = users[i % users.Count];
                var session = new UploadSession
                {
                    ProjectId = project.Id,
                    UserId = user.UserId,
                    Status = "Completed",
                    TotalFiles = 5 + (i % 10),
                    ProcessedFiles = 5 + (i % 10),
                    Notes = $"Upload session {i} for project {project.Name}",
                    CreatedAt = DateTime.UtcNow.AddDays(-10 + (i % 10)),
                    CreatedBy = "TestSystem"
                };
                
                sessions.Add(session);
                context.UploadSessions.Add(session);
            }
            
            await context.SaveChangesAsync();
            return sessions;
        }
        
        private async Task<List<Screen>> CreateScreens(OracleTestDbContext context, List<Project> projects, List<UploadSession> sessions, List<User> users)
        {
            var screens = new List<Screen>();
            var screenTypes = Screen.ScreenTypes.GetAll();
            var priorities = Screen.Priorities.GetAll();
            var statuses = Screen.AnalysisStatuses.GetAll();
            
            for (int i = 1; i <= _config.ExpectedScreenCount; i++)
            {
                var project = projects[i % projects.Count];
                var session = sessions[i % sessions.Count];
                var user = users[i % users.Count];
                
                var screen = new Screen
                {
                    Name = $"Screen {i}",
                    Description = $"Test screen {i} for project {project.Name}",
                    ScreenType = screenTypes[i % screenTypes.Count],
                    Priority = priorities[i % priorities.Count],
                    AnalysisStatus = statuses[i % statuses.Count],
                    ProjectId = project.Id,
                    UploadSessionId = session.Id,
                    UserId = user.UserId,
                    OriginalFileName = $"screen_{i}.png",
                    FilePath = $"/test/screens/screen_{i}.png",
                    ContentType = "image/png",
                    FileSizeBytes = 1024 * (100 + i % 500),
                    AnalysisConfidence = 0.7 + (i % 3) * 0.1,
                    ComplexityScore = 1 + (i % 5),
                    CreatedAt = DateTime.UtcNow.AddDays(-5 + (i % 5)),
                    CreatedBy = "TestSystem"
                };
                
                screens.Add(screen);
                context.Screens.Add(screen);
            }
            
            await context.SaveChangesAsync();
            return screens;
        }
        
        private async Task<List<QAgentTask>> CreateQAgentTasks(OracleTestDbContext context, List<User> users, List<Project> projects, List<Screen> screens)
        {
            var tasks = new List<QAgentTask>();
            var statuses = new[] { "Open", "InProgress", "Completed", "Cancelled" };
            var priorities = new[] { "Low", "Medium", "High", "Critical" };
            var taskTypes = new[] { "Analysis", "Review", "Testing", "Documentation" };
            
            for (int i = 1; i <= _config.ExpectedTaskCount; i++)
            {
                var user = users[i % users.Count];
                var project = projects[i % projects.Count];
                var screen = screens[i % screens.Count];
                
                var task = new QAgentTask
                {
                    TaskId = $"TASK_{i:D3}",
                    Title = $"Test Task {i}",
                    Description = $"Description for test task {i}",
                    Status = statuses[i % statuses.Length],
                    Priority = priorities[i % priorities.Length],
                    TaskType = taskTypes[i % taskTypes.Length],
                    UserId = user.UserId,
                    ProjectId = project.Id,
                    ScreenId = screen.Id,
                    AssignedTo = user.UserId,
                    DueDate = DateTime.UtcNow.AddDays(7 + (i % 14)),
                    CreatedAt = DateTime.UtcNow.AddDays(-3 + (i % 3)),
                    CreatedBy = "TestSystem"
                };
                
                tasks.Add(task);
                context.QAgentTasks.Add(task);
            }
            
            await context.SaveChangesAsync();
            return tasks;
        }
        
        private async Task<bool> VerifyRecordCounts()
        {
            try
            {
                using var context = new OracleTestDbContext(_config);
                
                _actualCounts["Users"] = await context.Users.CountAsync();
                _actualCounts["Projects"] = await context.Projects.CountAsync();
                _actualCounts["Screens"] = await context.Screens.CountAsync();
                _actualCounts["QAgentTasks"] = await context.QAgentTasks.CountAsync();
                _actualCounts["TestingRules"] = await context.TestingRules.CountAsync();
                _actualCounts["RuleCategories"] = await context.RuleCategories.CountAsync();
                _actualCounts["UploadSessions"] = await context.UploadSessions.CountAsync();
                
                bool allCountsMatch = true;
                
                foreach (var expectedCount in _expectedCounts)
                {
                    var tableName = expectedCount.Key;
                    var expected = expectedCount.Value;
                    var actual = _actualCounts.GetValueOrDefault(tableName, 0);
                    
                    if (actual == expected)
                    {
                        LogInfo($"✓ {tableName}: {actual} records (expected: {expected})");
                    }
                    else
                    {
                        LogError($"✗ {tableName}: {actual} records (expected: {expected}) - Difference: {actual - expected}");
                        allCountsMatch = false;
                    }
                }
                
                return allCountsMatch;
            }
            catch (Exception ex)
            {
                LogError($"Record count verification failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> TestDataRelationships()
        {
            try
            {
                using var context = new OracleTestDbContext(_config);
                
                // Test User-Project relationship
                var userWithProjects = await context.Users
                    .Include(u => u.Projects)
                    .FirstOrDefaultAsync();
                
                if (userWithProjects?.Projects?.Count > 0)
                {
                    LogInfo($"✓ User-Project relationship: User {userWithProjects.UserId} has {userWithProjects.Projects.Count} projects");
                }
                else
                {
                    LogError("✗ User-Project relationship: No projects found for user");
                    return false;
                }
                
                // Test Project-Screen relationship
                var projectWithScreens = await context.Projects
                    .Include(p => p.Screens)
                    .FirstOrDefaultAsync();
                
                if (projectWithScreens?.Screens?.Count > 0)
                {
                    LogInfo($"✓ Project-Screen relationship: Project {projectWithScreens.Name} has {projectWithScreens.Screens.Count} screens");
                }
                else
                {
                    LogError("✗ Project-Screen relationship: No screens found for project");
                    return false;
                }
                
                // Test Rule Category-Testing Rule relationship
                var categoryWithRules = await context.RuleCategories
                    .Include(c => c.TestingRules)
                    .FirstOrDefaultAsync();
                
                if (categoryWithRules?.TestingRules?.Count > 0)
                {
                    LogInfo($"✓ Category-Rule relationship: Category {categoryWithRules.CategoryName} has {categoryWithRules.TestingRules.Count} rules");
                }
                else
                {
                    LogError("✗ Category-Rule relationship: No rules found for category");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Data relationship test failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> TestPerformance()
        {
            try
            {
                using var context = new OracleTestDbContext(_config);
                var performanceResults = new Dictionary<string, long>();
                
                // Test 1: Simple select query
                var stopwatch = Stopwatch.StartNew();
                var userCount = await context.Users.CountAsync();
                stopwatch.Stop();
                performanceResults["Simple Count Query"] = stopwatch.ElapsedMilliseconds;
                
                // Test 2: Complex join query
                stopwatch.Restart();
                var complexQuery = await context.Projects
                    .Include(p => p.Screens)
                    .Include(p => p.User)
                    .Where(p => p.Screens.Count > 0)
                    .Take(10)
                    .ToListAsync();
                stopwatch.Stop();
                performanceResults["Complex Join Query"] = stopwatch.ElapsedMilliseconds;
                
                // Test 3: Aggregation query
                stopwatch.Restart();
                var aggregationQuery = await context.Screens
                    .GroupBy(s => s.ScreenType)
                    .Select(g => new { ScreenType = g.Key, Count = g.Count() })
                    .ToListAsync();
                stopwatch.Stop();
                performanceResults["Aggregation Query"] = stopwatch.ElapsedMilliseconds;
                
                // Log performance results
                foreach (var result in performanceResults)
                {
                    var message = $"Performance - {result.Key}: {result.Value}ms";
                    if (result.Value > 5000) // 5 seconds threshold
                    {
                        LogWarning(message + " (SLOW)");
                    }
                    else
                    {
                        LogInfo($"✓ {message}");
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Performance test failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task Cleanup()
        {
            if (!_config.EnableAutoCleanup)
            {
                LogInfo("Auto cleanup is disabled. Test data will remain in database.");
                return;
            }
            
            try
            {
                using var context = new OracleTestDbContext(_config);
                
                LogInfo("Cleaning up test data...");
                
                // Remove test data in reverse order of creation
                await context.Database.ExecuteSqlRawAsync("DELETE FROM RuleApplicationLogs WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM QAgentTasks WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM Screens WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM UploadSessions WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM Projects WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM TestingRules WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM RuleCategories WHERE CreatedBy = 'TestSystem'");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE CreatedBy = 'TestSystem'");
                
                LogInfo("✓ Test data cleanup completed");
            }
            catch (Exception ex)
            {
                LogWarning($"Cleanup failed: {ex.Message}");
            }
        }
        
        private async Task GenerateTestReport()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), $"Oracle_Test_Report_{timestamp}.md");
            
            var report = new StringBuilder();
            report.AppendLine("# Oracle Database Test Report");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            // Test Summary
            report.AppendLine("## Test Summary");
            report.AppendLine($"- **Status**: {(_errors.Count == 0 ? "PASSED" : "FAILED")}");
            report.AppendLine($"- **Total Errors**: {_errors.Count}");
            report.AppendLine($"- **Total Warnings**: {_warnings.Count}");
            report.AppendLine();
            
            // Configuration
            report.AppendLine("## Test Configuration");
            report.AppendLine($"- **Database**: {_config.TestDatabase}");
            report.AppendLine($"- **User**: {_config.TestUser}");
            report.AppendLine($"- **Connection Timeout**: {_config.CommandTimeoutSeconds}s");
            report.AppendLine($"- **Auto Cleanup**: {_config.EnableAutoCleanup}");
            report.AppendLine();
            
            // Record Counts
            report.AppendLine("## Record Count Verification");
            report.AppendLine("| Table | Expected | Actual | Status |");
            report.AppendLine("|-------|----------|---------|--------|");
            
            foreach (var expected in _expectedCounts)
            {
                var actual = _actualCounts.GetValueOrDefault(expected.Key, 0);
                var status = actual == expected.Value ? "✓ PASS" : "✗ FAIL";
                report.AppendLine($"| {expected.Key} | {expected.Value} | {actual} | {status} |");
            }
            report.AppendLine();
            
            // Errors
            if (_errors.Count > 0)
            {
                report.AppendLine("## Errors");
                foreach (var error in _errors)
                {
                    report.AppendLine($"- {error}");
                }
                report.AppendLine();
            }
            
            // Warnings
            if (_warnings.Count > 0)
            {
                report.AppendLine("## Warnings");
                foreach (var warning in _warnings)
                {
                    report.AppendLine($"- {warning}");
                }
                report.AppendLine();
            }
            
            // Full Log
            report.AppendLine("## Full Test Log");
            report.AppendLine("```");
            report.AppendLine(_testLog.ToString());
            report.AppendLine("```");
            
            await File.WriteAllTextAsync(reportPath, report.ToString());
            LogInfo($"Test report generated: {reportPath}");
        }
        
        private void LogInfo(string message)
        {
            var logMessage = $"[INFO] {DateTime.Now:HH:mm:ss} {message}";
            _testLog.AppendLine(logMessage);
            Console.WriteLine(logMessage);
        }
        
        private void LogWarning(string message)
        {
            var logMessage = $"[WARN] {DateTime.Now:HH:mm:ss} {message}";
            _testLog.AppendLine(logMessage);
            _warnings.Add(message);
            Console.WriteLine(logMessage);
        }
        
        private void LogError(string message)
        {
            var logMessage = $"[ERROR] {DateTime.Now:HH:mm:ss} {message}";
            _testLog.AppendLine(logMessage);
            _errors.Add(message);
            Console.WriteLine(logMessage);
        }
    }
}