using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models
{
    public class Pedido
    {
        public int Id { get; set; } // Identificador del pedido en cuestión
        public int UsuarioId { get; set; } // Muestra el usuario que ha realizado el pedido buscado por el ID
        public required Usuario Usuario { get; set; } // Esto permite al usuario navegar por el pedido apra gestionar
        
        [Display(Name = "Fecha del pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        
        [Required(ErrorMessage = "El estado del pedido es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Procesando, Enviado, Entregado, Cancelado
        
        [Required(ErrorMessage = "La dirección de envío es obligatoria")] // Está obligatorio pero lo voy a quitar en producción porque no lo uso
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        [Display(Name = "Dirección de envío")]
        public required string DireccionEnvio { get; set; }
        
        // Propiedad de navegación
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }

    public class DetallePedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public required Pedido Pedido { get; set; }
        public int ProductoId { get; set; }
        public required Producto Producto { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio unitario")]
        public decimal PrecioUnitario { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
