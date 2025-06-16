using TaskManager.API.Models;

namespace TaskManager.API.Repositories.Interfaces
{
    public interface ITeamRepository : IRepository<Team>
    {
        Task<bool> TeamNameExistsAsync(string name, int? excludedTeamId = null);
        Task<Team?> GetTeamWithProjectsAsync(int teamId);
        Task<IEnumerable<Team>> GetTeamsWithProjectsAsync();
        Task<Team?> GetTeamWithProjectSummaryAsync(int teamId);
        Task<IEnumerable<Team>> GetTeamsWithProjectSummaryAsync();
    }
}