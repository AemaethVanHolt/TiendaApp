-- =============================================
-- Script para crear la base de datos TiendaApp
-- Incluye: 
--   - Creación de la base de datos
--   - Creación de tablas
--   - Relaciones
--   - Datos iniciales
-- =============================================

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TiendaApp')
BEGIN
    CREATE DATABASE TiendaApp;
    PRINT 'Base de datos TiendaApp creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos TiendaApp ya existe.';
END
GO

-- Usar la base de datos
USE TiendaApp;
GO

-- Crear tabla Roles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(50) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_Roles_Nombre UNIQUE (Nombre)
    );
    PRINT 'Tabla Roles creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Roles ya existe.';
END
GO

-- Crear tabla Usuarios
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT PRIMARY KEY IDENTITY(1,1),
        NombreUsuario NVARCHAR(50) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        Password NVARCHAR(256) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Apellidos NVARCHAR(150) NULL,
        Telefono NVARCHAR(20) NULL,
        Direccion NVARCHAR(200) NULL,
        Ciudad NVARCHAR(100) NULL,
        Provincia NVARCHAR(100) NULL,
        CodigoPostal NVARCHAR(10) NULL,
        ProfileImageUrl NVARCHAR(500) NULL,
        EstaActivo BIT NOT NULL DEFAULT 1,
        FechaRegistro DATETIME2 NOT NULL DEFAULT GETDATE(),
        UltimoAcceso DATETIME2 NULL,
        RolId INT NOT NULL,
        CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES Roles(Id),
        CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
        CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario)
    );
    PRINT 'Tabla Usuarios creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Usuarios ya existe.';
END
GO

-- Crear tabla Productos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Productos')
BEGIN
    CREATE TABLE Productos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Precio DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL DEFAULT 0,
        ImagenUrl NVARCHAR(500) NULL,
        Tipo NVARCHAR(50) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME2 NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Tabla Productos creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Productos ya existe.';
END
GO

-- Crear tabla Ventas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ventas')
BEGIN
    CREATE TABLE Ventas (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UsuarioId INT NOT NULL,
        FechaVenta DATETIME2 NOT NULL DEFAULT GETDATE(),
        Total DECIMAL(18,2) NOT NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
        MetodoPago NVARCHAR(50) NULL,
        Cliente NVARCHAR(200) NULL,
        Comentarios NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Ventas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
    );
    PRINT 'Tabla Ventas creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Ventas ya existe.';
END
GO

-- Crear tabla DetallesVenta
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetallesVenta')
BEGIN
    CREATE TABLE DetallesVenta (
        Id INT PRIMARY KEY IDENTITY(1,1),
        VentaId INT NOT NULL,
        ProductoId INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        Total DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_DetallesVenta_Ventas FOREIGN KEY (VentaId) REFERENCES Ventas(Id) ON DELETE CASCADE,
        CONSTRAINT FK_DetallesVenta_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
    );
    PRINT 'Tabla DetallesVenta creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla DetallesVenta ya existe.';
END
GO

