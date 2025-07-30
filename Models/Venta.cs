using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models
{
    public class Venta
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";
        
        [StringLength(50)]
        public string? MetodoPago { get; set; }
        
        [StringLength(200)]
        public string? Cliente { get; set; }
        
        public string? Comentarios { get; set; }
        
        // Propiedades de navegación
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
