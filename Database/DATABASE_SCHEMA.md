# Database Schema Documentation

## Database: WebAPIDb

Bu dokümantasyon, MSSQL veritabanı yapısını detaylıca açıklamaktadır.

---

## Tablolar

### 1. Users Tablosu
Sistem kullanıcılarını saklar.

| Kolon | Tip | Açıklama | Kısıtlamalar |
|-------|-----|----------|-------------|
| Id | INT IDENTITY | Primary Key | PK, AUTO_INCREMENT |
| Username | NVARCHAR(50) | Kullanıcı adı | NOT NULL, UNIQUE |
| Email | NVARCHAR(100) | E-posta adresi | NOT NULL, UNIQUE |
| PasswordHash | NVARCHAR(MAX) | BCrypt hash şifresi | NOT NULL |
| FirstName | NVARCHAR(100) | İsim | NULL |
| LastName | NVARCHAR(100) | Soyisim | NULL |
| IsActive | BIT | Aktif/Pasif durum | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2(7) | Oluşturulma tarihi | NOT NULL, DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2(7) | Güncellenme tarihi | NULL |

**İndeksler:**
- `UQ_Users_Email` - Email için unique constraint
- `UQ_Users_Username` - Username için unique constraint

**İlişkiler:**
- ExceptionLogs tablosu ile 1:N ilişki
- AuditLogs tablosu ile 1:N ilişki

---

### 2. Categories Tablosu
Ürün kategorilerini saklar.

| Kolon | Tip | Açıklama | Kısıtlamalar |
|-------|-----|----------|-------------|
| Id | INT IDENTITY | Primary Key | PK, AUTO_INCREMENT |
| Name | NVARCHAR(100) | Kategori adı | NOT NULL |
| Description | NVARCHAR(500) | Açıklama | NULL |
| IsActive | BIT | Aktif/Pasif durum | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2(7) | Oluşturulma tarihi | NOT NULL, DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2(7) | Güncellenme tarihi | NULL |

**İlişkiler:**
- Products tablosu ile 1:N ilişki

---

### 3. Products Tablosu
Ürün bilgilerini saklar.

| Kolon | Tip | Açıklama | Kısıtlamalar |
|-------|-----|----------|-------------|
| Id | INT IDENTITY | Primary Key | PK, AUTO_INCREMENT |
| Name | NVARCHAR(200) | Ürün adı | NOT NULL |
| Description | NVARCHAR(1000) | Ürün açıklaması | NULL |
| Price | DECIMAL(18,2) | Fiyat | NOT NULL |
| SKU | NVARCHAR(50) | Stok Kodu | UNIQUE |
| StockQuantity | INT | Stok miktarı | NOT NULL, DEFAULT 0 |
| CategoryId | INT | Kategori ID (FK) | NOT NULL |
| IsActive | BIT | Aktif/Pasif durum | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2(7) | Oluşturulma tarihi | NOT NULL, DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2(7) | Güncellenme tarihi | NULL |

**İndeksler:**
- `IX_Products_CategoryId` - CategoryId için index
- `UQ_Products_SKU` - SKU için unique constraint

**Foreign Keys:**
- `FK_Products_Categories` - Categories(Id) ile ilişki

---

### 4. ExceptionLogs Tablosu
Uygulama hatalarını ve exception'ları loglar.

