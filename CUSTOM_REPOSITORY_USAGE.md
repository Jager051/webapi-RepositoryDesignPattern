# Custom Repository Pattern - Kullanım Kılavuzu

Bu proje artık her entity için özelleştirilmiş repository'ler içermektedir. Bu sayede her entity için özel sorgular yazabilir ve UnitOfWork üzerinden erişebilirsiniz.

## 📁 Proje Yapısı

### Interface'ler (WebAPI.Core/Interfaces)
- `IRepository<T>` - Generic repository interface (temel CRUD işlemleri)
- `IProductRepository` - Product için özel repository interface
- `ICategoryRepository` - Category için özel repository interface
- `IUserRepository` - User için özel repository interface
- `IUnitOfWork` - Tüm repository'leri bir araya getiren interface

### Implementasyonlar (WebAPI.Infrastructure/Repositories)
- `GenericRepository<T>` - Generic repository implementasyonu
- `ProductRepository` - Product için özel repository implementasyonu
- `CategoryRepository` - Category için özel repository implementasyonu
- `UserRepository` - User için özel repository implementasyonu
- `UnitOfWork` - Tüm repository'leri yöneten sınıf

## 🎯 Kullanım Örnekleri

### 1. Service Class'ında Kullanım

```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Özel repository metodunu kullanma
    public async Task<IEnumerable<Product>> GetActiveProducts()
    {
        return await _unitOfWork.Products.GetActiveProductsAsync();
    }

    // Generic repository metodunu kullanma
    public async Task<Product?> GetProduct(int id)
    {
        return await _unitOfWork.Products.GetByIdAsync(id);
    }

    // Birden fazla repository kullanma
    public async Task<bool> CreateProduct(Product product)
    {
        // Kategori kontrolü
        var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == product.CategoryId);
        if (!categoryExists)
            return false;

        // SKU benzersizliği kontrolü
        var isSkuUnique = await _unitOfWork.Products.IsSkuUniqueAsync(product.SKU);
        if (!isSkuUnique)
            return false;

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
```

### 2. Controller'da Kullanım

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string term)
    {
        var products = await _unitOfWork.Products.SearchProductsByNameAsync(term);
        return Ok(products);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
        return Ok(products);
    }
}
```

## 🔧 Özel Repository Metodları

### IProductRepository
```csharp
// Kategoriye göre ürünler (Category dahil)
await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);

// Aktif ürünler
await _unitOfWork.Products.GetActiveProductsAsync();

// Fiyat aralığına göre ürünler
await _unitOfWork.Products.GetProductsByPriceRangeAsync(minPrice, maxPrice);

// Düşük stoklu ürünler
await _unitOfWork.Products.GetLowStockProductsAsync(threshold);

// Ürün arama
await _unitOfWork.Products.SearchProductsByNameAsync(searchTerm);

// SKU ile ürün bulma
await _unitOfWork.Products.GetProductBySkuAsync(sku);

// Tüm ürünler (Category dahil)
await _unitOfWork.Products.GetProductsWithCategoryAsync();

// SKU benzersizlik kontrolü
await _unitOfWork.Products.IsSkuUniqueAsync(sku, excludeProductId);
```

### ICategoryRepository
```csharp
// Aktif kategoriler
await _unitOfWork.Categories.GetActiveCategoriesAsync();

// Kategoriler ve ürünleri
await _unitOfWork.Categories.GetCategoriesWithProductsAsync();

// Kategori ve ürünleri (tekil)
await _unitOfWork.Categories.GetCategoryWithProductsAsync(categoryId);

// Kategori arama
await _unitOfWork.Categories.SearchCategoriesByNameAsync(searchTerm);

// Kategori adı benzersizlik kontrolü
await _unitOfWork.Categories.IsCategoryNameUniqueAsync(name, excludeCategoryId);

// Kategorideki ürün sayısı
await _unitOfWork.Categories.GetProductCountByCategoryAsync(categoryId);
```

### IUserRepository
```csharp
// Kullanıcı adına göre kullanıcı bulma
await _unitOfWork.Users.GetByUsernameAsync(username);

// E-posta ile kullanıcı bulma
await _unitOfWork.Users.GetByEmailAsync(email);

// Aktif kullanıcılar
await _unitOfWork.Users.GetActiveUsersAsync();

// Kullanıcı adı benzersizlik kontrolü
await _unitOfWork.Users.IsUsernameUniqueAsync(username, excludeUserId);

// E-posta benzersizlik kontrolü
await _unitOfWork.Users.IsEmailUniqueAsync(email, excludeUserId);

// Kullanıcı arama
await _unitOfWork.Users.SearchUsersByNameAsync(searchTerm);
```

## 💡 Transaction Kullanımı

```csharp
public async Task<bool> ComplexOperation()
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        // İlk işlem
        var category = new Category { Name = "New Category" };
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        // İkinci işlem
        var product = new Product 
        { 
            Name = "New Product",
            CategoryId = category.Id 
        };
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
```

## 🚀 Yeni Repository Ekleme

Yeni bir entity için özel repository eklemek için:

1. **Interface oluşturun** (`WebAPI.Core/Interfaces/IYourEntityRepository.cs`):
```csharp
public interface IYourEntityRepository : IRepository<YourEntity>
{
    Task<IEnumerable<YourEntity>> YourCustomMethodAsync();
}
```

2. **Repository implementasyonu** (`WebAPI.Infrastructure/Repositories/YourEntityRepository.cs`):
```csharp
public class YourEntityRepository : GenericRepository<YourEntity>, IYourEntityRepository
{
    public YourEntityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<YourEntity>> YourCustomMethodAsync()
    {
        return await _dbSet.Where(x => x.SomeCondition).ToListAsync();
    }
}
```

3. **IUnitOfWork'e ekleyin**:
```csharp
public interface IUnitOfWork : IDisposable
{
    // Mevcut repository'ler...
    IYourEntityRepository YourEntities { get; }
}
```

4. **UnitOfWork implementasyonuna ekleyin**:
```csharp
private IYourEntityRepository? _yourEntityRepository;

public IYourEntityRepository YourEntities
{
    get
    {
        _yourEntityRepository ??= new YourEntityRepository(_context);
        return _yourEntityRepository;
    }
}
```

## ✅ Avantajlar

1. **Temiz Kod**: Her entity için özel sorgular organize edilmiş
2. **Type Safety**: Compile-time type kontrolü
3. **Yeniden Kullanılabilirlik**: Generic metodlar + özel metodlar
4. **Performans**: Include/Join optimizasyonları özel metodlarda
5. **Test Edilebilirlik**: Interface'ler sayesinde kolay mock'lama
6. **Maintainability**: Değişiklikler tek bir yerde yapılır

## 📌 Notlar

- Generic `Repository<T>()` metodu hala kullanılabilir
- Lazy initialization performans için optimize edilmiş
- Tüm custom metodlar async/await kullanıyor
- EF Core Include() metodları ile ilişkili veriler otomatik yüklenir

