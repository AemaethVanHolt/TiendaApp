using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TiendaApp.Data.SeedData;
using TiendaApp.Models;

namespace TiendaApp.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            var context = services.GetRequiredService<TiendaContext>();

            try
            {
                // Asegurarse de que la base de datos existe
                logger.LogInformation("Verificando si la base de datos existe...");
                if (!await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("La base de datos no existe. Creando...");
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Base de datos creada exitosamente.");
                }

                // Aplicar migraciones pendientes
                logger.LogInformation("Aplicando migraciones pendientes...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas exitosamente.");

                // Inicializar datos de semilla
                logger.LogInformation("Inicializando datos de semilla...");
                
                // 1. Crear roles primero
                await UserSeedData.SeedRolesAsync(context);
                
                // 2. Crear usuarios
                await UserSeedData.SeedUsersAsync(context);

                // 3. Cargar productos de ejemplo usando ProductSeedData
                logger.LogInformation("\n3. Verificando productos de ejemplo...");
                if (!await context.Productos.AnyAsync())
                {
                    await ProductSeedData.SeedProductsAsync(context);
                    logger.LogInformation("   - Productos de ejemplo insertados");
                }
                else
                {
                    logger.LogInformation("   - Ya existen productos en la base de datos");
                }

                // 4. Cargar ventas de ejemplo usando SaleSeedData
                logger.LogInformation("\n4. Verificando ventas de ejemplo...");
                if (!await context.Ventas.AnyAsync())
                {
                    await SaleSeedData.SeedSalesAsync(context);
                    logger.LogInformation("   - Ventas de ejemplo insertadas");
                }
                else
                {
                    logger.LogInformation("   - Ya existen ventas en la base de datos");
                }

                logger.LogInformation("\n=== INICIALIZACIÓN COMPLETADA CON ÉXITO ===\n");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR DURANTE LA INICIALIZACIÓN");
                throw;
            }
        }
    }
}