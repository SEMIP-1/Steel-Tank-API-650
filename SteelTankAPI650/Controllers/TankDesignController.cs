using Microsoft.AspNetCore.Mvc;
using SteelTankAPI650.Models;
using SteelTankAPI650.Services;     

namespace SteelTankAPI650.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TankDesignController : ControllerBase
    {
        private readonly ITankDesignService _service;

        public TankDesignController(ITankDesignService service)
        {
            _service = service;
        }

        [HttpPost("design")]
        public IActionResult DesignTank([FromBody] TankInput input)
        {
            if (input == null || input.ShellCourses.Count == 0)
                return BadRequest("Shell courses required.");

            var result = _service.DesignTank(input);

            return Ok(result);
        }
    }
}
