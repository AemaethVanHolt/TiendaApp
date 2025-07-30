using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using TiendaApp.Data;
using TiendaApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiendaApp.Models.ViewModels;

namespace TiendaApp.Controllers
{
    [Authorize(Roles = "Administrador")] // El ADMINISTRADOR ve la lista de usuarios completa
    public class UsuariosController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(TiendaContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index() // El ADMINISTRADOR ve la lista de usuarios
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.Apellidos)
                .ThenBy(u => u.Nombre)
                .Select(u => new UsuarioViewModel
                {
                    Id = u.Id.ToString(),
                    Nombre = u.Nombre ?? string.Empty,
                    Apellidos = u.Apellidos ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Telefono = u.Telefono ?? string.Empty,
                    Direccion = u.Direccion ?? string.Empty,
                    Ciudad = u.Ciudad ?? string.Empty,
                    Provincia = u.Provincia ?? string.Empty,
                    CodigoPostal = u.CodigoPostal ?? string.Empty,
                    EstaActivo = u.EstaActivo,
                    FechaRegistro = u.FechaRegistro,
                    UltimoAcceso = u.UltimoAcceso,
                    Rol = u.Rol != null ? u.Rol.Nombre : "Sin rol asignado"
                })
                .ToListAsync(); // Obtener la lista de usuarios con sus roles

            return View(usuarios);
        }

        public async Task<IActionResult> Detalles(int? id) // El ADMINISTRADOR ve los detalles de un usuario específico
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

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
                Rol = usuario.Rol?.Nombre ?? "Sin rol asignado" // Cuando se crea un nuevo usuario, el ADMINISTRADOR tiene que asignarle un rol en su perfil
            };

            return View(viewModel);
        }

        public IActionResult Crear()
        {
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre");
            return View(new UsuarioCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validar que el email no exista
                if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con este correo electrónico.");
                    ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", model.RolId);
                    return View(model);
                }

                // Mapear el ViewModel a la entidad Usuario
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
                    RolId = model.RolId,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FechaRegistro = DateTime.UtcNow,
                    EstaActivo = true,
                    NombreUsuario = model.Email // Usar el email como nombre de usuario por defecto
                };

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", model.RolId);
            return View(model);
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
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
                EstaActivo = usuario.EstaActivo,
                RolId = usuario.RolId
            };

            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", usuario.RolId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nombre,Apellidos,Email,Telefono,Direccion,Ciudad,Provincia,CodigoPostal,Activo,RolId")] Usuario usuario, string? nuevaContrasena)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioExistente = await _context.Usuarios.FindAsync(id);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades básicas
                    usuarioExistente.Nombre = usuario.Nombre;
                    usuarioExistente.Apellidos = usuario.Apellidos;
                    usuarioExistente.Email = usuario.Email;
                    usuarioExistente.Telefono = usuario.Telefono;
                    usuarioExistente.Direccion = usuario.Direccion;
                    usuarioExistente.Ciudad = usuario.Ciudad;
                    usuarioExistente.Provincia = usuario.Provincia;
                    usuarioExistente.CodigoPostal = usuario.CodigoPostal;
                    usuarioExistente.RolId = usuario.RolId;
                    usuarioExistente.EstaActivo = usuario.EstaActivo;

                    // Actualizar contraseña si se proporcionó una nueva
                    if (!string.IsNullOrEmpty(nuevaContrasena))
                    {
                        usuarioExistente.Password = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
                    }

                    _context.Update(usuarioExistente);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Nombre", usuario.RolId);
            return View(usuario);
        }

        public async Task<IActionResult> Eliminar(int? id) // Esto elimina el usuario seleccionado en el menú de usuarios del ADMINISTRADOR
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (usuario == null)
            {
                return NotFound();
            }

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
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Eliminar")] // Eliminación confirmada del usuario
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id) // Muestra la acción, y procede a dejar un mensaje de eliminación confirmada
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Usuario eliminado correctamente."; // Mensaje de eliminación confirmada del usuario seleccionado
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}