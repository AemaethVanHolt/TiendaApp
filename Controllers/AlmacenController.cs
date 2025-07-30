using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaApp.Controllers
{
    [Authorize(Roles = "Administrador")] // El ADMINISTRADOR es el único con acceso al almacén
    public class AlmacenController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<AlmacenController> _logger;

        public AlmacenController(TiendaContext context, ILogger<AlmacenController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(string? filtro, string? orden = "nombre", string? direccion = "asc")
        {
            ViewData["Filtro"] = filtro ?? string.Empty;
            ViewData["OrdenActual"] = orden ?? "nombre";
            ViewData["DireccionActual"] = direccion ?? "asc";
            ViewData["DireccionNombre"] = orden == "nombre" && direccion == "asc" ? "desc" : "asc"; // Cambia la dirección de ordenación al hacer clic en el encabezado de nombre
            ViewData["DireccionStock"] = orden == "stock" && direccion == "asc" ? "desc" : "asc";
            ViewData["DireccionPrecio"] = orden == "precio" && direccion == "asc" ? "desc" : "asc";

            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(filtro)) // Filtrar productos por nombre, descripción o tipo
            {
                var filtroLower = filtro.ToLowerInvariant();
                query = query.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLowerInvariant().Contains(filtroLower)) || 
                    (p.Descripcion != null && p.Descripcion.ToLowerInvariant().Contains(filtroLower)) ||
                    (p.Tipo != null && p.Tipo.ToLowerInvariant().Contains(filtroLower))); // Filtrar por nombre, descripción o tipo, en este caso, por TIPO
            }

            // Aplicar ordenación
            switch (orden)
            {
                case "nombre":
                    query = direccion == "asc" ? 
                        query.OrderBy(p => p.Nombre) : 
                        query.OrderByDescending(p => p.Nombre);
                    break;
                case "stock":
                    query = direccion == "asc" ? 
                        query.OrderBy(p => p.Stock) : 
                        query.OrderByDescending(p => p.Stock);
                    break;
                case "precio":
                    query = direccion == "asc" ? 
                        query.OrderBy(p => p.Precio) : 
                        query.OrderByDescending(p => p.Precio);
                    break;
                default:
                    query = query.OrderBy(p => p.Nombre);
                    break;
            }

            // Obtener productos con stock bajo
            ViewBag.ProductosStockBajo = await _context.Productos
                .Where(p => p.Stock < 10 && p.Activo) // Filtrar productos con stock bajo y activos
                .OrderBy(p => p.Stock) // Ordenar por stock
                .Take(10) // Limitar a los 10 productos con menor stock
                .ToListAsync(); // Obtener los productos con stock bajo

            return View(await query.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Detalles(int? id) // Método para mostrar los detalles de un producto
        {
            if (id == null) // Verificar si el ID es NULL
            {
                return NotFound();
            }

            var producto = await _context.Productos // Buscar el producto por ID
                .FirstOrDefaultAsync(m => m.Id == id); // Usar FirstOrDefaultAsync para obtener el primer producto que coincida con el ID

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Almacen/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Almacen/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

        // POST: Almacen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock,Tipo,ImagenUrl,EstaActivo")] Producto producto, IFormFile? imagenArchivo)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Manejar la carga de la imagen
                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "productos");
                        
                        // Crear el directorio si no existe
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generar un nombre único para el archivo
                        var uniqueFileName = $"producto_{producto.Id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(imagenArchivo.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Guardar el archivo
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagenArchivo.CopyToAsync(fileStream);
                        }

                        // Actualizar la URL de la imagen
                        producto.ImagenUrl = $"/img/productos/{uniqueFileName}";
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar el producto");
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el producto. Por favor, inténtelo de nuevo.");
                }
            }
            return View(producto);
        }

        // GET: Almacen/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Almacen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Precio,Stock,Tipo,ImagenUrl,EstaActivo")] Producto producto, IFormFile? imagenArchivo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(producto);
                    await _context.SaveChangesAsync();

                    // Manejar la carga de la imagen después de crear el producto para tener el ID
                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "productos");
                        
                        // Crear el directorio si no existe
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generar un nombre único para el archivo
                        var uniqueFileName = $"producto_{producto.Id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(imagenArchivo.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Guardar el archivo
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagenArchivo.CopyToAsync(fileStream);
                        }

                        // Actualizar la URL de la imagen
                        producto.ImagenUrl = $"/img/productos/{uniqueFileName}";
                        _context.Update(producto);
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Producto creado correctamente."; // Mensaje de éxito al crear el producto
                    return RedirectToAction(nameof(Index)); // Redirigir a la lista de productos
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear el producto");
                    ModelState.AddModelError("", "Ocurrió un error al crear el producto. Por favor, inténtelo de nuevo.");
                }
            }
            return View(producto);
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarStock(int id, int cantidad)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (cantidad < 0)
            {
                TempData["ErrorMessage"] = "La cantidad no puede ser negativa.";
                return RedirectToAction(nameof(Index));
            }

            producto.Stock = cantidad;
            _context.Update(producto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Stock de {producto.Nombre} actualizado a {cantidad} unidades.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarDesactivar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.Activo = !producto.Activo;
            _context.Update(producto);
            await _context.SaveChangesAsync();

            var estado = producto.Activo ? "activado" : "desactivado";
            TempData["SuccessMessage"] = $"Producto {producto.Nombre} {estado} correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
