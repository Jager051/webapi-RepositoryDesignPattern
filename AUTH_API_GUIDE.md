# Authentication API Kullanım Kılavuzu

## 🔐 Auth API - MSSQL + Redis Entegrasyonu

Bu API, kullanıcı kimlik doğrulaması için **MSSQL** ve **Redis** cache'i birlikte kullanmaktadır.

---

## 🚀 Özellikler

### ✅ Register (Kayıt)
- ✅ Email ve Username unique kontrolü (önce Redis, sonra MSSQL)
- ✅ BCrypt ile şifre hashleme
- ✅ MSSQL'e kayıt
- ✅ Redis'e cache'leme
- ✅ JWT token oluşturma
- ✅ AuditLog kaydı

### ✅ Login (Giriş)
- ✅ Email veya Username ile giriş
- ✅ Önce Redis cache kontrol
- ✅ Cache miss durumunda MSSQL'den getir
- ✅ BCrypt ile şifre doğrulama
- ✅ JWT token oluşturma
- ✅ Başarılı/Başarısız login audit log'u

---

## 📋 API Endpoints

### 1. Register (Kayıt Ol)

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 3,
    "username": "johndoe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "createdAt": "2025-10-15T10:30:00Z"
  },
  "message": "Registration successful"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "token": null,
  "user": null,
  "message": "User with this email or username already exists"
}
```

**Arka Planda Olan:**
1. ✅ Redis'de email/username kontrol edilir
2. ✅ MSSQL'de IsEmailUniqueAsync ve IsUsernameUniqueAsync çalışır
3. ✅ Şifre BCrypt ile hash'lenir
4. ✅ User MSSQL'e kaydedilir
5. ✅ User 3 farklı key ile Redis'e cache'lenir:
   - `user:{email}`
   - `user_by_username:{username}`
   - `user:{id}`
6. ✅ JWT token oluşturulur ve Redis'e cache'lenir
7. ✅ AuditLog tablosuna kayıt atılır

---

### 2. Login (Giriş Yap)

**Endpoint:** `POST /api/auth/login`

**Request Body (Email ile):**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Request Body (Username ile):**
```json
{
  "email": "johndoe",
  "password": "SecurePass123!"
}
```

> ⚠️ **Not:** Field adı "email" olsa da, hem email hem username kabul edilir.

**Success Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 3,
    "username": "johndoe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "createdAt": "2025-10-15T10:30:00Z"
  },
  "message": "Login successful"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "token": null,
  "user": null,
  "message": "Invalid email/username or password"
}
```

**Arka Planda Olan:**
1. ✅ @ karakteri kontrolü ile email/username ayrımı yapılır
2. ✅ Redis'de ilgili key kontrol edilir:
   - Email: `user:{email}`
   - Username: `user_by_username:{username}`
3. ✅ Cache hit: User Redis'den gelir
4. ✅ Cache miss: MSSQL'den GetByEmailAsync veya GetByUsernameAsync çalışır
5. ✅ User bulunursa Redis'e 15 dakikalığına cache'lenir
6. ✅ BCrypt.Verify ile şifre kontrol edilir
7. ✅ JWT token oluşturulur
8. ✅ Token Redis'e 24 saat süreyle cache'lenir
9. ✅ AuditLog tablosuna başarılı/başarısız login kaydı atılır

---

### 3. Validate Token (Token Doğrula)

**Endpoint:** `POST /api/auth/validate-token`

**Request Body:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Success Response (200 OK):**
```json
true
```

**Invalid Token Response (200 OK):**
```json
false
```

---

### 4. Get Current User (Mevcut Kullanıcı Bilgisi)

