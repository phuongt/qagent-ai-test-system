using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace QAgentWeb.Tests
{
    public class OracleTestRunner
    {
        private readonly ILogger<OracleTestRunner> _logger;
        private readonly IConfiguration _configuration;
        
        public OracleTestRunner(ILogger<OracleTestRunner> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("=== ORACLE DATABASE TEST RUNNER ===");
            Console.WriteLine($"Started at: {DateTime.Now}");
            Console.WriteLine();
            
            // Setup configuration and services
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information))
                .AddSingleton<IConfiguration>(configuration)
                .AddTransient<OracleTestRunner>()
                .BuildServiceProvider();
            
            var runner = serviceProvider.GetRequiredService<OracleTestRunner>();
            
            try
            {
                return await runner.RunTests(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return -1;
            }
        }
        
        public async Task<int> RunTests(string[] args)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var testResults = new List<TestResult>();
            
            try
            {
                // Parse command line arguments
                var testOptions = ParseArguments(args);
                LogTestConfiguration(testOptions);
                
                // Check prerequisites
                if (!CheckPrerequisites())
                {
                    _logger.LogError("Prerequisites check failed. Cannot continue.");
                    return -1;
                }
                
                // Run Oracle database tests
                var oracleTestResult = await RunOracleDbTest(testOptions);
                testResults.Add(oracleTestResult);
                
                // Generate summary
                totalStopwatch.Stop();
                var summary = GenerateTestSummary(testResults, totalStopwatch.Elapsed);
                
                _logger.LogInformation("=== TEST EXECUTION COMPLETED ===");
                _logger.LogInformation(summary);
                
                // Return appropriate exit code
                return testResults.All(r => r.Success) ? 0 : 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test execution failed with unexpected error");
                return -1;
            }
        }
        
        private TestOptions ParseArguments(string[] args)
        {
            var options = new TestOptions();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--verbose":
                    case "-v":
                        options.Verbose = true;
                        break;
                    case "--cleanup":
                    case "-c":
                        options.EnableCleanup = true;
                        break;
                    case "--no-cleanup":
                        options.EnableCleanup = false;
                        break;
                    case "--timeout":
                    case "-t":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int timeout))
                        {
                            options.TimeoutMinutes = timeout;
                            i++;
                        }
                        break;
                    case "--connection":
                        if (i + 1 < args.Length)
                        {
                            options.ConnectionString = args[i + 1];
                            i++;
                        }
                        break;
                    case "--help":
                    case "-h":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }
            
            return options;
        }
        
        private void ShowHelp()
        {
            Console.WriteLine("Oracle Database Test Runner");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -v, --verbose          Enable verbose logging");
            Console.WriteLine("  -c, --cleanup          Enable automatic cleanup after tests");
            Console.WriteLine("  --no-cleanup           Disable automatic cleanup");
            Console.WriteLine("  -t, --timeout <mins>   Set test timeout in minutes (default: 30)");
            Console.WriteLine("  --connection <conn>    Override connection string");
            Console.WriteLine("  -h, --help             Show this help message");
            Console.WriteLine();
            Console.WriteLine("Environment Variables:");
            Console.WriteLine("  ORACLE_HOST            Oracle database host (default: localhost)");
            Console.WriteLine("  ORACLE_PORT            Oracle database port (default: 1521)");
            Console.WriteLine("  ORACLE_SERVICE_NAME    Oracle service name (default: XEPDB1)");
            Console.WriteLine("  ORACLE_USER            Oracle username (default: QAGENT_USER)");
            Console.WriteLine("  ORACLE_PASSWORD        Oracle password (default: TestPassword123)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run --verbose --cleanup");
            Console.WriteLine("  dotnet run --timeout 60 --no-cleanup");
            Console.WriteLine("  dotnet run --connection \"User Id=test;Password=test;Data Source=localhost:1521/XEPDB1;\"");
        }
        
        private void LogTestConfiguration(TestOptions options)
        {
            _logger.LogInformation("Test Configuration:");
            _logger.LogInformation($"  Verbose: {options.Verbose}");
            _logger.LogInformation($"  Cleanup: {options.EnableCleanup}");
            _logger.LogInformation($"  Timeout: {options.TimeoutMinutes} minutes");
            _logger.LogInformation($"  Connection: {(!string.IsNullOrEmpty(options.ConnectionString) ? "Custom" : "Default")}");
            _logger.LogInformation("");
        }
        
        private bool CheckPrerequisites()
        {
            try
            {
                // Check if Oracle client is available
                _logger.LogInformation("Checking Oracle client availability...");
                
                // Try to load Oracle.ManagedDataAccess
                var oracleAssembly = System.Reflection.Assembly.LoadFrom("Oracle.ManagedDataAccess.dll");
                if (oracleAssembly == null)
                {
                    _logger.LogError("Oracle.ManagedDataAccess.dll not found. Please install Oracle.EntityFrameworkCore package.");
                    return false;
                }
                
                _logger.LogInformation("✓ Oracle client is available");
                
                // Check configuration
                var connectionString = _configuration.GetConnectionString("OracleTestConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Oracle connection string not configured. Please check appsettings.Test.json");
                    return false;
                }
                
                _logger.LogInformation("✓ Configuration is valid");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Prerequisites check failed");
                return false;
            }
        }
        
        private async Task<TestResult> RunOracleDbTest(TestOptions options)
        {
            _logger.LogInformation("Starting Oracle Database Tests...");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var oracleTest = new OracleDbTest();
                var success = await oracleTest.RunCompleteTest();
                
                stopwatch.Stop();
                return new TestResult
                {
                    TestName = "Oracle Database Test",
                    Success = success,
                    Duration = stopwatch.Elapsed,
                    Message = success ? "All tests passed" : "Some tests failed"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Oracle database test failed with exception");
                return new TestResult
                {
                    TestName = "Oracle Database Test",
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    Message = $"Test failed: {ex.Message}"
                };
            }
        }
        
        private string GenerateTestSummary(List<TestResult> results, TimeSpan totalDuration)
        {
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine("Test Summary:");
            summary.AppendLine($"Total Duration: {totalDuration.TotalMinutes:F1} minutes");
            summary.AppendLine($"Tests Run: {results.Count}");
            summary.AppendLine($"Passed: {results.Count(r => r.Success)}");
            summary.AppendLine($"Failed: {results.Count(r => !r.Success)}");
            summary.AppendLine();
            
            foreach (var result in results)
            {
                var status = result.Success ? "PASSED" : "FAILED";
                summary.AppendLine($"{result.TestName}: {status} ({result.Duration.TotalSeconds:F1}s)");
                if (!result.Success)
                {
                    summary.AppendLine($"  Error: {result.Message}");
                }
            }
            
            return summary.ToString();
        }
    }
    
    public class TestOptions
    {
        public bool Verbose { get; set; }
        public bool EnableCleanup { get; set; } = true;
        public int TimeoutMinutes { get; set; } = 30;
        public string ConnectionString { get; set; } = string.Empty;
    }
    
    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}