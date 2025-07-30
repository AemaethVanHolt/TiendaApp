using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models
{
    public class DetalleVenta // Detalles sobre los productos vendidos
    {
        public int Id { get; set; } // ID de la venta
        
        [Required]
        public int VentaId { get; set; } // Identificador único de la venta a la que pertenece el detalle que se muestra
        
        [Required]
        public int ProductoId { get; set; }
        
        [Required]
        public int Cantidad { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")] // Convertidor de tipo de datos para precio unitario y total
        public decimal PrecioUnitario { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } // Total a pagar por el producto (Cantidad * PrecioUnitario)

        // Propiedades de navegación
        public Venta Venta { get; set; } = null!; // Para navegación desde el detalle de la venta
        public Producto? Producto { get; set; }
    }
}