**Endpoint:** `GET /api/auth/user`

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "id": 3,
  "username": "johndoe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true,
  "createdAt": "2025-10-15T10:30:00Z"
}
```

**Error Response (401 Unauthorized):**
```json
"Invalid token"
```

---

## 🔍 Cache Stratejisi

### Redis Cache Keys

| Key Pattern | Değer | TTL | Kullanım |
|-------------|-------|-----|----------|
| `user:{email}` | User entity | 15 min | Email ile lookup |
| `user_by_username:{username}` | User entity | 15 min | Username ile lookup |
| `user:{id}` | User entity | 15 min | ID ile lookup |
| `token:{userId}` | JWT token | 24 hours | Token cache |
| `valid_token:{hash}` | bool | 1 hour | Token validation cache |

### Cache Akışı

**Register:**
```
1. Check Redis → user:{email}
2. Check Redis → user_by_username:{username}
3. Check MSSQL → IsEmailUniqueAsync()
4. Check MSSQL → IsUsernameUniqueAsync()
5. Save to MSSQL → Users table
6. Cache to Redis → 3 keys (email, username, id)
7. Cache token → token:{userId}
```

**Login:**
```
1. Determine if email or username
2. Check Redis → user:{email} or user_by_username:{username}
3. If not cached, query MSSQL
4. Cache result to Redis (if found)
5. Verify password with BCrypt
6. Generate JWT token
7. Cache token → token:{userId}
```

---

## 📊 Database Queries

### Register İşlemi Sırasında Çalışan Query'ler:

```sql
-- 1. Email unique kontrolü
SELECT COUNT(*) FROM Users WHERE Email = @email;

-- 2. Username unique kontrolü  
SELECT COUNT(*) FROM Users WHERE Username = @username;

-- 3. User insert
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsActive, CreatedAt)
VALUES (@username, @email, @passwordHash, @firstName, @lastName, 1, GETUTCDATE());

-- 4. Audit log insert
INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Description, NewValues, LogLevel, CreatedAt, IpAddress, ...)
VALUES (@userId, 'Register', 'User', @userId, 'New user registered: ...', @newValues, 'Information', GETUTCDATE(), @ip, ...);
```

### Login İşlemi Sırasında Çalışan Query'ler:

**Cache Miss Durumunda:**
```sql
-- Email ile login
SELECT * FROM Users WHERE Email = @email;

-- veya Username ile login
SELECT * FROM Users WHERE Username = @username;

-- Audit log insert (başarılı)
INSERT INTO AuditLogs (UserId, Action, Description, LogLevel, CreatedAt, IpAddress, ...)
VALUES (@userId, 'Login', 'User logged in successfully', 'Information', GETUTCDATE(), @ip, ...);

-- Audit log insert (başarısız)
INSERT INTO AuditLogs (UserId, Action, Description, LogLevel, CreatedAt, IpAddress, ...)
VALUES (@userId, 'LoginFailed', 'Failed login attempt', 'Warning', GETUTCDATE(), @ip, ...);
```

**Cache Hit Durumunda:**
```sql
-- Sadece audit log
INSERT INTO AuditLogs (...) VALUES (...);
```

---

## 🧪 Test Senaryoları

### Test 1: Register - Başarılı

**Request:**
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

**Beklenen Sonuç:**
- ✅ HTTP 200 OK
- ✅ success: true
- ✅ token dolu
- ✅ user bilgileri dolu
- ✅ MSSQL Users tablosunda kayıt
- ✅ Redis'de 3 key'de cache
- ✅ MSSQL AuditLogs tablosunda "Register" action

**Doğrulama:**
```sql
-- MSSQL'de
SELECT * FROM Users WHERE Email = 'test@example.com';
SELECT * FROM AuditLogs WHERE Action = 'Register' ORDER BY CreatedAt DESC;
```

```bash
# Redis'de
redis-cli
> GET user:test@example.com
> GET user_by_username:testuser
```

---

### Test 2: Register - Duplicate Email

**Request:**
```bash
# Aynı email ile tekrar kayıt dene
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser2",
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

**Beklenen Sonuç:**
- ✅ HTTP 400 Bad Request
- ✅ success: false
- ✅ message: "User with this email or username already exists"

---

### Test 3: Login - Başarılı (Email ile)

**Request:**
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

**Beklenen Sonuç:**
- ✅ HTTP 200 OK
- ✅ success: true
- ✅ token dolu
- ✅ AuditLogs'da "Login" action

---

### Test 4: Login - Başarılı (Username ile)

**Request:**
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser",
    "password": "Test123!"
  }'
```

**Beklenen Sonuç:**
- ✅ HTTP 200 OK
- ✅ success: true

---

### Test 5: Login - Yanlış Şifre

**Request:**
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "WrongPassword"
  }'
```

**Beklenen Sonuç:**
- ✅ HTTP 401 Unauthorized
- ✅ success: false
- ✅ AuditLogs'da "LoginFailed" action

---

### Test 6: Get Current User

