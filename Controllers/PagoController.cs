using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;
using TiendaApp.Models.ViewModels;
using TiendaApp.Extensions;
using System.Security.Claims;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TiendaApp.Controllers
{
    [Authorize]
    public class PagoController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<PagoController> _logger;

        public PagoController(TiendaContext context, ILogger<PagoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Pago/MetodoPago
        [Authorize] // Debe estar logado
        public IActionResult MetodoPago()
        {
            _logger.LogInformation("=== INICIO MetodoPago ===");
            _logger.LogInformation("Usuario autenticado: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("Nombre de usuario: {UserName}", User.Identity?.Name);
            
            // Obtener todas las claims del usuario
            _logger.LogInformation("=== Claims del usuario ===");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            _logger.LogInformation("=======================");

            // Obtener el ID del usuario actual de diferentes maneras
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                        User.FindFirstValue("UserId");
                        
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No se pudo obtener el ID del usuario. Redirigiendo a inicio de sesión.");
                return RedirectToAction("IniciarSesion", "Cuenta", new { returnUrl = "/Pago/MetodoPago" });
            }

            _logger.LogInformation("Usuario autenticado con ID: {UserId}", userId);

            // Obtener el carrito del usuario
            var carrito = ObtenerCarrito();
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("VerCarrito", "Carrito");
            }

            // Crear el modelo de vista
            var model = new TiendaApp.Models.ViewModels.MetodoPagoViewModel
            {
                Total = carrito.Sum(item => item.Precio * item.Cantidad)
            };

            return View(model);
        }

        // POST: Pago/ProcesarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarPago(MetodoPagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("MetodoPago", model); // Si el modelo no es válido, volver a mostrar el formulario
            }

            // Obtener el ID del usuario actual
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Obtener el carrito
            var carrito = ObtenerCarrito();
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("VerCarrito", "Carrito");
            }

            // Obtener el usuario de la base de datos para asegurar que existe
            var usuario = await _context.Usuarios.FindAsync(int.Parse(userId));
            if (usuario == null)
            {
                return Unauthorized();
            }

            // Obtener los productos del carrito de la base de datos
            var productosEnCarrito = await _context.Productos
                .Where(p => carrito.Select(c => c.ProductoId).Contains(p.Id))
                .ToListAsync();

            // Verificar que todos los productos existen y hay suficiente stock
            foreach (var item in carrito)
            {
                var producto = productosEnCarrito.FirstOrDefault(p => p.Id == item.ProductoId);
                if (producto == null)
                {
                    ModelState.AddModelError("", $"El producto con ID {item.ProductoId} ya no está disponible");
                    return View("MetodoPago", model);
                }

                if (producto.Stock < item.Cantidad)
                {
                    ModelState.AddModelError("", $"No hay suficiente stock para el producto: {producto.Nombre}");
                    return View("MetodoPago", model);
                }
            }

            // Calcular total
            var total = carrito.Sum(item => item.Precio * item.Cantidad);

            // Crear el pedido
            var pedido = new Pedido
            {
                UsuarioId = int.Parse(userId),
                Usuario = usuario,
                FechaPedido = DateTime.UtcNow,
                Estado = "Pendiente",
                Total = total,
                // Usar la dirección del perfil del usuario o solicitar una dirección
                DireccionEnvio = usuario.Direccion ?? "Dirección no especificada",
                Detalles = new List<DetallePedido>()
            };

            // Agregar los detalles del pedido
            foreach (var item in carrito)
            {
                var producto = productosEnCarrito.First(p => p.Id == item.ProductoId);
                var detalle = new DetallePedido
                {
                    Pedido = pedido,
                    ProductoId = item.ProductoId,
                    Producto = producto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio,
                    Subtotal = item.Precio * item.Cantidad
                };
                pedido.Detalles.Add(detalle);
            }

            // Guardar el pedido en la base de datos
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Limpiar el carrito
            LimpiarCarrito();

            // Redirigir a la página de confirmación
            return RedirectToAction("Confirmacion", new { id = pedido.Id });
        }

        // GET: Pago/Confirmacion/{id?}
        [Route("Pago/Confirmacion/{id?}")]
        public async Task<IActionResult> Confirmacion(int? id)
        {
            int ventaId;
            
            // Si no se proporciona un ID, intentar obtenerlo de TempData
            if (!id.HasValue)
            {
                if (TempData["VentaId"] == null || !int.TryParse(TempData["VentaId"]?.ToString(), out ventaId))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ventaId = id.Value;
            }
            
            // Obtener la venta con sus detalles
            var venta = await _context.Ventas
                .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // POST: Pago/ProcesarContrarreembolso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarContrarreembolso()
        {
            try
            {
                // Obtener el ID del usuario actual
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                            User.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Obtener el carrito de la sesión
                var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();

                if (!carrito.Any())
                {
                    return Json(new { success = false, message = "El carrito está vacío" });
                }

                // Calcular totales
                decimal total = carrito.Sum(item => item.Precio * item.Cantidad);

                // Crear la venta
                var venta = new Venta
                {
                    UsuarioId = int.Parse(userId),
                    FechaVenta = DateTime.Now,
                    Estado = "Pendiente",
                    MetodoPago = "Contrarreembolso",
                    Total = total,
                    DetallesVenta = carrito.Select(item => new DetalleVenta
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio,
                        Total = item.Precio * item.Cantidad
                    }).ToList()
                };

                // Actualizar el stock de los productos
                foreach (var item in carrito)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto != null)
                    {
                        if (producto.Stock < item.Cantidad)
                        {
                            TempData["Error"] = $"No hay suficiente stock para el producto: {producto.Nombre}";
                            return RedirectToAction("VerCarrito", "Carrito");
                        }
                        producto.Stock -= item.Cantidad;
                    }
                }

                // Guardar la venta
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // Limpiar el carrito
                HttpContext.Session.Remove("Carrito");
                
                // Guardar el ID de la venta en TempData para mostrarlo en la confirmación
                TempData["VentaId"] = venta.Id;

                // Devolver JSON con la URL de redirección
                return Json(new { 
                    success = true, 
                    redirectUrl = Url.Action("Confirmacion", "Pago", new { id = venta.Id }, Request.Scheme) 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el pago por contrarreembolso");
                TempData["Error"] = "Ocurrió un error al procesar su pedido. Por favor, intente nuevamente.";
                return RedirectToAction("MetodoPago");
            }
        }

        private List<ItemCarrito> ObtenerCarrito()
        {
            try
            {
                var carritoJson = HttpContext.Session.GetString("Carrito");
                if (string.IsNullOrEmpty(carritoJson))
                {
                    return new List<ItemCarrito>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var items = JsonSerializer.Deserialize<List<ItemCarrito>>(carritoJson, options);
                return items ?? new List<ItemCarrito>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el carrito de la sesión");
                return new List<ItemCarrito>();
            }
        }

        private void GuardarCarrito(List<ItemCarrito> items)
        {
            try
            {
                items ??= new List<ItemCarrito>();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var carritoJson = JsonSerializer.Serialize(items, options);
                HttpContext.Session.SetString("Carrito", carritoJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el carrito en la sesión");
                throw; // Relanzar la excepción para que el llamador pueda manejarla
            }
        }

        private void LimpiarCarrito()
        {
            HttpContext.Session.Remove("Carrito");
        }
    }
}
