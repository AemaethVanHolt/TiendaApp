using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly TiendaContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductosController(TiendaContext context, IWebHostEnvironment env)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // GET: Productos
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? tipo = null)
        {
            var query = _context.Productos.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoDecoded = Uri.UnescapeDataString(tipo);
                if (!string.IsNullOrWhiteSpace(tipoDecoded))
                {
                    query = query.Where(p => p.Tipo != null && p.Tipo == tipoDecoded);
                    ViewData["CategoriaActual"] = tipoDecoded;
                }
            }
            
            // Obtener la lista de categorías para el menú desplegable
            ViewData["Categorias"] = await _context.Productos
                .Where(p => p.Activo)
                .Select(p => p.Tipo)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
                
            return View(await query.ToListAsync());
        }

        // GET: Productos/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Admin
        [AllowAnonymous]
        public IActionResult Admin()
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Productos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("Id,Nombre,Descripcion,Precio,Stock,Imagen")] Producto producto, IFormFile imagen)
        {
            if (ModelState.IsValid)
            {
                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "img", "productos");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagen.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }

                    producto.UrlImagen = $"/img/productos/{uniqueFileName}";
                }
                else
                {
                    producto.UrlImagen = "/img/productos/producto-default.jpg";
                }

                producto.FechaCreacion = DateTime.UtcNow;
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: Productos/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        // POST: Productos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock,UrlImagen")] Producto producto, IFormFile? imagen)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imagen != null && imagen.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "img", "productos");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Eliminar imagen anterior si existe
                        if (!string.IsNullOrEmpty(producto.UrlImagen) && !producto.UrlImagen.Contains("producto-default.jpg"))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, producto.UrlImagen.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagen.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagen.CopyToAsync(fileStream);
                        }

                        producto.UrlImagen = $"/img/productos/{uniqueFileName}";
                    }

                    producto.FechaModificacion = DateTime.UtcNow;
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
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
            return View(producto);
        }

        // GET: Productos/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                // Eliminar la imagen si existe
                if (!string.IsNullOrEmpty(producto.UrlImagen) && !producto.UrlImagen.Contains("producto-default.jpg"))
                {
                    var imagePath = Path.Combine(_env.WebRootPath, producto.UrlImagen.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
