-- =============================================
-- Create Tables Script
-- WebAPI Database Tables
-- =============================================
USE WebAPIDb;
GO

-- =============================================
-- Table: Users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(50) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [FirstName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        
        CONSTRAINT UQ_Users_Email UNIQUE ([Email]),
        CONSTRAINT UQ_Users_Username UNIQUE ([Username])
    );
    
    PRINT 'Table Users created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Users already exists.';
END
GO

-- =============================================
-- Table: Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL
    );
    
    PRINT 'Table Categories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Categories already exists.';
END
GO

-- =============================================
-- Table: Products
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [SKU] NVARCHAR(50) NULL,
        [StockQuantity] INT NOT NULL DEFAULT 0,
        [CategoryId] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        
        CONSTRAINT FK_Products_Categories FOREIGN KEY ([CategoryId]) 
            REFERENCES [dbo].[Categories]([Id]),
        CONSTRAINT UQ_Products_SKU UNIQUE ([SKU])
    );
    
    CREATE INDEX IX_Products_CategoryId ON [dbo].[Products]([CategoryId]);
    
    PRINT 'Table Products created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Products already exists.';
END
GO

-- =============================================
-- Table: ExceptionLogs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExceptionLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExceptionLogs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NULL,
        [Message] NVARCHAR(2000) NOT NULL,
        [StackTrace] NVARCHAR(MAX) NULL,
        [ExceptionType] NVARCHAR(200) NULL,
        [Source] NVARCHAR(500) NULL,
        [InnerException] NVARCHAR(2000) NULL,
        [StatusCode] INT NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [HttpMethod] NVARCHAR(10) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [Severity] NVARCHAR(20) NOT NULL DEFAULT 'Error',
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        
        CONSTRAINT FK_ExceptionLogs_Users FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
    );
    
    CREATE INDEX IX_ExceptionLogs_UserId ON [dbo].[ExceptionLogs]([UserId]);
    CREATE INDEX IX_ExceptionLogs_CreatedAt ON [dbo].[ExceptionLogs]([CreatedAt]);
    CREATE INDEX IX_ExceptionLogs_Severity ON [dbo].[ExceptionLogs]([Severity]);
    
    PRINT 'Table ExceptionLogs created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ExceptionLogs already exists.';
END
GO

-- =============================================
-- Table: AuditLogs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [EntityName] NVARCHAR(100) NULL,
        [EntityId] INT NULL,
        [Description] NVARCHAR(1000) NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [HttpMethod] NVARCHAR(10) NULL,
        [StatusCode] INT NULL,
        [Duration] BIGINT NULL,
        [LogLevel] NVARCHAR(20) NOT NULL DEFAULT 'Information',
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
    );
    
    CREATE INDEX IX_AuditLogs_UserId ON [dbo].[AuditLogs]([UserId]);
    CREATE INDEX IX_AuditLogs_CreatedAt ON [dbo].[AuditLogs]([CreatedAt]);
    CREATE INDEX IX_AuditLogs_Action ON [dbo].[AuditLogs]([Action]);
    CREATE INDEX IX_AuditLogs_EntityName ON [dbo].[AuditLogs]([EntityName]);
    CREATE INDEX IX_AuditLogs_EntityName_EntityId ON [dbo].[AuditLogs]([EntityName], [EntityId]);
    
    PRINT 'Table AuditLogs created successfully.';
END
ELSE
BEGIN
    PRINT 'Table AuditLogs already exists.';
END
GO

PRINT 'All tables created successfully!';
GO

