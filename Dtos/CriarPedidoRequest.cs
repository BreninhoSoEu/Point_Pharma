using System.ComponentModel.DataAnnotations;

namespace Point_Pharma.Dtos
{
    public class CriarPedidoRequest
    {
        [Required]
        [StringLength(20)]
        public string FormaPagamento { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<CriarItemPedidoRequest> Itens { get; set; } = [];
    }

    public class CriarItemPedidoRequest
    {
        [Range(1, int.MaxValue)]
        public int ProdutoId { get; set; }

        [Range(1, 1000)]
        public int Quantidade { get; set; }
    }
}
