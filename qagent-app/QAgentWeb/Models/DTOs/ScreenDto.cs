using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models.DTOs
{
    public class ScreenDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string? FilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public string? AnalysisStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? GoogleDriveFileId { get; set; }
        public string? GoogleDriveFileUrl { get; set; }
        public string? Priority { get; set; }
        public string? ScreenType { get; set; }
        public string? UserId { get; set; }
    }

    public class CreateScreenDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        [Required]
        public int ProjectId { get; set; }

        [MaxLength(50)]
        public string ScreenType { get; set; } = "UI";

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class UpdateScreenDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        [MaxLength(50)]
        public string ScreenType { get; set; } = "UI";

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        [MaxLength(50)]
        public string? AnalysisStatus { get; set; }
    }

    public class ScreenUploadDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public List<IFormFile> Files { get; set; } = new();

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}