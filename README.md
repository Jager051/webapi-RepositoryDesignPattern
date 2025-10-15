# 🚀 WebAPI - Advanced Repository Pattern

ASP.NET Core 6.0 ile geliştirilmiş, **Custom Repository Pattern**, **Unit of Work**, **JWT Authentication** ve **Redis Caching** içeren profesyonel bir Web API projesi.

## ✨ Özellikler

### 🎯 Design Patterns
- ✅ **Generic Repository Pattern** - Temel CRUD işlemleri
- ✅ **Entity-Specific Custom Repositories** - Her entity için özel sorgular
- ✅ **Unit of Work Pattern** - Transaction yönetimi
- ✅ **Business Rules Pattern** - ⭐ Modüler validasyon kuralları
- ✅ **Orchestrator Pattern** - ⭐ Karmaşık işlem yönetimi
- ✅ **Dependency Injection** - Loose coupling
- ✅ **Service Layer Pattern** - İş mantığı ayrımı

### 🔐 Authentication & Security
- ✅ JWT Bearer Token Authentication
- ✅ BCrypt Password Hashing
- ✅ Role-based Authorization

### ⚡ Performance
- ✅ Redis Caching (distributed cache)
- ✅ EF Core Query Optimization
- ✅ Lazy Initialization
- ✅ Eager Loading (Include/Join)

### 📊 Database
- ✅ Entity Framework Core 6.0
- ✅ In-Memory Database (development)
- ✅ SQL Server support (production ready)
- ✅ Soft Delete support

## 🏗️ Proje Yapısı

```
WebAPI-RepositoryDesignPattern/
│
├── WebAPI.API/                     # API Layer
│   ├── Controllers/
│   │   ├── AuthController.cs       # Authentication endpoints
│   │   ├── ProductsController.cs   # Product CRUD + Custom endpoints
│   │   ├── CategoriesController.cs # Category CRUD + Custom endpoints
│   │   └── CacheController.cs      # Cache management
│   └── Program.cs                  # DI & Middleware configuration
│
├── WebAPI.Core/                    # Domain Layer
│   ├── Entities/                   # Domain models
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   └── User.cs
│   ├── DTOs/                       # Data Transfer Objects
│   └── Interfaces/                 # Repository interfaces
│       ├── IRepository<T>.cs       # Generic repository
│       ├── IProductRepository.cs   # ⭐ Custom Product repository
│       ├── ICategoryRepository.cs  # ⭐ Custom Category repository
│       ├── IUserRepository.cs      # ⭐ Custom User repository
│       ├── IUnitOfWork.cs          # ⭐ Unit of Work pattern
│       ├── IAuthService.cs
│       └── ICacheService.cs
│
├── WebAPI.Infrastructure/          # Data Access Layer
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Repositories/
│   │   ├── GenericRepository<T>.cs      # Base repository
│   │   ├── ProductRepository.cs         # ⭐ Custom Product queries
│   │   ├── CategoryRepository.cs        # ⭐ Custom Category queries
│   │   ├── UserRepository.cs            # ⭐ Custom User queries
│   │   └── UnitOfWork.cs                # ⭐ Lazy initialization
│   └── Services/
│       ├── RedisCacheService.cs
│       └── RedisConnectionService.cs
│
└── WebAPI.Services/                # Business Logic Layer
    ├── Services/
    │   ├── AuthService.cs          # Authentication logic
    │   ├── ProductService.cs       # ⭐ Uses orchestrators
    │   └── CategoryService.cs      # ⭐ Uses orchestrators
    ├── Orchestrators/              # ⭐ Complex operation orchestration
    │   ├── ProductOrchestrator.cs
    │   ├── UpdateProductOrchestrator.cs
    │   └── DeleteCategoryOrchestrator.cs
    └── BusinessRules/              # ⭐ Modular validation rules
        ├── ProductBusinessRules/
        │   ├── ProductSkuMustBeUniqueRule.cs
        │   ├── ProductPriceMustBeValidRule.cs
        │   └── ProductMustHaveValidCategoryRule.cs
        └── CategoryBusinessRules/
            ├── CategoryNameMustBeUniqueRule.cs
            └── CategoryCannotBeDeletedWithProductsRule.cs
```

## 🎯 Custom Repository Pattern

### Neden Custom Repository?

