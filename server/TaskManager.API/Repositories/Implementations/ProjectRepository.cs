using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Repositories.Implementations
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(TaskManagerDbContext context) : base(context)
        {

        }
        /// <summary>
        /// Gets all projects that belong to a specific team
        /// </summary>
        public async Task<IEnumerable<Project>> GetProjectsByTeamIdAsync(int teamId)
        {
            return await _dbSet
                .Include(p => p.Team)
                .Include(p => p.Tasks)
                .Where(p => p.TeamId == teamId)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a project with the given name already exists in a team
        /// Optionally excludes a specific project (useful for updates)
        /// </summary>
        public async Task<bool> ProjectExistsInTeamAsync(string name, int teamId, int? excludeProjectId = null)
        {
            var query = _dbSet.Where(p => p.Name == name && p.TeamId == teamId);

            if (excludeProjectId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProjectId.Value);
            }

            return await query.AnyAsync();
        }
        /// <summary>
        /// Gets a specific project by it's ID including it's team and task
        /// </summary>
        // - GetProjectWithTeamAndTasksAsync

        public async Task<Project?> GetProjectWithTeamAndTasksAsync(int projectId)
        {
            return await _dbSet
                  .Include(p => p.Team)
                  .Include(p => p.Tasks)
                  .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        ///<summary>
        /// Get all projects with their basic info and tasks count for summary view
        /// </summary>
        /// 
        public async Task<IEnumerable<Project>> GetProjectsWithSummaryAsync()
        {
            return await _dbSet
                .Include(p => p.Team)
                .Include(p => p.Tasks)
                .ToListAsync();

        }
    }
}