using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using System.Text;

namespace QAgentWeb.Services
{
    public interface IDatabaseMigrationService
    {
        Task<bool> RunMigrationAsync();
        Task<bool> SeedDataAsync();
        Task<Dictionary<string, int>> GetTableCountsAsync();
    }

    public class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(ApplicationDbContext context, ILogger<DatabaseMigrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> RunMigrationAsync()
        {
            try
            {
                _logger.LogInformation("Starting database migration...");

                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Run migrations
                await _context.Database.MigrateAsync();

                _logger.LogInformation("Database migration completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database migration");
                return false;
            }
        }

        public async Task<bool> SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding...");

                // Check if data already exists
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Data already exists, skipping seed");
                    return true;
                }

                // Seed Users
                var users = new[]
                {
                    new Models.User
                    {
                        UserId = "admin-001",
                        Email = "admin@qagent.com",
                        FirstName = "Quản trị",
                        LastName = "viên",
                        PasswordHash = "$2a$11$rQZJKAx6p.6VwZqYgzjzUeH8JKxGzJQJ5K5K5K5K5K5K5K5K5K5K5K",
                        IsActive = true,
                        CreatedBy = "system"
                    },
                    new Models.User
                    {
                        UserId = "user-001",
                        Email = "user1@qagent.com",
                        FirstName = "Nguyễn Văn",
                        LastName = "A",
                        PasswordHash = "$2a$11$rQZJKAx6p.6VwZqYgzjzUeH8JKxGzJQJ5K5K5K5K5K5K5K5K5K5K5K",
                        IsActive = true,
                        CreatedBy = "system"
                    },
                    new Models.User
                    {
                        UserId = "user-002",
                        Email = "user2@qagent.com",
                        FirstName = "Trần Thị",
                        LastName = "B",
                        PasswordHash = "$2a$11$rQZJKAx6p.6VwZqYgzjzUeH8JKxGzJQJ5K5K5K5K5K5K5K5K5K5K5K",
                        IsActive = true,
                        CreatedBy = "system"
                    },
                    new Models.User
                    {
                        UserId = "manager-001",
                        Email = "manager1@qagent.com",
                        FirstName = "Lê Văn",
                        LastName = "C",
                        PasswordHash = "$2a$11$rQZJKAx6p.6VwZqYgzjzUeH8JKxGzJQJ5K5K5K5K5K5K5K5K5K5K5K",
                        IsActive = true,
                        CreatedBy = "system"
                    }
                };

                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();

                // Seed Projects
                var projects = new[]
                {
                    new Models.Project
                    {
                        Name = "Website E-commerce",
                        Description = "Phát triển website bán hàng trực tuyến cho khách hàng ABC Company",
                        Domain = Models.Project.Domains.Web,
                        Status = Models.Project.Statuses.Uploaded,
                        UserId = "admin-001",
                        CreatedBy = "admin"
                    },
                    new Models.Project
                    {
                        Name = "Mobile App Banking",
                        Description = "Ứng dụng mobile banking cho ngân hàng XYZ",
                        Domain = Models.Project.Domains.Mobile,
                        Status = Models.Project.Statuses.Analyzing,
                        UserId = "admin-001",
                        CreatedBy = "admin"
                    },
                    new Models.Project
                    {
                        Name = "CRM System",
                        Description = "Hệ thống quản lý khách hàng cho công ty DEF",
                        Domain = Models.Project.Domains.Web,
                        Status = Models.Project.Statuses.Draft,
                        UserId = "manager-001",
                        CreatedBy = "manager1"
                    },
                    new Models.Project
                    {
                        Name = "Landing Page Campaign",
                        Description = "Trang landing cho chiến dịch marketing Q1",
                        Domain = Models.Project.Domains.Web,
                        Status = Models.Project.Statuses.Completed,
                        UserId = "user-001",
                        CreatedBy = "user1"
                    }
                };

                await _context.Projects.AddRangeAsync(projects);
                await _context.SaveChangesAsync();

                // Seed Upload Sessions
                var uploadSessions = new[]
                {
                    new Models.UploadSession
                    {
                        SessionId = 1001,
                        Status = "Completed",
                        TotalFiles = 5,
                        ProcessedFiles = 5,
                        SuccessfulFiles = 5,
                        ProjectId = 1,
                        UserId = "user-001",
                        StartedAt = new DateTime(2024, 1, 20, 10, 0, 0),
                        CreatedBy = "user1"
                    },
                    new Models.UploadSession
                    {
                        SessionId = 1002,
                        Status = "InProgress",
                        TotalFiles = 3,
                        ProcessedFiles = 2,
                        SuccessfulFiles = 2,
                        ProjectId = 2,
                        UserId = "user-002",
                        StartedAt = new DateTime(2024, 1, 21, 14, 30, 0),
                        CreatedBy = "user2"
                    },
                    new Models.UploadSession
                    {
                        SessionId = 1003,
                        Status = "Started",
                        TotalFiles = 0,
                        ProcessedFiles = 0,
                        SuccessfulFiles = 0,
                        ProjectId = 3,
                        UserId = "manager-001",
                        StartedAt = new DateTime(2024, 1, 22, 9, 15, 0),
                        CreatedBy = "manager1"
                    }
                };

                await _context.UploadSessions.AddRangeAsync(uploadSessions);
                await _context.SaveChangesAsync();

