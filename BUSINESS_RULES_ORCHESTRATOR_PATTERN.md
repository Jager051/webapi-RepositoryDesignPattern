# 🎯 Business Rules & Orchestrator Pattern

Bu dokümanda **Business Rules Pattern** ve **Orchestrator Pattern** implementasyonları açıklanmaktadır.

## 🌟 Neden Bu Katmanlar?

### ❌ Önceki Durum
```csharp
public async Task<ProductDto> CreateProduct(CreateProductDto dto)
{
    // Service içinde iş kuralları dağınık
    if (string.IsNullOrEmpty(dto.SKU))
        throw new Exception("SKU required");
    
    var existing = await _repo.FindBySku(dto.SKU);
    if (existing != null)
        throw new Exception("SKU exists");
    
    if (dto.Price <= 0)
        throw new Exception("Invalid price");
    
    // Business logic karmaşık ve test edilmesi zor
}
```

### ✅ Yeni Durum
```csharp
public async Task<ProductDto> CreateProduct(CreateProductDto dto)
{
    // Orchestrator tüm business logic'i yönetiyor
    var orchestrator = new ProductOrchestrator(_unitOfWork);
    var result = await orchestrator.ExecuteAsync(dto);
    
    if (!result.Success)
        throw new InvalidOperationException(result.ErrorMessage);
    
    return result.Data!;
}
```

## 🏗️ Mimari Yapı

```
┌─────────────────────────────────────────┐
│         Controllers (API Layer)         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Services (Business Logic)          │
│  - ProductService                       │
│  - CategoryService                      │
│                                         │
│  Orchestrator'ları NEW ile oluşturur    │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Orchestrators Layer             │
│  ┌─────────────────────────────────┐   │
│  │ ProductOrchestrator             │   │
│  │ UpdateProductOrchestrator       │   │
│  │ DeleteCategoryOrchestrator      │   │
│  │                                 │   │
│  │ Business Rules'ları NEW ile     │   │
│  │ oluşturur ve yönetir            │   │
│  └─────────────────────────────────┘   │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│        Business Rules Layer             │
│  ┌─────────────────────────────────┐   │
│  │ ProductSkuMustBeUniqueRule      │   │
│  │ ProductPriceMustBeValidRule     │   │
│  │ CategoryNameMustBeUniqueRule    │   │
│  │ ...                             │   │
│  │                                 │   │
│  │ Sadece validation yapar         │   │
│  └─────────────────────────────────┘   │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      UnitOfWork + Repositories          │
└─────────────────────────────────────────┘
```

## 📋 Business Rules Pattern

### Interface Tanımı

```csharp
public interface IBusinessRule<T>
{
    Task<BusinessRuleResult> ValidateAsync(T entity);
}

public class BusinessRuleResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
}
```

### Product Business Rules

#### 1. SKU Benzersizlik Kontrolü
```csharp
public class ProductSkuMustBeUniqueRule : IBusinessRule<Product>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductSkuMustBeUniqueRule(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessRuleResult> ValidateAsync(Product product)
    {
        var isUnique = await _unitOfWork.Products
            .IsSkuUniqueAsync(product.SKU, product.Id);
        
        if (!isUnique)
        {
            return BusinessRuleResult.Failure(
                $"Product with SKU '{product.SKU}' already exists", 
                "PRODUCT_SKU_DUPLICATE");
        }

        return BusinessRuleResult.Success();
    }
}
```

#### 2. Kategori Geçerliliği
```csharp
public class ProductMustHaveValidCategoryRule : IBusinessRule<Product>
{
    // Kategorinin var olduğunu ve aktif olduğunu kontrol eder
}
```

#### 3. Fiyat Geçerliliği
```csharp
public class ProductPriceMustBeValidRule : IBusinessRule<Product>
{
    // Fiyatın 0.01 ile 999999.99 arasında olduğunu kontrol eder
    // UnitOfWork'e ihtiyaç yok, sadece validasyon
}
```

#### 4. Stok Geçerliliği
```csharp
public class ProductStockMustBeValidRule : IBusinessRule<Product>
{
    // Stok miktarının 0 ile 100000 arasında olduğunu kontrol eder
}
```

### Category Business Rules

#### 1. İsim Benzersizliği
```csharp
public class CategoryNameMustBeUniqueRule : IBusinessRule<Category>
{
    // Kategori isminin benzersiz olduğunu kontrol eder
}
```

#### 2. Silme Kuralı
```csharp
public class CategoryCannotBeDeletedWithProductsRule : IBusinessRule<Category>
{
    // Kategorinin ürünü varsa silinemez
}
```

## 🎭 Orchestrator Pattern

### Interface Tanımı

