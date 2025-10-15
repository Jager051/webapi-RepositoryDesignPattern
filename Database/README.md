# Database Setup Guide

Bu klasör, MSSQL veritabanı kurulumu için gerekli SQL scriptlerini içerir.

## 📋 İçindekiler

1. **00_MASTER_SETUP.sql** - Ana kurulum scripti (tüm scriptleri çalıştırır)
2. **01_CreateDatabase.sql** - Veritabanı oluşturma
3. **02_CreateTables.sql** - Tablo oluşturma
4. **03_SeedData.sql** - Başlangıç verilerini ekleme
5. **DATABASE_SCHEMA.md** - Detaylı veritabanı dokümantasyonu

---

## 🚀 Hızlı Başlangıç

### Yöntem 1: Tek Script ile Kurulum (Önerilen)

1. SQL Server Management Studio (SSMS) açın
2. SQL Server'a bağlanın
3. `00_MASTER_SETUP.sql` dosyasını açın
4. **SQLCMD Mode**'u aktifleştirin:
   - SSMS menüsünden: **Query** → **SQLCMD Mode**
5. F5 ile çalıştırın

### Yöntem 2: Adım Adım Kurulum

Her scripti sırayla çalıştırın:

```sql
-- 1. Database oluştur
:r 01_CreateDatabase.sql

-- 2. Tabloları oluştur
:r 02_CreateTables.sql

-- 3. Başlangıç verilerini ekle
:r 03_SeedData.sql
```

### Yöntem 3: Manuel Kurulum

1. `01_CreateDatabase.sql` - Database oluştur
2. `02_CreateTables.sql` - Tabloları oluştur
3. `03_SeedData.sql` - Seed data ekle

---

## 📊 Oluşturulan Tablolar

| Tablo | Açıklama | Kayıt Sayısı (Seed) |
|-------|----------|---------------------|
| **Users** | Sistem kullanıcıları | 2 |
| **Categories** | Ürün kategorileri | 3 |
| **Products** | Ürün bilgileri | 3 |
| **ExceptionLogs** | Hata logları | 0 |
| **AuditLogs** | Audit/Activity logları | 0 |

---

## ⚙️ Connection String Ayarı

### 1. appsettings.json Güncelleme

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=WebAPIDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### Yaygın Connection String Örnekleri:

**Windows Authentication:**
```
Server=localhost;Database=WebAPIDb;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server Authentication:**
```
Server=localhost;Database=WebAPIDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;
```

**Docker MSSQL:**
```
Server=localhost,1433;Database=WebAPIDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

**Azure SQL:**
```
Server=tcp:yourserver.database.windows.net,1433;Database=WebAPIDb;User Id=yourusername@yourserver;Password=yourpassword;Encrypt=True;
```

---

## 🔐 Seed Data Bilgileri

### Kullanıcılar:

| Username | Email | Password | Role |
|----------|-------|----------|------|
| admin | admin@example.com | admin123 | Admin |
| user1 | user@example.com | user123 | User |

> ⚠️ **ÖNEMLİ:** Seed script'teki password hash'leri placeholder'dır. İlk çalıştırmada uygulama gerçek BCrypt hash'leri oluşturacaktır.

### Kategoriler:
- Electronics
- Clothing
- Books

### Ürünler:
- Laptop (SKU: LAP001)
- T-Shirt (SKU: TSH001)
- Programming Book (SKU: BK001)

---

## 🛠️ Program.cs Konfigürasyonu

Uygulamanızda DbContext'i kaydedin:

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// ... diğer servisler
```

---

## 📝 Entity Framework Migration Kullanımı

Eğer scriptler yerine EF Migration kullanmak isterseniz:

### 1. Migration Oluşturma:

```bash
dotnet ef migrations add InitialCreate --project WebAPI.Infrastructure --startup-project WebAPI.API
```

### 2. Database Güncelleme:

```bash
dotnet ef database update --project WebAPI.Infrastructure --startup-project WebAPI.API
```

### 3. Migration Geri Alma:

```bash
dotnet ef database update PreviousMigrationName --project WebAPI.Infrastructure --startup-project WebAPI.API
```

### 4. Migration Silme:

```bash
dotnet ef migrations remove --project WebAPI.Infrastructure --startup-project WebAPI.API
```

---

## ✅ Kurulum Doğrulama

### SQL Server'da:

```sql
USE WebAPIDb;

-- Tabloları listele
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Kayıt sayılarını kontrol et
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'ExceptionLogs', COUNT(*) FROM ExceptionLogs
UNION ALL
SELECT 'AuditLogs', COUNT(*) FROM AuditLogs;

-- Foreign key ilişkilerini kontrol et
SELECT 
    fk.name AS ForeignKey,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
ORDER BY tp.name;
```

### Uygulamada:

```bash
# Uygulamayı çalıştır
dotnet run --project WebAPI.API

# Veya
cd WebAPI.API
dotnet watch run
```

---

## 🔍 Sorun Giderme

### Hata: "Cannot open database 'WebAPIDb'"

**Çözüm:** Database henüz oluşmamış. `01_CreateDatabase.sql` scriptini çalıştırın.

### Hata: "Invalid object name 'Users'"

**Çözüm:** Tablolar oluşmamış. `02_CreateTables.sql` scriptini çalıştırın.

### Hata: "Login failed for user"

**Çözüm:** 
1. Connection string'deki kullanıcı adı/şifre doğruluğunu kontrol edin
2. SQL Server'da kullanıcının yetkilerini kontrol edin
3. Windows Authentication kullanmayı deneyin

### Hata: "A network-related or instance-specific error"

**Çözüm:**
1. SQL Server servisinin çalıştığını kontrol edin
2. TCP/IP protokolünün etkin olduğunu kontrol edin
3. Firewall ayarlarını kontrol edin
4. Server adını ve port'u kontrol edin

---

## 📚 Daha Fazla Bilgi

Detaylı veritabanı şeması ve dokümantasyon için:
- [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) - Tablo yapıları, ilişkiler ve örnekler

---

## 🗑️ Database Silme

Eğer database'i tamamen silmek isterseniz:

```sql
USE master;
GO

ALTER DATABASE WebAPIDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE WebAPIDb;
GO
```

---

## 📞 Destek

Sorunlarla karşılaşırsanız:
1. Hata mesajını tam olarak okuyun
2. Connection string'i kontrol edin
3. SQL Server servislerinin çalıştığını kontrol edin
4. Log dosyalarını inceleyin

