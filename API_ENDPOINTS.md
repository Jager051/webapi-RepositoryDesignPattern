# 🌐 API Endpoints - Custom Repository Pattern

Bu dokümanda yeni eklenen custom repository pattern ile oluşturulan tüm API endpoint'ler listelenmiştir.

## 🔐 Authentication

Tüm endpoint'ler JWT Bearer token gerektirir (Auth endpoint'leri hariç).

**Header:**
```
Authorization: Bearer {your_jwt_token}
```

---

## 📦 Products API

### Temel İşlemler

#### GET `/api/products`
Tüm ürünleri getir (cache'li)
```
Response: ProductDto[]
```

#### GET `/api/products/{id}`
ID'ye göre ürün getir
```
Response: ProductDto
```

#### POST `/api/products`
Yeni ürün oluştur
```
Request Body: CreateProductDto
Response: ProductDto
```

#### PUT `/api/products/{id}`
Ürün güncelle
```
Request Body: UpdateProductDto
Response: ProductDto
```

#### DELETE `/api/products/{id}`
Ürün sil (soft delete)
```
Response: 204 NoContent
```

---

### 🆕 Custom Repository Endpoint'leri

#### GET `/api/products/active`
**Sadece aktif ürünleri** getir (Category dahil)
```bash
# Örnek
GET /api/products/active

Response: ProductDto[]
```

#### GET `/api/products/category/{categoryId}`
**Kategoriye göre** ürünleri getir (Category dahil)
```bash
# Örnek
GET /api/products/category/1

Response: ProductDto[]
```

#### GET `/api/products/price-range`
**Fiyat aralığına göre** ürünleri filtrele
```bash
# Örnek
GET /api/products/price-range?minPrice=10&maxPrice=100

Query Parameters:
  - minPrice: decimal (required)
  - maxPrice: decimal (required)

Response: ProductDto[]
```

#### GET `/api/products/low-stock`
**Düşük stoklu** ürünleri getir
```bash
# Örnek
GET /api/products/low-stock?threshold=10

Query Parameters:
  - threshold: int (default: 10)

Response: ProductDto[]
```

#### GET `/api/products/search`
**Ürün arama** (isim ve açıklamada)
```bash
# Örnek
GET /api/products/search?searchTerm=laptop

Query Parameters:
  - searchTerm: string (required)

Response: ProductDto[]
```

#### GET `/api/products/sku/{sku}`
**SKU koduna göre** ürün bul
```bash
# Örnek
GET /api/products/sku/PRD-001

Response: ProductDto
```

---

## 📁 Categories API

### Temel İşlemler

#### GET `/api/categories`
Tüm kategorileri getir (Products dahil)
```
Response: CategoryDto[]
```

#### GET `/api/categories/{id}`
ID'ye göre kategori getir (Products dahil)
```
Response: CategoryDto
```

#### POST `/api/categories`
Yeni kategori oluştur
```
Request Body: CreateCategoryDto
Response: CategoryDto
```

#### PUT `/api/categories/{id}`
Kategori güncelle
```
Request Body: UpdateCategoryDto
Response: CategoryDto
```

#### DELETE `/api/categories/{id}`
Kategori sil (ürünü varsa siler)
```
Response: 204 NoContent / 400 BadRequest
```

---

### 🆕 Custom Repository Endpoint'leri

#### GET `/api/categories/active`
**Sadece aktif** kategorileri getir
```bash
# Örnek
GET /api/categories/active

Response: CategoryDto[]
```

#### GET `/api/categories/with-products`
**Ürünleriyle birlikte** tüm kategorileri getir
```bash
# Örnek
GET /api/categories/with-products

Response: CategoryDto[]
# Her CategoryDto içinde ProductCount özelliği var
```

#### GET `/api/categories/{id}/product-count`
**Kategorideki ürün sayısını** getir
```bash
# Örnek
GET /api/categories/1/product-count

Response: int (ürün sayısı)
```

#### GET `/api/categories/search`
**Kategori arama** (isim ve açıklamada)
```bash
# Örnek
GET /api/categories/search?searchTerm=electronics

Query Parameters:
  - searchTerm: string (required)

Response: CategoryDto[]
```

---

## 👤 Auth API

#### POST `/api/auth/register`
Yeni kullanıcı kaydı
```json
Request Body:
{
  "username": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}

Response: AuthResponseDto
{
  "success": true,
  "token": "jwt_token_here",
  "user": UserDto,
  "message": "Registration successful"
}
```

#### POST `/api/auth/login`
Kullanıcı girişi (username veya email ile)
```json
Request Body:
{
  "email": "user@example.com",  // veya username
  "password": "string"
}

Response: AuthResponseDto
{
  "success": true,
  "token": "jwt_token_here",
  "user": UserDto,
  "message": "Login successful"
}
```

#### GET `/api/auth/validate`
Token doğrulama
```
Header: Authorization: Bearer {token}
Response: bool
```

---

## 📊 DTO Modelleri

### ProductDto
```json
{
  "id": 1,
  "name": "Product Name",
  "description": "Product Description",
  "price": 99.99,
  "sku": "PRD-001",
  "stockQuantity": 100,
  "isActive": true,
  "categoryId": 1,
  "categoryName": "Category Name",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### CategoryDto
```json
{
  "id": 1,
  "name": "Category Name",
  "description": "Category Description",
  "isActive": true,
  "productCount": 5,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### UserDto
```json
{
  "id": 1,
  "username": "johndoe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

## 🚀 Kullanım Örnekleri

### cURL ile Kullanım

#### 1. Kayıt Ol
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

#### 2. Giriş Yap
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

#### 3. Aktif Ürünleri Getir
```bash
curl -X GET http://localhost:5000/api/products/active \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### 4. Düşük Stoklu Ürünleri Getir
```bash
curl -X GET "http://localhost:5000/api/products/low-stock?threshold=5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### 5. Ürün Ara
```bash
curl -X GET "http://localhost:5000/api/products/search?searchTerm=laptop" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### 6. Fiyat Aralığına Göre Filtrele
```bash
curl -X GET "http://localhost:5000/api/products/price-range?minPrice=50&maxPrice=200" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 📝 Cache Stratejisi

### Ürünler
- **GetAll**: 10 dakika cache
- **GetById**: 15 dakika cache
- **Create/Update/Delete**: Cache invalidation

### Kullanıcılar
- **User lookup**: 15 dakika cache (email, username, id ile)
- **Token**: 24 saat cache

---

## 🔍 Custom Repository Özellikleri

### Performance Optimizations
1. **Eager Loading**: İlişkili veriler otomatik yüklenir (N+1 problemi yok)
2. **Filtered Includes**: Sadece aktif ürünler/kategoriler
3. **Indexing**: SKU, Email, Username için optimum sorgular
4. **Caching**: Redis ile performans artışı

### Validation
1. **SKU Uniqueness**: Ürün oluştururken/güncellerken
2. **Email Uniqueness**: Kullanıcı kaydında
3. **Username Uniqueness**: Kullanıcı kaydında
4. **Category Name Uniqueness**: Kategori işlemlerinde

---

## 🎯 Yeni Endpoint Ekleme Örneği

Custom repository metodunu kullanarak yeni endpoint eklemek için:

```csharp
// 1. Repository'de metod zaten var
// IProductRepository.GetLowStockProductsAsync(threshold)

// 2. Service'de metod ekle
public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold)
{
    var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
    return products.Select(MapToProductDto);
}

// 3. Controller'da endpoint ekle
[HttpGet("low-stock")]
public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts(
    [FromQuery] int threshold = 10)
{
    var products = await _productService.GetLowStockProductsAsync(threshold);
    return Ok(products);
}
```

**Sonuç**: `/api/products/low-stock?threshold=5` endpoint'i kullanıma hazır!

---

## 📚 İlgili Dökümanlar

- [CUSTOM_REPOSITORY_USAGE.md](./CUSTOM_REPOSITORY_USAGE.md) - Detaylı kullanım kılavuzu
- [REPOSITORY_PATTERN_SUMMARY.md](./REPOSITORY_PATTERN_SUMMARY.md) - Mimari özet

---

**Not**: Tüm endpoint'ler Swagger UI'da da görüntülenebilir: `http://localhost:5000/swagger`

