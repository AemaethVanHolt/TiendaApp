using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using TiendaApp.Models;
using TiendaApp.Data;
using TiendaApp.Models.ViewModels;

namespace TiendaApp.Controllers
{
    [Authorize(Roles = "Cliente, Administrador")]
    public class CarritoController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(TiendaContext context, ILogger<CarritoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Vaciar()
        {
            HttpContext.Session.Remove("Carrito");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> VerCarrito()
        {
            var carrito = await ObtenerCarritoAsync();
            
            // Verificar si hay productos en el carrito
            if (carrito == null || !carrito.Any())
            {
                ViewBag.Mensaje = "Tu carrito está vacío";
                return View(new Carrito());
            }

            // Verificar stock de cada producto en el carrito
            var productosSinStock = new List<string>();
            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null || producto.Stock < item.Cantidad)
                {
                    productosSinStock.Add(item.Nombre);
                }
            }

            if (productosSinStock.Any())
            {
                ViewBag.Error = "Algunos productos no tienen suficiente stock o ya no están disponibles.";
                ViewBag.ProductosSinStock = productosSinStock;
            }

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAlCarrito(int productoId, int cantidad = 1)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return NotFound();
            }

            if (cantidad < 1)
            {
                cantidad = 1;
            }

            // Verificar stock disponible
            if (producto.Stock < cantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible para este producto.";
                return RedirectToAction("Index", "Productos");
            }

