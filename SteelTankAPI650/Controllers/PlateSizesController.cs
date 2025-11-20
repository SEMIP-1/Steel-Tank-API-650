using Microsoft.AspNetCore.Mvc;
using SteelTankAPI650.Models.Config;
using SteelTankAPI650.Services.Config;

namespace SteelTankAPI650.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlateSizesController : ControllerBase
    {
        private readonly IDesignDataRepository _repo;

        public PlateSizesController(IDesignDataRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_repo.PlateSizes);
        }

        [HttpPost]
        public IActionResult Add([FromBody] PlateSize size)
        {
            // You can expose PlateSizesInternal similarly to MaterialsInternal
            var list = (_repo as ExcelDesignDataRepository)?.PlateSizesInternal;
            if (list == null) return BadRequest("Editable list not available.");

            if (list.Any(p => Math.Abs(p.ThicknessMM - size.ThicknessMM) < 0.0001))
                return BadRequest("This plate thickness already exists.");

            list.Add(size);
            (_repo as ExcelDesignDataRepository)?.SaveChanges(); // extend SaveChanges if needed
            return Ok(size);
        }

        [HttpPut("{thk}")]
        public IActionResult Update(double thk, [FromBody] PlateSize updated)
        {
            var repoImpl = _repo as ExcelDesignDataRepository;
            if (repoImpl == null) return BadRequest("Editable list not available.");

            var existing = repoImpl.PlateSizesInternal
                                   .FirstOrDefault(p => Math.Abs(p.ThicknessMM - thk) < 0.0001);
            if (existing == null)
                return NotFound("Plate size not found.");

            existing.ThicknessMM = updated.ThicknessMM;
            repoImpl.SaveChanges();
            return Ok(existing);
        }

        [HttpDelete("{thk}")]
        public IActionResult Delete(double thk)
        {
            var repoImpl = _repo as ExcelDesignDataRepository;
            if (repoImpl == null) return BadRequest("Editable list not available.");

            var existing = repoImpl.PlateSizesInternal
                                   .FirstOrDefault(p => Math.Abs(p.ThicknessMM - thk) < 0.0001);
            if (existing == null)
                return NotFound("Plate size not found.");

            repoImpl.PlateSizesInternal.Remove(existing);
            repoImpl.SaveChanges();
            return Ok();
        }
    }
}
