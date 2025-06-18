using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;
using TaskManager.API.Repositories.Implementations;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        // private readonly TaskManagerDbContext _context;
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;


        public ProjectsController(IProjectRepository projectRepository, ITeamRepository teamRepository)
        {
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Returns a list of all projects with summary information.
        /// </summary>
        /// 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectSummaryDto>>> GetProjects()
        {
            var projects = await _projectRepository.GetProjectsWithSummaryAsync();
            var projectsDto = projects.Select(MapToProjectResponseDto);
            // var projects = await _context.Projects
            //     .Include(p => p.Team)
            //     .Include(p => p.Tasks)
            //     .Select(p => new ProjectSummaryDto
            //     {
            //         Id = p.Id,
            //         Name = p.Name,
            //         Description = p.Description,
            //         CreatedAt = p.CreatedAt,
            //         DueDate = p.DueDate,
            //         IsComplete = p.IsComplete,
            //         TaskCount = p.Tasks.Count,
            //         CompletedTaskCount = p.Tasks.Count(t => t.IsComplete)
            //     })
            // .ToListAsync();

            return Ok(projectsDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponseDto>> GetProject(int id)
        {
            var project = await _projectRepository.GetProjectWithTeamAndTasksAsync(id);
            if (project == null)
            {
                return NotFound($"Project with id: {id} not found");
            }
            var projectDto = MapToProjectResponseDto(project);
            return Ok(projectDto);
            // var project = await _context.Projects
            // .Include(p => p.Team)
            // .Include(p => p.Tasks)
            // .Where(p => p.Id == id)
            // .Select(p => new ProjectResponseDto
            // {
            //     Id = p.Id,
            //     Name = p.Name,
            //     Description = p.Description,
            //     CreatedAt = p.CreatedAt,
            //     DueDate = p.DueDate,
            //     IsComplete = p.IsComplete,
            //     TeamId = p.TeamId,
            //     Team = p.Team != null ? new TeamSummaryDto
            //     {
            //         Id = p.Team.Id,
            //         Name = p.Team.Name,
            //         Description = p.Team.Description
            //     } : null,
            //     Tasks = p.Tasks.Select(task => new TaskSummaryDto
            //     {
            //         Id = task.Id,
            //         Title = task.Title,
            //         Description = task.Description,
            //         IsComplete = task.IsComplete,
            //         CreatedAt = task.CreatedAt
            //     }).ToList()
            // }).FirstOrDefaultAsync();
        }

        ///<summary>
        /// Creates a new project
        /// </summary>
        /// 
        [HttpPost]
        public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectDto dto)
        {
            //Validate team exists
            var team = await _teamRepository.GetByIdAsync(dto.TeamId);
            if (team == null)
            {
                return NotFound($"Team with id: {dto.TeamId} does not exist");
            }

            // Check if project with name exists
            var projectExists = await _projectRepository.AnyAsync(p => p.Name == dto.Name && p.TeamId == dto.TeamId);
            // var projectExists = await _context.Projects
            //     .AnyAsync(p => p.Name == dto.Name && p.TeamId == dto.TeamId);
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

            await _projectRepository.AddAsync(project);

            await _projectRepository.SaveChangesAsync();

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
            // Find project to update
            var project = await _projectRepository.GetProjectWithTeamAndTasksAsync(id);

            if (project == null)
            {
                return NotFound($"Project with id {id} not found.");
            }

            var nameExists = await _projectRepository.ProjectExistsInTeamAsync(dto.Name, project.TeamId, project.Id);
            // var nameExists = await _context.Projects
            //     .AnyAsync(p => p.Name == dto.Name && p.TeamId == project.TeamId && p.Id != id);
            if (nameExists)
            {
                return BadRequest($"Project with name {dto.Name} alredy exists in this team");
            }

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.DueDate = dto.DueDate;
            project.IsComplete = dto.IsComplete;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

            return NoContent();

        }

        ///<summary>
        ///Deletes a project and all it's tasks
        ///</summary>
        ///
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var project = await _projectRepository.GetProjectWithTeamAndTasksAsync(id);
            // var project = await _context.Projects
            //    .Include(p => p.Tasks)
            //    .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound($"Project with id: {id} not found");
            }

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Gets all projects for a specific team
        /// </summary>
        /// 
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<ProjectSummaryDto>>> GetProjectsByTeam(int teamId)
        {
            var teamExists = await _teamRepository.AnyAsync(t => t.Id == teamId);
            if (!teamExists)
            {
                return NotFound($"Team with id: {teamId} not found");
            }

            var projects = await _projectRepository.GetProjectsByTeamIdAsync(teamId);

            // var projects = await _context.Projects
            // .Include(p => p.Tasks)
            // .Where(p => p.TeamId == teamId)
            // .Select(p => new ProjectSummaryDto
            // {
            //     Id = p.Id,
            //     Name = p.Name,
            //     Description = p.Description,
            //     CreatedAt = p.CreatedAt,
            //     DueDate = p.DueDate,
            //     IsComplete = p.IsComplete,
            //     TaskCount = p.Tasks.Count,
            //     CompletedTaskCount = p.Tasks.Count(t => t.IsComplete)
            // }).ToListAsync();

            return Ok(projects);
        }

        #region Private helper methods
        ///<summary>
        /// Map Project to ProjectResponseDto
        ///</summary>
        ///
        private static ProjectResponseDto MapToProjectResponseDto(Project project)
        {
            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                DueDate = project.DueDate,
                IsComplete = project.IsComplete,
                TeamId = project.TeamId,
                Team = project.Team != null ? new TeamSummaryDto
                {
                    Id = project.Team.Id,
                    Name = project.Team.Name,
                    Description = project.Team.Description
                } : null,
                Tasks = project.Tasks.Select(task => new TaskSummaryDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsComplete = task.IsComplete,
                    CreatedAt = task.CreatedAt
                }).ToList()
            };
        }
        #endregion

    }
}