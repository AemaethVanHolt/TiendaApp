using System;
using System.ComponentModel.DataAnnotations;

namespace TiendaApp.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;
        
        [Display(Name = "Nombre completo")]
        public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();
        
        [Display(Name = "Correo electrónico")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
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
        public string? ProfileImageUrl { get; set; }
        
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;
        
        [Display(Name = "Cuenta activa")]
        public bool Activo { get; set; }
        
        // Propiedad para compatibilidad con la vista
        public bool EstaActivo { get => Activo; set => Activo = value; }
        
        [Display(Name = "Fecha de registro")]
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; }
        
        [Display(Name = "Último acceso")]
        [DataType(DataType.DateTime)]
        public DateTime? UltimoAcceso { get; set; }
    }
}
