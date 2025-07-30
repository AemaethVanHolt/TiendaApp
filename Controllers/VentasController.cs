using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TiendaApp.Data;
using TiendaApp.Models;
using TiendaApp.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaApp.Controllers
{
    [Authorize(Roles = "Administrador")] // Solo el administrador está autorizado a acceder a la lista de ventas
    public class VentasController : Controller
    {
        private readonly TiendaContext _context;

        public VentasController(TiendaContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener todas las ventas con la información del usuario y detalles
                var ventas = await _context.Ventas
                    .Include(v => v.Usuario)
                    .Include(v => v.DetallesVenta)
                        .ThenInclude(dv => dv.Producto)
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();

                return View(ventas);
            }
            catch (Exception ex)
            {
                // Log the error (you should use a proper logging framework)
                Console.WriteLine($"Error al obtener las ventas: {ex.Message}");
                // Return an empty list to prevent null reference exceptions in the view
                return View(new List<Venta>());
            }
        }

        // GET: Ventas/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (venta == null)
            {
                return NotFound();
            }

            // Crear el ViewModel
            var viewModel = new VentaDetalleViewModel
            {
                Venta = venta,
                Detalles = venta.DetallesVenta.Select(dv => new DetalleVentaItem
                {
                    ProductoId = dv.ProductoId,
                    ProductoNombre = dv.Producto?.Nombre ?? "Producto no disponible",
                    Producto = dv.Producto,
                    ImagenUrl = dv.Producto?.ImagenUrl,
                    PrecioUnitario = dv.PrecioUnitario,
                    Cantidad = dv.Cantidad
                }).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Reporte()
        {
            var fechaInicio = DateTime.Today.AddMonths(-1); // Reporte del último mes, cambiar si se quieren más meses a -2, -3 etc.
            var fechaFin = DateTime.Today;

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin) // Filtrar ventas por fecha
                .ToListAsync();

            var ventasPorDia = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            ViewBag.VentasPorDia = ventasPorDia;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(ventas);
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }
    }
}