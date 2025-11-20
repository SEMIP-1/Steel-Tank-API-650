using Microsoft.AspNetCore.Mvc;
using SteelTankAPI650.Models.Bottom;
using SteelTankAPI650.Services.Bottom;

namespace SteelTankAPI650.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BottomPlateController : ControllerBase
    {
        private readonly IBottomPlateDesignService _service;

        public BottomPlateController(IBottomPlateDesignService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult<BottomPlateResult> Design([FromBody] BottomPlateInput input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _service.DesignBottom(input);
            return Ok(result);
        }
    }
}
