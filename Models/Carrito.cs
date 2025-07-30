using System;
using System.Collections.Generic;
using System.Linq;

namespace TiendaApp.Models
{
    public class Carrito : List<ItemCarrito>
    {
        public decimal Subtotal => this.Sum(item => item.Subtotal);
        public decimal Iva => Subtotal * 0.21m; // 21% de IVA
        public decimal Total => Subtotal + Iva;
    }

    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }
}
