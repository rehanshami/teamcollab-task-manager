namespace TaskManager.API.DTOs
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProjectId { get; set; }
        public ProjectSummaryDto? Project { get; set; }
    }
}