# TiendaApp

Aplicación de tienda desarrollada con ASP.NET Core y Entity Framework Core.

## Requisitos Previos

- .NET 9.0 SDK, lo he cambiado de .NET 8.0 a 9.0
- SQL Server 2022
- Visual Studio 2022 o Visual Studio Code

## Configuración Inicial

1. **Clonar el repositorio**
   ```bash
   git clone [la url la meto cuando pueda]
   cd TiendaApp
   ```

2. **Configurar la base de datos**
   - Asegurarse de que SQL Server esté en ejecución, que siempre se me olvida
   - Actualizar la cadena de conexión en `appsettings.json` si es necesario porque algún capullo tiene SQLEXPRESS

3. **Aplicar migraciones**
   ```bash
   dotnet ef database update --project TiendaApp.csproj
   ```

4. **Restaurar paquetes NuGet**
   ```bash
   dotnet restore
   ```

5. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

   La aplicación estará disponible en: `https://localhost:5001` y `http://localhost:5000` El de seguridad es el 5001, pero funciona bien en el 5000

## Estructura del Proyecto

- **Controllers**: Controladores de la aplicación
- **Data**: Contexto de base de datos y configuraciones para las migraciones a SQLServer
- **Models**: Modelos de datos
- **Views**: Vistas de la aplicación
- **wwwroot**: Archivos estáticos (CSS, JS, imágenes)

## Credenciales de Acceso

- **Administrador**:
  - Email: admin@tienda.com
  - Contraseña: admin123

- **Cliente**:
  - Email: cliente@tienda.com
  - Contraseña: cliente123

## Solución de Problemas

Si se encuentra algún problema al ejecutar la aplicación:

1. Verificar que SQL Server esté en ejecución, que falla
2. Asegurarse de que la cadena de conexión en `appsettings.json` sea correcta
3. Intenta limpiar y reconstruir la solución
   ```bash
   dotnet clean
   dotnet build
   ```
4. Si persisten los problemas, eliminar la carpeta `bin` y `obj` y volver a restaurar los paquetes