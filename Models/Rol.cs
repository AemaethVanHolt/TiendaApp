using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TiendaApp.Models
{
    public class Rol
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede tener más de 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        // Propiedades de navegación
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
