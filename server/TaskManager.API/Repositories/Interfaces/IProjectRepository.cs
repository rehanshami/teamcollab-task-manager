using TaskManager.API.Models;

namespace TaskManager.API.Repositories.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetProjectsByTeamIdAsync(int teamId);
        Task<bool> ProjectExistsInTeamAsync(string name, int teamId, int? excludeProjectId = null);
        Task<Project?> GetProjectWithTeamAndTasksAsync(int projectId);
        Task<IEnumerable<Project>> GetProjectsWithSummaryAsync();
    }
}

// Interfaces are the contract or blueprints. They define what methods a class should have, not how they work

