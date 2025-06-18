using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;
using TaskManager.API.Repositories.Interfaces;
using TaskManager.API.Repositories.Implementations;


namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        // private readonly TaskManagerDbContext _context;
        // public TasksController(TaskManagerDbContext context)
        // {
        //     _context = context;
        // }
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;

        public TasksController(ITaskRepository taskRepository, IProjectRepository projectRepository, ITeamRepository teamRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
        }

        ///<summary>
        ///Returns a list of all tasks.
        ///</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
        {
            // var tasks = await _context.Tasks
            //     .Include(t => t.Project)
            //     .Select(t => new TaskResponseDto
            //     {
            //         Id = t.Id,
            //         Title = t.Title,
            //         Description = t.Description,
            //         IsComplete = t.IsComplete,
            //         CreatedAt = t.CreatedAt,
            //         ProjectId = t.ProjectId,
            //         Project = t.Project != null ? new ProjectSummaryDto
            //         {
            //             Id = t.Project.Id,
            //             Name = t.Project.Name,
            //             Description = t.Project.Description,
            //             CreatedAt = t.CreatedAt,
            //             DueDate = t.Project.DueDate,
            //             IsComplete = t.Project.IsComplete,
            //             TaskCount = t.Project.Tasks.Count(),
            //             CompletedTaskCount = t.Project.Tasks.Count(task => task.IsComplete == true)
            //         } : null
            //     }).ToListAsync();
            var tasks = await _taskRepository.FindAsync(t => true);
            var taskDtos = tasks.Select(MapToTaskResponseDto).ToList();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var task = await _taskRepository.GetTaskWithProjectAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id: {id} not found");
            }
            var taskDto = MapToTaskResponseDto(task);
            return Ok(taskDto);
            // var task = await _context.Tasks
            //     .Include(t => t.Project)
            //     .Where(t => t.Id == id)
            //     .Select(t => new TaskResponseDto
            //     {
            //         Id = t.Id,
            //         Title = t.Title,
            //         Description = t.Description,
            //         IsComplete = t.IsComplete,
            //         CreatedAt = t.CreatedAt,
            //         ProjectId = t.ProjectId,
            //         Project = t.Project != null ? new ProjectSummaryDto
            //         {
            //             Id = t.Project.Id,
            //             Name = t.Project.Name,
            //             Description = t.Project.Description,
            //             CreatedAt = t.Project.CreatedAt,
            //             DueDate = t.Project.DueDate,
            //             IsComplete = t.Project.IsComplete,
            //             TaskCount = t.Project.Tasks.Count(),
            //             CompletedTaskCount = t.Project.Tasks.Count(task => task.IsComplete)
            //         } : null
            //     }).FirstOrDefaultAsync();

            // if (task == null)
            // {
            //     return NotFound($"Task with id: {id} not found");
            // }
            // return Ok(task);
        }
        ///<summary>
        /// Create a new task
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> Create(CreateTaskDto dto)
        {
            // Validate project exists or not
            var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
            if (project == null)
            {
                return NotFound($"Project with id: {dto.ProjectId} not found");
            }

            // Validate if task with same name exists already in the project
            var taskExists = await _taskRepository.TaskExistsInProjectAsync(dto.Title, dto.ProjectId);
            // var taskExists = await _context.Tasks.AnyAsync(t => t.Title == dto.Title && t.ProjectId == dto.ProjectId);
            if (taskExists)
            {
                return BadRequest($"Task with name: {dto.Title}, already exists in Project Id: {dto.ProjectId}");
            }


            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                // TeamId = dto.TeamId,
                ProjectId = dto.ProjectId,
                IsComplete = false,
                CreatedAt = DateTime.UtcNow
            }
            ;

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            var responseDto = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsComplete = task.IsComplete,
                CreatedAt = task.CreatedAt,
                // TeamId = task.TeamId,
                ProjectId = task.ProjectId,
                Project = new ProjectSummaryDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description
                }
            };

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, responseDto);
        }

        ///<summary>
        /// Update a task
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateTaskDto dto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            // var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id:{id} not found");
            }
            // Check if new title conflicts with existing tasks
            if (task.Title != dto.Title)
            {
                var titleExists = await _taskRepository.TaskExistsInProjectAsync(dto.Title, task.ProjectId, id);
                if (titleExists)
                {
                    return BadRequest($"Task with title: {dto.Title} already exists in this project");
                }
            }
            // Update task properties
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsComplete = dto.IsComplete;
            // Use repository to update and save    
            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Delete a task
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            // var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id: {id} not found");
            }
            await _taskRepository.DeleteAsync(task);
            await _taskRepository.SaveChangesAsync();
            return NoContent();
        }

        ///<summary>
        /// Get all tasks for a specific project
        ///</summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasksByProject(int projectId)
        {
            // Check if project exists
            var projectExists = await _projectRepository.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound($"Project with id: {projectId} not found.");
            }
            ;

            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            var taskDtos = tasks.Select(MapToTaskResponseDto).ToList();

            return Ok(taskDtos);
        }

        ///<summary>
        /// Get all tasks for specific teamId
        ///</summary>
        ///
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasksByTeam(int teamId)
        {
            var teamExists = await _teamRepository.AnyAsync(t => t.Id == teamId);
            if (!teamExists)
            {
                return NotFound($"Team with id: {teamId} not found");
            }

            var tasks = await _taskRepository.GetTasksByTeamIdAsync(teamId);
            var taskDtos = tasks.Select(MapToTaskResponseDto).ToList();

            return Ok(taskDtos);
        }

        #region Private Helper Methods
        ///<summary>
        /// Maps TaskItem entity to TaskResponseDto
        /// </summary>
        /// 
        private static TaskResponseDto MapToTaskResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsComplete = task.IsComplete,
                CreatedAt = task.CreatedAt,
                ProjectId = task.ProjectId,
                Project = task.Project != null ? new ProjectSummaryDto
                {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    Description = task.Project.Description,
                    CreatedAt = task.CreatedAt,
                    DueDate = task.Project.DueDate,
                    IsComplete = task.Project.IsComplete,
                    TaskCount = task.Project.Tasks?.Count ?? 0,
                    CompletedTaskCount = task.Project.Tasks?.Count(t => t.IsComplete == true) ?? 0
                } : null
            };
        }
        #endregion
    }
}


