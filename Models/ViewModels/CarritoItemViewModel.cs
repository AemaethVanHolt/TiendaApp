using System;

namespace TiendaApp.Models.ViewModels
{
    public class CarritoItemViewModel // Este modelo se utiliza para representar un artículo en el carrito de compras
    {
        public int ProductoId { get; set; } // Identificador único del producto
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; } // URL de la imagen del producto
        public decimal Precio { get; set; } // Precio unitario del producto
        public int Cantidad { get; set; } // Cantidad del producto en el carrito
        public decimal Subtotal => Precio * Cantidad;  // Subtotal del producto (Precio * Cantidad)
    }
}