                // Seed Screens
                var screens = new[]
                {
                    new Models.Screen
                    {
                        Name = "Homepage Design",
                        Description = "Thiết kế trang chủ website e-commerce",
                        OriginalFileName = "homepage_design.png",
                        FilePath = "/uploads/screens/homepage_design.png",
                        ContentType = "image/png",
                        FileSizeBytes = 2048576,
                        AnalysisStatus = "Completed",
                        ProjectId = 1,
                        UploadSessionId = 1,
                        UserId = "user-001",
                        CreatedBy = "user1"
                    },
                    new Models.Screen
                    {
                        Name = "Product Listing",
                        Description = "Giao diện danh sách sản phẩm",
                        OriginalFileName = "product_list.jpg",
                        FilePath = "/uploads/screens/product_list.jpg",
                        ContentType = "image/jpeg",
                        FileSizeBytes = 1536000,
                        AnalysisStatus = "Completed",
                        ProjectId = 1,
                        UploadSessionId = 1,
                        UserId = "user-001",
                        CreatedBy = "user1"
                    },
                    new Models.Screen
                    {
                        Name = "Shopping Cart",
                        Description = "Màn hình giỏ hàng",
                        OriginalFileName = "shopping_cart.png",
                        FilePath = "/uploads/screens/shopping_cart.png",
                        ContentType = "image/png",
                        FileSizeBytes = 1024000,
                        AnalysisStatus = "InProgress",
                        ProjectId = 1,
                        UploadSessionId = 1,
                        UserId = "user-001",
                        CreatedBy = "user1"
                    },
                    new Models.Screen
                    {
                        Name = "Login Screen",
                        Description = "Màn hình đăng nhập mobile app",
                        OriginalFileName = "login_mobile.png",
                        FilePath = "/uploads/screens/login_mobile.png",
                        ContentType = "image/png",
                        FileSizeBytes = 512000,
                        AnalysisStatus = "Pending",
                        ProjectId = 2,
                        UploadSessionId = 2,
                        UserId = "user-002",
                        CreatedBy = "user2"
                    },
                    new Models.Screen
                    {
                        Name = "Dashboard Overview",
                        Description = "Tổng quan dashboard CRM",
                        OriginalFileName = "crm_dashboard.jpg",
                        FilePath = "/uploads/screens/crm_dashboard.jpg",
                        ContentType = "image/jpeg",
                        FileSizeBytes = 3072000,
                        AnalysisStatus = "Pending",
                        ProjectId = 3,
                        UploadSessionId = 3,
                        UserId = "manager-001",
                        CreatedBy = "manager1"
                    }
                };

                await _context.Screens.AddRangeAsync(screens);
                await _context.SaveChangesAsync();

                // Seed Tasks
                var tasks = new[]
                {
                    new Models.QAgentTask
                    {
                        Title = "Phân tích UI Homepage",
                        Description = "Phân tích và đưa ra gợi ý cải thiện giao diện trang chủ",
                        Status = "Completed",
                        Priority = "High",
                        Category = "UI Analysis",
                        AssignedTo = "user1",
                        DueDate = new DateTime(2024, 1, 25, 17, 0, 0),
                        Progress = 100,
                        UserId = "user-001",
                        CreatedBy = "admin"
                    },
                    new Models.QAgentTask
                    {
                        Title = "Tối ưu UX Product Listing",
                        Description = "Đánh giá và cải thiện trải nghiệm người dùng trang sản phẩm",
                        Status = "InProgress",
                        Priority = "Medium",
                        Category = "UX Review",
                        AssignedTo = "user1",
                        DueDate = new DateTime(2024, 1, 28, 17, 0, 0),
                        Progress = 60,
                        UserId = "user-001",
                        CreatedBy = "admin"
                    },
                    new Models.QAgentTask
                    {
                        Title = "Code Review Shopping Cart",
                        Description = "Review code và logic xử lý giỏ hàng",
                        Status = "Pending",
                        Priority = "High",
                        Category = "Code Review",
                        AssignedTo = "user2",
                        DueDate = new DateTime(2024, 1, 30, 17, 0, 0),
                        Progress = 0,
                        UserId = "user-002",
                        CreatedBy = "manager1"
                    },
                    new Models.QAgentTask
                    {
                        Title = "Security Assessment Login",
                        Description = "Đánh giá bảo mật màn hình đăng nhập",
                        Status = "Pending",
                        Priority = "High",
                        Category = "Security Review",
                        AssignedTo = "manager1",
                        DueDate = new DateTime(2024, 2, 5, 17, 0, 0),
                        Progress = 0,
                        UserId = "manager-001",
                        CreatedBy = "admin"
                    },
                    new Models.QAgentTask
                    {
                        Title = "Performance Test Dashboard",
                        Description = "Kiểm tra hiệu suất dashboard với dữ liệu lớn",
                        Status = "Pending",
                        Priority = "Medium",
                        Category = "Performance Test",
                        AssignedTo = "user2",
                        DueDate = new DateTime(2024, 3, 15, 17, 0, 0),
                        Progress = 0,
                        UserId = "user-002",
                        CreatedBy = "manager1"
                    }
                };

                await _context.QAgentTasks.AddRangeAsync(tasks);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Data seeding completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data seeding");
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetTableCountsAsync()
        {
            try
            {
                var counts = new Dictionary<string, int>
                {
                    ["Users"] = await _context.Users.CountAsync(u => !u.IsDeleted),
                    ["Projects"] = await _context.Projects.CountAsync(p => !p.IsDeleted),
                    ["UploadSessions"] = await _context.UploadSessions.CountAsync(s => !s.IsDeleted),
                    ["Screens"] = await _context.Screens.CountAsync(s => !s.IsDeleted),
                    ["QAgentTasks"] = await _context.QAgentTasks.CountAsync(t => !t.IsDeleted)
                };

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table counts");
                return new Dictionary<string, int>();
            }
        }
    }
} 