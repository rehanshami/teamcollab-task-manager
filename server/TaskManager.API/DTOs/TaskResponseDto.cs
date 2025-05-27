namespace TaskManager.API.DTOs
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TeamId { get; set; }
        public TeamSummaryDto? Team { get; set; }
    }
}