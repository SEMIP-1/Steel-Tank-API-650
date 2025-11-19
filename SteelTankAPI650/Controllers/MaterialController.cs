using Microsoft.AspNetCore.Mvc;
using SteelTankAPI650.Models.Config;
using SteelTankAPI650.Services.Config;

namespace SteelTankAPI650.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly IDesignDataRepository _repo;

        public MaterialsController(IDesignDataRepository repo)
        {
            _repo = repo;
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_repo.Materials);
        }

        // GET ONE
        [HttpGet("{grade}")]
        public IActionResult Get(string grade)
        {
            var mat = _repo.GetMaterial(grade);
            if (mat == null)
                return NotFound($"Material '{grade}' not found.");

            return Ok(mat);
        }

        // ADD
        [HttpPost]
        public IActionResult Add([FromBody] MaterialDefinition mat)
        {
            if (_repo.GetMaterial(mat.Grade) != null)
                return BadRequest("Material already exists.");

            _repo.MaterialsInternal.Add(mat);   // internal list exposed from repo
            _repo.SaveChanges();                // persist to Excel

            return Ok(mat);
        }

        // UPDATE
        [HttpPut("{grade}")]
        public IActionResult Update(string grade, [FromBody] MaterialDefinition updated)
        {
            var mat = _repo.GetMaterial(grade);
            if (mat == null)
                return NotFound($"Material '{grade}' not found.");

            mat.Sd_MPa = updated.Sd_MPa;
            mat.StMultiplier = updated.StMultiplier;
            mat.Density = updated.Density;
            mat.Note = updated.Note;

            _repo.SaveChanges();
            return Ok(mat);
        }

        // DELETE
        [HttpDelete("{grade}")]
        public IActionResult Delete(string grade)
        {
            var mat = _repo.GetMaterial(grade);
            if (mat == null)
                return NotFound($"Material '{grade}' not found.");

            _repo.MaterialsInternal.Remove(mat);
            _repo.SaveChanges();

            return Ok();
        }
    }
}
