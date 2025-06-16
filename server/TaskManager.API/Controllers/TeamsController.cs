using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;

        public TeamsController(TaskManagerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamResponseDto>>> GetTeams()
        {

            var teams = await _context.Teams
                .Include(t => t.Projects)
                .Select(t => new TeamResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    Projects = t.Projects.Select(project => new ProjectSummaryDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        IsComplete = project.IsComplete,
                        CreatedAt = project.CreatedAt,
                        TaskCount = project.Tasks.Count(),
                        CompletedTaskCount = project.Tasks.Count(tasks => tasks.IsComplete)

                    }).ToList()
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamResponseDto>> GetTeam(int id)
        {
            var team = await _context.Teams
            .Include(t => t.Projects)
            .Where(t => t.Id == id)
            .Select(t => new TeamResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                Projects = t.Projects.Select(project => new ProjectSummaryDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    IsComplete = project.IsComplete,
                    CreatedAt = project.CreatedAt,
                    TaskCount = project.Tasks.Count(),
                    CompletedTaskCount = project.Tasks.Count(t => t.IsComplete)

                }).ToList()
            }).FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound($"Team with id: {id} not found");
            }

            return Ok(team);
        }

        [HttpPost]
        public async Task<ActionResult<TeamResponseDto>> Create(CreateTeamDto dto)
        {
            var nameExists = await _context.Teams.AnyAsync(t => t.Name == dto.Name);
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

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

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
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound($"Team with id: {id} not found");
            }

            var nameExists = await _context.Teams.AnyAsync(t => t.Name == dto.Name && t.Id != id);
            if (nameExists)
            {
                return BadRequest($"Team with name {dto.Name} already exists");
            }
            team.Name = dto.Name;
            team.Description = dto.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
            {
                return NotFound($"Team with id: {id} not found");
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}