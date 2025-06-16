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

            var projects = new List<Project>
            {
                new Project
                {
                    Name = "Website Redesign",
                    Description = "Complete redesign of the company website",
                    TeamId = teams[0].Id,
                    CreatedAt = DateTime.UtcNow
                },

                new Project
                {
                    Name = "Marketing Campaign Q1",
                    Description = "First quarter marketing campaign",
                    TeamId = teams[1].Id,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Projects.AddRange(projects);
            context.SaveChanges();

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Seeded task 1",
                    Description = "This task was seeded",
                    IsComplete = false,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projects[0].Id
                },
                new TaskItem
                {
                    Title = "Seeded task 2",
                    Description = "This task was seeded",
                    IsComplete = true,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projects[0].Id
                },
                new TaskItem
                {
                    Title = "Seeded task 3",
                    Description = "This task was seeded",
                    IsComplete = true,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projects[0].Id
                },
                new TaskItem
                {
                    Title = "Seeded task 4",
                    Description = "This task was seeded",
                    IsComplete = true,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projects[1].Id
                },
                new TaskItem
                {
                    Title = "Seeded task 5",
                    Description = "This task was seeded",
                    IsComplete = false,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projects[1].Id
                }
            };

            context.Tasks.AddRange(tasks);
            context.SaveChanges();
        }
    }
}