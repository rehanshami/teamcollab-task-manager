namespace TaskManager.API.DTOs
{
    public class TeamResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        // public List<TaskSummaryDto> Tasks { get; set; } = new List<TaskSummaryDto>();  // Only basic task info
        public List<ProjectSummaryDto> Projects { get; set; } = new List<ProjectSummaryDto>();

    }
}