            var carrito = await ObtenerCarritoAsync();
            var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new ItemCarrito
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    ImagenUrl = producto.ImagenUrl,
                    Precio = producto.Precio,
                    Cantidad = cantidad
                });
            }

            await GuardarCarritoAsync(carrito);
            TempData["Success"] = "Producto agregado al carrito correctamente.";
            return RedirectToAction("VerCarrito");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCantidad(int productoId, int cantidad)
        {
            if (cantidad < 0)
            {
                return BadRequest("La cantidad no puede ser negativa.");
            }

            var carrito = await ObtenerCarritoAsync();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item == null)
            {
                return NotFound("Producto no encontrado en el carrito.");
            }

            if (cantidad == 0)
            {
                return await EliminarDelCarrito(productoId);
            }

            // Verificar stock disponible
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto != null && producto.Stock < cantidad)
            {
                return BadRequest("No hay suficiente stock disponible para la cantidad solicitada.");
            }

            item.Cantidad = cantidad;

            await GuardarCarritoAsync(carrito);
            return Ok(new { 
                subtotal = item.Subtotal.ToString("C"),
                total = carrito.Total.ToString("C") 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDelCarrito(int productoId)
        {
            var carrito = await ObtenerCarritoAsync();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item);
                await GuardarCarritoAsync(carrito);
                TempData["Success"] = "Producto eliminado del carrito correctamente.";
            }
            else
            {
                return NotFound("Producto no encontrado en el carrito.");
            }

            return RedirectToAction("VerCarrito");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarPedido()
        {
            var carrito = await ObtenerCarritoAsync();
            if (!carrito.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (usuarioId == 0)
            {
                return RedirectToAction("IniciarSesion", "Cuenta");
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return RedirectToAction("IniciarSesion", "Cuenta");
            }

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Usuario = usuario,
                FechaPedido = DateTime.UtcNow,
                Estado = "Pendiente",
                DireccionEnvio = usuario.Direccion ?? "Dirección no especificada",
                Total = carrito.Sum(i => i.Precio * i.Cantidad)
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null) continue;

                var detalle = new DetallePedido
                {
                    PedidoId = pedido.Id,
                    Pedido = pedido,
                    ProductoId = item.ProductoId,
                    Producto = producto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio,
                    Subtotal = item.Precio * item.Cantidad
                };
                _context.DetallesPedido.Add(detalle);
            }

            await _context.SaveChangesAsync();
            await GuardarCarritoAsync(new Carrito());

            return RedirectToAction(nameof(Index), "Pedidos");
        }

        private Task<Carrito> ObtenerCarritoAsync()
        {
            try
            {
                var carritoJson = HttpContext.Session.GetString("Carrito");
                if (string.IsNullOrEmpty(carritoJson))
                {
                    _logger.LogInformation("No se encontró carrito en la sesión, devolviendo carrito vacío");
                    return Task.FromResult(new Carrito());
                }

                _logger.LogInformation("Carrito JSON de la sesión: " + carritoJson);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                var items = JsonSerializer.Deserialize<List<ItemCarrito>>(carritoJson, options);
                var carrito = new Carrito();
                
                if (items != null)
                {
                    _logger.LogInformation($"Deserializados {items.Count} ítems del carrito");
                    carrito.AddRange(items);
                }
                else
                {
                    _logger.LogWarning("La deserialización del carrito devolvió null");
                }
                
                return Task.FromResult(carrito);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar el carrito de la sesión. JSON: " + HttpContext.Session.GetString("Carrito"));
                // Si hay un error al deserializar, devolvemos un carrito vacío
                return Task.FromResult(new Carrito());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener el carrito");
                return Task.FromResult(new Carrito());
            }
        }

        private Task GuardarCarritoAsync(Carrito carrito)
        {
            try
            {
                carrito ??= new Carrito();
                var items = carrito.ToList();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                var carritoJson = JsonSerializer.Serialize(items, options);
                _logger.LogInformation($"Guardando carrito en sesión: {carritoJson}");
                
                HttpContext.Session.SetString("Carrito", carritoJson);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el carrito en la sesión");
                return Task.CompletedTask;
            }
        }

        // Método para obtener la cantidad de ítems en el carrito (usado por AJAX)
        [HttpGet]
        public async Task<JsonResult> GetCount()
        {
            var carrito = await ObtenerCarritoAsync();
            return Json(new { count = carrito?.Sum(i => i.Cantidad) ?? 0 });
        }

        // POST: Carrito/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateQuantity(int productoId, int cantidad)
        {
            try
            {
                var carrito = await ObtenerCarritoAsync();
                var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

                if (item == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado en el carrito" });
                }

                // Obtener el producto para verificar el stock
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Verificar que la cantidad solicitada no supere el stock disponible
                if (cantidad > producto.Stock)
                {
                    return Json(new { 
                        success = false, 
                        message = "No hay suficiente stock disponible",
                        maxQuantity = producto.Stock
                    });
                }

                // Actualizar la cantidad
                item.Cantidad = cantidad;

                // Si la cantidad es 0 o menos, eliminar el ítem
                if (item.Cantidad <= 0)
                {
                    carrito.Remove(item);
                }

                // Guardar los cambios en la sesión
                await GuardarCarritoAsync(carrito);

                // Calcular el nuevo subtotal
                var subtotal = item.Precio * item.Cantidad;
                var total = carrito.Sum(i => i.Precio * i.Cantidad);

                return Json(new { 
                    success = true, 
                    subtotal = subtotal.ToString("C2"),
                    total = total.ToString("C2"),
                    count = carrito.Sum(i => i.Cantidad)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la cantidad del producto {ProductoId}", productoId);
                return Json(new { success = false, message = "Error al actualizar la cantidad" });
            }
        }

        // Método para obtener los ítems del carrito (usado por AJAX)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                _logger.LogInformation("Obteniendo carrito de la sesión...");
                var carrito = await ObtenerCarritoAsync();
                _logger.LogInformation($"Carrito obtenido. Cantidad de ítems: {carrito.Count}");
                
                var items = carrito.Select(item => new CarritoItemViewModel
                {
                    ProductoId = item.ProductoId,
                    Nombre = item.Nombre,
                    ImagenUrl = item.ImagenUrl,
                    Precio = item.Precio,
                    Cantidad = item.Cantidad
                }).ToList();

                _logger.LogInformation($"Items procesados: {items.Count}");
                _logger.LogInformation($"Total de ítems: {items.Sum(i => i.Cantidad)}");
                _logger.LogInformation($"Total: {items.Sum(i => i.Subtotal)}");

                return Json(new 
                { 
                    success = true,
                    items = items,
                    total = items.Sum(i => i.Subtotal),
                    count = items.Sum(i => i.Cantidad)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los ítems del carrito");
                return Json(new 
                { 
                    success = false,
                    message = "Error al cargar el carrito",
                    error = ex.Message
                });
            }
        }



        // Método para eliminar un ítem del carrito (usado por AJAX)
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productoId)
        {
            var carrito = await ObtenerCarritoAsync();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item);
                await GuardarCarritoAsync(carrito);
                return Json(new { success = true });
            }

            return NotFound("Producto no encontrado en el carrito");
        }
    }

    // Las clases Carrito e ItemCarrito ahora están definidas en TiendaApp.Models
}
