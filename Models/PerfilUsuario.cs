using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Point_Pharma.Models
{
    public class PerfilUsuario
    {
        public string UsuarioId { get; set; } = string.Empty;

        public IdentityUser Usuario { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }
    }
}
