using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using TiendaApp.Models;

namespace TiendaApp.Data.SeedData
{
    public static class UserSeedData
    {
        public static async Task SeedRolesAsync(TiendaContext context)
        {
            // Los roles se insertan automáticamente en OnModelCreating
            // Solo necesitamos asegurarnos de que la base de datos esté creada
            await context.Database.EnsureCreatedAsync();
        }

        public static async Task SeedUsersAsync(TiendaContext context)
        {
            if (!await context.Usuarios.AnyAsync())
            {
                // Asegurarse de que la base de datos y los roles existen
                await SeedRolesAsync(context);
                
                // Obtener los roles (ya deberían existir por OnModelCreating)
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador");
                var clientRole = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");

                if (adminRole == null || clientRole == null)
                {
                    throw new InvalidOperationException("Los roles no están configurados correctamente.");
                }

                // Crear la lista de usuarios
                var users = new List<Usuario>
                {
                    // Usuario administrador
                    new Usuario
                    {
                        Email = "admin@tienda.com",
                        NombreUsuario = "admin",
                        Nombre = "Administrador",
                        Apellidos = "del Sistema",
                        Telefono = "612345678",
                        RolId = adminRole.Id,
                        EstaActivo = true,
                        Direccion = "Calle Principal 123, 1ºA",
                        Ciudad = "Madrid",
                        Provincia = "Madrid",
                        CodigoPostal = "28001",
                        ProfileImageUrl = "/images/usuarios/M11.png",
                        FechaRegistro = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        Password = "admin123"
                    },
                    // Cliente 1
                    new Usuario
                    {
                        Email = "cliente1@tienda.com",
                        NombreUsuario = "cliente1",
                        Nombre = "Juan",
                        Apellidos = "Pérez García",
                        Telefono = "693847261",
                        RolId = clientRole.Id,
                        EstaActivo = true,
                        Direccion = "Avenida Diagonal 456, 2ºB",
                        Ciudad = "Barcelona",
                        Provincia = "Barcelona",
                        CodigoPostal = "08001",
                        ProfileImageUrl = "/images/usuarios/H1.jpg",
                        FechaRegistro = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        Password = "cliente123"
                    },
                    // Cliente 2
                    new Usuario
                    {
                        Email = "cliente2@tienda.com",
                        NombreUsuario = "cliente2",
                        Nombre = "María",
                        Apellidos = "González López",
                        Telefono = "678912345",
                        RolId = clientRole.Id,
                        EstaActivo = true,
                        Direccion = "Calle Colón 789, 3ºC",
                        Ciudad = "Valencia",
                        Provincia = "Valencia",
                        CodigoPostal = "46001",
                        ProfileImageUrl = "/images/usuarios/M1.jpg",
                        FechaRegistro = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        Password = "cliente123"
                    },
                    // Cliente 3
                    new Usuario
                    {
                        Email = "cliente3@tienda.com",
                        NombreUsuario = "cliente3",
                        Nombre = "Carlos",
                        Apellidos = "Martínez Ruiz",
                        Telefono = "655443322",
                        RolId = clientRole.Id,
                        EstaActivo = true,
                        Direccion = "Plaza Mayor 10, 4ºD",
                        Ciudad = "Sevilla",
                        Provincia = "Sevilla",
                        CodigoPostal = "41001",
                        ProfileImageUrl = "/images/usuarios/H2.jpg",
                        FechaRegistro = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        Password = "cliente123"
                    },
                    // Cliente 4
                    new Usuario
                    {
                        Email = "cliente4@tienda.com",
                        NombreUsuario = "cliente4",
                        Nombre = "Laura",
                        Apellidos = "Fernández Sánchez",
                        Telefono = "677889900",
                        RolId = clientRole.Id,
                        EstaActivo = true,
                        Direccion = "Calle Nueva 5, 1ºA",
                        Ciudad = "Málaga",
                        Provincia = "Málaga",
                        CodigoPostal = "29001",
                        ProfileImageUrl = "/images/usuarios/M2.jpg",
                        FechaRegistro = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        Password = "cliente123"
                    },
                    // Cliente 5
                    new Usuario
                    {
                        Email = "cliente5@tienda.com",
                        NombreUsuario = "cliente5",
                        Nombre = "David",
                        Apellidos = "López Gómez",
                        Telefono = "699887766",
                        RolId = clientRole.Id,
                        EstaActivo = true,
                        Direccion = "Avenida Libertad 15, 3ºC",
                        Ciudad = "Bilbao",
                        Provincia = "Vizcaya",
                        CodigoPostal = "48001",
                        ProfileImageUrl = "/images/usuarios/H3.jpg",
                        FechaRegistro = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        Password = "cliente123"
                    }
                };

                // Insertar los usuarios en la base de datos
                try
                {
                    await context.Usuarios.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Usuarios de prueba creados exitosamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar usuarios: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Error interno: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
        }
    }
}
