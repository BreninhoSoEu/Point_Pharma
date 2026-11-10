
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Point_Pharma.Models
{
    public class Produto : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string Categoria { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoAntigo { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantidadeEstoque { get; set; }

        [Required]
        public int FarmaciaId { get; set; }

        public Farmacias? Farmacia { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Preco <= 0)
            {
                yield return new ValidationResult(
                    "O preço deve ser maior que zero.",
                    new[] { nameof(Preco) }
                );
            }

            if (PrecoAntigo.HasValue && PrecoAntigo.Value <= 0)
            {
                yield return new ValidationResult(
                    "O preço antigo deve ser maior que zero.",
                    new[] { nameof(PrecoAntigo) }
                );
            }
        }
    }
}