-- Crear tabla Carritos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Carritos')
BEGIN
    CREATE TABLE Carritos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UsuarioId INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Carritos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
    );
    
    CREATE TABLE CarritoItems (
        Id INT PRIMARY KEY IDENTITY(1,1),
        CarritoId INT NOT NULL,
        ProductoId INT NOT NULL,
        Cantidad INT NOT NULL DEFAULT 1,
        FechaAgregado DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_CarritoItems_Carritos FOREIGN KEY (CarritoId) REFERENCES Carritos(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CarritoItems_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
    );
    
    PRINT 'Tablas Carritos y CarritoItems creadas exitosamente.';
END
ELSE
BEGIN
    PRINT 'Las tablas de carrito ya existen.';
END
GO

-- Insertar datos iniciales en Roles
IF NOT EXISTS (SELECT 1 FROM Roles)
BEGIN
    INSERT INTO Roles (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES 
        ('Administrador', 'Administrador del sistema con todos los permisos', 1, GETDATE()),
        ('Cliente', 'Usuario estándar con permisos limitados', 1, GETDATE());
    
    PRINT 'Datos de Roles insertados correctamente.';
END
ELSE
BEGIN
    PRINT 'Ya existen datos en la tabla Roles.';
END
GO

-- Insertar usuario administrador (la contraseña es 'admin123' hasheada con SHA256)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@tienda.com')
BEGIN
    DECLARE @adminRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Administrador');
    
    INSERT INTO Usuarios (
        NombreUsuario, 
        Email, 
        Password, 
        Nombre, 
        Apellidos, 
        Telefono, 
        Direccion, 
        Ciudad, 
        Provincia, 
        CodigoPostal, 
        ProfileImageUrl, 
        EstaActivo, 
        FechaRegistro, 
        UltimoAcceso, 
        RolId
    ) VALUES (
        'admin', 
        'admin@tienda.com', 
        '9C9064C59F1FFA2E174EE754D2979BE80DD30DB552EC03E7E327E9B1A4BD594E', -- admin1234
        'Administrador', 
        'del Sistema', 
        '600000000', 
        'Calle Principal 1', 
        'Madrid', 
        'Madrid', 
        '28001', 
        '/images/usuarios/admin.png', 
        1, 
        GETDATE(), 
        GETDATE(), 
        @adminRolId
    );
    
    PRINT 'Usuario administrador creado exitosamente.';
    PRINT 'Email: admin@tienda.com';
    PRINT 'Contraseña: admin1234';
END
ELSE
BEGIN
    PRINT 'El usuario administrador ya existe.';
END
GO

-- Insertar usuario de prueba (cliente)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'cliente@tienda.com')
BEGIN
    DECLARE @clienteRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Cliente');
    
    INSERT INTO Usuarios (
        NombreUsuario, 
        Email, 
        Password, 
        Nombre, 
        Apellidos, 
        Telefono, 
        Direccion, 
        Ciudad, 
        Provincia, 
        CodigoPostal, 
        ProfileImageUrl, 
        EstaActivo, 
        FechaRegistro, 
        UltimoAcceso, 
        RolId
    ) VALUES (
        'cliente', 
        'cliente@tienda.com', 
        'A6B7E786862B2A24BEA93738D178DCA77CECFEEF3E211ACBDFC2A43A531FABE8', -- cliente123
        'Cliente', 
        'de Prueba', 
        '611111111', 
        'Calle Cliente 2', 
        'Barcelona', 
        'Barcelona', 
        '08001', 
        '/images/usuarios/cliente.png', 
        1, 
        GETDATE(), 
        GETDATE(), 
        @clienteRolId
    );
    
    PRINT 'Usuario de prueba (cliente) creado exitosamente.';
    PRINT 'Email: cliente@tienda.com';
    PRINT 'Contraseña: cliente123';
END
ELSE
BEGIN
    PRINT 'El usuario de prueba (cliente) ya existe.';
END
GO

-- Insertar productos de ejemplo
IF NOT EXISTS (SELECT 1 FROM Productos)
BEGIN
    -- Teléfonos
    INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, ImagenUrl, Tipo, Activo, FechaCreacion, FechaModificacion)
    VALUES 
        ('Smartphone X1 Pro', 'Último modelo con cámara de 108MP y pantalla AMOLED de 6.7"', 999.99, 30, '/images/productos/phone1.jpg', 'Teléfonos', 1, GETDATE(), GETDATE()),
        ('Smartphone Gama Media', 'Excelente relación calidad-precio con cámara triple de 64MP', 299.99, 75, '/images/productos/phone2.jpg', 'Teléfonos', 1, GETDATE(), GETDATE()),
        ('Teléfono Básico', 'Ideal para usuarios que buscan lo esencial', 129.99, 120, '/images/productos/phone3.jpg', 'Teléfonos', 1, GETDATE(), GETDATE()),
        
        -- Portátiles
        ('Portátil Ultrabook Pro', 'Ligero y potente con procesador i7 y 16GB RAM', 1299.99, 15, '/images/productos/laptop1.jpg', 'Portátiles', 1, GETDATE(), GETDATE()),
        ('Portátil Gama Media', 'Equilibrado para trabajo y entretenimiento', 699.99, 25, '/images/productos/laptop2.jpg', 'Portátiles', 1, GETDATE(), GETDATE()),
        
        -- Tablets
        ('Tablet Pro 12.9"', 'Pantalla Retina, potente y versátil', 899.99, 20, '/images/productos/tablet1.jpg', 'Tablets', 1, GETDATE(), GETDATE()),
        ('Tablet 10"', 'Ideal para lectura y navegación', 199.99, 40, '/images/productos/tablet2.jpg', 'Tablets', 1, GETDATE(), GETDATE()),
        
        -- Auriculares
        ('Auriculares Inalámbricos Pro', 'Cancelación de ruido y sonido envolvente', 249.99, 50, '/images/productos/headphones1.jpg', 'Audio', 1, GETDATE(), GETDATE()),
        ('Auriculares Deportivos', 'Resistentes al agua y sudor', 79.99, 80, '/images/productos/headphones2.jpg', 'Audio', 1, GETDATE(), GETDATE()),
        
        -- Smartwatches
        ('Smartwatch Pro', 'Seguimiento de salud y notificaciones', 299.99, 35, '/images/productos/watch1.jpg', 'Wearables', 1, GETDATE(), GETDATE()),
        ('Smartwatch Básico', 'Monitoreo de actividad y notificaciones', 129.99, 60, '/images/productos/watch2.jpg', 'Wearables', 1, GETDATE(), GETDATE());
    
    PRINT 'Productos de ejemplo insertados correctamente.';
END
ELSE
BEGIN
    PRINT 'Ya existen productos en la base de datos.';
END
GO

