-- =============================================
-- Seed Initial Data
-- =============================================
USE WebAPIDb;
GO

-- =============================================
-- Seed Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM Categories WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Categories ON;
    
    INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt)
    VALUES 
        (1, 'Electronics', 'Electronic devices and gadgets', 1, GETUTCDATE()),
        (2, 'Clothing', 'Fashion and apparel', 1, GETUTCDATE()),
        (3, 'Books', 'Books and literature', 1, GETUTCDATE());
    
    SET IDENTITY_INSERT Categories OFF;
    
    PRINT 'Categories seeded successfully.';
END
ELSE
BEGIN
    PRINT 'Categories already seeded.';
END
GO

-- =============================================
-- Seed Users (with BCrypt hashed passwords)
-- Note: Password hashes are for demo purposes
-- admin123 -> $2a$11$xxx (you should generate real hashes)
-- user123 -> $2a$11$xxx (you should generate real hashes)
-- =============================================
IF NOT EXISTS (SELECT * FROM Users WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Users ON;
    
    INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, IsActive, CreatedAt)
    VALUES 
        (1, 'admin', 'admin@example.com', '$2a$11$N3z.I8QxQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ', 'Admin', 'User', 1, GETUTCDATE()),
        (2, 'user1', 'user@example.com', '$2a$11$N3z.I8QxQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ5vQ', 'John', 'Doe', 1, GETUTCDATE());
    
    SET IDENTITY_INSERT Users OFF;
    
    PRINT 'Users seeded successfully.';
    PRINT 'NOTE: Password hashes are placeholders. Run application to generate proper BCrypt hashes.';
END
ELSE
BEGIN
    PRINT 'Users already seeded.';
END
GO

-- =============================================
-- Seed Products
-- =============================================
IF NOT EXISTS (SELECT * FROM Products WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Products ON;
    
    INSERT INTO Products (Id, Name, Description, Price, SKU, StockQuantity, CategoryId, IsActive, CreatedAt)
    VALUES 
        (1, 'Laptop', 'High-performance laptop', 999.99, 'LAP001', 10, 1, 1, GETUTCDATE()),
        (2, 'T-Shirt', 'Cotton t-shirt', 19.99, 'TSH001', 50, 2, 1, GETUTCDATE()),
        (3, 'Programming Book', 'Learn C# programming', 49.99, 'BK001', 25, 3, 1, GETUTCDATE());
    
    SET IDENTITY_INSERT Products OFF;
    
    PRINT 'Products seeded successfully.';
END
ELSE
BEGIN
    PRINT 'Products already seeded.';
END
GO

PRINT 'All seed data inserted successfully!';
GO

