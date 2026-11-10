using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Point_Pharma.Models
{
    public class ItemPedido
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }

        public Pedido Pedido { get; set; } = null!;

        // Nullable so the order history remains readable if a product is removed.
        public int? ProdutoId { get; set; }

        public Produto? Produto { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeProduto { get; set; } = string.Empty;

        public int FarmaciaId { get; set; }

        [Required]
        [StringLength(200)]
        public string NomeFarmacia { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoUnitario { get; set; }

        [Column(TypeName = "decimal(14,2)")]
        public decimal Subtotal { get; set; }
    }
}
