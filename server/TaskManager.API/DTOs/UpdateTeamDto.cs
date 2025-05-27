namespace TaskManager.API.DTOs
{
    public class UpdateTeamDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}