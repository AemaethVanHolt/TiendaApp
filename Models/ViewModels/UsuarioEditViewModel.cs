using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TiendaApp.Models.ViewModels
{
    public class UsuarioEditViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }
        
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }
        
        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }
        
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }
        
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }
        
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
        
        [Display(Name = "Imagen de perfil")]
        public IFormFile? ProfileImage { get; set; }
        
        [Display(Name = "URL de la imagen de perfil")]
        public string? ProfileImageUrl { get; set; }
        
        [Display(Name = "Rol")]
        public string? RolSeleccionado { get; set; }
        
        [Display(Name = "Cuenta activa")]
        public bool Activo { get; set; }
        
        // Propiedad para compatibilidad con la vista
        public bool EstaActivo { get => Activo; set => Activo = value; }
        
        // Propiedad para compatibilidad con la vista
        public int RolId { get; set; }
    }
}
