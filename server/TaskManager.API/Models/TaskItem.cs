namespace TaskManager.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        //Foreign key to team
        // public int TeamId { get; set; }
        // public Team Team { get; set; }

        // In your TaskItem.cs model, update the foreign key:
        public int ProjectId { get; set; }  // Changed from TeamId
        public Project Project { get; set; } = null!;  // Changed from Team



    }
}