# 🏛️ Clean Architecture - Layered Orchestrator Pattern

## 🎯 Mimari Prensipler

### Katman Sorumluluları

```
┌─────────────────────────────────────────────────────────────┐
│                     API LAYER (Controllers)                  │
│  Sorumluluk: HTTP isteklerini alır, Service'leri çağırır   │
│  Bağımlılık: Service Layer                                  │
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                    SERVICE LAYER                             │
│  Sorumluluk:                                                │
│   ✅ Orchestrator koordinasyonu                             │
│   ✅ Cache yönetimi                                         │
│   ✅ Hata yönetimi                                          │
│   ❌ VERİ ERİŞİMİ YOK - Orchestrator'a delege eder         │
│  Bağımlılık: Orchestrators, Cache                          │
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                 ORCHESTRATOR LAYER                           │
│  Sorumluluk:                                                │
│   ✅ Business rules koordinasyonu                           │
│   ✅ Transaction yönetimi                                   │
│   ✅ Veri erişimi (UnitOfWork üzerinden)                   │
│   ✅ Entity-DTO mapping                                     │
│  Bağımlılık: Business Rules, UnitOfWork                    │
│                                                             │
│  Alt Katmanlar:                                            │
│  ├── Query/ (Read operations - GET)                       │
│  └── Command/ (Write operations - POST/PUT/DELETE)        │
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│              BUSINESS RULES LAYER                            │
│  Sorumluluk:                                                │
│   ✅ Domain validasyonları                                  │
│   ✅ İş kuralları kontrolü                                  │
│   ✅ Tek sorumluluk (Single Responsibility)                │
│  Bağımlılık: UnitOfWork (sadece okuma için)                │
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                   UNIT OF WORK LAYER                         │
│  Sorumluluk:                                                │
│   ✅ Repository yönetimi                                    │
│   ✅ Transaction koordinasyonu                              │
│  Bağımlılık: Repositories                                  │
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                  REPOSITORY LAYER                            │
│  Sorumluluk:                                                │
│   ✅ Veri erişimi                                           │
│   ✅ CRUD operasyonları                                     │
│   ✅ Custom queries                                         │
│  Bağımlılık: DbContext                                     │
└─────────────────────────────────────────────────────────────┘
```

## 🎨 Veri Akışı

### Query (Read) İşlemi
```
Controller
    ↓
Service (cache check)
    ↓
Query Orchestrator (NEW ile oluşturuluyor)
    ↓
UnitOfWork
    ↓
Custom Repository
    ↓
Database
    ↓
Entity → DTO (Orchestrator'da mapping)
    ↓
Service (cache set)
    ↓
Controller → Response
```

### Command (Write) İşlemi
```
Controller (CreateProductDto)
    ↓
Service
    ↓
Command Orchestrator (NEW ile oluşturuluyor)
    ↓
Business Rules (validasyon)
    ├─ Rule 1: SKU benzersiz mi?
    ├─ Rule 2: Kategori var mı?
    ├─ Rule 3: Fiyat geçerli mi?
    └─ Rule 4: Stok geçerli mi?
    ↓
Transaction Start
    ↓
UnitOfWork → Repository → Database
    ↓
Transaction Commit/Rollback
    ↓
Service (cache invalidate)
    ↓
Controller → Response
```

## 📁 Klasör Yapısı

```
WebAPI.Services/
├── Services/
│   ├── ProductService.cs          # Orchestrator koordinatörü
│   └── CategoryService.cs         # Orchestrator koordinatörü
│
├── Orchestrators/
│   ├── Query/                     # Read işlemleri
│   │   ├── GetProductsOrchestrator.cs
│   │   └── GetCategoriesOrchestrator.cs
│   │
│   └── Command/                   # Write işlemleri
│       ├── ProductOrchestrator.cs
│       ├── UpdateProductOrchestrator.cs
│       ├── CreateCategoryOrchestrator.cs
│       ├── UpdateCategoryOrchestrator.cs
│       └── DeleteCategoryOrchestrator.cs
│
└── BusinessRules/
    ├── ProductBusinessRules/
    │   ├── ProductSkuMustBeUniqueRule.cs
    │   ├── ProductMustHaveValidCategoryRule.cs
    │   ├── ProductPriceMustBeValidRule.cs
    │   └── ProductStockMustBeValidRule.cs
    │
    └── CategoryBusinessRules/
        ├── CategoryNameMustBeUniqueRule.cs
        └── CategoryCannotBeDeletedWithProductsRule.cs
```

## 💡 Kod Örnekleri

### ❌ YANLIŞ - Service'de Direkt UnitOfWork Kullanımı

```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ProductDto> CreateProduct(CreateProductDto dto)
    {
        // ❌ Service doğrudan veri erişimi yapıyor
        var product = new Product { ... };
        
        // ❌ Validasyon service içinde dağınık
        if (string.IsNullOrEmpty(dto.SKU))
            throw new Exception("SKU required");
        
        // ❌ Transaction yönetimi service'de
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
        
        return MapToDto(product);
    }
}
```

