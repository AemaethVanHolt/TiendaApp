using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        
        [Required]
        public int VentaId { get; set; }
        
        [Required]
        public int ProductoId { get; set; }
        
        [Required]
        public int Cantidad { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        
        // Propiedades de navegación
        public Venta Venta { get; set; } = null!;
        public Producto? Producto { get; set; }
    }
}
