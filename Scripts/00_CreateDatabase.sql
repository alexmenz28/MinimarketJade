-- ============================================================
-- Minimarket Jade - Crear base de datos (SQL Server)
-- Ejecutar en la instancia donde se desea la BD.
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'MinimarketJade')
BEGIN
    CREATE DATABASE MinimarketJade
        COLLATE Latin1_General_CI_AI;
END
GO

USE MinimarketJade;
GO

-- A continuación ejecutar: 01_CreateTables.sql
