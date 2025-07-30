using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers
{
    public class HomeController : Controller // Este controlador maneja las acciones relacionadas con la página de inicio y el panel de control de la tienda
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TiendaContext _context;

        public HomeController(ILogger<HomeController> logger, TiendaContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productosDestacados = await _context.Productos
                .Where(p => p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(8)
                .ToListAsync();

            return View(productosDestacados);
        }

        [Authorize(Roles = "Administrador")] // Autorización solo a ADMINISTRADORES al panel de control
        public IActionResult PanelControl()
        {
            var modelo = new PanelControlViewModel // En el panel de administración, muestra los productos en tabla
            {
                TotalProductos = _context.Productos.Count(),
                TotalUsuarios = _context.Usuarios.Count(),
                TotalPedidos = _context.Pedidos.Count(),
                PedidosRecientes = _context.Pedidos
                    .Include(p => p.Usuario)
                    .OrderByDescending(p => p.FechaPedido)
                    .Take(5)
                    .ToList(),
                ProductosBajoStock = _context.Productos
                    .Where(p => p.Stock < 10 && p.EstaActivo)
                    .OrderBy(p => p.Stock)
                    .Take(5)
                    .ToList()
            };

            return View(modelo);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };

            switch (statusCode) // Mensajes de error en caso de faltar datos o errores del servidor
            {
                case 404:
                    ViewBag.ErrorMessage = "La página solicitada no existe.";
                    break;
                case 403:
                    ViewBag.ErrorMessage = "No tiene permisos para acceder a este recurso.";
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Ha ocurrido un error en el servidor.";
                    break;
                default:
                    ViewBag.ErrorMessage = "Ha ocurrido un error inesperado.";
                    break;
            }

            return View("Error", errorModel);
        }
    }

    public class PanelControlViewModel
    {
        public int TotalProductos { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalPedidos { get; set; }
        public List<Pedido> PedidosRecientes { get; set; } = new List<Pedido>();
        public List<Producto> ProductosBajoStock { get; set; } = new List<Producto>();
    }
}