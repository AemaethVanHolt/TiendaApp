using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TiendaApp.Models.ViewModels
{
    public class UsuarioCreateViewModel // Este modelo se utiliza para crear un nuevo usuario en la aplicación
    {
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
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
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
        
        [Display(Name = "Rol")]
        public string? RolSeleccionado { get; set; }
        
        [Display(Name = "Cuenta activa")]
        public bool Activo { get; set; } = true;
        
        // Propiedad para compatibilidad con la vista
        public bool EstaActivo { get => Activo; set => Activo = value; }
        
        // Propiedad para compatibilidad con la vista
        public int RolId { get; set; }
    }
}
