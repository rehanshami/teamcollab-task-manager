namespace TaskManager.API.DTOs
{
    public class UpdateTaskDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool IsComplete { get; set; }
    }
}