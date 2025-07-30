using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models
{
    public class Venta
    {
        public int Id { get; set; } // Identificador único de la venta
        
        [Required]
        public int UsuarioId { get; set; } // Identificador del usuario que ha realizado la venta
        
        [Required]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow; // Fecha y hora de la venta
        
        [Required]
        [Column(TypeName = "decimal(18,2)")] // Convertidor de tipo de datos
        public decimal Total { get; set; } // Precio total de la venta
        
        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; //Si no se ha abonado la cantidad total del monto, queda la venta en pendiente
        
        [StringLength(50)]
        public string? MetodoPago { get; set; } // No están funcionales porque no he metido RedSys. El pago se hace automáticamente
        
        [StringLength(200)]
        public string? Cliente { get; set; }
        
        public string? Comentarios { get; set; }
        
        // Propiedades de navegación
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
