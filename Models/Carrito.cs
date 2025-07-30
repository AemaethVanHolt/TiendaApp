using System;
using System.Collections.Generic;
using System.Linq;

namespace TiendaApp.Models
{
    public class Carrito : List<ItemCarrito> // Hereda de List<ItemCarrito> para manejar una colecci�n de art�culos en el carrito
    {
        public decimal Subtotal => this.Sum(item => item.Subtotal);
        public decimal Iva => Subtotal * 0.21m; // 21% de IVA
        public decimal Total => Subtotal + Iva; // Se calcula el subtotal y el IVA del 21%
    }

    public class ItemCarrito // Productos que est�n en el carrito de la compra
    {
        public int ProductoId { get; set; } // ID �nica del producto
        public string Nombre { get; set; } = string.Empty; // Nombre del producto
        public string? ImagenUrl { get; set; } // URL de la imagen del producto
        public decimal Precio { get; set; } // Precio unitario del producto
        public int Cantidad { get; set; } // Cantidad del producto en el carrito
        public decimal Subtotal => Precio * Cantidad;  // Subtotal del producto (Precio * Cantidad)
    }
}
