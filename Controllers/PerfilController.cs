using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;
using TiendaApp.Models.ViewModels;

namespace TiendaApp.Controllers // Para los detalles del perfil, sobre todo del administrador.
{
    [Authorize(Roles = "Administrador")]
    public class PerfilController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(TiendaContext context, ILogger<PerfilController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            // Usar el claim personalizado 'UserId' que se establece en el login
            var userIdClaim = User?.FindFirst("UserId")?.Value ?? string.Empty;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Usuario no válido");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Mapear a ViewModel
            var viewModel = new UsuarioViewModel
            {
                Id = usuario.Id.ToString(),
                Nombre = usuario.Nombre ?? string.Empty,
                Apellidos = usuario.Apellidos ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Telefono = usuario.Telefono ?? string.Empty,
                Direccion = usuario.Direccion ?? string.Empty,
                Ciudad = usuario.Ciudad ?? string.Empty,
                Provincia = usuario.Provincia ?? string.Empty,
                CodigoPostal = usuario.CodigoPostal ?? string.Empty,
                EstaActivo = usuario.EstaActivo,
                FechaRegistro = usuario.FechaRegistro,
                UltimoAcceso = usuario.UltimoAcceso,
                Rol = usuario.Rol?.Nombre ?? "Sin rol asignado"
                // Asegúrate de mapear todas las propiedades necesarias
            };

            return View(viewModel);
        }

        // GET: Perfil/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new UsuarioEditViewModel
            {
                Id = usuario.Id.ToString(),
                Nombre = usuario.Nombre ?? string.Empty,
                Apellidos = usuario.Apellidos ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Telefono = usuario.Telefono ?? string.Empty,
                Direccion = usuario.Direccion ?? string.Empty,
                Ciudad = usuario.Ciudad ?? string.Empty,
                Provincia = usuario.Provincia ?? string.Empty,
                CodigoPostal = usuario.CodigoPostal ?? string.Empty,
                RolSeleccionado = usuario.Rol?.Nombre ?? "Sin rol asignado",
                ProfileImageUrl = usuario.ProfileImageUrl
            };

            ViewBag.Roles = await _context.Roles
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = r.Nombre,
                    Text = r.Nombre
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: Perfil/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string id, [Bind("Id,Nombre,Apellidos,Email,Telefono,Direccion,Ciudad,Provincia,CodigoPostal,RolSeleccionado,ProfileImage")] UsuarioEditViewModel viewModel)
        {
            if (id != viewModel.Id || !int.TryParse(id, out int usuarioId))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Usuarios
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.Id == usuarioId);
                        
                    if (usuario == null)
                    {
                        return NotFound();
                    }

                    // Actualizar solo los campos permitidos
                    usuario.Nombre = viewModel.Nombre ?? string.Empty;
                    usuario.Apellidos = viewModel.Apellidos ?? string.Empty;
                    usuario.Telefono = viewModel.Telefono ?? string.Empty;
                    usuario.Direccion = viewModel.Direccion ?? string.Empty;
                    usuario.Ciudad = viewModel.Ciudad ?? string.Empty;
                    usuario.Provincia = viewModel.Provincia ?? string.Empty;
                    usuario.CodigoPostal = viewModel.CodigoPostal ?? string.Empty;
                    
                    // Manejar la imagen de perfil si se cargó una nueva
                    if (viewModel.ProfileImage != null && viewModel.ProfileImage.Length > 0)
                    {
                        // Crear el directorio si no existe
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        // Generar un nombre de archivo único
                        var uniqueFileName = $"profile_{usuarioId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(viewModel.ProfileImage.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        // Guardar el archivo
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ProfileImage.CopyToAsync(fileStream);
                        }
                        
                        // Actualizar la URL de la imagen en el usuario
                        usuario.ProfileImageUrl = $"/uploads/profiles/{uniqueFileName}";
                    }
                    
                    // Actualizar el rol si se proporcionó uno nuevo
                    if (!string.IsNullOrEmpty(viewModel.RolSeleccionado) && viewModel.RolSeleccionado != "Sin rol asignado")
                    {
                        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == viewModel.RolSeleccionado);
                        if (rol != null)
                        {
                            usuario.RolId = rol.Id;
                        }
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Error al actualizar el perfil del usuario {UsuarioId}", usuarioId);
                    
                    if (!UsuarioExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Ocurrió un error al guardar los cambios. Por favor, inténtelo de nuevo.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al actualizar el perfil del usuario {UsuarioId}", usuarioId);
                    ModelState.AddModelError("", "Ocurrió un error inesperado. Por favor, inténtelo de nuevo más tarde.");
                }
            }
            
            // Si llegamos hasta aquí, algo falló. Recargar los roles para el dropdown.
            ViewBag.Roles = await _context.Roles
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = r.Nombre,
                    Text = r.Nombre
                })
                .ToListAsync();
                
            return View(viewModel);
        }

        private bool UsuarioExists(string id)
        {
            if (!int.TryParse(id, out int usuarioId))
            {
                return false;
            }
            return _context.Usuarios.Any(e => e.Id == usuarioId);
        }
    }
}
