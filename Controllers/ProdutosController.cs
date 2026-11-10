using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Point_Pharma.Data;
using Point_Pharma.Models;

namespace Point_Pharma.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProdutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> Get()
        {
            var produtos = await _context.Produtos
                .AsNoTracking()
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("Farmacia/{farmaciaId:int}")]
        public async Task<ActionResult<IEnumerable<Produto>>> GetPorFarmacia(int farmaciaId)
        {
            var farmaciaExiste = await _context.Farmacias
                .AnyAsync(f => f.Id == farmaciaId);

            if (!farmaciaExiste)
            {
                return NotFound("Farmácia não encontrada.");
            }

            var produtos = await _context.Produtos
                .AsNoTracking()
                .Where(p => p.FarmaciaId == farmaciaId)
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Produto>> GetPorId(int id)
        {
            var produto = await _context.Produtos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> Post(Produto produto)
        {
            var farmaciaExiste = await _context.Farmacias
                .AnyAsync(f => f.Id == produto.FarmaciaId);

            if (!farmaciaExiste)
            {
                return BadRequest("A farmácia informada não existe.");
            }

            _context.Produtos.Add(produto);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPorId),
                new { id = produto.Id },
                produto
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }

            var produtoExistente = await _context.Produtos.FindAsync(id);

            if (produtoExistente == null)
            {
                return NotFound();
            }

            var farmaciaExiste = await _context.Farmacias
                .AnyAsync(f => f.Id == produto.FarmaciaId);

            if (!farmaciaExiste)
            {
                return BadRequest("A farmácia informada não existe.");
            }

            produtoExistente.Nome = produto.Nome;
            produtoExistente.Descricao = produto.Descricao;
            produtoExistente.Categoria = produto.Categoria;
            produtoExistente.Preco = produto.Preco;
            produtoExistente.PrecoAntigo = produto.PrecoAntigo;
            produtoExistente.QuantidadeEstoque = produto.QuantidadeEstoque;
            produtoExistente.FarmaciaId = produto.FarmaciaId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return NotFound();
            }

            _context.Produtos.Remove(produto);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
