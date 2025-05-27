using TaskManager.API.Models;

namespace TaskManager.API.Data
{
    public static class DbInitializer
    {
        public static void Seed(TaskManagerDbContext context)
        {
            if (context.Teams.Any()) return; //Already seeded

            var teams = new List<Team>
            {
                new Team
                {
                    Name = "Development team",
                    Description = "Software development team"
                },
                new Team
                {
                    Name = "Marketing Team",
                    Description = "Marketing and promotions team"
                },
            };

            // Add teams to context and save to get their IDs
            context.Teams.AddRange(teams);
            context.SaveChanges();

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Seeded task 1",
                    Description = "This task was seeded",
                    IsComplete = false,
                    CreatedAt = DateTime.UtcNow,
                    TeamId = teams[0].Id
                },
                new TaskItem
                {
                    Title = "Seeded task 2",
                    Description = "This task was seeded",
                    IsComplete = true,
                    CreatedAt = DateTime.UtcNow,
                    TeamId = teams[1].Id
                }
            };

            context.Tasks.AddRange(tasks);
            context.SaveChanges();
        }
    }
}