using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;

        public ProjectsController(TaskManagerDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of all projects with summary information.
        /// </summary>
        /// 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectSummaryDto>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Team)
                .Include(p => p.Tasks)
                .Select(p => new ProjectSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    DueDate = p.DueDate,
                    IsComplete = p.IsComplete,
                    TaskCount = p.Tasks.Count,
                    CompletedTaskCount = p.Tasks.Count(t => t.IsComplete)
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponseDto>> GetProject(int id)
        {
            var project = await _context.Projects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .Where(p => p.Id == id)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                DueDate = p.DueDate,
                IsComplete = p.IsComplete,
                TeamId = p.TeamId,
                Team = p.Team != null ? new TeamSummaryDto
                {
                    Id = p.Team.Id,
                    Name = p.Team.Name,
                    Description = p.Team.Description
                } : null,
                Tasks = p.Tasks.Select(task => new TaskSummaryDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsComplete = task.IsComplete,
                    CreatedAt = task.CreatedAt
                }).ToList()


            }).FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound($"Project with id: {id} not found");
            }

            return Ok(project);
        }

        ///<summary>
        /// Creates a new project
        /// </summary>
        /// 
        [HttpPost]
        public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectDto dto)
        {
            //Validate team exists
            var team = await _context.Teams.FindAsync(dto.TeamId);
            if (team == null)
            {
                return NotFound($"Team with id: {dto.TeamId} does not exist");
            }

            // Check if project with name exists
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Name == dto.Name && p.TeamId == dto.TeamId);
            if (projectExists)
            {
                return BadRequest($"Project with name: {dto.Name} already exists in this team");
            }

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                TeamId = dto.TeamId,
                DueDate = dto.DueDate,
                IsComplete = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            var responseDto = new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                DueDate = project.DueDate,
                IsComplete = project.IsComplete,
                TeamId = project.TeamId,
                Team = new TeamSummaryDto
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description

                },
                Tasks = new List<TaskSummaryDto>()
            };
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, responseDto);
        }
        ///<summary>
        ///Updates an existing project
        ///</summary>
        ///
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound($"Project with id {id} not found.");
            }

            var nameExists = await _context.Projects
                .AnyAsync(p => p.Name == dto.Name && p.TeamId == project.TeamId && p.Id != id);
            if (nameExists)
            {
                return BadRequest($"Project with name {dto.Name} alredy exists in this team");
            }

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.DueDate = dto.DueDate;
            project.IsComplete = dto.IsComplete;

            await _context.SaveChangesAsync();

            return NoContent();

        }

        ///<summary>
        ///Deletes a project and all it's tasks
        ///</summary>
        ///
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var project = await _context.Projects
               .Include(p => p.Tasks)
               .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound($"Project with id: {id} not found");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Gets all projects for a specific team
        /// </summary>
        /// 
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<ProjectSummaryDto>>> GetProjectsByTeam(int teamId)
        {
            var teamExists = await _context.Teams.AnyAsync(t => t.Id == teamId);
            if (!teamExists)
            {
                return NotFound($"Team with id: {teamId} not found");
            }

            var projects = await _context.Projects
            .Include(p => p.Tasks)
            .Where(p => p.TeamId == teamId)
            .Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                DueDate = p.DueDate,
                IsComplete = p.IsComplete,
                TaskCount = p.Tasks.Count,
                CompletedTaskCount = p.Tasks.Count(t => t.IsComplete)
            }).ToListAsync();

            return Ok(projects);
        }
    }
}