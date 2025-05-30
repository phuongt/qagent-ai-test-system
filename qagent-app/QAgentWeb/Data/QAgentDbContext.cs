using Microsoft.EntityFrameworkCore;
using QAgentWeb.Models;

namespace QAgentWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Authentication Models
        public DbSet<User> Users { get; set; }
        
        // UC01 Models
        public DbSet<Project> Projects { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<UploadSession> UploadSessions { get; set; }
        
        // UC02 Models
        public DbSet<StandardizedScreen> StandardizedScreens { get; set; }
        public DbSet<AnalysisLog> AnalysisLogs { get; set; }
        
        // UC03 - ISTQB Rules Models
        public DbSet<TestingRule> TestingRules { get; set; }
        public DbSet<RuleCategory> RuleCategories { get; set; }
        public DbSet<RuleApplicationLog> RuleApplicationLogs { get; set; }
        
        // Existing models
        public DbSet<QAgentTask> QAgentTasks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure soft delete filter for all BaseEntity
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Screen>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<UploadSession>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<StandardizedScreen>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AnalysisLog>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<QAgentTask>().HasQueryFilter(e => !e.IsDeleted);
            
            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.EmailConfirmationToken).HasMaxLength(500);
                entity.Property(e => e.ResetPasswordToken).HasMaxLength(500);
                
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserId).IsUnique();
                
                // Relationships đã được cấu hình ở phía ngược lại
            });
            
            // Configure Project
            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Domain).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GoogleDriveFolderId).HasMaxLength(100);
                entity.Property(e => e.GoogleDriveFolderUrl).HasMaxLength(500);
                entity.Property(e => e.UserId).HasMaxLength(50);
                
                // Relationships
                entity.HasMany(p => p.Screens)
                    .WithOne(s => s.Project)
                    .HasForeignKey(s => s.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(p => p.UploadSessions)
                    .WithOne(u => u.Project)
                    .HasForeignKey(u => u.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Projects)
                    .HasPrincipalKey(u => u.UserId)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure Screen
            modelBuilder.Entity<Screen>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ScreenType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AnalysisStatus).HasMaxLength(50);
                entity.Property(e => e.OriginalFileName).HasMaxLength(200);
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.GoogleDriveFileId).HasMaxLength(100);
                entity.Property(e => e.GoogleDriveFileUrl).HasMaxLength(500);
                entity.Property(e => e.ContentType).HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(50);
                
                // Relationships
                entity.HasOne(s => s.UploadSession)
                    .WithMany(u => u.Screens)
                    .HasForeignKey(s => s.UploadSessionId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(s => s.User)
                    .WithMany(u => u.Screens)
                    .HasPrincipalKey(u => u.UserId)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure UploadSession
            modelBuilder.Entity<UploadSession>(entity =>
            {
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.UserId).HasMaxLength(50);
                
                // Relationship với User
                entity.HasOne(us => us.User)
                    .WithMany(u => u.UploadSessions)
                    .HasPrincipalKey(u => u.UserId)
                    .HasForeignKey(us => us.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure QAgentTask
            modelBuilder.Entity<QAgentTask>(entity =>
            {
                entity.Property(e => e.TaskId)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
                    
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Priority)
                    .IsRequired()
                    .HasMaxLength(20);
                    
                entity.Property(e => e.TaskType)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.AssignedTo)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Category)
                    .HasMaxLength(100);
                    
                entity.Property(e => e.UserId)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.DeletedBy)
                    .HasMaxLength(50);
                    
                // Relationships
                entity.HasOne(t => t.User)
                    .WithMany(u => u.QAgentTasks)
                    .HasPrincipalKey(u => u.UserId)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(t => t.Project)
                    .WithMany()
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(t => t.Screen)
                    .WithMany()
                    .HasForeignKey(t => t.ScreenId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(t => t.ParentTask)
                    .WithMany(t => t.SubTasks)
                    .HasForeignKey(t => t.ParentTaskId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure StandardizedScreen
            modelBuilder.Entity<StandardizedScreen>(entity =>
            {
                entity.Property(e => e.ScreenId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FunctionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ScreenName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ScreenType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Configure JSON columns
                entity.Property(e => e.UIElements)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<UIElement>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<UIElement>());
                        
                entity.Property(e => e.BusinessFunctions)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<BusinessFunction>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<BusinessFunction>());
                        
                entity.Property(e => e.Workflows)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<Workflow>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<Workflow>());
                        
                entity.Property(e => e.SourceImages)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<string>());
                
                // Relationships
                entity.HasMany(s => s.AnalysisLogs)
                    .WithOne(a => a.StandardizedScreen)
                    .HasPrincipalKey(s => s.ScreenId)
                    .HasForeignKey(a => a.StandardizedScreenId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure AnalysisLog
            modelBuilder.Entity<AnalysisLog>(entity =>
            {
                entity.Property(e => e.FunctionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.AnalysisStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AIModelUsed).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.StandardizedScreenId).HasMaxLength(50);
            });
            
            // Configure TestingRule
            modelBuilder.Entity<TestingRule>(entity =>
            {
                entity.Property(e => e.RuleId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RuleName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RuleCategory).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.DescriptionVi).HasMaxLength(1000);
                entity.Property(e => e.RuleTemplate).HasMaxLength(2000);
                entity.Property(e => e.Version).HasMaxLength(20);
                
                // JSON fields
                entity.Property(e => e.Applicability)
                    .HasConversion(
                        v => v,
                        v => v);
                        
                entity.Property(e => e.Examples)
                    .HasConversion(
                        v => v,
                        v => v);
                
                // Relationships
                entity.HasOne(r => r.Category)
                    .WithMany(c => c.TestingRules)
                    .HasForeignKey(r => r.RuleCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(r => r.ApplicationLogs)
                    .WithOne(l => l.TestingRule)
                    .HasForeignKey(l => l.TestingRuleId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => e.RuleId).IsUnique();
                entity.HasIndex(e => e.RuleCategory);
                entity.HasIndex(e => e.Priority);
            });
            
            // Configure RuleCategory
            modelBuilder.Entity<RuleCategory>(entity =>
            {
                entity.Property(e => e.CategoryId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ISTQBReference).HasMaxLength(200);
                
                entity.HasIndex(e => e.CategoryId).IsUnique();
                entity.HasIndex(e => e.SortOrder);
            });
            
            // Configure RuleApplicationLog
            modelBuilder.Entity<RuleApplicationLog>(entity =>
            {
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                
                // JSON field
                entity.Property(e => e.AppliedContext)
                    .HasConversion(
                        v => v,
                        v => v);
                
                // Relationships
                entity.HasOne(l => l.Project)
                    .WithMany()
                    .HasForeignKey(l => l.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(l => l.Screen)
                    .WithMany()
                    .HasForeignKey(l => l.ScreenId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(l => l.User)
                    .WithMany()
                    .HasPrincipalKey(u => u.UserId)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.AppliedAt);
                entity.HasIndex(e => e.WasSuccessful);
            });
            
            // UC03 - ISTQB Rules soft delete filters
            modelBuilder.Entity<TestingRule>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RuleCategory>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RuleApplicationLog>().HasQueryFilter(e => !e.IsDeleted);
        }
        
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = "system"; // TODO: Get from current user
                        break;
                        
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = "system"; // TODO: Get from current user
                        break;
                }
            }
        }
    }
} 