**Request:**
```bash
curl -X GET "https://localhost:5001/api/auth/user" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Beklenen Sonuç:**
- ✅ HTTP 200 OK
- ✅ User bilgileri dolu

---

## 📈 Performance Metrikleri

### Cache Hit Scenario (Login)
```
Total Time: ~50ms
├─ Redis Lookup: ~2ms
├─ BCrypt Verify: ~30ms
├─ JWT Generation: ~10ms
└─ Audit Log Insert: ~8ms
```

### Cache Miss Scenario (Login)
```
Total Time: ~100ms
├─ Redis Lookup: ~2ms (miss)
├─ MSSQL Query: ~20ms
├─ Redis Cache Set: ~2ms
├─ BCrypt Verify: ~30ms
├─ JWT Generation: ~10ms
├─ Token Cache: ~2ms
└─ Audit Log Insert: ~8ms
```

### Register
```
Total Time: ~120ms
├─ Redis Check (2x): ~4ms
├─ MSSQL Unique Check (2x): ~40ms
├─ BCrypt Hash: ~30ms
├─ MSSQL Insert: ~15ms
├─ Redis Cache (3x): ~6ms
├─ JWT Generation: ~10ms
└─ Audit Log Insert: ~8ms
```

---

## 🔐 Security Features

1. **BCrypt Password Hashing**
   - Salt otomatik oluşturulur
   - Hash strength: 11 rounds (default)

2. **JWT Token**
   - Expiry: 24 hours
   - Algorithm: HS256
   - Claims: userId, email, username

3. **Input Validation**
   - Required fields validation
   - Email format validation
   - ModelState validation

4. **Audit Logging**
   - Tüm login denemeler loglanır
   - IP address kaydedilir
   - User agent bilgisi saklanır

---

## 🛠️ Troubleshooting

### Problem 1: "User not found" hatası
**Çözüm:** Redis cache'i temizleyin
```bash
redis-cli FLUSHALL
```

### Problem 2: Password verify hatası
**Çözüm:** BCrypt.Net.BCrypt versiyonunu kontrol edin

### Problem 3: Database connection error
**Çözüm:** Connection string'i kontrol edin
```json
"DefaultConnection": "Server=localhost;Database=WebAPIDb;..."
```

---

## 📝 Örnek Kullanım (Postman Collection)

### Collection: Authentication API

**1. Register**
- Method: POST
- URL: `{{baseUrl}}/api/auth/register`
- Body: raw (JSON)

**2. Login with Email**
- Method: POST
- URL: `{{baseUrl}}/api/auth/login`
- Body: raw (JSON)

**3. Login with Username**
- Method: POST
- URL: `{{baseUrl}}/api/auth/login`
- Body: raw (JSON)

**4. Get Current User**
- Method: GET
- URL: `{{baseUrl}}/api/auth/user`
- Headers: `Authorization: Bearer {{token}}`

**Environment Variables:**
```json
{
  "baseUrl": "https://localhost:5001",
  "token": "will-be-set-automatically"
}
```

---

## 📊 Monitoring Queries

### Son 24 saatteki login aktiviteleri
```sql
SELECT 
    u.Username,
    al.Action,
    al.Description,
    al.IpAddress,
    al.CreatedAt
FROM AuditLogs al
LEFT JOIN Users u ON al.UserId = u.Id
WHERE al.Action IN ('Login', 'LoginFailed')
  AND al.CreatedAt > DATEADD(HOUR, -24, GETUTCDATE())
ORDER BY al.CreatedAt DESC;
```

### Başarısız login denemeleri
```sql
SELECT 
    u.Username,
    u.Email,
    COUNT(*) as FailedAttempts,
    MAX(al.CreatedAt) as LastAttempt
FROM AuditLogs al
LEFT JOIN Users u ON al.UserId = u.Id
WHERE al.Action = 'LoginFailed'
  AND al.CreatedAt > DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY u.Username, u.Email
ORDER BY FailedAttempts DESC;
```

### Yeni kayıtlar
```sql
SELECT 
    u.Username,
    u.Email,
    u.CreatedAt,
    al.IpAddress
FROM Users u
LEFT JOIN AuditLogs al ON u.Id = al.UserId AND al.Action = 'Register'
WHERE u.CreatedAt > DATEADD(DAY, -7, GETUTCDATE())
ORDER BY u.CreatedAt DESC;
```

---

**Dokümantasyon Güncellenme Tarihi:** 15 Ekim 2025

