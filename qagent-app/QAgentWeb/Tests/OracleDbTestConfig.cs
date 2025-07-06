using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QAgentWeb.Data;
using QAgentWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Tests
{
    public class OracleDbTestConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string TestDatabase { get; set; } = "QAGENT_TEST";
        public string TestUser { get; set; } = "QAGENT_USER";
        public string TestPassword { get; set; } = "TestPassword123";
        public int ExpectedProjectCount { get; set; } = 10;
        public int ExpectedScreenCount { get; set; } = 50;
        public int ExpectedUserCount { get; set; } = 5;
        public int ExpectedTaskCount { get; set; } = 25;
        public int ExpectedRuleCount { get; set; } = 100;
        public bool EnableAutoCleanup { get; set; } = true;
        public int CommandTimeoutSeconds { get; set; } = 120;
        
        public OracleDbTestConfig()
        {
            // Tự động load từ configuration hoặc environment variables
            LoadFromConfiguration();
        }
        
        private void LoadFromConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            ConnectionString = configuration.GetConnectionString("OracleTestConnection") ?? 
                             BuildOracleConnectionString();
            
            TestDatabase = configuration["OracleTest:Database"] ?? TestDatabase;
            TestUser = configuration["OracleTest:User"] ?? TestUser;
            TestPassword = configuration["OracleTest:Password"] ?? TestPassword;
            
            // Load expected counts từ configuration
            if (int.TryParse(configuration["OracleTest:ExpectedProjectCount"], out int projectCount))
                ExpectedProjectCount = projectCount;
            if (int.TryParse(configuration["OracleTest:ExpectedScreenCount"], out int screenCount))
                ExpectedScreenCount = screenCount;
            if (int.TryParse(configuration["OracleTest:ExpectedUserCount"], out int userCount))
                ExpectedUserCount = userCount;
            if (int.TryParse(configuration["OracleTest:ExpectedTaskCount"], out int taskCount))
                ExpectedTaskCount = taskCount;
            if (int.TryParse(configuration["OracleTest:ExpectedRuleCount"], out int ruleCount))
                ExpectedRuleCount = ruleCount;
                
            if (bool.TryParse(configuration["OracleTest:EnableAutoCleanup"], out bool cleanup))
                EnableAutoCleanup = cleanup;
            if (int.TryParse(configuration["OracleTest:CommandTimeoutSeconds"], out int timeout))
                CommandTimeoutSeconds = timeout;
        }
        
        private string BuildOracleConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("ORACLE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("ORACLE_PORT") ?? "1521";
            var serviceName = Environment.GetEnvironmentVariable("ORACLE_SERVICE_NAME") ?? "XEPDB1";
            
            return $"User Id={TestUser};Password={TestPassword};Data Source={host}:{port}/{serviceName};";
        }
    }
    
    public class OracleTestDbContext : ApplicationDbContext
    {
        private readonly OracleDbTestConfig _config;
        
        public OracleTestDbContext(OracleDbTestConfig config) : base(CreateOptions(config))
        {
            _config = config;
        }
        
        private static DbContextOptions<ApplicationDbContext> CreateOptions(OracleDbTestConfig config)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseOracle(config.ConnectionString, options =>
            {
                options.CommandTimeout(config.CommandTimeoutSeconds);
                options.EnableRetryOnFailure(3);
            });
            return builder.Options;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Oracle-specific configurations
            ConfigureOracleSpecificSettings(modelBuilder);
        }
        
        private void ConfigureOracleSpecificSettings(ModelBuilder modelBuilder)
        {
            // Configure Oracle sequence and identity columns
            modelBuilder.Entity<User>()
                .Property(e => e.Id)
                .UseIdentityColumn(1, 1);
                
            modelBuilder.Entity<Project>()
                .Property(e => e.Id)
                .UseIdentityColumn(1, 1);
                
            modelBuilder.Entity<Screen>()
                .Property(e => e.Id)
                .UseIdentityColumn(1, 1);
                
            modelBuilder.Entity<QAgentTask>()
                .Property(e => e.Id)
                .UseIdentityColumn(1, 1);
                
            modelBuilder.Entity<TestingRule>()
                .Property(e => e.Id)
                .UseIdentityColumn(1, 1);
                
            // Configure Oracle-specific string lengths and types
            modelBuilder.Entity<User>()
                .Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnType("VARCHAR2(50)");
                
            modelBuilder.Entity<Project>()
                .Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnType("VARCHAR2(200)");
                
            modelBuilder.Entity<Screen>()
                .Property(e => e.AnalysisResult)
                .HasColumnType("CLOB");
                
            modelBuilder.Entity<TestingRule>()
                .Property(e => e.RuleTemplate)
                .HasColumnType("CLOB");
                
            // Configure Oracle date/time columns
            modelBuilder.Entity<User>()
                .Property(e => e.CreatedAt)
                .HasColumnType("TIMESTAMP");
                
            modelBuilder.Entity<Project>()
                .Property(e => e.CreatedAt)
                .HasColumnType("TIMESTAMP");
        }
    }
}