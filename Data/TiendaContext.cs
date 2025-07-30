using Microsoft.EntityFrameworkCore;
using TiendaApp.Models;

namespace TiendaApp.Data
{
    public class TiendaContext : DbContext
    {
        private readonly IWebHostEnvironment _env;

        public TiendaContext(DbContextOptions<TiendaContext> options, IWebHostEnvironment env) : base(options)
        {
            _env = env;
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configuración de Rol
            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.Nombre)
                .IsUnique();

            // Configuración de semillas iniciales
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Administrador del sistema", Activo = true, FechaCreacion = DateTime.UtcNow },
                new Rol { Id = 2, Nombre = "Cliente", Descripcion = "Cliente de la tienda", Activo = true, FechaCreacion = DateTime.UtcNow }
            );

            // Ignorar cambios pendientes en el modelo durante el desarrollo
            if (_env.IsDevelopment())
            {
                modelBuilder.Model.GetEntityTypes()
                    .SelectMany(e => e.GetProperties())
                    .Where(p => p.ClrType == typeof(string))
                    .ToList()
                    .ForEach(property => property.IsNullable = true);
            }
        }
    }
}