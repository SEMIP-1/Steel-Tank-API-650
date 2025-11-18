using Microsoft.AspNetCore.Mvc;
using SteelTankAPI650.Models;
using SteelTankAPI650.Services.Shell;

namespace SteelTankAPI650.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShellDesignController : ControllerBase
    {
        private readonly IShellDesignService _shellService;

        public ShellDesignController(IShellDesignService shellService)
        {
            _shellService = shellService;
        }

        [HttpPost("calculate")]
        public IActionResult CalculateShell([FromBody] ShellDesignInput input)
        {
            if (input == null || input.ShellCourses.Count == 0)
                return BadRequest("Invalid shell design input.");

            var result = _shellService.CalculateShell(input);
            return Ok(result);
        }
    }
}
