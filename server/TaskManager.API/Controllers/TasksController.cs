using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        public TasksController(TaskManagerDbContext context)
        {
            _context = context;
        }

        ///<summary>
        ///Returns a list of all tasks.
        ///</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsComplete = t.IsComplete,
                    CreatedAt = t.CreatedAt,
                    ProjectId = t.ProjectId,
                    Project = t.Project != null ? new ProjectSummaryDto
                    {
                        Id = t.Project.Id,
                        Name = t.Project.Name,
                        Description = t.Project.Description,
                        CreatedAt = t.CreatedAt,
                        DueDate = t.Project.DueDate,
                        IsComplete = t.Project.IsComplete,
                        TaskCount = t.Project.Tasks.Count(),
                        CompletedTaskCount = t.Project.Tasks.Count(task => task.IsComplete == true)
                    } : null
                }).ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.Id == id)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsComplete = t.IsComplete,
                    CreatedAt = t.CreatedAt,
                    ProjectId = t.ProjectId,
                    Project = t.Project != null ? new ProjectSummaryDto
                    {
                        Id = t.Project.Id,
                        Name = t.Project.Name,
                        Description = t.Project.Description,
                        CreatedAt = t.Project.CreatedAt,
                        DueDate = t.Project.DueDate,
                        IsComplete = t.Project.IsComplete,
                        TaskCount = t.Project.Tasks.Count(),
                        CompletedTaskCount = t.Project.Tasks.Count(task => task.IsComplete)
                    } : null
                }).FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound($"Task with id: {id} not found");
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> Create(CreateTaskDto dto)
        {
            // Validate project exists or not
            var project = await _context.Projects.FindAsync(dto.ProjectId);
            if (project == null)
            {
                return NotFound($"Project with id: {dto.ProjectId} not found");
            }

            // Validate if task with same name exists already in the project
            var taskExists = await _context.Tasks.AnyAsync(t => t.Title == dto.Title && t.ProjectId == dto.ProjectId);
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

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

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

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateTaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with id:{id} not found");
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsComplete = dto.IsComplete;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with id: {id} not found");
            }
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}