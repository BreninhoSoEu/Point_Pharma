using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Point_Pharma.Data;
using Point_Pharma.Dtos;
using Point_Pharma.Models;

namespace Point_Pharma.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private const int MaximoItensPedido = 100;
        private const int QuantidadeMaximaPorProduto = 1000;

        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<PedidoResponse>> Post(CriarPedidoRequest request)
        {
            if (request.Itens.Count > MaximoItensPedido)
            {
                return BadRequest($"O pedido aceita no máximo {MaximoItensPedido} produtos diferentes.");
            }

            var itensSolicitados = request.Itens
                .GroupBy(item => item.ProdutoId)
                .Select(grupo => new
                {
                    ProdutoId = grupo.Key,
                    Quantidade = grupo.Sum(item => item.Quantidade)
                })
                .OrderBy(item => item.ProdutoId)
                .ToList();

            if (itensSolicitados.Any(item => item.Quantidade > QuantidadeMaximaPorProduto))
            {
                return BadRequest($"A quantidade máxima por produto é {QuantidadeMaximaPorProduto}.");
            }

            var formaPagamento = request.FormaPagamento.Trim().ToLowerInvariant() switch
            {
                "pix" => "Pix",
                "card" => "Cartão de crédito",
                "cash" => "Dinheiro na retirada",
                _ => null
            };

            if (formaPagamento is null)
            {
                return BadRequest("A forma de pagamento informada é inválida.");
            }

            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var idsProdutos = itensSolicitados.Select(item => item.ProdutoId).ToList();
            var produtos = await _context.Produtos
                .AsNoTracking()
                .Include(produto => produto.Farmacia)
                .Where(produto => idsProdutos.Contains(produto.Id))
                .OrderBy(produto => produto.Id)
                .ToListAsync();

            if (produtos.Count != itensSolicitados.Count)
            {
                return BadRequest("Um ou mais produtos não foram encontrados.");
            }

            var produtosPorId = produtos.ToDictionary(produto => produto.Id);
            foreach (var item in itensSolicitados)
            {
                var produto = produtosPorId[item.ProdutoId];
                if (produto.QuantidadeEstoque < item.Quantidade)
                {
                    return Conflict($"Estoque insuficiente para {produto.Nome}. Disponível: {produto.QuantidadeEstoque}.");
                }

                var linhasAtualizadas = await _context.Produtos
                    .Where(p => p.Id == item.ProdutoId && p.QuantidadeEstoque >= item.Quantidade)
                    .ExecuteUpdateAsync(atualizacao => atualizacao.SetProperty(
                        p => p.QuantidadeEstoque,
                        p => p.QuantidadeEstoque - item.Quantidade));

                if (linhasAtualizadas != 1)
                {
                    return Conflict($"O estoque de {produto.Nome} mudou. Atualize o carrinho e tente novamente.");
                }
            }

            var pedido = new Pedido
            {
                UsuarioId = User.Identity?.IsAuthenticated == true
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : null,
                DataPedido = DateTime.UtcNow,
                Status = "Confirmado",
                FormaPagamento = formaPagamento,
                Itens = itensSolicitados.Select(item =>
                {
                    var produto = produtosPorId[item.ProdutoId];
                    var subtotal = produto.Preco * item.Quantidade;
                    return new ItemPedido
                    {
                        ProdutoId = produto.Id,
                        NomeProduto = produto.Nome,
                        FarmaciaId = produto.FarmaciaId,
                        NomeFarmacia = produto.Farmacia?.Nome ?? $"Farmácia #{produto.FarmaciaId}",
                        Quantidade = item.Quantidade,
                        PrecoUnitario = produto.Preco,
                        Subtotal = subtotal
                    };
                }).ToList()
            };

            pedido.ValorTotal = pedido.Itens.Sum(item => item.Subtotal);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var resposta = Converter(pedido);
            return CreatedAtAction(
                nameof(GetPorCodigo),
                new { codigo = pedido.CodigoPedido },
                resposta);
        }

        [HttpGet("{codigo:guid}")]
        public async Task<ActionResult<PedidoResponse>> GetPorCodigo(Guid codigo)
        {
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.CodigoPedido == codigo);

            if (pedido is null)
            {
                return NotFound("Pedido não encontrado.");
            }

            return Ok(Converter(pedido));
        }

        [Authorize]
        [HttpGet("minhas")]
        public async Task<ActionResult<IEnumerable<PedidoResponse>>> GetMeusPedidos()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized();
            }

            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Itens)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();

            return Ok(pedidos.Select(Converter));
        }

        [Authorize]
        [HttpPost("vincular-visitante")]
        public async Task<IActionResult> VincularPedidosDeVisitante(VincularPedidosRequest request)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized();
            }

            var codigos = request.Codigos.Distinct().Order().ToList();
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var vinculados = 0;
            foreach (var codigo in codigos)
            {
                vinculados += await _context.Pedidos
                    .Where(p => p.CodigoPedido == codigo && p.UsuarioId == null)
                    .ExecuteUpdateAsync(atualizacao => atualizacao.SetProperty(
                        p => p.UsuarioId,
                        usuarioId));
            }

            await transaction.CommitAsync();
            return Ok(new { pedidosVinculados = vinculados });
        }

        private static PedidoResponse Converter(Pedido pedido)
        {
            return new PedidoResponse
            {
                CodigoPedido = pedido.CodigoPedido,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status,
                FormaPagamento = pedido.FormaPagamento,
                ValorTotal = pedido.ValorTotal,
                Itens = pedido.Itens.Select(item => new ItemPedidoResponse
                {
                    ProdutoId = item.ProdutoId,
                    NomeProduto = item.NomeProduto,
                    FarmaciaId = item.FarmaciaId,
                    NomeFarmacia = item.NomeFarmacia,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Subtotal = item.Subtotal
                }).ToList()
            };
        }
    }
}
