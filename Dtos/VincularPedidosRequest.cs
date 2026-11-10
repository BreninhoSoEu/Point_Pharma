using System.ComponentModel.DataAnnotations;

namespace Point_Pharma.Dtos
{
    public class VincularPedidosRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public List<Guid> Codigos { get; set; } = [];
    }
}
