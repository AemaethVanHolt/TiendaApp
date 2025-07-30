using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class PedidosController : Controller
    {
        private readonly TiendaContext _context;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(TiendaContext context, ILogger<PedidosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized("Usuario no autenticado correctamente");
            }
            var esAdmin = User.IsInRole("Administrador");

            IQueryable<Pedido> pedidosQuery = _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto);

            if (!esAdmin)
            {
                pedidosQuery = pedidosQuery.Where(p => p.UsuarioId == usuarioId);
            }

            var pedidos = await pedidosQuery.OrderByDescending(p => p.FechaPedido).ToListAsync();
            return View(pedidos);
        }

        // GET: Pedidos/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (pedido == null)
            {
                return NotFound();
            }

            // Verificar que el usuario tenga permiso para ver este pedido
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId) || 
                (!User.IsInRole("Administrador") && pedido.UsuarioId != usuarioId))
            {
                return Forbid();
            }

            return View(pedido);
        }

        // GET: Pedidos/RealizarPedido
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> RealizarPedido()
        {
            var carrito = await ObtenerCarritoAsync();
            if (carrito == null || !carrito.Any())
            {
                TempData["Mensaje"] = "El carrito está vacío. Agrega productos antes de realizar un pedido.";
                return RedirectToAction("Index", "Productos");
            }

            var modelo = new RealizarPedidoViewModel
            {
                Items = carrito.ToList(),
                DireccionEnvio = string.Empty,
                Total = carrito.Total
            };

            return View(modelo);
        }

        // POST: Pedidos/RealizarPedido
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> RealizarPedido(RealizarPedidoViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var carrito = await ObtenerCarritoAsync();
                if (carrito == null || !carrito.Any())
                {
                    TempData["Error"] = "No hay productos en el carrito.";
                    return RedirectToAction("Index", "Productos");
                }

                // Verificar el stock de los productos
                foreach (var item in carrito)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null || producto.Stock < item.Cantidad)
                    {
                        TempData["Error"] = $"No hay suficiente stock para el producto: {item.Nombre}";
                        return RedirectToAction("VerCarrito", "Carrito");
                    }
                }

                // Actualizar el stock y crear el pedido
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var userIdClaim = User.FindFirstValue("UserId");
                        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
                        {
                            ModelState.AddModelError("", "Usuario no autenticado correctamente");
                            return View(modelo);
                        }
                        
                        var usuario = await _context.Usuarios.FindAsync(usuarioId);
                        if (usuario == null)
                        {
                            ModelState.AddModelError("", "Usuario no encontrado");
                            return View(modelo);
                        }

                        if (carrito == null || !carrito.Any())
                        {
                            ModelState.AddModelError("", "El carrito está vacío");
                            return View(modelo);
                        }

                        var pedido = new Pedido
                        {
                            UsuarioId = usuarioId,
                            Usuario = usuario,
                            FechaPedido = DateTime.Now,
                            DireccionEnvio = modelo.DireccionEnvio ?? string.Empty,
                            Estado = "Pendiente",
                            Total = carrito.Sum(i => i.Subtotal),
                            Detalles = new List<DetallePedido>()
                        };

                        // Agregar los detalles al pedido
                        foreach (var item in carrito)
                        {
                            var producto = _context.Productos.Find(item.ProductoId);
                            if (producto != null)
                            {
                                pedido.Detalles.Add(new DetallePedido
                                {
                                    ProductoId = item.ProductoId,
                                    Producto = producto,
                                    Cantidad = item.Cantidad,
                                    PrecioUnitario = item.Precio,
                                    Subtotal = item.Subtotal,
                                    Pedido = pedido,
                                    PedidoId = pedido.Id
                                });
                            }
                        }

                        _context.Pedidos.Add(pedido);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        // Limpiar el carrito
                        await LimpiarCarritoAsync();

                        TempData["Mensaje"] = "¡Pedido realizado con éxito!";
                        return RedirectToAction(nameof(Detalles), new { id = pedido.Id });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error al procesar el pedido");
                        ModelState.AddModelError("", "Ocurrió un error al procesar el pedido. Por favor, inténtalo de nuevo.");
                    }
                }
            }

            // Si llegamos hasta aquí, algo salió mal
            var carritoActual = await ObtenerCarritoAsync();
            modelo.Items = carritoActual?.ToList() ?? new List<ItemCarrito>();
            return View(modelo);
        }

        // GET: Pedidos/Cancelar/5
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Cancelar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (pedido == null)
            {
                return NotFound();
            }

            // Verificar que el pedido pertenece al usuario actual
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId) || 
                pedido.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            // Solo se pueden cancelar pedidos en estado Pendiente
            if (pedido.Estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden cancelar pedidos en estado 'Pendiente'.";
                return RedirectToAction(nameof(Index));
            }

            return View(pedido);
        }

        // POST: Pedidos/Cancelar/5
        [HttpPost, ActionName("Cancelar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CancelarConfirmado(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (pedido == null)
            {
                return NotFound();
            }

            // Verificar que el pedido pertenece al usuario actual
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId) || 
                pedido.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            // Solo se pueden cancelar pedidos en estado Pendiente
            if (pedido.Estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden cancelar pedidos en estado 'Pendiente'.";
                return RedirectToAction(nameof(Index));
            }

            // Devolver el stock de los productos
            foreach (var detalle in pedido.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.Stock += detalle.Cantidad;
                }
            }

            // Actualizar el estado del pedido
            pedido.Estado = "Cancelado";
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "El pedido ha sido cancelado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Pedidos/CambiarEstado/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            var modelo = new CambiarEstadoPedidoViewModel
            {
                PedidoId = pedido.Id,
                EstadoActual = pedido.Estado,
                EstadosPosibles = new List<string> { "Pendiente", "Procesando", "Enviado", "Entregado", "Cancelado" },
                NuevoEstado = pedido.Estado
            };

            return View(modelo);
        }

        // POST: Pedidos/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(CambiarEstadoPedidoViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var pedido = await _context.Pedidos.FindAsync(modelo.PedidoId);
                if (pedido == null)
                {
                    return NotFound();
                }

                pedido.Estado = modelo.NuevoEstado;
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "El estado del pedido ha sido actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            modelo.EstadosPosibles = new List<string> { "Pendiente", "Procesando", "Enviado", "Entregado", "Cancelado" };
            return View(modelo);
        }

        private Task<Carrito> ObtenerCarritoAsync()
        {
            var carritoJson = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoJson))
            {
                return Task.FromResult(new Carrito());
            }

            var carrito = System.Text.Json.JsonSerializer.Deserialize<Carrito>(carritoJson);
            return Task.FromResult(carrito ?? new Carrito());
        }

        private Task LimpiarCarritoAsync()
        {
            HttpContext.Session.Remove("Carrito");
            return Task.CompletedTask;
        }
    }

    public class RealizarPedidoViewModel
    {
        public List<ItemCarrito> Items { get; set; } = new List<ItemCarrito>();
        
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        [Display(Name = "Dirección de envío (opcional)")]
        public string? DireccionEnvio { get; set; }
        
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
    }

    public class CambiarEstadoPedidoViewModel
    {
        public int PedidoId { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public string NuevoEstado { get; set; } = string.Empty;
        public List<string> EstadosPosibles { get; set; } = new List<string>();
    }
}
