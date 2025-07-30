using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers
{
    public class CuentaController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<CuentaController> _logger;

        public CuentaController(TiendaContext context, ILogger<CuentaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult IniciarSesion(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Buscar el usuario por email
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                // Verificar si el usuario existe y la contraseña es correcta
                _logger.LogInformation("=== INICIO DE SESIÓN ===");
                _logger.LogInformation("Email: {Email}", model.Email);
                _logger.LogInformation("Usuario encontrado en BD: {Encontrado}", usuario != null);
                
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
                    return View(model);
                }
                
                _logger.LogInformation("=== INFORMACIÓN DEL USUARIO ===");
                _logger.LogInformation("ID: {Id}", usuario.Id);
                _logger.LogInformation("Nombre: {Nombre}", usuario.Nombre);
                _logger.LogInformation("Rol: {Rol}", usuario.Rol?.Nombre);
                _logger.LogInformation("Contraseña almacenada: {Password}", usuario.Password);
                _logger.LogInformation("Contraseña ingresada: {Password}", model.Password);
                
                var isPasswordValid = PasswordHelper.VerifyPassword(model.Password, usuario.Password);
                _logger.LogInformation("¿Contraseña válida? {IsValid}", isPasswordValid);
                _logger.LogInformation("¿Usuario activo? {Activo}", usuario.EstaActivo);
                
                if (usuario.EstaActivo && isPasswordValid)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nombre),
                        new Claim(ClaimTypes.Surname, usuario.Apellidos ?? string.Empty),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Cliente"),
                        new Claim("UserId", usuario.Id.ToString())
                    };
                    
                    _logger.LogInformation("=== CLAIMS DEL USUARIO ===");
                    foreach (var claim in claims)
                    {
                        _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = model.Recordarme,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        });

                    _logger.LogInformation("Usuario {Nombre} ha iniciado sesión.", usuario.Nombre);
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el correo electrónico ya está registrado
                if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                    return View(model);
                }

                // Obtener el rol de Cliente
                var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
                if (rolCliente == null)
                {
                    // Si por alguna razón no existe el rol Cliente, lo creamos
                    rolCliente = new Rol
                    {
                        Nombre = "Cliente",
                        Descripcion = "Cliente de la tienda",
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow
                    };
                    _context.Roles.Add(rolCliente);
                    await _context.SaveChangesAsync();
                }

                // Crear nuevo usuario
                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Direccion = model.Direccion,
                    Ciudad = model.Ciudad,
                    Provincia = model.Provincia,
                    CodigoPostal = model.CodigoPostal,
                    FechaRegistro = DateTime.UtcNow,
                    EstaActivo = true,
                    RolId = rolCliente.Id,
                    Rol = rolCliente
                };

                // Establecer la contraseña hasheada
                usuario.SetPassword(model.Password);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Iniciar sesión automáticamente después del registro
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Surname, usuario.Apellidos ?? string.Empty),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Cliente"),
                    new Claim("UserId", usuario.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                _logger.LogInformation("Usuario {Nombre} se ha registrado.", usuario.Nombre);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario ha cerrado sesión.");
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
        
        [Authorize]
        public IActionResult CambiarContrasena()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Obtener el ID del usuario actual
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario.");
                return View(model);
            }
            
            // Buscar el usuario en la base de datos
            var usuario = await _context.Usuarios.FindAsync(int.Parse(userId));
            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return View(model);
            }
            
            // Verificar la contraseña actual
            if (!PasswordHelper.VerifyPassword(model.ContrasenaActual, usuario.Password))
            {
                ModelState.AddModelError(nameof(model.ContrasenaActual), "La contraseña actual no es correcta.");
                return View(model);
            }
            
            // Actualizar la contraseña
            usuario.Password = HashPassword(model.NuevaContrasena);
            _context.Update(usuario);
            await _context.SaveChangesAsync();
            
            // Cerrar sesión para que el usuario tenga que volver a iniciar sesión
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Redirigir a la página de inicio de sesión con un mensaje
            TempData["MensajeExito"] = "Tu contraseña ha sido cambiada correctamente. Por favor, inicia sesión de nuevo con tu nueva contraseña.";
            return RedirectToAction("IniciarSesion");
        }
        
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }

    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El {0} debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Apellidos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres")]
        public string? Apellidos { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string? Telefono { get; set; }

        [Display(Name = "Dirección")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        [Display(Name = "Ciudad")]
        [StringLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres")]
        public string? Ciudad { get; set; }

        [Display(Name = "Provincia")]
        [StringLength(100, ErrorMessage = "La provincia no puede exceder los 100 caracteres")]
        public string? Provincia { get; set; }

        [Display(Name = "Código Postal")]
        [StringLength(10, ErrorMessage = "El código postal no puede exceder los 10 caracteres")]
        public string? CodigoPostal { get; set; }
    }
    
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaContrasena { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NuevaContrasena", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}

public static class PasswordHelper
{
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            var hashedPassword = Convert.ToBase64String(hashedBytes);
            
            // Para depuración
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Stored Hash: {storedHash}");
            Console.WriteLine($"Computed Hash: {hashedPassword}");
            
            return string.Equals(hashedPassword, storedHash, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en VerifyPassword: {ex}");
            return false;
        }
    }
}
