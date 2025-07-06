using System.ComponentModel.DataAnnotations;

namespace QAgentWeb.Models.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Domain { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UserId { get; set; }
        public string? GoogleDriveFolderId { get; set; }
        public string? GoogleDriveFolderUrl { get; set; }
        public int ScreenCount { get; set; }
        public List<ScreenDto>? Screens { get; set; }
    }

    public class CreateProjectDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Domain { get; set; } = "";
    }

    public class UpdateProjectDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string Description { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Domain { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "";
    }
}