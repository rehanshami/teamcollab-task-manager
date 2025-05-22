using Microsoft.AspNetCore.Mvc;

namespace TaskManager.API.AddControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API is running");
        }
    }
}