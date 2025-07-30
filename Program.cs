using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using TiendaApp.Data;
using TiendaApp.Data.SeedData;
using TiendaApp.Models;

// Punto de entrada asíncrono
async Task Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Configurar logging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    // Agregar servicios al contenedor.
    builder.Services.AddControllersWithViews();

    // Configurar el contexto de la base de datos
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("No se ha configurado la cadena de conexión 'DefaultConnection'.");

    builder.Services.AddDbContext<TiendaApp.Data.TiendaContext>((serviceProvider, options) =>
    {
        var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        });
        
        // Deshabilitar la verificación de migraciones pendientes en desarrollo
        if (env.IsDevelopment())
        {
            options.ConfigureWarnings(warnings => 
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    });

    // Configurar autenticación por cookies
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "TiendaApp.Auth";
            options.LoginPath = "/Cuenta/IniciarSesion";
            options.LogoutPath = "/Cuenta/CerrarSesion";
            options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            
            // Configuración adicional para depuración
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = context =>
                {
                    Console.WriteLine("=== Validando principal de autenticación ===");
                    Console.WriteLine($"Usuario autenticado: {context.Principal?.Identity?.IsAuthenticated}");
                    Console.WriteLine($"Nombre de usuario: {context.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                }
            };
        });

    // Configurar la autorización
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequiereAdmin", policy => 
            policy.RequireRole("Administrador"));
        
        options.AddPolicy("RequiereCliente", policy => 
            policy.RequireRole("Cliente"));
    });

    // Configurar el servicio de sesión
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    var app = builder.Build();

    // Configurar el pipeline de solicitudes HTTP.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    // Inicializar la base de datos
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<TiendaContext>();
            
            // Asegurarse de que la base de datos existe y aplicar migraciones
            logger.LogInformation("Aplicando migraciones...");
            await context.Database.MigrateAsync();
            
            // Inicializar datos
            logger.LogInformation("Inicializando datos...");
            await DbInitializer.Initialize(services);
            
            logger.LogInformation("Base de datos inicializada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
            throw; // Relanzar para detener la aplicación si hay un error crítico
        }
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    
    // Asegurarse de que el middleware de autenticación y autorización esté en el orden correcto
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Usar sesión después de autenticación
    app.UseSession();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.Run();
}

// Llamar al punto de entrada asíncrono
await Main(args);
