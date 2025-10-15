-- =============================================
-- MASTER SETUP SCRIPT
-- Run this script to create database, tables and seed data
-- =============================================

PRINT '========================================';
PRINT 'Starting Database Setup';
PRINT '========================================';
PRINT '';

-- Step 1: Create Database
PRINT 'Step 1: Creating Database...';
:r 01_CreateDatabase.sql

PRINT '';
PRINT '========================================';

-- Step 2: Create Tables
PRINT 'Step 2: Creating Tables...';
:r 02_CreateTables.sql

PRINT '';
PRINT '========================================';

-- Step 3: Seed Data
PRINT 'Step 3: Seeding Initial Data...';
:r 03_SeedData.sql

PRINT '';
PRINT '========================================';
PRINT 'Database Setup Completed Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Database Name: WebAPIDb';
PRINT 'Tables Created: Users, Categories, Products, ExceptionLogs, AuditLogs';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Update connection string in appsettings.json';
PRINT '2. Run the application';
PRINT '';
GO

