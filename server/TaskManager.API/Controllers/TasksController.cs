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
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        }

        ///<summary>
        /// Gets all tasks with their associated project information
        ///</summary>
        ///<returns>A list of all tasks</returns>
        ///<response code="200">Returns the list of tasks</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
        {
            var tasks = await _taskRepository.FindAsync(t => true);
            var taskDtos = tasks.Select(MapToTaskResponseDto).ToList();
            return Ok(taskDtos);
        }

        /// <summary>
        /// Gets a specific task by Id
        /// </summary>
        /// <param name="id">The task Id</param>
        /// <returns>The task with specified Id</returns>
        /// <response code="200">Returns the task</response>
        /// <response code="404">If the task is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var task = await _taskRepository.GetTaskWithProjectAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id: {id} not found");
            }
            var taskDto = MapToTaskResponseDto(task);
            return Ok(taskDto);
        }

        ///<summary>
        /// Create a new task
        /// </summary>
        /// <param name="dto">The task creation data</param>
        /// <returns>The created task</returns>
        /// <response code="201">Returns the newly created task</response>
        /// <response code="400">If the task data is invalid</response>
        /// <response code="404">If the project is not found</response>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponseDto>> Create(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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

            var createdTask = await _taskRepository.GetTaskWithProjectAsync(task.Id);
            var responseDto = MapToTaskResponseDto(createdTask!);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, responseDto);
        }

        ///<summary>
        /// Updates an existing task
        /// </summary>
        /// <param ="id">The task id</param>
        /// <param ="dto">The updated task data</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Task updated successfully</response>
        /// <response code="400">If the task data is invalid</response>
        /// <response code="404">If the task is not found</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, UpdateTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            task.Title = dto.Title.Trim();
            task.Description = dto.Description.Trim();
            task.IsComplete = dto.IsComplete;
            // Use repository to update and save    
            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        /// <params ="id">Id of the task to delete</params>
        /// <returns>No return on success</returns>
        /// <response code="204">Task deleted successfully</response>
        /// <response code="404">If task is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        ///<param name="projectId">The project Id</param>
        ///<returns>A list of all tasks for a specified project</returns>
        ///<response code="200">Returns the list of tasks</response>
        ///<response code="404">Project with Id not found</response>
        [HttpGet("project/{projectId:int}")]
        [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// <param name="teamId">The team Id</param>
        /// <returns>A list of tasks for the specified team</returns>
        /// <response code="200">Returns the list of tasks for specified team</response>
        /// <response code="404">Team is not found</response>
        [HttpGet("team/{teamId}")]
        [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// <summary>
        /// Maps TaskItem entity to TaskResponseDto
        /// </summary>
        /// <param name="task">The task entity to map</param>
        /// <returns>The mapped TaskResponseDto</returns>
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
                    CreatedAt = task.Project.CreatedAt,
                    DueDate = task.Project.DueDate,
                    IsComplete = task.Project.IsComplete,
                    TaskCount = task.Project.Tasks?.Count(t => true) ?? 0,
                    CompletedTaskCount = task.Project.Tasks?.Count(t => t.IsComplete == true) ?? 0
                } : null
            };
        }
        #endregion
    }
}


