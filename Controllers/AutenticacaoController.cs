using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Point_Pharma.Data;
using Point_Pharma.Dtos;
using Point_Pharma.Models;

namespace Point_Pharma.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AutenticacaoController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registrar(RegistrarUsuarioRequest request)
        {
            var dataAtual = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.DataNascimento < new DateOnly(1900, 1, 1)
                || request.DataNascimento > dataAtual)
            {
                return BadRequest(new { message = "Informe uma data de nascimento válida." });
            }

            var email = request.Email.Trim();
            var usuario = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var resultado = await _userManager.CreateAsync(usuario, request.Senha);

            if (!resultado.Succeeded)
            {
                var erros = resultado.Errors.Select(erro => erro.Code switch
                {
                    "DuplicateEmail" or "DuplicateUserName" => "Este e-mail já está cadastrado.",
                    "PasswordTooShort" => "A senha precisa ter pelo menos 8 caracteres.",
                    "PasswordRequiresDigit" => "A senha precisa conter pelo menos um número.",
                    _ => erro.Description
                }).Distinct().ToArray();

                return BadRequest(new { errors = erros });
            }

            _context.PerfisUsuarios.Add(new PerfilUsuario
            {
                UsuarioId = usuario.Id,
                Nome = request.Nome.Trim(),
                DataNascimento = request.DataNascimento
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Conta criada com sucesso." });
        }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<ActionResult<PerfilUsuarioResponse>> GetPerfil()
        {
            var usuario = await UsuarioAtualAsync();
            if (usuario is null)
            {
                return Unauthorized();
            }

            var perfil = await _context.PerfisUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == usuario.Id);

            return Ok(Converter(usuario, perfil));
        }

        [Authorize]
        [HttpPut("perfil")]
        public async Task<ActionResult<PerfilUsuarioResponse>> AtualizarPerfil(AtualizarPerfilRequest request)
        {
            var usuario = await UsuarioAtualAsync();
            if (usuario is null)
            {
                return Unauthorized();
            }

            var dataAtual = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.DataNascimento < new DateOnly(1900, 1, 1)
                || request.DataNascimento > dataAtual)
            {
                return BadRequest(new { message = "Informe uma data de nascimento válida." });
            }

            var perfil = await _context.PerfisUsuarios
                .FirstOrDefaultAsync(p => p.UsuarioId == usuario.Id);

            if (perfil is null)
            {
                perfil = new PerfilUsuario { UsuarioId = usuario.Id };
                _context.PerfisUsuarios.Add(perfil);
            }

            perfil.Nome = request.Nome.Trim();
            perfil.DataNascimento = request.DataNascimento;

            await _context.SaveChangesAsync();
            return Ok(Converter(usuario, perfil));
        }

        private async Task<IdentityUser?> UsuarioAtualAsync()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(usuarioId)
                ? null
                : await _userManager.FindByIdAsync(usuarioId);
        }

        private static PerfilUsuarioResponse Converter(IdentityUser usuario, PerfilUsuario? perfil)
        {
            return new PerfilUsuarioResponse
            {
                Id = usuario.Id,
                Email = usuario.Email ?? string.Empty,
                Nome = perfil?.Nome,
                DataNascimento = perfil?.DataNascimento
            };
        }
    }
}
