using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Repositories.Implementations
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        public TeamRepository(TaskManagerDbContext context) : base(context)
        {

        }

        /// <summary>
        /// Checks if a team name already exists. Optionally excludes a specific team
        /// </summary>

        public async Task<bool> TeamNameExistsAsync(string name, int? excludedTeamId = null)
        {
            var query = _dbSet.Where(t => t.Name == name);
            if (excludedTeamId.HasValue)
            {
                query = query.Where(t => t.Id != excludedTeamId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Gets a specific team by ID including all its projects
        /// </summary>

        public async Task<Team?> GetTeamWithProjectsAsync(int teamId)
        {
            return await _dbSet
                .Include(t => t.Projects)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        /// <summary>
        /// Gets all teams including their projects
        /// </summary>
        public async Task<IEnumerable<Team>> GetTeamsWithProjectsAsync()
        {
            return await _dbSet
                .Include(t => t.Projects)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a team with their projects summary including tasks.
        /// </summary>
        /// 
        public async Task<Team?> GetTeamWithProjectSummaryAsync(int teamId)
        {
            return await _dbSet
                .Include(t => t.Projects)
                .ThenInclude(p => p.Tasks)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        /// <summary>
        /// Gets all teams with their projects and tasks
        /// </summary>
        /// 
        public async Task<IEnumerable<Team>> GetTeamsWithProjectSummaryAsync()
        {
            return await _dbSet
                .Include(t => t.Projects)
                .ThenInclude(p => p.Tasks)
                .ToListAsync();
        }
    }
}



//        Task<bool> TeamNameExistsAsync(string name, int? excludedTeamId = null);
// Task<Team?> GetTeamWithProjectsAsync(int teamId);
// Task<IEnumerable<Team>> GetTeamsWithProjectsAsync();