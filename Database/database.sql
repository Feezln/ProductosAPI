-- Script para crear la base de datos y la tabla de productos
-- Ejecutar en SQL Server Management Studio o con:
--   sqlcmd -S localhost -E -i Database\database.sql

IF DB_ID('TiendaDB') IS NULL
    CREATE DATABASE TiendaDB;
GO

USE TiendaDB;
GO

IF OBJECT_ID('Productos') IS NULL
BEGIN
    CREATE TABLE Productos (
        Id          INT IDENTITY(1,1) PRIMARY KEY,   -- se genera solo: 1, 2, 3...
        Nombre      NVARCHAR(100)  NOT NULL,
        Descripcion NVARCHAR(255)  NULL,
        Precio      DECIMAL(10,2)  NOT NULL,
        Stock       INT            NOT NULL DEFAULT 0
    );

    -- Datos de ejemplo
    INSERT INTO Productos (Nombre, Descripcion, Precio, Stock) VALUES
        ('Teclado', 'Teclado mecánico USB', 799.00, 15),
        ('Mouse', 'Mouse inalámbrico', 349.50, 30),
        ('Monitor', 'Monitor 24 pulgadas Full HD', 2899.99, 8);
END
GO