| Kolon | Tip | Açıklama | Kısıtlamalar |
|-------|-----|----------|-------------|
| Id | INT IDENTITY | Primary Key | PK, AUTO_INCREMENT |
| UserId | INT | Hatayı alan kullanıcı (FK) | NULL |
| Message | NVARCHAR(2000) | Hata mesajı | NOT NULL |
| StackTrace | NVARCHAR(MAX) | Stack trace | NULL |
| ExceptionType | NVARCHAR(200) | Exception tipi | NULL |
| Source | NVARCHAR(500) | Hatanın kaynağı | NULL |
| InnerException | NVARCHAR(2000) | Inner exception | NULL |
| StatusCode | INT | HTTP status code | NULL |
| RequestPath | NVARCHAR(500) | İstek path'i | NULL |
| HttpMethod | NVARCHAR(10) | HTTP metodu (GET, POST, vb.) | NULL |
| IpAddress | NVARCHAR(50) | Kullanıcı IP adresi | NULL |
| UserAgent | NVARCHAR(500) | Browser/Client bilgisi | NULL |
| Severity | NVARCHAR(20) | Önem derecesi | NOT NULL, DEFAULT 'Error' |
| CreatedAt | DATETIME2(7) | Oluşturulma tarihi | NOT NULL, DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2(7) | Güncellenme tarihi | NULL |

**İndeksler:**
- `IX_ExceptionLogs_UserId` - UserId için index
- `IX_ExceptionLogs_CreatedAt` - CreatedAt için index
- `IX_ExceptionLogs_Severity` - Severity için index

**Foreign Keys:**
- `FK_ExceptionLogs_Users` - Users(Id) ile ilişki (ON DELETE SET NULL)

**Severity Değerleri:**
- `Critical` - Kritik hatalar
- `Error` - Normal hatalar (varsayılan)
- `Warning` - Uyarılar

**Kullanım Alanları:**
- Exception tracking
- Error monitoring
- Debug ve troubleshooting
- Kullanıcı bazlı hata analizi

---

### 5. AuditLogs Tablosu
Kullanıcı aksiyonlarını ve sistem olaylarını loglar.

| Kolon | Tip | Açıklama | Kısıtlamalar |
|-------|-----|----------|-------------|
| Id | INT IDENTITY | Primary Key | PK, AUTO_INCREMENT |
| UserId | INT | İşlemi yapan kullanıcı (FK) | NULL |
| Action | NVARCHAR(100) | Yapılan aksiyon | NOT NULL |
| EntityName | NVARCHAR(100) | Etkilenen entity/tablo | NULL |
| EntityId | INT | Etkilenen entity ID | NULL |
| Description | NVARCHAR(1000) | Açıklama | NULL |
| OldValues | NVARCHAR(MAX) | Eski değerler (JSON) | NULL |
| NewValues | NVARCHAR(MAX) | Yeni değerler (JSON) | NULL |
| IpAddress | NVARCHAR(50) | Kullanıcı IP adresi | NULL |
| UserAgent | NVARCHAR(500) | Browser/Client bilgisi | NULL |
| RequestPath | NVARCHAR(500) | İstek path'i | NULL |
| HttpMethod | NVARCHAR(10) | HTTP metodu | NULL |
| StatusCode | INT | HTTP status code | NULL |
| Duration | BIGINT | İşlem süresi (ms) | NULL |
| LogLevel | NVARCHAR(20) | Log seviyesi | NOT NULL, DEFAULT 'Information' |
| CreatedAt | DATETIME2(7) | Oluşturulma tarihi | NOT NULL, DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2(7) | Güncellenme tarihi | NULL |

**İndeksler:**
- `IX_AuditLogs_UserId` - UserId için index
- `IX_AuditLogs_CreatedAt` - CreatedAt için index
- `IX_AuditLogs_Action` - Action için index
- `IX_AuditLogs_EntityName` - EntityName için index
- `IX_AuditLogs_EntityName_EntityId` - EntityName ve EntityId için composite index

**Foreign Keys:**
- `FK_AuditLogs_Users` - Users(Id) ile ilişki (ON DELETE SET NULL)

**Action Örnekleri:**
- `Create` - Yeni kayıt ekleme
- `Update` - Kayıt güncelleme
- `Delete` - Kayıt silme
- `Login` - Kullanıcı girişi
- `Logout` - Kullanıcı çıkışı
- `Export` - Veri dışa aktarma
- `Import` - Veri içe aktarma

**LogLevel Değerleri:**
- `Information` - Bilgi logları (varsayılan)
- `Warning` - Uyarı logları
- `Error` - Hata logları

