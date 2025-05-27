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
                .Include(t => t.Team)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsComplete = t.IsComplete,
                    CreatedAt = t.CreatedAt,
                    TeamId = t.TeamId,
                    Team = t.Team != null ? new TeamSummaryDto
                    {
                        Id = t.Team.Id,
                        Name = t.Team.Name,
                        Description = t.Team.Description
                    } : null
                }).ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Team)
                .Where(t => t.Id == id)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsComplete = t.IsComplete,
                    CreatedAt = t.CreatedAt,
                    TeamId = t.TeamId,
                    Team = t.Team != null ? new TeamSummaryDto
                    {
                        Id = t.Team.Id,
                        Name = t.Team.Name,
                        Description = t.Team.Description
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
            var teamExists = await _context.Teams.AnyAsync(t => t.Id == dto.TeamId);
            if (!teamExists)
            {
                return BadRequest($"Team with id:{dto.TeamId} not found");
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                TeamId = dto.TeamId,
                IsComplete = false,
                CreatedAt = DateTime.UtcNow
            }
            ;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var team = await _context.Teams.FindAsync(dto.TeamId);
            var responseDto = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsComplete = task.IsComplete,
                CreatedAt = task.CreatedAt,
                TeamId = task.TeamId,
                Team = new TeamSummaryDto
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description
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