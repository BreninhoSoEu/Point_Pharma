
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Point_Pharma.Data;
using Point_Pharma.Models;

namespace Point_Pharma.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmaciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FarmaciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Farmacias>>> Get()
        {
            var farmacias = await _context.Farmacias.ToListAsync();

            return Ok(farmacias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Farmacias>> GetPorId(int id)
        {
            var farmacia = await _context.Farmacias.FindAsync(id);

            if (farmacia == null)
            {
                return NotFound();
            }

            return Ok(farmacia);
        }

        [HttpPost]
        public async Task<ActionResult<Farmacias>> Post(Farmacias farmacia)
        {
            _context.Farmacias.Add(farmacia);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPorId),
                new { id = farmacia.Id },
                farmacia
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Farmacias farmacia)
        {
            if (id != farmacia.Id)
            {
                return BadRequest();
            }

            var farmaciaExistente = await _context.Farmacias.FindAsync(id);

            if (farmaciaExistente == null)
            {
                return NotFound();
            }

            farmaciaExistente.Nome = farmacia.Nome;
            farmaciaExistente.Endereco = farmacia.Endereco;
            farmaciaExistente.Contato = farmacia.Contato;
            farmaciaExistente.HorarioFuncionamento = farmacia.HorarioFuncionamento;
            farmaciaExistente.Cnpj = farmacia.Cnpj;
            farmaciaExistente.Email = farmacia.Email;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var farmacia = await _context.Farmacias.FindAsync(id);

            if (farmacia == null)
            {
                return NotFound();
            }

            _context.Farmacias.Remove(farmacia);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
