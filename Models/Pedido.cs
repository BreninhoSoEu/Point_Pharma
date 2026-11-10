using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Point_Pharma.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public Guid CodigoPedido { get; set; } = Guid.NewGuid();

        public string? UsuarioId { get; set; }

        public IdentityUser? Usuario { get; set; }

        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Confirmado";

        [Required]
        [StringLength(30)]
        public string FormaPagamento { get; set; } = string.Empty;

        [Column(TypeName = "decimal(16,2)")]
        public decimal ValorTotal { get; set; }

        public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}
