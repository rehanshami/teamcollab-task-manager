namespace TaskManager.API.DTOs
{
    public class CreateTaskDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public int ProjectId { get; set; }
    }
}