using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Data.SeedData
{
    public static class ProductSeedData
    {
        public static async Task SeedProductsAsync(TiendaContext context)
        {
            Console.WriteLine("Verificando si ya existen productos...");
            bool productosExisten = await context.Productos.AnyAsync();
            Console.WriteLine($"Productos existentes: {productosExisten}");
            
            if (productosExisten)
            {
                Console.WriteLine("Ya existen productos en la base de datos. No se cargarán datos de prueba.");
                return;
            }
            
            Console.WriteLine("No hay productos existentes. Cargando datos de prueba...");

            var productos = new List<Producto>
            {
                // Teléfonos
                new Producto
                {
                    Nombre = "Smartphone X1 Pro",
                    Descripcion = "Último modelo con cámara de 108MP y pantalla AMOLED de 6.7\"",
                    Precio = 999.99m,
                    Stock = 30,
                    ImagenUrl = "/images/productos/Smartphone X1 Pro.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Teléfonos",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Smartphone Gama Media",
                    Descripcion = "Excelente relación calidad-precio con cámara triple de 64MP",
                    Precio = 299.99m,
                    Stock = 75,
                    ImagenUrl = "/images/productos/Smartphone Gama Media.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Teléfonos",
                    Activo = true
                },
                
                // Portátiles
                new Producto
                {
                    Nombre = "Portátil Pro 15\"",
                    Descripcion = "Portátil potente con procesador i7, 16GB RAM y 512GB SSD",
                    Precio = 1299.99m,
                    Stock = 25,
                    ImagenUrl = "/images/productos/Portátil Pro 15.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Portátiles",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Portátil Ultraligero",
                    Descripcion = "Solo 1.2kg con pantalla táctil y 12h de batería",
                    Precio = 899.99m,
                    Stock = 20,
                    ImagenUrl = "/images/productos/Portátil Ultraligero.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Portátiles",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Workstation Profesional",
                    Descripcion = "Potente estación de trabajo con tarjeta gráfica dedicada",
                    Precio = 2499.99m,
                    Stock = 8,
                    ImagenUrl = "/images/productos/Workstation Profesional.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Portátiles",
                    Activo = true
                },
                
                // Audio
                new Producto
                {
                    Nombre = "Auriculares Inalámbricos Pro",
                    Descripcion = "Cancelación de ruido activa y sonido envolvente 3D",
                    Precio = 279.99m,
                    Stock = 45,
                    ImagenUrl = "/images/productos/Auriculares Inalámbricos Pro.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Audio",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Auriculares Deportivos",
                    Descripcion = "Resistentes al sudor con 8h de autonomía",
                    Precio = 79.99m,
                    Stock = 85,
                    ImagenUrl = "/images/productos/Auriculares Deportivos.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Audio",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Altavoz Inteligente",
                    Descripcion = "Control por voz y calidad de sonido premium",
                    Precio = 129.99m,
                    Stock = 60,
                    ImagenUrl = "/images/productos/Altavoz Inteligente.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Audio",
                    Activo = true
                },
                
                // Wearables
                new Producto
                {
                    Nombre = "Smartwatch Pro",
                    Descripcion = "Reloj inteligente con monitor de frecuencia cardíaca y GPS",
                    Precio = 249.99m,
                    Stock = 40,
                    ImagenUrl = "/images/productos/Smartwatch Pro.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Wearables",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Pulsera Actividad",
                    Descripcion = "Seguimiento de actividad física y sueño",
                    Precio = 79.99m,
                    Stock = 120,
                    ImagenUrl = "/images/productos/Pulsera Actividad.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Wearables",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Auriculares deportivos con sensor cardíaco",
                    Descripcion = "Rastreador de frecuencia cardíaca integrado en auriculares",
                    Precio = 149.99m,
                    Stock = 35,
                    ImagenUrl = "/images/productos/Auriculares deportivos con sensor cardíaco.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Wearables",
                    Activo = true
                },
                
                // Tablets
                new Producto
                {
                    Nombre = "Tablet 10\"",
                    Descripcion = "Tablet con pantalla Full HD y 128GB de almacenamiento",
                    Precio = 349.99m,
                    Stock = 30,
                    ImagenUrl = "/images/productos/Tablet 10.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Tablets",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Tablet Pro 12.9\"",
                    Descripcion = "Tablet profesional con lápiz táctil incluido",
                    Precio = 1099.99m,
                    Stock = 15,
                    ImagenUrl = "/images/productos/Tablet Pro 12.9.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Tablets",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Tablet Infantil",
                    Descripcion = "Tablet resistente con controles parentales y contenido educativo",
                    Precio = 129.99m,
                    Stock = 45,
                    ImagenUrl = "/images/productos/Tablet Infantil.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Tablets",
                    Activo = true
                },
                
                // Audio
                new Producto
                {
                    Nombre = "Altavoz Bluetooth",
                    Descripcion = "Altavoz portátil resistente al agua con 20h de duración",
                    Precio = 89.99m,
                    Stock = 75,
                    ImagenUrl = "/images/productos/Altavoz Bluetooth.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Audio",
                    Activo = true
                },
                
                // Cámaras
                new Producto
                {
                    Nombre = "Cámara Mirrorless Pro",
                    Descripcion = "Cámara sin espejo con sensor full frame y 4K",
                    Precio = 2299.99m,
                    Stock = 10,
                    ImagenUrl = "/images/productos/Cámara Mirrorless Pro.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Fotografía",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Drone 4K con cámara",
                    Descripcion = "Dron con cámara 4K y estabilización en 3 ejes",
                    Precio = 799.99m,
                    Stock = 12,
                    ImagenUrl = "/images/productos/Drone 4K con cámara.webp",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Fotografía",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Cámara Deportiva",
                    Descripcion = "Resistente al agua y golpes, ideal para deportes extremos",
                    Precio = 349.99m,
                    Stock = 25,
                    ImagenUrl = "/images/productos/Cámara Deportiva.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Fotografía",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Objetivo 50mm f/1.8",
                    Descripcion = "Objetivo luminoso ideal para retratos",
                    Precio = 199.99m,
                    Stock = 30,
                    ImagenUrl = "/images/productos/Objetivo 50mm f1.8.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Fotografía",
                    Activo = true
                },
                
                // Periféricos
                new Producto
                {
                    Nombre = "Teclado Mecánico Gaming",
                    Descripcion = "Teclado con switches mecánicos y retroiluminación RGB personalizable",
                    Precio = 129.99m,
                    Stock = 45,
                    ImagenUrl = "/images/productos/Teclado Mecánico Gaming.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Periféricos",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Ratón Gaming Pro",
                    Descripcion = "16.000 DPI y diseño ergonómico para gaming",
                    Precio = 89.99m,
                    Stock = 65,
                    ImagenUrl = "/images/productos/Ratón Gaming Pro.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Periféricos",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Alfombrilla Gaming XL",
                    Descripcion = "Superficie de tela de alta precisión para gaming",
                    Precio = 29.99m,
                    Stock = 120,
                    ImagenUrl = "/images/productos/alfombra.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Periféricos",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Webcam Full HD",
                    Descripcion = "Cámara web con micrófono integrado y enfoque automático",
                    Precio = 79.99m,
                    Stock = 55,
                    ImagenUrl = "/images/productos/Webcam Full HD.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Periféricos",
                    Activo = true
                },
                
                // Monitores
                new Producto
                {
                    Nombre = "Monitor 27\" 4K HDR",
                    Descripcion = "Monitor profesional con resolución 4K, HDR y 99% sRGB",
                    Precio = 449.99m,
                    Stock = 15,
                    ImagenUrl = "/images/productos/Monitor 27 4K HDR.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Monitores",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Monitor Curvo 34\" UWQHD",
                    Descripcion = "Ultrawide QHD con 144Hz, FreeSync y curvatura 1500R",
                    Precio = 699.99m,
                    Stock = 12,
                    ImagenUrl = "/images/productos/Monitor Curvo 34 UWQHD.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Monitores",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Monitor Gaming 24.5\" 240Hz",
                    Descripcion = "Tasa de refresco de 240Hz y 1ms para gaming competitivo",
                    Precio = 329.99m,
                    Stock = 18,
                    ImagenUrl = "/images/productos/Monitor Gaming 24.5 240Hz.webp",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Monitores",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Monitor Profesional 32\" 4K",
                    Descripcion = "Pantalla 4K con calibración de fábrica y 98% DCI-P3",
                    Precio = 899.99m,
                    Stock = 8,
                    ImagenUrl = "/images/productos/Monitor Profesional 32 4K.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Monitores",
                    Activo = true
                },
                
                // Almacenamiento
                new Producto
                {
                    Nombre = "Disco Duro Externo 2TB",
                    Descripcion = "Disco duro portátil USB 3.2 con 2TB de capacidad",
                    Precio = 79.99m,
                    Stock = 65,
                    ImagenUrl = "/images/productos/Disco Duro Externo 2TB.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Almacenamiento",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "SSD SATA 1TB",
                    Descripcion = "Unidad de estado sólido SATA III con hasta 560MB/s de lectura",
                    Precio = 109.99m,
                    Stock = 45,
                    ImagenUrl = "/images/productos/SSD SATA 1TB.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Almacenamiento",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "SSD NVMe 1TB",
                    Descripcion = "Unidad NVMe PCIe 4.0 con hasta 7000MB/s de lectura",
                    Precio = 129.99m,
                    Stock = 38,
                    ImagenUrl = "/images/productos/SSD NVMe 1TB.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Almacenamiento",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Disco Duro Externo 5TB",
                    Descripcion = "Gran capacidad para copias de seguridad y almacenamiento",
                    Precio = 129.99m,
                    Stock = 25,
                    ImagenUrl = "/images/productos/Disco Duro Externo 5TB.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Almacenamiento",
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Lector de Tarjetas USB 3.0",
                    Descripcion = "Soporta múltiples formatos de tarjetas SD y microSD",
                    Precio = 14.99m,
                    Stock = 120,
                    ImagenUrl = "/images/productos/Lector de Tarjetas USB 3.0.jpg",
                    FechaCreacion = DateTime.Now,
                    Tipo = "Almacenamiento",
                    Activo = true
                }
            };

            Console.WriteLine("Agregando productos al contexto...");
            await context.Productos.AddRangeAsync(productos);
            Console.WriteLine("Guardando cambios en la base de datos...");
            var result = await context.SaveChangesAsync();
            Console.WriteLine($"Se insertaron {result} productos en la base de datos.");
            
            // Verificar que los productos se hayan guardado
            var totalProductos = await context.Productos.CountAsync();
            Console.WriteLine($"Total de productos en la base de datos: {totalProductos}");
        }
    }
}