❌ **Eski Yöntem (Sadece Generic)**
```csharp
// Karmaşık LINQ sorguları her yerde tekrar ediyor
var products = await _unitOfWork.Repository<Product>()
    .FindAsync(p => p.CategoryId == categoryId);
// Category yüklenmiyor, N+1 problem!
```

✅ **Yeni Yöntem (Custom Repository)**
```csharp
// Temiz, optimize edilmiş, yeniden kullanılabilir
var products = await _unitOfWork.Products
    .GetProductsByCategoryAsync(categoryId);
// Category otomatik yükleniyor, optimize edilmiş!
```

### Custom Repository Metodları

#### 📦 IProductRepository
```csharp
// 8 özel metod
GetProductsByCategoryAsync(categoryId)
GetActiveProductsAsync()
GetProductsByPriceRangeAsync(min, max)
GetLowStockProductsAsync(threshold)
SearchProductsByNameAsync(searchTerm)
GetProductBySkuAsync(sku)
GetProductsWithCategoryAsync()
IsSkuUniqueAsync(sku, excludeId)
```

#### 📁 ICategoryRepository
```csharp
// 6 özel metod
GetActiveCategoriesAsync()
GetCategoriesWithProductsAsync()
GetCategoryWithProductsAsync(id)
SearchCategoriesByNameAsync(searchTerm)
IsCategoryNameUniqueAsync(name, excludeId)
GetProductCountByCategoryAsync(id)
```

#### 👤 IUserRepository
```csharp
// 6 özel metod
GetByUsernameAsync(username)
GetByEmailAsync(email)
GetActiveUsersAsync()
IsUsernameUniqueAsync(username, excludeId)
IsEmailUniqueAsync(email, excludeId)
SearchUsersByNameAsync(searchTerm)
```

## 🚀 Hızlı Başlangıç

### 1. Projeyi Klonla
```bash
git clone <repository-url>
cd webapi-RepositoryDesignPattern
```

### 2. Bağımlılıkları Yükle
```bash
dotnet restore
```

### 3. Redis'i Başlat (Docker)
```bash
docker run -d -p 6379:6379 redis
```

### 4. Uygulamayı Çalıştır
```bash
cd WebAPI.API
dotnet run
```

### 5. Swagger'ı Aç
```
https://localhost:7000/swagger
```

## 💡 Kullanım Örnekleri

### Service'de Kullanım
```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProducts(int threshold)
    {
        // Custom repository metodunu kullan
        var products = await _unitOfWork.Products
            .GetLowStockProductsAsync(threshold);
        
        return products.Select(MapToDto);
    }

    public async Task<bool> CreateProduct(CreateProductDto dto)
    {
        // Transaction kullan
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // SKU benzersizliğini kontrol et
            var isUnique = await _unitOfWork.Products
                .IsSkuUniqueAsync(dto.SKU);
            
            if (!isUnique)
                return false;

            // Kategori var mı kontrol et
            var categoryExists = await _unitOfWork.Categories
                .ExistsAsync(c => c.Id == dto.CategoryId);
            
            if (!categoryExists)
                return false;

            var product = MapToEntity(dto);
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }
}
```

### Controller'da Kullanım
```csharp
[HttpGet("low-stock")]
public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
{
    var products = await _productService.GetLowStockProductsAsync(threshold);
    return Ok(products);
}

[HttpGet("search")]
public async Task<IActionResult> SearchProducts([FromQuery] string term)
{
    var products = await _productService.SearchProductsAsync(term);
    return Ok(products);
}
```

## 📡 API Endpoints

### Products
- `GET /api/products` - Tüm ürünler
- `GET /api/products/{id}` - ID'ye göre ürün
- `GET /api/products/active` - ⭐ Aktif ürünler
- `GET /api/products/category/{id}` - ⭐ Kategoriye göre
- `GET /api/products/price-range?min=X&max=Y` - ⭐ Fiyat aralığı
- `GET /api/products/low-stock?threshold=N` - ⭐ Düşük stok
- `GET /api/products/search?term=X` - ⭐ Arama
- `GET /api/products/sku/{sku}` - ⭐ SKU ile arama
- `POST /api/products` - Yeni ürün
- `PUT /api/products/{id}` - Ürün güncelle
- `DELETE /api/products/{id}` - Ürün sil

