using TaskManager.API.Models;

namespace TaskManager.API.Data
{
    public static class DbInitializer
    {
        public static void Seed(TaskManagerDbContext context)
        {
            if (context.Tasks.Any()) return; //Already seeded

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Seeded task 1",
                    Description = "This task was seeded",
                    IsComplete = false,
                    CreatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Seeded task 2",
                    Description = "This task was seeded",
                    IsComplete = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Tasks.AddRange(tasks);
            context.SaveChanges();
        }
    }
}