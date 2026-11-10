using System.ComponentModel.DataAnnotations;

namespace Point_Pharma.Dtos
{
    public class RegistrarUsuarioRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d).{8,}$")]
        public string Senha { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }
    }

    public class AtualizarPerfilRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }
    }

    public class PerfilUsuarioResponse
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Nome { get; set; }

        public DateOnly? DataNascimento { get; set; }
    }
}