```csharp
public interface IOrchestrator<TInput, TOutput>
{
    Task<OrchestratorResult<TOutput>> ExecuteAsync(TInput input);
}

public class OrchestratorResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; }
}
```

### Product Creation Orchestrator

```csharp
public class ProductOrchestrator : IOrchestrator<CreateProductDto, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductOrchestrator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrchestratorResult<ProductDto>> ExecuteAsync(CreateProductDto input)
    {
        // 1. Map DTO to Entity
        var product = MapToEntity(input);

        // 2. Create business rules (NEW ile - sadece ihtiyaç olduğunda)
        var rules = new List<IBusinessRule<Product>>
        {
            new ProductSkuMustBeUniqueRule(_unitOfWork),
            new ProductMustHaveValidCategoryRule(_unitOfWork),
            new ProductPriceMustBeValidRule(),
            new ProductStockMustBeValidRule()
        };

        // 3. Validate all rules
        var validationErrors = new List<string>();
        foreach (var rule in rules)
        {
            var result = await rule.ValidateAsync(product);
            if (!result.IsValid)
                validationErrors.Add(result.ErrorMessage);
        }

        if (validationErrors.Any())
            return OrchestratorResult<ProductDto>.ValidationFailure(validationErrors);

        // 4. Execute with transaction
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return OrchestratorResult<ProductDto>.SuccessResult(MapToDto(product));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OrchestratorResult<ProductDto>.FailureResult(ex.Message);
        }
    }
}
```

## 💡 Kullanım Örnekleri

### Service'de Kullanım

```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Orchestrator'ı NEW ile oluştur - boşuna DI kullanma
        var orchestrator = new ProductOrchestrator(_unitOfWork);
        var result = await orchestrator.ExecuteAsync(dto);

        if (!result.Success)
        {
            var errorMessage = result.ValidationErrors.Any() 
                ? string.Join(", ", result.ValidationErrors)
                : result.ErrorMessage;
            throw new InvalidOperationException(errorMessage);
        }

        await _cacheService.RemoveByPatternAsync("products:*");
        return result.Data!;
    }
}
```

### Controller'da Kullanım

```csharp
[HttpPost]
public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
{
    try
    {
        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
    catch (InvalidOperationException ex)
    {
        // Business rule hatası
        return BadRequest(new { error = ex.Message });
    }
}
```

## 🎯 Avantajlar

### 1. Separation of Concerns
```
✅ Business Rules    → Sadece validation
✅ Orchestrator      → İş akışı yönetimi
✅ Service           → Koordinasyon ve cache
✅ Repository        → Veri erişimi
```

### 2. Yeniden Kullanılabilirlik
```csharp
// Aynı rule farklı yerlerde kullanılabilir
var skuRule = new ProductSkuMustBeUniqueRule(_unitOfWork);

// Create'de
await skuRule.ValidateAsync(newProduct);

// Update'de
await skuRule.ValidateAsync(existingProduct);
```

### 3. Test Edilebilirlik
```csharp
[Test]
public async Task ProductSku_MustBeUnique()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    mockUnitOfWork.Setup(x => x.Products.IsSkuUniqueAsync("SKU-001", null))
        .ReturnsAsync(false);
    
    var rule = new ProductSkuMustBeUniqueRule(mockUnitOfWork.Object);
    var product = new Product { SKU = "SKU-001" };

    // Act
    var result = await rule.ValidateAsync(product);

    // Assert
    Assert.IsFalse(result.IsValid);
    Assert.AreEqual("PRODUCT_SKU_DUPLICATE", result.ErrorCode);
}
```

### 4. Performans - NEW ile Oluşturma
```csharp
// ❌ DI ile - her request'te oluşturulur
public class ProductService
{
    private readonly ProductOrchestrator _orchestrator; // Boşuna memory
    
    public ProductService(ProductOrchestrator orchestrator)
    {
        _orchestrator = orchestrator; // Her zaman bellekte
    }
}

// ✅ NEW ile - sadece ihtiyaç olduğunda
public class ProductService
{
    public async Task<ProductDto> Create(CreateProductDto dto)
    {
        // Sadece create işleminde oluştur
        var orchestrator = new ProductOrchestrator(_unitOfWork);
        return await orchestrator.ExecuteAsync(dto);
    }
}
```

## 📊 Business Rules Listesi

### Product Rules
| Rule | Açıklama | UnitOfWork Gerekli |
|------|----------|-------------------|
| `ProductSkuMustBeUniqueRule` | SKU benzersizliği | ✅ |
| `ProductMustHaveValidCategoryRule` | Kategori kontrolü | ✅ |
| `ProductPriceMustBeValidRule` | Fiyat aralığı | ❌ |
| `ProductStockMustBeValidRule` | Stok limiti | ❌ |