### Categories
- `GET /api/categories` - Tüm kategoriler
- `GET /api/categories/{id}` - ID'ye göre kategori
- `GET /api/categories/active` - ⭐ Aktif kategoriler
- `GET /api/categories/with-products` - ⭐ Ürünlerle birlikte
- `GET /api/categories/{id}/product-count` - ⭐ Ürün sayısı
- `GET /api/categories/search?term=X` - ⭐ Arama
- `POST /api/categories` - Yeni kategori
- `PUT /api/categories/{id}` - Kategori güncelle
- `DELETE /api/categories/{id}` - Kategori sil

### Authentication
- `POST /api/auth/register` - Kayıt ol
- `POST /api/auth/login` - Giriş yap
- `GET /api/auth/validate` - Token doğrula

⭐ = Custom Repository metodu kullanıyor

## 📚 Dökümanlar

- 📖 [CUSTOM_REPOSITORY_USAGE.md](./CUSTOM_REPOSITORY_USAGE.md) - Detaylı kullanım kılavuzu
- 📊 [REPOSITORY_PATTERN_SUMMARY.md](./REPOSITORY_PATTERN_SUMMARY.md) - Mimari özet
- 🎯 [BUSINESS_RULES_ORCHESTRATOR_PATTERN.md](./BUSINESS_RULES_ORCHESTRATOR_PATTERN.md) - Business Rules & Orchestrator
- 🏛️ [CLEAN_ARCHITECTURE_LAYERS.md](./CLEAN_ARCHITECTURE_LAYERS.md) - ⭐ Temiz Katmanlı Mimari
- 🌐 [API_ENDPOINTS.md](./API_ENDPOINTS.md) - Tüm endpoint'ler ve örnekler

## 🛠️ Teknolojiler

- ASP.NET Core 6.0
- Entity Framework Core 6.0
- JWT Authentication
- Redis (StackExchange.Redis)
- BCrypt.Net
- Swagger/OpenAPI
- In-Memory Database (dev)

## 🎨 Avantajlar

### 1. Temiz Kod
```csharp
// ❌ Karmaşık
var products = await _context.Products
    .Include(p => p.Category)
    .Where(p => p.IsActive && !p.IsDeleted)
    .OrderBy(p => p.Name)
    .ToListAsync();

// ✅ Temiz
var products = await _unitOfWork.Products.GetActiveProductsAsync();
```

### 2. Type Safety
```csharp
// ✅ Compile-time kontrol
var products = await _unitOfWork.Products.GetProductsByCategoryAsync(1);

// ❌ Runtime hatası riski
var products = await _unitOfWork.Repository<Product>()
    .FindAsync(p => p.CategoryId == 1);
```

### 3. Performans
- **N+1 Problem Yok**: Include() otomatik
- **Lazy Loading**: Repository'ler gerektiğinde oluşturulur
- **Caching**: Redis ile hızlı erişim
- **Optimized Queries**: Hand-tuned LINQ

### 4. Test Edilebilirlik
```csharp
// Mock ile kolay test
var mockUnitOfWork = new Mock<IUnitOfWork>();
mockUnitOfWork.Setup(x => x.Products.GetActiveProductsAsync())
    .ReturnsAsync(mockProducts);
```

## 🔄 Yeni Repository Ekleme

```csharp
// 1. Interface oluştur
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
}

// 2. Implementasyon
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
    {
        return await _dbSet.Where(o => o.UserId == userId).ToListAsync();
    }
}

// 3. UnitOfWork'e ekle
public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
}

// 4. Kullan!
var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(123);
```

## 📈 Performans İyileştirmeleri

1. **Redis Caching**: 10-15 dakika cache süresi
2. **Include Optimization**: İlişkili veriler tek sorguda
3. **Lazy Initialization**: Bellek optimizasyonu
4. **Async/Await**: Non-blocking I/O
5. **Query Optimization**: Index kullanımı

## 🧪 Test

```bash
# Unit testler çalıştır
dotnet test

# Build
dotnet build

# Publish
dotnet publish -c Release
```

## 📝 Lisans

MIT License

## 👨‍💻 Geliştirici Notları

Bu proje **Repository Pattern**'in modern kullanımını göstermektedir:
- Generic repository temel işlemler için
- Custom repository özel işlemler için
- Unit of Work transaction yönetimi için
- Service Layer iş mantığı için

**En İyi Pratikler**:
✅ Her entity için özel repository oluşturun
✅ Karmaşık sorguları repository içinde saklayın
✅ Transaction gereken yerlerde UnitOfWork kullanın
✅ Service layer'da iş mantığını tutun
✅ Controller'ları ince tutun

---

**Happy Coding! 🚀**

