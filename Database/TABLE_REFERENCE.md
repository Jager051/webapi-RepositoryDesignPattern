# Hızlı Tablo Referansı

## 📋 Tüm Tablolar - Hızlı Bakış

### 1️⃣ Users Tablosu
```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7)
);
```

### 2️⃣ Categories Tablosu
```sql
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7)
);
```

### 3️⃣ Products Tablosu
```sql
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    SKU NVARCHAR(50) UNIQUE,
    StockQuantity INT NOT NULL DEFAULT 0,
    CategoryId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7),
    
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories(Id)
);
```

### 4️⃣ ExceptionLogs Tablosu
```sql
CREATE TABLE ExceptionLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    Message NVARCHAR(2000) NOT NULL,
    StackTrace NVARCHAR(MAX),
    ExceptionType NVARCHAR(200),
    Source NVARCHAR(500),
    InnerException NVARCHAR(2000),
    StatusCode INT,
    RequestPath NVARCHAR(500),
    HttpMethod NVARCHAR(10),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    Severity NVARCHAR(20) NOT NULL DEFAULT 'Error',
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7),
    
    CONSTRAINT FK_ExceptionLogs_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE SET NULL
);

-- Indexes
CREATE INDEX IX_ExceptionLogs_UserId ON ExceptionLogs(UserId);
CREATE INDEX IX_ExceptionLogs_CreatedAt ON ExceptionLogs(CreatedAt);
CREATE INDEX IX_ExceptionLogs_Severity ON ExceptionLogs(Severity);
```

### 5️⃣ AuditLogs Tablosu
```sql
CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    Action NVARCHAR(100) NOT NULL,
    EntityName NVARCHAR(100),
    EntityId INT,
    Description NVARCHAR(1000),
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    RequestPath NVARCHAR(500),
    HttpMethod NVARCHAR(10),
    StatusCode INT,
    Duration BIGINT,
    LogLevel NVARCHAR(20) NOT NULL DEFAULT 'Information',
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7),
    
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE SET NULL
);

-- Indexes
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_EntityName ON AuditLogs(EntityName);
CREATE INDEX IX_AuditLogs_EntityName_EntityId ON AuditLogs(EntityName, EntityId);
```

---

## 🔗 İlişki Diyagramı

```
┌──────────────┐
│    Users     │
│  (Id, ...)   │
└──────┬───────┘
       │
       │ 1:N
       │
       ├─────────────┐
       │             │
       ▼             ▼
┌──────────────┐  ┌──────────────┐
│ExceptionLogs │  │  AuditLogs   │
│(UserId FK)   │  │(UserId FK)   │
└──────────────┘  └──────────────┘


┌──────────────┐
│ Categories   │
│  (Id, ...)   │
└──────┬───────┘
       │
       │ 1:N
       │
       ▼
┌──────────────┐
│   Products   │
│(CategoryId FK)│
└──────────────┘
```

---

## 🎯 Kullanım Örnekleri

### Yeni Exception Log Ekleme
```sql
INSERT INTO ExceptionLogs (
    UserId, Message, ExceptionType, Severity, 
    RequestPath, HttpMethod, IpAddress
)
VALUES (
    1, 
    'Object reference not set to an instance of an object',
    'NullReferenceException',
    'Error',
    '/api/products/123',
    'GET',
    '192.168.1.100'
);
```

### Yeni Audit Log Ekleme
```sql
INSERT INTO AuditLogs (
    UserId, Action, EntityName, EntityId,
    Description, NewValues, LogLevel
)
VALUES (
    1,
    'Create',
    'Product',
    123,
    'New product created',
    '{"Name":"Laptop","Price":999.99}',
    'Information'
);
```

### Son 24 Saatteki Hatalar
```sql
SELECT 
    el.Severity,
    COUNT(*) as ErrorCount,
    u.Username
FROM ExceptionLogs el
LEFT JOIN Users u ON el.UserId = u.Id
WHERE el.CreatedAt > DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY el.Severity, u.Username
ORDER BY ErrorCount DESC;
```

### Kullanıcı Aktivite Özeti
```sql
SELECT 
    u.Username,
    al.Action,
    COUNT(*) as ActionCount
FROM AuditLogs al
LEFT JOIN Users u ON al.UserId = u.Id
WHERE al.CreatedAt > DATEADD(DAY, -7, GETUTCDATE())
GROUP BY u.Username, al.Action
ORDER BY ActionCount DESC;
```

### Entity Değişiklik Geçmişi
```sql
SELECT 
    al.Action,
    al.Description,
    al.OldValues,
    al.NewValues,
    al.CreatedAt,
    u.Username
FROM AuditLogs al
LEFT JOIN Users u ON al.UserId = u.Id
WHERE al.EntityName = 'Product'
  AND al.EntityId = 1
ORDER BY al.CreatedAt DESC;
```

---

## 🔧 Bakım Script'leri

### Eski Logları Temizleme
```sql
-- 90 günden eski exception logları sil
DELETE FROM ExceptionLogs 
WHERE CreatedAt < DATEADD(DAY, -90, GETUTCDATE());

-- 1 yıldan eski audit logları sil
DELETE FROM AuditLogs 
WHERE CreatedAt < DATEADD(YEAR, -1, GETUTCDATE());
```

### Tablo Boyutlarını Kontrol
```sql
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB, 
    SUM(a.used_pages) * 8 AS UsedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.NAME IN ('Users', 'Categories', 'Products', 'ExceptionLogs', 'AuditLogs')
GROUP BY t.Name, p.Rows
ORDER BY UsedSpaceKB DESC;
```

### İndeks Fragmentasyonu Kontrol
```sql
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(
    DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i 
    ON ips.object_id = i.object_id 
    AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;
```

---

## 📊 Entity C# Örnekleri

### ExceptionLog Entity Kullanımı
```csharp
var exceptionLog = new ExceptionLog
{
    UserId = currentUserId,
    Message = exception.Message,
    StackTrace = exception.StackTrace,
    ExceptionType = exception.GetType().Name,
    Severity = "Error",
    RequestPath = httpContext.Request.Path,
    HttpMethod = httpContext.Request.Method,
    IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
    UserAgent = httpContext.Request.Headers["User-Agent"]
};

await _context.ExceptionLogs.AddAsync(exceptionLog);
await _context.SaveChangesAsync();
```

### AuditLog Entity Kullanımı
```csharp
var auditLog = new AuditLog
{
    UserId = currentUserId,
    Action = "Update",
    EntityName = "Product",
    EntityId = product.Id,
    Description = $"Product {product.Name} updated",
    OldValues = JsonSerializer.Serialize(oldProduct),
    NewValues = JsonSerializer.Serialize(product),
    LogLevel = "Information",
    IpAddress = GetClientIpAddress(),
    RequestPath = httpContext.Request.Path
};

await _context.AuditLogs.AddAsync(auditLog);
await _context.SaveChangesAsync();
```

---

## 🎨 C# Entity Definitions

### ExceptionLog.cs
```csharp
public class ExceptionLog : BaseEntity
{
    public int? UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ExceptionType { get; set; }
    public string? Source { get; set; }
    public string? InnerException { get; set; }
    public int? StatusCode { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Severity { get; set; } = "Error";
    
    public virtual User? User { get; set; }
}
```

### AuditLog.cs
```csharp
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public int? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public int? StatusCode { get; set; }
    public long? Duration { get; set; }
    public string LogLevel { get; set; } = "Information";
    
    public virtual User? User { get; set; }
}
```

