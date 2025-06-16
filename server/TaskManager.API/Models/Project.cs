namespace TaskManager.API.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsComplete { get; set; } = false;

        // Foreign key
        public int TeamId { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    }
}