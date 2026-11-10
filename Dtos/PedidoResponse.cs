namespace Point_Pharma.Dtos
{
    public class PedidoResponse
    {
        public Guid CodigoPedido { get; set; }

        public DateTime DataPedido { get; set; }

        public string Status { get; set; } = string.Empty;

        public string FormaPagamento { get; set; } = string.Empty;

        public decimal ValorTotal { get; set; }

        public IReadOnlyList<ItemPedidoResponse> Itens { get; set; } = [];
    }

    public class ItemPedidoResponse
    {
        public int? ProdutoId { get; set; }

        public string NomeProduto { get; set; } = string.Empty;

        public int FarmaciaId { get; set; }

        public string NomeFarmacia { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
