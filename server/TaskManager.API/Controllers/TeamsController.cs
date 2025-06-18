using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        // private readonly TaskManagerDbContext _context;

        private readonly ITeamRepository _teamRepository;

        public TeamsController(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }
        // public TeamsController(TaskManagerDbContext context)
        // {
        //     _context = context;
        // }

        /// <summary>
        /// Get all teams
        /// </summary>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamResponseDto>>> GetTeams()
        {

            var teams = await _teamRepository.GetTeamsWithProjectSummaryAsync();
            var teamDtos = teams.Select(MapToTeamResponseDto).ToList();
            return Ok(teamDtos);
            // .Include(t => t.Projects)
            // .Select(t => new TeamResponseDto
            // {
            //     Id = t.Id,
            //     Name = t.Name,
            //     Description = t.Description,
            //     CreatedAt = t.CreatedAt,
            //     Projects = t.Projects.Select(project => new ProjectSummaryDto
            //     {
            //         Id = project.Id,
            //         Name = project.Name,
            //         Description = project.Description,
            //         IsComplete = project.IsComplete,
            //         CreatedAt = project.CreatedAt,
            //         TaskCount = project.Tasks.Count(),
            //         CompletedTaskCount = project.Tasks.Count(tasks => tasks.IsComplete)

            //     }).ToList()
            // })
            // .ToListAsync();

            // return Ok(teams);
        }

        /// <summary>
        /// Get team by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamResponseDto>> GetTeam(int id)
        {
            var team = await _teamRepository.GetTeamWithProjectSummaryAsync(id);
            if (team == null)
            {
                return NotFound($"Team with id: {id} not found");
            }
            var teamDto = MapToTeamResponseDto(team);
            return Ok(teamDto);
            // .Include(t => t.Projects)
            // .Where(t => t.Id == id)
            // .Select(t => new TeamResponseDto
            // {
            //     Id = t.Id,
            //     Name = t.Name,
            //     Description = t.Description,
            //     CreatedAt = t.CreatedAt,
            //     Projects = t.Projects.Select(project => new ProjectSummaryDto
            //     {
            //         Id = project.Id,
            //         Name = project.Name,
            //         Description = project.Description,
            //         IsComplete = project.IsComplete,
            //         CreatedAt = project.CreatedAt,
            //         TaskCount = project.Tasks.Count(),
            //         CompletedTaskCount = project.Tasks.Count(t => t.IsComplete)

            //     }).ToList()
            // }).FirstOrDefaultAsync();
            // return Ok(team);
        }

        /// <summary>
        /// Create a new team
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TeamResponseDto>> Create(CreateTeamDto dto)
        {
            var nameExists = await _teamRepository.AnyAsync(t => t.Name == dto.Name);
            // var nameExists = await _context.Teams.AnyAsync(t => t.Name == dto.Name);
            if (nameExists)
            {
                return BadRequest($"Team with name {dto.Name} already exists");
            }
            // Create team object
            var team = new Team
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
            };

            await _teamRepository.AddAsync(team);
            await _teamRepository.SaveChangesAsync();

            // Create the response DTO
            var responseDto = new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                CreatedAt = team.CreatedAt,
                Projects = new List<ProjectSummaryDto>()
            };

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateTeamDto dto)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound($"Team with id: {id} not found");
            }

            // Check if new team name conflicts with an existing team
            var nameExists = await _teamRepository.AnyAsync(t => t.Name == dto.Name && t.Id != id);
            if (nameExists)
            {
                return BadRequest($"Team with name {dto.Name} already exists");
            }
            team.Name = dto.Name;
            team.Description = dto.Description;

            await _teamRepository.UpdateAsync(team);
            await _teamRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var team = await _teamRepository.GetByIdAsync(id);

            if (team == null)
            {
                return NotFound($"Team with id: {id} not found");
            }

            await _teamRepository.DeleteAsync(team);
            await _teamRepository.SaveChangesAsync();
            return NoContent();
        }

        #region Private Helper Methods
        ///<summary>
        /// Maps TaskItem entity to TaskResponseDto
        /// </summary>
        private static TeamResponseDto MapToTeamResponseDto(Team team)
        {
            return new TeamResponseDto
            {

                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                CreatedAt = team.CreatedAt,
                Projects = team.Projects.Select(project => new ProjectSummaryDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    IsComplete = project.IsComplete,
                    CreatedAt = project.CreatedAt,
                    TaskCount = project.Tasks?.Count ?? 0,
                    CompletedTaskCount = project.Tasks?.Count(tasks => tasks.IsComplete) ?? 0
                }).ToList()
            };
        }
        #endregion
    }
}