**Kullanım Alanları:**
- Audit trail
- Compliance tracking
- Security monitoring
- User activity tracking
- Data change history

---

## Entity İlişkileri (ER Diagram)

```
Users
  |
  +-- 1:N --> ExceptionLogs (UserId)
  |
  +-- 1:N --> AuditLogs (UserId)

Categories
  |
  +-- 1:N --> Products (CategoryId)
```

---

## İndeks Stratejisi

### Performance İçin Oluşturulan İndeksler:

1. **Unique İndeksler:**
   - Users.Email
   - Users.Username
   - Products.SKU

2. **Foreign Key İndeksler:**
   - Products.CategoryId
   - ExceptionLogs.UserId
   - AuditLogs.UserId

3. **Query Performance İndeksler:**
   - ExceptionLogs.CreatedAt
   - ExceptionLogs.Severity
   - AuditLogs.CreatedAt
   - AuditLogs.Action
   - AuditLogs.EntityName
   - AuditLogs.(EntityName, EntityId) - Composite

---

## Veritabanı Boyut Tahmini

### Başlangıç Boyutu:
- Users: ~50 KB
- Categories: ~10 KB
- Products: ~100 KB
- ExceptionLogs: Değişken (log miktarına bağlı)
- AuditLogs: Değişken (aktivite miktarına bağlı)

### Log Yönetimi Önerileri:
1. **ExceptionLogs:** 90 gün sonra arşivleme
2. **AuditLogs:** 1 yıl sonra arşivleme
3. Kritik loglar için backup stratejisi
4. Log rotation politikası

---

## Güvenlik Notları

1. **Password Storage:** BCrypt hash kullanılıyor (salt otomatik)
2. **Sensitive Data:** OldValues ve NewValues alanlarında hassas veri şifrelenmeli
3. **IP Tracking:** GDPR uyumluluğu için retention policy gerekli
4. **User Agent:** Privacy için hash'lenebilir

---

## Bakım ve Optimizasyon

### Düzenli Bakım:
```sql
-- İndeks yeniden organize
ALTER INDEX ALL ON ExceptionLogs REORGANIZE;
ALTER INDEX ALL ON AuditLogs REORGANIZE;

-- İstatistik güncelleme
UPDATE STATISTICS ExceptionLogs;
UPDATE STATISTICS AuditLogs;

-- Eski logları arşivleme
DELETE FROM ExceptionLogs WHERE CreatedAt < DATEADD(DAY, -90, GETUTCDATE());
DELETE FROM AuditLogs WHERE CreatedAt < DATEADD(YEAR, -1, GETUTCDATE());
```

---

## Yedekleme Stratejisi

1. **Full Backup:** Haftalık
2. **Differential Backup:** Günlük
3. **Transaction Log Backup:** Her 15 dakikada bir
4. **Log Tables Backup:** Aylık arşiv

---

## Monitoring Queries

### En çok hata alan kullanıcılar:
```sql
SELECT TOP 10 
    u.Username,
    COUNT(*) as ErrorCount
FROM ExceptionLogs el
LEFT JOIN Users u ON el.UserId = u.Id
WHERE el.CreatedAt > DATEADD(DAY, -7, GETUTCDATE())
GROUP BY u.Username
ORDER BY ErrorCount DESC;
```

### En sık yapılan aksiyonlar:
```sql
SELECT 
    Action,
    EntityName,
    COUNT(*) as ActionCount
FROM AuditLogs
WHERE CreatedAt > DATEADD(DAY, -7, GETUTCDATE())
GROUP BY Action, EntityName
ORDER BY ActionCount DESC;
```

### Kritik hatalar:
```sql
SELECT *
FROM ExceptionLogs
WHERE Severity = 'Critical'
  AND CreatedAt > DATEADD(DAY, -1, GETUTCDATE())
ORDER BY CreatedAt DESC;
```

