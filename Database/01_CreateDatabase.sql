-- =============================================
-- Create Database and Use it
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WebAPIDb')
BEGIN
    CREATE DATABASE WebAPIDb;
END
GO

USE WebAPIDb;
GO

PRINT 'Database WebAPIDb created/selected successfully.';
GO

