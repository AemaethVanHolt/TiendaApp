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
        [DataType(DataType.Date)] // Esto es para mostrar un selector de fecha
        public DateTime? FechaNacimiento { get; set; } // Fecha de nacimiento del usuario

        [Display(Name = "Imagen de perfil")]
        public string? ProfileImageUrl { get; set; } // URL de la imagen de perfil del usuario

        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty; // Rol del usuario, por ejemplo "Cliente", "Administrador", etc.

        [Display(Name = "Cuenta activa")]
        public bool Activo { get; set; } // Indica si la cuenta del usuario está activa o no

        // Propiedad para compatibilidad con la vista
        public bool EstaActivo { get => Activo; set => Activo = value; } // Esta propiedad se usa para mantener la compatibilidad con las vistas que esperan una propiedad llamada "EstaActivo"

        [Display(Name = "Fecha de registro")]
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; } // Fecha en que el usuario se registró en la aplicación

        [Display(Name = "Último acceso")]
        [DataType(DataType.DateTime)]
        public DateTime? UltimoAcceso { get; set; } // Fecha del último acceso del usuario a la aplicación, puede ser nula si nunca ha accedido
    }
}