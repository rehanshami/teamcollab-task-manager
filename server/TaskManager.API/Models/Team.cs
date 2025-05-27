using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    }
}