using System;
using System.Collections.Generic;
using System.Linq;
using TiendaApp.Models;

namespace TiendaApp.Models.ViewModels
{
    public class VentaDetalleViewModel
    {
        public Venta Venta { get; set; } = new Venta();
        public List<DetalleVentaItem> Detalles { get; set; } = new List<DetalleVentaItem>();
        
        // Propiedades calculadas
        public decimal Subtotal => Detalles.Sum(d => d.Subtotal);
        public decimal Iva => Subtotal * 0.21m; // 21% de IVA
        public decimal Total => Subtotal + Iva;
    }

    public class DetalleVentaItem
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Total => PrecioUnitario * Cantidad;
        public decimal Subtotal => Total; // Para compatibilidad con la vista
        public Producto? Producto { get; set; } // Referencia al producto completo
    }
}
