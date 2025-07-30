using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Data.SeedData
{
    public static class SaleSeedData
    {
        private static readonly ILogger _logger;

        static SaleSeedData()
        {
            // Configurar el logger estático
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger("SaleSeedData");
        }

        public static async Task SeedSalesAsync(TiendaContext context)
        {
            // No verificamos si ya hay ventas, para asegurar que siempre se generen
            // nuevas ventas para los usuarios existentes
            _logger.LogInformation("Iniciando generación de ventas de ejemplo...");
            
            // Limpiar ventas existentes para evitar duplicados
            var ventasExistentes = await context.Ventas.ToListAsync();
            if (ventasExistentes.Any())
            {
                context.Ventas.RemoveRange(ventasExistentes);
                await context.SaveChangesAsync();
                _logger.LogInformation($"Eliminadas {ventasExistentes.Count} ventas existentes");
            }

            // Obtener usuarios que no son administradores
            var clientes = await context.Usuarios
                .Where(u => u.Email != "admin@tienda.com")
                .ToListAsync();

            // Obtener todos los productos con stock disponible
            var productos = await context.Productos
                .Where(p => p.Stock > 0)
                .ToListAsync();

            if (!clientes.Any() || !productos.Any())
            {
                return; // No hay suficientes datos para crear ventas
            }

            var random = new Random();
            var ventas = new List<Venta>();
            var hoy = DateTime.Now;
            var inicioAño = new DateTime(hoy.Year, 1, 1);
            int dias = (hoy - inicioAño).Days;

            // Primera pasada: Asegurar que cada cliente tenga al menos una compra
            foreach (var cliente in clientes)
            {
                var fechaVenta = inicioAño.AddDays(random.Next(dias));
                var venta = CrearVenta(context, cliente.Id, fechaVenta, productos, random);
                if (venta != null)
                {
                    ventas.Add(venta);
                }
            }

            // Segunda pasada: Crear ventas adicionales (entre 20 y 40 ventas más)
            int ventasAdicionales = random.Next(20, 41);
            for (int i = 0; i < ventasAdicionales; i++)
            {
                var cliente = clientes[random.Next(clientes.Count)];
                var fechaVenta = inicioAño.AddDays(random.Next(dias));
                var venta = CrearVenta(context, cliente.Id, fechaVenta, productos, random);
                if (venta != null)
                {
                    ventas.Add(venta);
                }
            }

            // Tercera pasada: Crear algunas ventas recientes (últimos 7 días)
            for (int i = 0; i < 10; i++)
            {
                var cliente = clientes[random.Next(clientes.Count)];
                var fechaVenta = hoy.AddDays(-random.Next(7)).AddHours(-random.Next(24));
                var venta = CrearVenta(context, cliente.Id, fechaVenta, productos, random);
                if (venta != null)
                {
                    ventas.Add(venta);
                }
            }

            await context.Ventas.AddRangeAsync(ventas);
            await context.SaveChangesAsync();
        }

        private static Venta? CrearVenta(TiendaContext context, int usuarioId, DateTime fechaVenta, List<Producto>? productos, Random random)
        {
            // Validar parámetros
            if (usuarioId <= 0 || productos == null)
                return null;
                
            // Filtrar productos con stock disponible
            var productosDisponibles = productos.Where(p => p.Stock > 0).ToList();
            if (!productosDisponibles.Any())
                return null;

            var venta = new Venta
            {
                UsuarioId = usuarioId,
                FechaVenta = fechaVenta,
                Estado = "Completada",
                Total = 0,
                DetallesVenta = new List<DetalleVenta>()
            };

            // Determinar cuántos productos tendrá la venta (1-5 productos)
            int maxProductos = Math.Min(5, productosDisponibles.Count);
            int numProductos = random.Next(1, maxProductos + 1);
            
            // Mezclar productos y tomar la cantidad necesaria
            var productosVenta = productosDisponibles
                .OrderBy(x => random.Next())
                .Take(numProductos)
                .ToList();

            foreach (var producto in productosVenta)
            {
                // Asegurar que no se pida más cantidad que el stock disponible
                int maxCantidad = Math.Min(5, producto.Stock);
                if (maxCantidad <= 0) continue;
                
                int cantidad = random.Next(1, maxCantidad + 1);
                decimal totalProducto = producto.Precio * cantidad;
                
                venta.DetallesVenta.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio,
                    Total = totalProducto
                });

                venta.Total += totalProducto;
                producto.Stock -= cantidad; // Actualizar stock del producto
            }

            // Si por alguna razón no se añadieron productos a la venta, retornar null
            return venta.DetallesVenta?.Any() == true ? venta : null;
        }
    }
}
