# Query Orchestrators Refactoring

## Özet
Query klasöründeki orchestrator'lar artık `IOrchestrator<TInput, TOutput>` interface'ini implement ediyor. Bu refactoring ile CQRS pattern'ine daha uygun bir yapı elde edildi.

## Değişiklikler

### ❌ Silinen Dosyalar
- `GetCategoriesOrchestrator.cs` - IOrchestrator implement etmiyordu
- `GetProductsOrchestrator.cs` - IOrchestrator implement etmiyordu

### ✅ Oluşturulan Category Query Orchestrators

1. **GetAllCategoriesOrchestrator**
   - Input: `object?` (parametresiz)
   - Output: `IEnumerable<CategoryDto>`
   - Tüm kategorileri ürünleriyle birlikte getirir

2. **GetCategoryByIdOrchestrator**
   - Input: `int` (categoryId)
   - Output: `CategoryDto?`
   - Belirli bir kategoriyi ID'ye göre getirir

3. **GetActiveCategoriesOrchestrator**
   - Input: `object?` (parametresiz)
   - Output: `IEnumerable<CategoryDto>`
   - Sadece aktif kategorileri getirir

4. **SearchCategoriesOrchestrator**
   - Input: `string` (searchTerm)
   - Output: `IEnumerable<CategoryDto>`
   - Kategorileri isme göre arar
   - Validation: searchTerm boş olamaz

5. **GetCategoryProductCountOrchestrator**
   - Input: `int` (categoryId)
   - Output: `int`
   - Kategorideki ürün sayısını getirir

### ✅ Oluşturulan Product Query Orchestrators

1. **GetAllProductsOrchestrator**
   - Input: `object?` (parametresiz)
   - Output: `IEnumerable<ProductDto>`
   - Tüm ürünleri kategorileriyle birlikte getirir

2. **GetProductByIdOrchestrator**
   - Input: `int` (productId)
   - Output: `ProductDto?`
   - Belirli bir ürünü ID'ye göre getirir

3. **GetProductsByCategoryOrchestrator**
   - Input: `int` (categoryId)
   - Output: `IEnumerable<ProductDto>`
   - Belirli bir kategorideki ürünleri getirir

4. **GetActiveProductsOrchestrator**
   - Input: `object?` (parametresiz)
   - Output: `IEnumerable<ProductDto>`
   - Sadece aktif ürünleri getirir

5. **GetProductsByPriceRangeOrchestrator**
   - Input: `PriceRangeQuery` (MinPrice, MaxPrice)
   - Output: `IEnumerable<ProductDto>`
   - Fiyat aralığına göre ürünleri getirir
   - Validation: Fiyatlar negatif olamaz, MinPrice > MaxPrice olamaz

6. **GetLowStockProductsOrchestrator**
   - Input: `int` (threshold)
   - Output: `IEnumerable<ProductDto>`
   - Stoku eşik değerinin altında olan ürünleri getirir
   - Validation: threshold negatif olamaz

7. **SearchProductsOrchestrator**
   - Input: `string` (searchTerm)
   - Output: `IEnumerable<ProductDto>`
   - Ürünleri isme göre arar
   - Validation: searchTerm boş olamaz

8. **GetProductBySkuOrchestrator**
   - Input: `string` (sku)
   - Output: `ProductDto?`
   - Ürünü SKU'ya göre getirir
   - Validation: SKU boş olamaz

## Yapısal İyileştirmeler

### 1. IOrchestrator Implementation
Tüm query orchestrator'lar artık `IOrchestrator<TInput, TOutput>` interface'ini implement ediyor ve `ExecuteAsync` metodunu kullanıyor.

### 2. OrchestratorResult Dönüşü
Her orchestrator standardize edilmiş bir sonuç döndürüyor:
- `SuccessResult`: Başarılı işlemler için
- `FailureResult`: Hata durumları için
- `ValidationFailure`: Validasyon hataları için

### 3. Exception Handling
Tüm orchestrator'larda try-catch blokları ile exception handling yapıldı.

### 4. Validation
Uygun orchestrator'lara input validation eklendi.

### 5. CQRS Pattern Uyumu
Her query operasyonu için ayrı orchestrator oluşturularak CQRS (Command Query Responsibility Segregation) pattern'ine uyum sağlandı.

## Kullanım Örneği

### Eski Yöntem (Silindi)
```csharp
var orchestrator = new GetCategoriesOrchestrator(unitOfWork);
var categories = await orchestrator.GetAllAsync();
```

### Yeni Yöntem
```csharp
var orchestrator = new GetAllCategoriesOrchestrator(unitOfWork);
var result = await orchestrator.ExecuteAsync(null);

if (result.Success)
{
    var categories = result.Data;
    // İşlemler...
}
else
{
    // Hata yönetimi
    var error = result.ErrorMessage;
}
```

## Faydalar

1. ✅ **Standartlaşma**: Tüm orchestrator'lar aynı interface'i kullanıyor
2. ✅ **Hata Yönetimi**: Merkezi ve tutarlı hata yönetimi
3. ✅ **Validation**: Input validasyonu
4. ✅ **CQRS**: Her sorgu için ayrı orchestrator
5. ✅ **Bakım Kolaylığı**: Tek sorumluluk prensibi (Single Responsibility)
6. ✅ **Test Edilebilirlik**: Her orchestrator izole şekilde test edilebilir

## Not
Controller'lar ve Service'ler bu yeni orchestrator'ları kullanacak şekilde güncellenmelidir.