-- Crear algunas ventas de ejemplo
IF NOT EXISTS (SELECT 1 FROM Ventas)
BEGIN
    DECLARE @clienteId INT = (SELECT Id FROM Usuarios WHERE Email = 'cliente@tienda.com');
    DECLARE @adminId INT = (SELECT Id FROM Usuarios WHERE Email = 'admin@tienda.com');
    
    -- Insertar venta 1
    DECLARE @ventaId1 INT;
    
    INSERT INTO Ventas (UsuarioId, FechaVenta, Total, Estado, MetodoPago, Cliente, Comentarios)
    VALUES (@clienteId, DATEADD(DAY, -5, GETDATE()), 1299.98, 'Completada', 'Tarjeta', 'Cliente de Prueba', 'Entrega estándar');
    
    SET @ventaId1 = SCOPE_IDENTITY();
    
    -- Detalles de la venta 1
    INSERT INTO DetallesVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Total)
    VALUES 
        (@ventaId1, 1, 1, 999.99, 999.99),
        (@ventaId1, 5, 1, 299.99, 299.99);
    
    -- Insertar venta 2
    DECLARE @ventaId2 INT;
    
    INSERT INTO Ventas (UsuarioId, FechaVenta, Total, Estado, MetodoPago, Cliente, Comentarios)
    VALUES (@clienteId, DATEADD(DAY, -2, GETDATE()), 449.98, 'En proceso', 'PayPal', 'Otro Cliente', 'Urgente');
    
    SET @ventaId2 = SCOPE_IDENTITY();
    
    -- Detalles de la venta 2
    INSERT INTO DetallesVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Total)
    VALUES 
        (@ventaId2, 3, 2, 129.99, 259.98),
        (@ventaId2, 7, 1, 199.99, 199.99);
    
    PRINT 'Ventas de ejemplo creadas correctamente.';
END
ELSE
BEGIN
    PRINT 'Ya existen ventas en la base de datos.';
END
GO

-- Crear un carrito de ejemplo
IF NOT EXISTS (SELECT 1 FROM Carritos)
BEGIN
    DECLARE @usuarioCarritoId INT = (SELECT Id FROM Usuarios WHERE Email = 'cliente@tienda.com');
    DECLARE @carritoId INT;
    
    INSERT INTO Carritos (UsuarioId, FechaCreacion, FechaActualizacion)
    VALUES (@usuarioCarritoId, GETDATE(), GETDATE());
    
    SET @carritoId = SCOPE_IDENTITY();
    
    -- Agregar items al carrito
    INSERT INTO CarritoItems (CarritoId, ProductoId, Cantidad, FechaAgregado)
    VALUES 
        (@carritoId, 2, 1, GETDATE()),
        (@carritoId, 4, 2, GETDATE()),
        (@carritoId, 6, 1, GETDATE());
    
    PRINT 'Carrito de ejemplo creado correctamente.';
END
ELSE
BEGIN
    PRINT 'Ya existen carritos en la base de datos.';
END
GO

-- Crear una vista para el reporte de ventas
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'VistaReporteVentas')
BEGIN
    EXEC('CREATE VIEW VistaReporteVentas AS
    SELECT 
        v.Id AS VentaId,
        v.FechaVenta,
        v.Total,
        v.Estado,
        v.MetodoPago,
        v.Cliente,
        u.Email AS EmailCliente,
        u.Telefono AS TelefonoCliente,
        (
            SELECT STRING_AGG(CONCAT(p.Nombre, '' (x'', dv.Cantidad, '')''), '', '') 
            FROM DetallesVenta dv 
            JOIN Productos p ON dv.ProductoId = p.Id 
            WHERE dv.VentaId = v.Id
        ) AS ProductosComprados
    FROM Ventas v
    JOIN Usuarios u ON v.UsuarioId = u.Id');
    
    PRINT 'Vista de reporte de ventas creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La vista de reporte de ventas ya existe.';
END
GO

-- Crear un procedimiento almacenado para obtener las ventas por rango de fechas
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'ObtenerVentasPorFecha')
BEGIN
    EXEC('CREATE PROCEDURE ObtenerVentasPorFecha
        @FechaInicio DATETIME2,
        @FechaFin DATETIME2
    AS
    BEGIN
        SELECT 
            v.Id AS VentaId,
            v.FechaVenta,
            v.Total,
            v.Estado,
            v.MetodoPago,
            v.Cliente,
            u.Email AS EmailCliente,
            (
                SELECT STRING_AGG(CONCAT(p.Nombre, '' (x'', dv.Cantidad, '')''), '', '') 
                FROM DetallesVenta dv 
                JOIN Productos p ON dv.ProductoId = p.Id 
                WHERE dv.VentaId = v.Id
            ) AS ProductosComprados
        FROM Ventas v
        JOIN Usuarios u ON v.UsuarioId = u.Id
        WHERE v.FechaVenta BETWEEN @FechaInicio AND @FechaFin
        ORDER BY v.FechaVenta DESC;
    END');
    
    PRINT 'Procedimiento almacenado ObtenerVentasPorFecha creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El procedimiento almacenado ObtenerVentasPorFecha ya existe.';
END
GO

-- Mensaje final
PRINT '=============================================';
PRINT ' Script de base de datos ejecutado exitosamente.';
PRINT ' Usuarios creados:';
PRINT ' - admin@tienda.com / admin1234 (Administrador)';
PRINT ' - cliente@tienda.com / cliente123 (Cliente)';
PRINT '=============================================';
