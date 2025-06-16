using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    // For creating new projects

    public class CreateProjectDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        public DateTime? DueDate { get; set; }
    }

    // For updating projects

    public class UpdateProjectDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required String Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsComplete { get; set; }

    }

    // For returning project data with full details

    public class ProjectResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsComplete { get; set; }
        public int TeamId { get; set; }
        public TeamSummaryDto? Team { get; set; }
        public List<TaskSummaryDto> Tasks { get; set; } = new List<TaskSummaryDto>();
    }


    // For summary views (when showing projects in a list)

    public class ProjectSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsComplete { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}