**Sorunlar**:
- ❌ Service katmanı veri erişimi yapıyor (SRP ihlali)
- ❌ Business rules dağınık
- ❌ Test edilmesi zor
- ❌ Yeniden kullanılamaz

### ✅ DOĞRU - Orchestrator Pattern ile Temiz Katmanlar

```csharp
// SERVICE - Sadece koordinasyon
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public async Task<ProductDto> CreateProduct(CreateProductDto dto)
    {
        // ✅ Orchestrator'a delege eder
        var orchestrator = new ProductOrchestrator(_unitOfWork);
        var result = await orchestrator.ExecuteAsync(dto);

        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);

        // ✅ Service sadece cache yönetir
        await _cacheService.RemoveByPatternAsync("products:*");
        
        return result.Data!;
    }

    public async Task<IEnumerable<ProductDto>> GetAll()
    {
        // ✅ Cache kontrolü service'de
        var cached = await _cacheService.GetAsync<List<ProductDto>>("products:all");
        if (cached != null) return cached;

        // ✅ Veri erişimi orchestrator'da
        var orchestrator = new GetProductsOrchestrator(_unitOfWork);
        var products = await orchestrator.GetAllAsync();
        
        // ✅ Cache set service'de
        await _cacheService.SetAsync("products:all", products, TimeSpan.FromMinutes(10));
        return products;
    }
}

// ORCHESTRATOR - Business logic ve veri erişimi
public class ProductOrchestrator : IOrchestrator<CreateProductDto, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<OrchestratorResult<ProductDto>> ExecuteAsync(CreateProductDto input)
    {
        // 1. Entity oluştur
        var product = MapToEntity(input);

        // 2. Business rules'ları NEW ile oluştur
        var rules = new List<IBusinessRule<Product>>
        {
            new ProductSkuMustBeUniqueRule(_unitOfWork),
            new ProductPriceMustBeValidRule()
        };

        // 3. Validate
        var errors = await ValidateRules(rules, product);
        if (errors.Any())
            return OrchestratorResult<ProductDto>.ValidationFailure(errors);

        // 4. Transaction
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return OrchestratorResult<ProductDto>.SuccessResult(MapToDto(product));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

**Avantajlar**:
- ✅ Her katman tek sorumluluğa sahip
- ✅ Service sadece koordinasyon yapar
- ✅ Orchestrator business logic'i yönetir
- ✅ Business rules izole ve test edilebilir
- ✅ Yeniden kullanılabilir

## 🎯 Katman Sorumlulukları Detay

### 1. Service Layer

**Yapması Gerekenler**:
```csharp
✅ Orchestrator'ları NEW ile oluşturmak
✅ Cache yönetimi (get/set/invalidate)
✅ Orchestrator sonuçlarını kontrol etmek
✅ Hata yönetimi ve loglama
```

**Yapmaması Gerekenler**:
```csharp
❌ Direkt UnitOfWork çağrıları
❌ Entity oluşturma/güncelleme
❌ Business rule validasyonu
❌ Transaction yönetimi
❌ Entity-DTO mapping
```

**Örnek**:
```csharp
public class ProductService
{
    public async Task<ProductDto> Create(CreateProductDto dto)
    {
        // ✅ Orchestrator oluştur
        var orchestrator = new ProductOrchestrator(_unitOfWork);
        
        // ✅ Çalıştır
        var result = await orchestrator.ExecuteAsync(dto);
        
        // ✅ Hata kontrolü
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
        
        // ✅ Cache yönetimi
        await _cacheService.RemoveByPatternAsync("products:*");
        
        return result.Data!;
    }
}
```

### 2. Orchestrator Layer

**Query Orchestrator (Read)**:
```csharp
public class GetProductsOrchestrator
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        // ✅ Repository çağrısı
        var products = await _unitOfWork.Products.GetProductsWithCategoryAsync();
        
        // ✅ Mapping
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        // ✅ Repository çağrısı
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        
        // ✅ Mapping
        return product != null ? MapToDto(product) : null;
    }

    private static ProductDto MapToDto(Product product)
    {
        // ✅ DTO mapping burada
        return new ProductDto { ... };
    }
}
```

**Command Orchestrator (Write)**:
```csharp
public class ProductOrchestrator : IOrchestrator<CreateProductDto, ProductDto>
{
    public async Task<OrchestratorResult<ProductDto>> ExecuteAsync(CreateProductDto input)
    {
        // ✅ 1. Mapping
        var product = MapToEntity(input);

        // ✅ 2. Business rules
        var rules = CreateBusinessRules();
        var errors = await ValidateRules(rules, product);
        if (errors.Any())
            return OrchestratorResult<ProductDto>.ValidationFailure(errors);

        // ✅ 3. Transaction
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return OrchestratorResult<ProductDto>.SuccessResult(MapToDto(product));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### 3. Business Rules Layer

**Single Responsibility**:
```csharp
// ✅ Her rule tek bir şeyi kontrol eder
public class ProductSkuMustBeUniqueRule : IBusinessRule<Product>
{
    public async Task<BusinessRuleResult> ValidateAsync(Product product)
    {
        var isUnique = await _unitOfWork.Products.IsSkuUniqueAsync(product.SKU);
        
        if (!isUnique)
        {
            return BusinessRuleResult.Failure(
                "SKU already exists", 
                "PRODUCT_SKU_DUPLICATE");
        }

        return BusinessRuleResult.Success();
    }
}
```

## 📊 Mimari Karşılaştırması

### Önceki Mimari (Service'de Direkt UnitOfWork)

```
Controller → Service → UnitOfWork → Repository → DB
              ↓
         (Business logic
          Validation
          Transaction
          Cache
          Mapping)
         Tüm sorumluluklar Service'de!
```

**Sorunlar**:
- ❌ Service çok şişkin
- ❌ Test edilmesi zor
- ❌ Kod tekrarı
- ❌ SRP ihlali

### Yeni Mimari (Orchestrator Pattern)

```
Controller → Service → Orchestrator → UnitOfWork → Repository → DB
              ↓           ↓
           (Cache)    (Business Logic
                       Validation
                       Transaction
                       Mapping)
```

**Avantajlar**:
- ✅ Her katman tek sorumluluk
- ✅ Test edilebilir
- ✅ Yeniden kullanılabilir
- ✅ Modüler
- ✅ SOLID prensiplerine uygun

## 🚀 Kullanım Senaryoları

### Senaryo 1: Basit Query
```csharp
// Controller
[HttpGet]
public async Task<IActionResult> GetProducts()
{
    var products = await _productService.GetAllProductsAsync();
    return Ok(products);
}

// Service (sadece cache)
public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
{
    var cached = await _cacheService.GetAsync<List<ProductDto>>("products");
    if (cached != null) return cached;

    var orchestrator = new GetProductsOrchestrator(_unitOfWork);
    var products = await orchestrator.GetAllAsync();
    
    await _cacheService.SetAsync("products", products, TimeSpan.FromMinutes(10));
    return products;
}

// Orchestrator (veri erişimi + mapping)
public async Task<IEnumerable<ProductDto>> GetAllAsync()
{
    var products = await _unitOfWork.Products.GetProductsWithCategoryAsync();
    return products.Select(MapToDto);
}
```

### Senaryo 2: Karmaşık Command
```csharp
// Controller
[HttpPost]
public async Task<IActionResult> CreateProduct(CreateProductDto dto)
{
    try
    {
        var product = await _productService.CreateProductAsync(dto);
        return Created($"/api/products/{product.Id}", product);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}

// Service (koordinasyon + cache)
public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
{
    var orchestrator = new ProductOrchestrator(_unitOfWork);
    var result = await orchestrator.ExecuteAsync(dto);

    if (!result.Success)
        throw new InvalidOperationException(result.ErrorMessage);

    await _cacheService.RemoveByPatternAsync("products:*");
    return result.Data!;
}

// Orchestrator (business logic + transaction + veri erişimi)
public async Task<OrchestratorResult<ProductDto>> ExecuteAsync(CreateProductDto input)
{
    var product = MapToEntity(input);
    
    // Business rules
    var rules = new List<IBusinessRule<Product>>
    {
        new ProductSkuMustBeUniqueRule(_unitOfWork),
        new ProductPriceMustBeValidRule()
    };
    
    var errors = await ValidateRules(rules, product);
    if (errors.Any())
        return OrchestratorResult<ProductDto>.ValidationFailure(errors);
    
    // Transaction
    await _unitOfWork.BeginTransactionAsync();
    try
    {
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
        
        return OrchestratorResult<ProductDto>.SuccessResult(MapToDto(product));
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## 📋 Checklist - Yeni Özellik Eklerken

### Query (Read) Ekleme
- [ ] Query Orchestrator'a metod ekle
- [ ] Service'de cache yönetimi ekle
- [ ] Controller endpoint'i ekle
- [ ] Test yaz

### Command (Write) Ekleme
- [ ] Business Rule'ları tanımla ve oluştur
- [ ] Command Orchestrator oluştur
- [ ] Service'de orchestrator kullan
- [ ] Controller endpoint'i ekle
- [ ] Cache invalidation ekle
- [ ] Test yaz

## 🎓 Best Practices

### ✅ DO
1. Service'de sadece orchestrator koordinasyonu yap
2. Tüm veri erişimi orchestrator'da olsun
3. Business rules'ları tek sorumluluklu tut
4. Orchestrator'ları NEW ile oluştur
5. Cache yönetimini service'de tut

### ❌ DON'T
1. Service'de direkt UnitOfWork kullanma
2. Business logic'i service'e koyma
3. Transaction yönetimini service'de yapma
4. Entity-DTO mapping'i service'de yapma
5. Orchestrator'ları DI ile inject etme

## 🎯 Sonuç

**Temiz Mimari** ile:
- ✅ Her katman tek sorumluluğa sahip
- ✅ Test edilebilirlik maksimum
- ✅ Kod tekrarı minimum
- ✅ Bakımı kolay
- ✅ Genişletilebilir
- ✅ SOLID prensiplerine uygun

**Akış**:
```
API → Service → Orchestrator → UnitOfWork → Repository → DB
     (Cache)   (Business)     (Data Access)  (Queries)
```

---

**Clean Code = Happy Developers! 🎉**

