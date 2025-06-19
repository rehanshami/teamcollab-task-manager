using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Repositories.Implementations
{
    public class TaskRepository : Repository<TaskItem>, ITaskRepository
    {
        public TaskRepository(TaskManagerDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _dbSet.Where(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByTeamIdAsync(int teamId)
        {
            return await _dbSet
                .Include(t => t.Project)
                .Where(t => t.Project != null && t.Project.TeamId == teamId)
                .ToListAsync();
        }

        public async Task<bool> TaskExistsInProjectAsync(string title, int projectId, int? excludeTaskId = null)
        {
            var query = _dbSet.Where(t => t.Title == title && t.ProjectId == projectId);

            if (excludeTaskId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTaskId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<TaskItem?> GetTaskWithProjectAsync(int taskId)
        {
            return await _dbSet
                .Include(t => t.Project)
                .ThenInclude(p => p.Tasks)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }
    }
}