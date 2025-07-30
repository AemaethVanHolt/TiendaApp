using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Display(Name = "URL de la imagen")]
        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        [Display(Name = "Tipo de producto")]
        [StringLength(50)]
        public string? Tipo { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Última modificación")]
        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación (temporalmente eliminadas para resolver problemas de migración)
        // public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();

        // Propiedad calculada para compatibilidad
        [NotMapped]
        public string? UrlImagen 
        { 
            get => ImagenUrl;
            set => ImagenUrl = value;
        }

        // Propiedad para compatibilidad
        [NotMapped]
        public bool EstaActivo 
        { 
            get => Activo;
            set => Activo = value;
        }
    }
}
