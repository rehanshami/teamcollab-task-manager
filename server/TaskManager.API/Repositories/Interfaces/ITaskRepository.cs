using TaskManager.API.Models;

namespace TaskManager.API.Repositories.Interfaces
{
    public interface ITaskRepository : IRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId);
        Task<IEnumerable<TaskItem>> GetTasksByTeamIdAsync(int teamId);
        Task<bool> TaskExistsInProjectAsync(string title, int projectId, int? excludeTaskId = null);
        Task<TaskItem?> GetTaskWithProjectAsync(int taskId);
    }
}