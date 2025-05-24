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
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            var tasks = await _context.Tasks.ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id: {id} not found");
            }
            return task;
        }

        [HttpPost]
        public IActionResult Create(CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                IsComplete = false,
                CreatedAt = DateTime.UtcNow
            }
            ;

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
    }
}