### Category Rules
| Rule | Açıklama | UnitOfWork Gerekli |
|------|----------|-------------------|
| `CategoryNameMustBeUniqueRule` | İsim benzersizliği | ✅ |
| `CategoryCannotBeDeletedWithProductsRule` | Silme kontrolü | ✅ |

## 🚀 Yeni Business Rule Ekleme

### 1. Rule Class'ı Oluştur
```csharp
public class ProductNameMustNotContainBadWordsRule : IBusinessRule<Product>
{
    private readonly string[] _badWords = { "spam", "fake", "scam" };

    public Task<BusinessRuleResult> ValidateAsync(Product product)
    {
        var containsBadWord = _badWords.Any(word => 
            product.Name.Contains(word, StringComparison.OrdinalIgnoreCase));

        if (containsBadWord)
        {
            return Task.FromResult(BusinessRuleResult.Failure(
                "Product name contains inappropriate words", 
                "PRODUCT_NAME_INAPPROPRIATE"));
        }

        return Task.FromResult(BusinessRuleResult.Success());
    }
}
```

### 2. Orchestrator'a Ekle
```csharp
var rules = new List<IBusinessRule<Product>>
{
    new ProductSkuMustBeUniqueRule(_unitOfWork),
    new ProductMustHaveValidCategoryRule(_unitOfWork),
    new ProductPriceMustBeValidRule(),
    new ProductStockMustBeValidRule(),
    new ProductNameMustNotContainBadWordsRule() // ⭐ Yeni rule
};
```

### 3. Test Et
```csharp
[Test]
public async Task ProductName_MustNotContainBadWords()
{
    var rule = new ProductNameMustNotContainBadWordsRule();
    var product = new Product { Name = "Fake Product" };

    var result = await rule.ValidateAsync(product);

    Assert.IsFalse(result.IsValid);
}
```

## 🎓 Best Practices

### ✅ DO
```csharp
// Business rule'lar tek sorumluluklu
public class ProductSkuMustBeUniqueRule : IBusinessRule<Product>
{
    // Sadece SKU kontrolü yapar
}

// Orchestrator'ları service'de NEW ile oluştur
var orchestrator = new ProductOrchestrator(_unitOfWork);

// Hata mesajları açıklayıcı ve kodlu
return BusinessRuleResult.Failure(
    "Product with SKU 'ABC' already exists", 
    "PRODUCT_SKU_DUPLICATE");
```

### ❌ DON'T
```csharp
// Bir rule'da çok fazla sorumluluk
public class ProductValidationRule : IBusinessRule<Product>
{
    // SKU, Price, Stock, Category hepsini kontrol ediyor
    // ❌ Yanlış! Her biri ayrı rule olmalı
}

// Orchestrator'ları DI ile inject etme
public ProductService(ProductOrchestrator orchestrator)
{
    // ❌ Boşuna memory kullanımı
}

// Belirsiz hata mesajları
return BusinessRuleResult.Failure("Invalid", "ERROR");
// ❌ Ne hatası? Neden?
```

## 📈 Performans İyileştirmeleri

### 1. Lazy Loading Rules
```csharp
// Sadece gerekli rule'lar çalıştırılır
var rules = new List<IBusinessRule<Product>>();

if (!string.IsNullOrEmpty(product.SKU))
{
    rules.Add(new ProductSkuMustBeUniqueRule(_unitOfWork));
}

if (product.Price > 0)
{
    rules.Add(new ProductPriceMustBeValidRule());
}
```

### 2. Parallel Validation
```csharp
// Bağımsız rule'lar paralel çalışabilir
var tasks = rules.Select(rule => rule.ValidateAsync(product));
var results = await Task.WhenAll(tasks);

var errors = results.Where(r => !r.IsValid).Select(r => r.ErrorMessage);
```

## 🔍 Debugging Tips

```csharp
// Hangi rule'un fail ettiğini görmek için
foreach (var rule in rules)
{
    var result = await rule.ValidateAsync(product);
    if (!result.IsValid)
    {
        Console.WriteLine($"Rule: {rule.GetType().Name}");
        Console.WriteLine($"Error: {result.ErrorMessage}");
        Console.WriteLine($"Code: {result.ErrorCode}");
    }
}
```

## 📚 İlgili Dökümanlar

- [README.md](./README.md) - Ana dokümantasyon
- [CUSTOM_REPOSITORY_USAGE.md](./CUSTOM_REPOSITORY_USAGE.md) - Repository pattern
- [REPOSITORY_PATTERN_SUMMARY.md](./REPOSITORY_PATTERN_SUMMARY.md) - Mimari özet

---

**Sonuç**: Business Rules ve Orchestrator pattern'leri ile kodunuz daha modüler, test edilebilir ve bakımı kolay hale gelir! 🎯

