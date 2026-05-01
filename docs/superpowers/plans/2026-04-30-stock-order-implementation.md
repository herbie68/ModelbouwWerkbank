# Stock Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the new `StockOrder` screen in the real `Modelbouwer` WPF workspace with a new-first order flow, editable existing orders, closed-order read-only behavior, product popup handling, and real persistence against `supplyorder`, `supplyorderline`, and `productsupplier`.

**Architecture:** Keep inventory logic in `StockService` and introduce a dedicated `StockOrderService` for order header and line persistence. Use a dedicated `StockOrderViewModel` instead of `EntityPageViewModel<T>`, because the screen owns multiple collections and a mixed in-memory/persisted workflow. Reuse `ProductService` and `SupplierService` where possible, and add only the extra supplier-product methods needed for popup lookup and upsert.

**Tech Stack:** WPF, MVVM, CommunityToolkit.Mvvm, Syncfusion grids/controls, MySQL via `MySql.Data`, MSTest + Moq.

---

## File Structure

**Create**
- `Modelbouwer/Interfaces/IStockOrderService.cs`
- `Modelbouwer/Models/StockOrderModel.cs`
- `Modelbouwer/Models/StockOrderLineModel.cs`
- `Modelbouwer/Models/StockOrderProductDialogModel.cs`
- `Modelbouwer/Services/StockOrderService.cs`
- `Modelbouwer/ViewModels/StockOrderProductDialogViewModel.cs`
- `Modelbouwer/Views/StockOrderProductDialog.xaml`
- `Modelbouwer/Views/StockOrderProductDialog.xaml.cs`
- `Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs`
- `Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs`
- `Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs`

**Modify**
- `Modelbouwer/App.xaml.cs`
- `Modelbouwer/Interfaces/ISupplierService.cs`
- `Modelbouwer/Services/SupplierService.cs`
- `Modelbouwer/ViewModels/StockOrderViewModel.cs`
- `Modelbouwer/Views/StockOrderView.xaml`
- `Modelbouwer/Views/StockOrderView.xaml.cs`
- `Modelbouwer.UnitTests/GlobalUsings.cs`
- `Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs`

**Leave untouched**
- `Modelbouwer/Models/OrderHeaderModel.cs`
- `Modelbouwer/Models/OrderLineModel.cs`

Those older order models are too narrow for the new screen and should not be retrofitted during this feature.

---

### Task 1: Repair The Unit Test Baseline So New StockOrder Tests Can Run

**Files:**
- Modify: `Modelbouwer.UnitTests/GlobalUsings.cs`
- Modify: `Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs`

- [ ] **Step 1: Capture the current failing baseline**

Run:

```powershell
dotnet restore Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore
```

Expected:
- Restore succeeds.
- Test compile fails before any `StockOrder` work, currently because `Mock<>` is unresolved and the product page test constructor is out of sync with production code.

- [ ] **Step 2: Add the missing global Moq import**

Update `Modelbouwer.UnitTests/GlobalUsings.cs` to include:

```csharp
global using Moq;
global using Modelbouwer.Exceptions;
global using Modelbouwer.Helpers;
global using Modelbouwer.Interfaces;
global using Modelbouwer.Models;
global using Modelbouwer.Services;
global using Modelbouwer.ViewModels;
global using Modelbouwer.Views;
```

- [ ] **Step 3: Bring `ProductPageViewModelTests` back in sync with the current constructor**

Update the setup block in `Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs` to create the missing mocks and pass them into `ProductPageViewModel`:

```csharp
private Mock<IStorageLocationService> _mockStorageLocationService;
private Mock<ISupplierService> _mockSupplierService;

[TestInitialize]
public void Setup()
{
    _mockProductService = new Mock<IProductService>();
    _mockUnitService = new Mock<IUnitService>();
    _mockBrandService = new Mock<IBrandService>();
    _mockCategoryService = new Mock<ICategoryService>();
    _mockStorageLocationService = new Mock<IStorageLocationService>();
    _mockSupplierService = new Mock<ISupplierService>();
    _mockValidator = new Mock<IEntityValidator<ProductModel>>();

    _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<ProductModel>());
    _mockUnitService.Setup(s => s.GetAllUnitsAsync()).ReturnsAsync(new List<UnitModel>());
    _mockBrandService.Setup(s => s.GetAllBrandsAsync()).ReturnsAsync(new List<BrandModel>());
    _mockCategoryService.Setup(s => s.GetAllCategorysAsync()).ReturnsAsync(new List<CategoryModel>());
    _mockStorageLocationService.Setup(s => s.GetAllStorageLocationsAsync()).ReturnsAsync(new List<StorageLocationModel>());
    _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(new List<SupplierModel>());
    _mockSupplierService.Setup(s => s.GetAllProductSuppliersAsync()).ReturnsAsync(new List<Modelbouwer.Model.ProductSupplierModel>());

    _viewModel = new ProductPageViewModel(
        _mockProductService.Object,
        _mockUnitService.Object,
        _mockBrandService.Object,
        _mockCategoryService.Object,
        _mockStorageLocationService.Object,
        _mockSupplierService.Object,
        _mockValidator.Object);
}
```

- [ ] **Step 4: Re-run the focused baseline tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductPageViewModelTests"
```

Expected:
- The test project compiles.
- `ProductPageViewModelTests` runs far enough that new `StockOrder` tests can be added with confidence.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer.UnitTests/GlobalUsings.cs Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs
git commit -m "test: restore unit test baseline for current viewmodel constructors"
```

---

### Task 2: Introduce Dedicated Stock Order Models And Service Contracts

**Files:**
- Create: `Modelbouwer/Interfaces/IStockOrderService.cs`
- Create: `Modelbouwer/Models/StockOrderModel.cs`
- Create: `Modelbouwer/Models/StockOrderLineModel.cs`
- Create: `Modelbouwer/Models/StockOrderProductDialogModel.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs`

- [ ] **Step 1: Write the first failing test for popup fallback behavior**

Create `Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs` with:

```csharp
namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockOrderProductDialogViewModelTests
{
    [TestMethod]
    public void CreateFromProductAndEmptyProductSupplier_FallsBackToProductValues()
    {
        var product = new ProductModel
        {
            ProductId = 5,
            ProductCode = "P-005",
            ProductName = "Wheel Set",
            ProductPrice = 12.5,
            ProductStandardQuantity = 3
        };

        var supplier = new SupplierModel
        {
            Id = 11,
            CurrencyId = 2
        };

        var model = StockOrderProductDialogModel.Create(product, supplier, null);

        Assert.AreEqual(5, model.ProductId);
        Assert.AreEqual(11, model.SupplierId);
        Assert.AreEqual("P-005", model.SupplierProductNumber);
        Assert.AreEqual("Wheel Set", model.SupplierProductName);
        Assert.AreEqual(12.5, model.UnitPrice);
        Assert.AreEqual(3, model.Amount);
    }
}
```

- [ ] **Step 2: Run the new test to verify it fails**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderProductDialogViewModelTests"
```

Expected:
- FAIL because `StockOrderProductDialogModel` does not exist yet.

- [ ] **Step 3: Add the dedicated models and contract**

Create `Modelbouwer/Models/StockOrderModel.cs`:

```csharp
namespace Modelbouwer.Models;

public partial class StockOrderModel : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private int _supplierId;
    [ObservableProperty] private int _currencyId;
    [ObservableProperty] private string? _supplierName;
    [ObservableProperty] private string? _currencySymbol;
    [ObservableProperty] private string? _orderNumber;
    [ObservableProperty] private DateTime? _orderDate;
    [ObservableProperty] private double _shippingCosts;
    [ObservableProperty] private double _orderCosts;
    [ObservableProperty] private string? _memo;
    [ObservableProperty] private bool _closed;
    [ObservableProperty] private DateTime? _closedDate;
    [ObservableProperty] private bool _hasStockLog;

    public double LinesTotal { get; set; }
    public double GrandTotal => Math.Round(LinesTotal + ShippingCosts + OrderCosts, 2, MidpointRounding.AwayFromZero);
}
```

Create `Modelbouwer/Models/StockOrderLineModel.cs`:

```csharp
namespace Modelbouwer.Models;

public partial class StockOrderLineModel : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private int _supplyOrderId;
    [ObservableProperty] private int _supplierId;
    [ObservableProperty] private int _productId;
    [ObservableProperty] private string? _productCode;
    [ObservableProperty] private string? _productName;
    [ObservableProperty] private string? _supplierProductName;
    [ObservableProperty] private double _amount;
    [ObservableProperty] private double _openAmount;
    [ObservableProperty] private double _price;
    [ObservableProperty] private double _realRowTotal;
    [ObservableProperty] private double _received;
    [ObservableProperty] private double _expected;
    [ObservableProperty] private bool _closed;
    [ObservableProperty] private DateTime? _closedDate;
}
```

Create `Modelbouwer/Models/StockOrderProductDialogModel.cs`:

```csharp
namespace Modelbouwer.Models;

public partial class StockOrderProductDialogModel : ObservableObject
{
    [ObservableProperty] private int _productSupplierId;
    [ObservableProperty] private int _supplierId;
    [ObservableProperty] private int _productId;
    [ObservableProperty] private int _currencyId;
    [ObservableProperty] private string? _productCode;
    [ObservableProperty] private string? _productName;
    [ObservableProperty] private string? _supplierProductNumber;
    [ObservableProperty] private string? _supplierProductName;
    [ObservableProperty] private string? _productUrl;
    [ObservableProperty] private double _unitPrice;
    [ObservableProperty] private double _amount;
    [ObservableProperty] private bool _productSupplierExists;

    public double RowTotal => Math.Round(UnitPrice * Amount, 2, MidpointRounding.AwayFromZero);

    public static StockOrderProductDialogModel Create(
        ProductModel product,
        SupplierModel supplier,
        Modelbouwer.Model.ProductSupplierModel? productSupplier)
    {
        return new StockOrderProductDialogModel
        {
            ProductSupplierId = productSupplier?.ProductSupplierId ?? 0,
            ProductSupplierExists = productSupplier != null,
            ProductId = product.ProductId,
            SupplierId = supplier.Id,
            CurrencyId = productSupplier?.CurrencyId > 0 ? productSupplier.CurrencyId : supplier.CurrencyId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            SupplierProductNumber = string.IsNullOrWhiteSpace(productSupplier?.ProductNumber) ? product.ProductCode : productSupplier.ProductNumber,
            SupplierProductName = string.IsNullOrWhiteSpace(productSupplier?.ProductName) ? product.ProductName : productSupplier.ProductName,
            ProductUrl = productSupplier?.URL ?? string.Empty,
            UnitPrice = productSupplier is { Price: > 0 } ? productSupplier.Price : product.ProductPrice,
            Amount = product.ProductStandardQuantity > 0 ? product.ProductStandardQuantity : 1
        };
    }
}
```

Create `Modelbouwer/Interfaces/IStockOrderService.cs`:

```csharp
namespace Modelbouwer.Interfaces;

public interface IStockOrderService
{
    Task<List<StockOrderModel>> GetAllOrdersAsync();
    Task<List<StockOrderLineModel>> GetOrderLinesAsync(int orderId);
    Task<int> InsertOrderAsync(StockOrderModel order);
    Task UpdateOrderAsync(StockOrderModel order);
    Task DeleteOrderAsync(int orderId);
    Task<int> InsertOrderLineAsync(StockOrderLineModel line);
    Task UpdateOrderLineAsync(StockOrderLineModel line);
    Task DeleteOrderLineAsync(int lineId);
}
```

- [ ] **Step 4: Re-run the fallback test**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderProductDialogViewModelTests"
```

Expected:
- PASS.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/Interfaces/IStockOrderService.cs Modelbouwer/Models/StockOrderModel.cs Modelbouwer/Models/StockOrderLineModel.cs Modelbouwer/Models/StockOrderProductDialogModel.cs Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs
git commit -m "feat: add stock order models and service contract"
```

---

### Task 3: Implement `StockOrderService` For Real Header And Line Persistence

**Files:**
- Create: `Modelbouwer/Services/StockOrderService.cs`
- Modify: `Modelbouwer/App.xaml.cs`
- Test: `Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs`

- [ ] **Step 1: Write the failing service tests**

Create `Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs` with:

```csharp
using System.Data.Common;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class StockOrderServiceTests
{
    private Mock<GenericDataService> _mockDataService = null!;
    private StockOrderService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockDataService = new Mock<GenericDataService>();
        _service = new StockOrderService(_mockDataService.Object);
    }

    [TestMethod]
    public async Task GetAllOrdersAsync_ReturnsMappedOrders()
    {
        _mockDataService
            .Setup(s => s.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Func<DbDataReader, StockOrderModel>>(), null))
            .ReturnsAsync(new List<StockOrderModel> { new() { Id = 9, OrderNumber = "SO-9", Closed = true } });

        var result = await _service.GetAllOrdersAsync();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(9, result[0].Id);
        Assert.AreEqual("SO-9", result[0].OrderNumber);
        Assert.IsTrue(result[0].Closed);
    }

    [TestMethod]
    public async Task InsertOrderLineAsync_PassesOpenAmountEqualToAmount()
    {
        var line = new StockOrderLineModel
        {
            SupplyOrderId = 4,
            SupplierId = 7,
            ProductId = 12,
            SupplierProductName = "Axle",
            Amount = 5,
            Price = 3.5,
            RealRowTotal = 17.5
        };

        _mockDataService
            .Setup(s => s.ExecuteScalarAsync<uint>(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(44u);

        var result = await _service.InsertOrderLineAsync(line);

        Assert.AreEqual(44, result);

        _mockDataService.Verify(s => s.ExecuteScalarAsync<uint>(
            It.IsAny<string>(),
            It.Is<Dictionary<string, object>>(p =>
                (double)p[$"@{DBNames.OrderLineFieldNameAmount}"] == 5d &&
                (double)p[$"@{DBNames.OrderLineFieldNameOpenAmount}"] == 5d)),
            Times.Once);
    }
}
```

- [ ] **Step 2: Run the service tests to verify they fail**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderServiceTests"
```

Expected:
- FAIL because `StockOrderService` does not exist yet.

- [ ] **Step 3: Implement the service and register it in DI**

Create `Modelbouwer/Services/StockOrderService.cs`:

```csharp
namespace Modelbouwer.Services;

public class StockOrderService : IStockOrderService
{
    private readonly GenericDataService _dataService;

    public StockOrderService(GenericDataService dataService)
    {
        _dataService = dataService;
    }

    public string CompleteOrderListQuery = $@"
SELECT
    {DBNames.OrderViewFieldNameId},
    {DBNames.OrderViewFieldNameSupplierId},
    {DBNames.OrderViewFieldNameSupplierName},
    {DBNames.OrderViewFieldNameCurrencyId},
    {DBNames.OrderViewFieldNameCurrencySymbol},
    {DBNames.OrderViewFieldNameOrderNumber},
    {DBNames.OrderViewFieldNameOrderDate},
    {DBNames.OrderViewFieldNameOrderShippingCosts},
    {DBNames.OrderViewFieldNameOrderOrderCosts},
    {DBNames.OrderViewFieldNameClosed},
    {DBNames.OrderViewFieldNameClosedDate},
    {DBNames.OrderViewFieldNameOrderMemo},
    {DBNames.OrderViewFieldNameHasStackLog}
FROM {DBNames.Database}.{DBNames.OrderView}
ORDER BY {DBNames.OrderViewFieldNameOrderDate} DESC, {DBNames.OrderViewFieldNameId} DESC;";

    public string OrderLinesQuery = $@"
SELECT
    {DBNames.OrderLineViewFieldNameOrderId},
    {DBNames.OrderLineViewFieldNameProductId},
    {DBNames.OrderLineViewFieldNameProductCode},
    {DBNames.OrderLineViewFieldNameProductName},
    {DBNames.OrderLineFieldNameId},
    {DBNames.OrderLineFieldNameSupplierId},
    {DBNames.OrderLineFieldNameSupplierProductName},
    {DBNames.OrderLineFieldNameAmount},
    {DBNames.OrderLineFieldNameOpenAmount},
    {DBNames.OrderLineFieldNamePrice},
    {DBNames.OrderLineFieldNameRealRowTotal},
    {DBNames.OrderLineViewFieldNameReceived},
    {DBNames.OrderLineViewFieldNameExpected},
    {DBNames.OrderLineViewFieldNameClosed},
    {DBNames.OrderLineViewFieldNameClosedDate}
FROM {DBNames.Database}.{DBNames.OrderLineView}
WHERE {DBNames.OrderLineViewFieldNameOrderId} = @OrderId
ORDER BY {DBNames.OrderLineFieldNameId};";

    public string InsertOrderQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.OrderTable} (
    {DBNames.OrderFieldNameSupplierId},
    {DBNames.OrderFieldNameCurrencyId},
    {DBNames.OrderFieldNameOrderNumber},
    {DBNames.OrderFieldNameOrderDate},
    {DBNames.OrderFieldNameShippingCosts},
    {DBNames.OrderFieldNameOrderCosts},
    {DBNames.OrderFieldNameOrderMemo},
    {DBNames.OrderFieldNameClosed},
    {DBNames.OrderFieldNameClosedDate}
) VALUES (
    @{DBNames.OrderFieldNameSupplierId},
    @{DBNames.OrderFieldNameCurrencyId},
    @{DBNames.OrderFieldNameOrderNumber},
    @{DBNames.OrderFieldNameOrderDate},
    @{DBNames.OrderFieldNameShippingCosts},
    @{DBNames.OrderFieldNameOrderCosts},
    @{DBNames.OrderFieldNameOrderMemo},
    @{DBNames.OrderFieldNameClosed},
    @{DBNames.OrderFieldNameClosedDate}
);
{DBNames.SqlSelectLastId}";

    public string UpdateOrderQuery = $@"
UPDATE {DBNames.Database}.{DBNames.OrderTable}
SET
    {DBNames.OrderFieldNameSupplierId} = @{DBNames.OrderFieldNameSupplierId},
    {DBNames.OrderFieldNameCurrencyId} = @{DBNames.OrderFieldNameCurrencyId},
    {DBNames.OrderFieldNameOrderNumber} = @{DBNames.OrderFieldNameOrderNumber},
    {DBNames.OrderFieldNameOrderDate} = @{DBNames.OrderFieldNameOrderDate},
    {DBNames.OrderFieldNameShippingCosts} = @{DBNames.OrderFieldNameShippingCosts},
    {DBNames.OrderFieldNameOrderCosts} = @{DBNames.OrderFieldNameOrderCosts},
    {DBNames.OrderFieldNameOrderMemo} = @{DBNames.OrderFieldNameOrderMemo},
    {DBNames.OrderFieldNameClosed} = @{DBNames.OrderFieldNameClosed},
    {DBNames.OrderFieldNameClosedDate} = @{DBNames.OrderFieldNameClosedDate}
WHERE {DBNames.OrderFieldNameId} = @{DBNames.OrderFieldNameId};";

    public string DeleteOrderQuery = $@"
DELETE FROM {DBNames.Database}.{DBNames.OrderTable}
WHERE {DBNames.OrderFieldNameId} = @{DBNames.OrderFieldNameId};";

    public string InsertOrderLineQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.OrderLineTable} (
    {DBNames.OrderLineFieldNameSupplierOrderId},
    {DBNames.OrderLineFieldNameSupplierId},
    {DBNames.OrderLineFieldNameProductId},
    {DBNames.OrderLineFieldNameSupplierProductName},
    {DBNames.OrderLineFieldNameAmount},
    {DBNames.OrderLineFieldNameOpenAmount},
    {DBNames.OrderLineFieldNamePrice},
    {DBNames.OrderLineFieldNameRealRowTotal},
    {DBNames.OrderLineFieldNameClosed},
    {DBNames.OrderLineFieldNameClosedDate}
) VALUES (
    @{DBNames.OrderLineFieldNameSupplierOrderId},
    @{DBNames.OrderLineFieldNameSupplierId},
    @{DBNames.OrderLineFieldNameProductId},
    @{DBNames.OrderLineFieldNameSupplierProductName},
    @{DBNames.OrderLineFieldNameAmount},
    @{DBNames.OrderLineFieldNameOpenAmount},
    @{DBNames.OrderLineFieldNamePrice},
    @{DBNames.OrderLineFieldNameRealRowTotal},
    @{DBNames.OrderLineFieldNameClosed},
    @{DBNames.OrderLineFieldNameClosedDate}
);
{DBNames.SqlSelectLastId}";
```

Also add the CRUD methods with small parameter helpers:

```csharp
private static Dictionary<string, object> CreateOrderParameters(StockOrderModel order) => new()
{
    [$"@{DBNames.OrderFieldNameId}"] = order.Id,
    [$"@{DBNames.OrderFieldNameSupplierId}"] = order.SupplierId,
    [$"@{DBNames.OrderFieldNameCurrencyId}"] = order.CurrencyId,
    [$"@{DBNames.OrderFieldNameOrderNumber}"] = order.OrderNumber ?? string.Empty,
    [$"@{DBNames.OrderFieldNameOrderDate}"] = order.OrderDate,
    [$"@{DBNames.OrderFieldNameShippingCosts}"] = order.ShippingCosts,
    [$"@{DBNames.OrderFieldNameOrderCosts}"] = order.OrderCosts,
    [$"@{DBNames.OrderFieldNameOrderMemo}"] = order.Memo ?? string.Empty,
    [$"@{DBNames.OrderFieldNameClosed}"] = order.Closed ? 1 : 0,
    [$"@{DBNames.OrderFieldNameClosedDate}"] = order.ClosedDate
};

private static Dictionary<string, object> CreateOrderLineParameters(StockOrderLineModel line) => new()
{
    [$"@{DBNames.OrderLineFieldNameId}"] = line.Id,
    [$"@{DBNames.OrderLineFieldNameSupplierOrderId}"] = line.SupplyOrderId,
    [$"@{DBNames.OrderLineFieldNameSupplierId}"] = line.SupplierId,
    [$"@{DBNames.OrderLineFieldNameProductId}"] = line.ProductId,
    [$"@{DBNames.OrderLineFieldNameSupplierProductName}"] = line.SupplierProductName ?? string.Empty,
    [$"@{DBNames.OrderLineFieldNameAmount}"] = line.Amount,
    [$"@{DBNames.OrderLineFieldNameOpenAmount}"] = line.OpenAmount,
    [$"@{DBNames.OrderLineFieldNamePrice}"] = line.Price,
    [$"@{DBNames.OrderLineFieldNameRealRowTotal}"] = line.RealRowTotal,
    [$"@{DBNames.OrderLineFieldNameClosed}"] = line.Closed ? 1 : 0,
    [$"@{DBNames.OrderLineFieldNameClosedDate}"] = line.ClosedDate
};
```

Register the service in `Modelbouwer/App.xaml.cs`:

```csharp
services.AddSingleton<StockOrderService>();
services.AddScoped<IStockOrderService, StockOrderService>();
```

- [ ] **Step 4: Re-run the service tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderServiceTests"
```

Expected:
- PASS.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/Services/StockOrderService.cs Modelbouwer/App.xaml.cs Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs
git commit -m "feat: add stock order persistence service"
```

---

### Task 4: Extend `SupplierService` For Supplier-Product Lookup And Upsert

**Files:**
- Modify: `Modelbouwer/Interfaces/ISupplierService.cs`
- Modify: `Modelbouwer/Services/SupplierService.cs`
- Test: `Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs`

- [ ] **Step 1: Write the next failing supplier-product tests**

Append to `Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs`:

```csharp
[TestMethod]
public async Task GetProductSupplierAsync_ReturnsNullWhenNoMatchExists()
{
    var supplierService = new SupplierService(_mockDataService.Object);

    _mockDataService
        .Setup(s => s.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Func<DbDataReader, Modelbouwer.Model.ProductSupplierModel>>(), It.IsAny<Dictionary<string, object>>()))
        .ReturnsAsync(new List<Modelbouwer.Model.ProductSupplierModel>());

    var result = await supplierService.GetProductSupplierAsync(3, 8);

    Assert.IsNull(result);
}
```

- [ ] **Step 2: Run the supplier-product test to verify it fails**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~GetProductSupplierAsync"
```

Expected:
- FAIL because `GetProductSupplierAsync` does not exist.

- [ ] **Step 3: Add the new interface methods and SQL**

Update `Modelbouwer/Interfaces/ISupplierService.cs`:

```csharp
Task<Modelbouwer.Model.ProductSupplierModel?> GetProductSupplierAsync(int supplierId, int productId);
Task<int> InsertProductSupplierAsync(Dictionary<string, object?> queryParameters);
Task UpdateProductSupplierAsync(Dictionary<string, object?> queryParameters);
```

Add to `Modelbouwer/Services/SupplierService.cs`:

```csharp
public string ProductSupplierBySupplierAndProductQuery = $@"
SELECT
    ps.{DBNames.ProductSupplierFieldNameId},
    ps.{DBNames.ProductSupplierFieldNameProductId},
    ps.{DBNames.ProductSupplierFieldNameSupplierId},
    s.{DBNames.SupplierFieldNameName},
    ps.{DBNames.ProductSupplierFieldNameCurrencyId},
    c.{DBNames.CurrencyFieldNameSymbol},
    ps.{DBNames.ProductSupplierFieldNameProductNumber},
    (CASE ps.{DBNames.ProductSupplierFieldNameProductName} WHEN '' THEN p.{DBNames.ProductFieldNameName} ELSE ps.{DBNames.ProductSupplierFieldNameProductName} END) AS ProductName,
    ps.{DBNames.ProductSupplierFieldNamePrice},
    ps.{DBNames.ProductSupplierFieldNameProductUrl},
    CASE WHEN ps.{DBNames.ProductSupplierFieldNameDefaultSupplier} = '*' THEN 1 ELSE 0 END AS {DBNames.ProductSupplierFieldNameDefaultSupplier}
FROM {DBNames.Database}.{DBNames.ProductSupplierTable} ps
LEFT JOIN {DBNames.Database}.{DBNames.ProductTable} p ON ps.{DBNames.ProductSupplierFieldNameProductId} = p.{DBNames.ProductFieldNameId}
LEFT JOIN {DBNames.Database}.{DBNames.SupplierTable} s ON ps.{DBNames.ProductSupplierFieldNameSupplierId} = s.{DBNames.SupplierFieldNameId}
LEFT JOIN {DBNames.Database}.{DBNames.CurrencyTable} c ON ps.{DBNames.ProductSupplierFieldNameCurrencyId} = c.{DBNames.CurrencyFieldNameId}
WHERE ps.{DBNames.ProductSupplierFieldNameSupplierId} = @SupplierId
  AND ps.{DBNames.ProductSupplierFieldNameProductId} = @ProductId;";

public string InsertProductSupplierQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.ProductSupplierTable} (
    {DBNames.ProductSupplierFieldNameProductId},
    {DBNames.ProductSupplierFieldNameSupplierId},
    {DBNames.ProductSupplierFieldNameCurrencyId},
    {DBNames.ProductSupplierFieldNameProductNumber},
    {DBNames.ProductSupplierFieldNameProductName},
    {DBNames.ProductSupplierFieldNamePrice},
    {DBNames.ProductSupplierFieldNameProductUrl},
    {DBNames.ProductSupplierFieldNameDefaultSupplier}
) VALUES (
    @{DBNames.ProductSupplierFieldNameProductId},
    @{DBNames.ProductSupplierFieldNameSupplierId},
    @{DBNames.ProductSupplierFieldNameCurrencyId},
    @{DBNames.ProductSupplierFieldNameProductNumber},
    @{DBNames.ProductSupplierFieldNameProductName},
    @{DBNames.ProductSupplierFieldNamePrice},
    @{DBNames.ProductSupplierFieldNameProductUrl},
    ''
);
{DBNames.SqlSelectLastId}";

public string UpdateProductSupplierQuery = $@"
UPDATE {DBNames.Database}.{DBNames.ProductSupplierTable}
SET
    {DBNames.ProductSupplierFieldNameCurrencyId} = @{DBNames.ProductSupplierFieldNameCurrencyId},
    {DBNames.ProductSupplierFieldNameProductNumber} = @{DBNames.ProductSupplierFieldNameProductNumber},
    {DBNames.ProductSupplierFieldNameProductName} = @{DBNames.ProductSupplierFieldNameProductName},
    {DBNames.ProductSupplierFieldNamePrice} = @{DBNames.ProductSupplierFieldNamePrice},
    {DBNames.ProductSupplierFieldNameProductUrl} = @{DBNames.ProductSupplierFieldNameProductUrl}
WHERE {DBNames.ProductSupplierFieldNameId} = @{DBNames.ProductSupplierFieldNameId};";
```

Implement:

```csharp
public async Task<Modelbouwer.Model.ProductSupplierModel?> GetProductSupplierAsync(int supplierId, int productId)
{
    var parameters = new Dictionary<string, object>
    {
        ["@SupplierId"] = supplierId,
        ["@ProductId"] = productId
    };

    var results = await _dataService.ExecuteQueryAsync(ProductSupplierBySupplierAndProductQuery, reader => new ProductSupplierModel
    {
        ProductSupplierId = DatabaseValueConverter.GetInt(reader[$"{DBNames.ProductSupplierFieldNameId}"]),
        ProductId = DatabaseValueConverter.GetInt(reader[$"{DBNames.ProductSupplierFieldNameProductId}"]),
        SupplierId = DatabaseValueConverter.GetInt(reader[$"{DBNames.ProductSupplierFieldNameSupplierId}"]),
        SupplierName = DatabaseValueConverter.GetString(reader[$"{DBNames.SupplierFieldNameName}"]),
        CurrencyId = DatabaseValueConverter.GetInt(reader[$"{DBNames.SupplierFieldNameCurrencyId}"]),
        CurrencySymbol = DatabaseValueConverter.GetString(reader[$"{DBNames.CurrencyFieldNameSymbol}"]),
        ProductNumber = DatabaseValueConverter.GetString(reader[$"{DBNames.ProductSupplierFieldNameProductNumber}"]),
        ProductName = DatabaseValueConverter.GetString(reader["ProductName"]),
        Price = DatabaseValueConverter.GetDouble(reader[$"{DBNames.ProductSupplierFieldNamePrice}"]),
        URL = DatabaseValueConverter.GetString(reader[$"{DBNames.ProductSupplierFieldNameProductUrl}"]),
        DefaultSupplier = DatabaseValueConverter.GetInt(reader[$"{DBNames.ProductSupplierFieldNameDefaultSupplier}"]) == 1
    }, parameters);

    return results.FirstOrDefault();
}
```

- [ ] **Step 4: Re-run the supplier-product tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~GetProductSupplierAsync"
```

Expected:
- PASS.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/Interfaces/ISupplierService.cs Modelbouwer/Services/SupplierService.cs Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs
git commit -m "feat: add supplier product lookup and upsert support for stock orders"
```

---

### Task 5: Implement The New-First `StockOrderViewModel`

**Files:**
- Modify: `Modelbouwer/ViewModels/StockOrderViewModel.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs`

- [ ] **Step 1: Write the failing viewmodel tests for the core workflow**

Create `Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs` with:

```csharp
namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockOrderViewModelTests
{
    private Mock<IStockOrderService> _mockStockOrderService = null!;
    private Mock<IProductService> _mockProductService = null!;
    private Mock<ISupplierService> _mockSupplierService = null!;
    private StockOrderViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockStockOrderService = new Mock<IStockOrderService>();
        _mockProductService = new Mock<IProductService>();
        _mockSupplierService = new Mock<ISupplierService>();

        _mockStockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(new List<StockOrderModel>());
        _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<ProductModel>());
        _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(new List<SupplierModel>());
        _mockSupplierService.Setup(s => s.GetAllCurrenciesAsync()).ReturnsAsync(new List<CurrencyModel>());

        _viewModel = new StockOrderViewModel(
            _mockStockOrderService.Object,
            _mockProductService.Object,
            _mockSupplierService.Object);
    }

    [TestMethod]
    public void Constructor_StartsInNewOrderMode()
    {
        Assert.IsTrue(_viewModel.IsNewOrder);
        Assert.IsNotNull(_viewModel.EditableOrder);
        Assert.AreEqual(0, _viewModel.EditableOrder.Id);
    }

    [TestMethod]
    public async Task SaveOrderAsync_NewOrder_InsertsHeaderBeforeLines()
    {
        _viewModel.EditableOrder.SupplierId = 4;
        _viewModel.EditableOrder.CurrencyId = 2;
        _viewModel.EditableOrder.OrderNumber = "SO-2026-01";
        _viewModel.EditableOrder.OrderDate = new DateTime(2026, 4, 30);
        _viewModel.PendingOrderLines.Add(new StockOrderLineModel
        {
            ProductId = 10,
            SupplierId = 4,
            Amount = 3,
            OpenAmount = 3,
            Price = 9.5,
            RealRowTotal = 28.5,
            SupplierProductName = "Wheel Set"
        });

        _mockStockOrderService.Setup(s => s.InsertOrderAsync(It.IsAny<StockOrderModel>())).ReturnsAsync(88);
        _mockStockOrderService.Setup(s => s.InsertOrderLineAsync(It.IsAny<StockOrderLineModel>())).ReturnsAsync(101);
        _mockStockOrderService.Setup(s => s.GetOrderLinesAsync(88)).ReturnsAsync(new List<StockOrderLineModel>());

        await _viewModel.SaveOrderAsync();

        _mockStockOrderService.Verify(s => s.InsertOrderAsync(It.IsAny<StockOrderModel>()), Times.Once);
        _mockStockOrderService.Verify(s => s.InsertOrderLineAsync(It.Is<StockOrderLineModel>(l => l.SupplyOrderId == 88)), Times.Once);
    }

    [TestMethod]
    public void SelectClosedOrder_SetsCanEditOrderToFalse()
    {
        var closedOrder = new StockOrderModel { Id = 12, Closed = true };

        _viewModel.ApplySelectedOrder(closedOrder, new List<StockOrderLineModel>());

        Assert.IsTrue(_viewModel.IsClosedOrder);
        Assert.IsFalse(_viewModel.CanEditOrder);
    }
}
```

- [ ] **Step 2: Run the new viewmodel tests to verify they fail**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderViewModelTests"
```

Expected:
- FAIL because the current `StockOrderViewModel` is empty.

- [ ] **Step 3: Implement the dedicated viewmodel**

Replace `Modelbouwer/ViewModels/StockOrderViewModel.cs` with:

```csharp
using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class StockOrderViewModel : ObservableObject
{
    private readonly IStockOrderService _stockOrderService;
    private readonly IProductService _productService;
    private readonly ISupplierService _supplierService;

    public ObservableCollection<StockOrderModel> Orders { get; } = [];
    public ObservableCollection<StockOrderLineModel> OrderLines { get; } = [];
    public ObservableCollection<StockOrderLineModel> PendingOrderLines { get; } = [];
    public ObservableCollection<ProductModel> AvailableProducts { get; } = [];
    public ObservableCollection<SupplierModel> Suppliers { get; } = [];
    public ObservableCollection<CurrencyModel> Currencies { get; } = [];

    [ObservableProperty] private StockOrderModel _editableOrder = new();
    [ObservableProperty] private StockOrderModel? _selectedOrder;
    [ObservableProperty] private StockOrderLineModel? _selectedOrderLine;
    [ObservableProperty] private ProductModel? _selectedProduct;
    [ObservableProperty] private SupplierModel? _selectedSupplier;
    [ObservableProperty] private CurrencyModel? _selectedCurrency;
    [ObservableProperty] private bool _isNewOrder = true;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasUnsavedChanges;
    [ObservableProperty] private bool _enableSupplierOrderFilter;

    public bool IsClosedOrder => EditableOrder.Closed;
    public bool CanEditOrder => !IsBusy && !IsClosedOrder;
    public ObservableCollection<StockOrderLineModel> VisibleOrderLines => IsNewOrder ? PendingOrderLines : OrderLines;

    public IRelayCommand NewOrderCommand { get; }
    public IAsyncRelayCommand SaveOrderCommand { get; }
    public IAsyncRelayCommand DeleteOrderCommand { get; }
    public IRelayCommand ResetOrderCommand { get; }
    public IAsyncRelayCommand AddProductToOrderCommand { get; }
    public IAsyncRelayCommand EditOrderLineCommand { get; }
    public IAsyncRelayCommand DeleteOrderLineCommand { get; }

    public StockOrderViewModel(
        IStockOrderService stockOrderService,
        IProductService productService,
        ISupplierService supplierService)
    {
        _stockOrderService = stockOrderService;
        _productService = productService;
        _supplierService = supplierService;

        NewOrderCommand = new RelayCommand(BeginNewOrder);
        SaveOrderCommand = new AsyncRelayCommand(SaveOrderAsync);
        DeleteOrderCommand = new AsyncRelayCommand(DeleteOrderAsync);
        ResetOrderCommand = new RelayCommand(ResetOrder);
        AddProductToOrderCommand = new AsyncRelayCommand(AddSelectedProductAsync);
        EditOrderLineCommand = new AsyncRelayCommand(EditSelectedOrderLineAsync);
        DeleteOrderLineCommand = new AsyncRelayCommand(DeleteSelectedOrderLineAsync);

        BeginNewOrder();
        _ = InitializeAsync();
    }
```

Add the workflow methods:

```csharp
public async Task InitializeAsync()
{
    await LoadReferenceDataAsync();
    await LoadOrdersAsync();
}

public void BeginNewOrder()
{
    EditableOrder = new StockOrderModel
    {
        Id = 0,
        OrderDate = DateTime.Today
    };

    SelectedOrder = null;
    SelectedOrderLine = null;
    PendingOrderLines.Clear();
    OrderLines.Clear();
    IsNewOrder = true;
    HasUnsavedChanges = false;
    RefreshLookupsFromEditableOrder();
    OnPropertyChanged(nameof(IsClosedOrder));
    OnPropertyChanged(nameof(CanEditOrder));
    OnPropertyChanged(nameof(VisibleOrderLines));
}

public void ApplySelectedOrder(StockOrderModel order, IEnumerable<StockOrderLineModel> lines)
{
    SelectedOrder = order;
    EditableOrder = new StockOrderModel
    {
        Id = order.Id,
        SupplierId = order.SupplierId,
        CurrencyId = order.CurrencyId,
        SupplierName = order.SupplierName,
        CurrencySymbol = order.CurrencySymbol,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        ShippingCosts = order.ShippingCosts,
        OrderCosts = order.OrderCosts,
        Memo = order.Memo,
        Closed = order.Closed,
        ClosedDate = order.ClosedDate,
        HasStockLog = order.HasStockLog
    };

    OrderLines.Clear();
    foreach (var line in lines)
        OrderLines.Add(line);

    PendingOrderLines.Clear();
    IsNewOrder = false;
    HasUnsavedChanges = false;
    RefreshLookupsFromEditableOrder();
    RecalculateTotals();
    OnPropertyChanged(nameof(IsClosedOrder));
    OnPropertyChanged(nameof(CanEditOrder));
    OnPropertyChanged(nameof(VisibleOrderLines));
}

public async Task SaveOrderAsync()
{
    var validationMessage = ValidateOrderForSave();
    if (validationMessage != null)
    {
        MessageBox.Show(validationMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    if (IsNewOrder)
    {
        var newOrderId = await _stockOrderService.InsertOrderAsync(EditableOrder);
        EditableOrder.Id = newOrderId;

        foreach (var line in PendingOrderLines)
        {
            line.SupplyOrderId = newOrderId;
            await _stockOrderService.InsertOrderLineAsync(line);
        }

        var lines = await _stockOrderService.GetOrderLinesAsync(newOrderId);
        await LoadOrdersAsync();
        ApplySelectedOrder(Orders.First(o => o.Id == newOrderId), lines);
        return;
    }

    await _stockOrderService.UpdateOrderAsync(EditableOrder);
    await LoadOrdersAsync();
}
```

Use one pure validation helper so tests can cover logic without `MessageBox` coupling:

```csharp
internal string? ValidateOrderForSave()
{
    if (EditableOrder.Closed)
        return "Closed orders can not be saved.";

    if (EditableOrder.SupplierId <= 0)
        return "Supplier is verplicht.";

    if (string.IsNullOrWhiteSpace(EditableOrder.OrderNumber))
        return "Ordernummer is verplicht.";

    if (EditableOrder.OrderDate == null)
        return "Besteldatum is verplicht.";

    return null;
}
```

- [ ] **Step 4: Re-run the viewmodel tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderViewModelTests"
```

Expected:
- PASS.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/ViewModels/StockOrderViewModel.cs Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs
git commit -m "feat: implement stock order page viewmodel workflow"
```

---

### Task 6: Build The Product Popup Workflow And Hook It To Order Lines

**Files:**
- Create: `Modelbouwer/ViewModels/StockOrderProductDialogViewModel.cs`
- Create: `Modelbouwer/Views/StockOrderProductDialog.xaml`
- Create: `Modelbouwer/Views/StockOrderProductDialog.xaml.cs`
- Modify: `Modelbouwer/ViewModels/StockOrderViewModel.cs`
- Modify: `Modelbouwer/App.xaml.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs`

- [ ] **Step 1: Write the failing dialog confirmation test**

Append to `Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs`:

```csharp
[TestMethod]
public void ConfirmCommand_WithPositiveAmountAndPrice_CompletesSuccessfully()
{
    var model = new StockOrderProductDialogModel
    {
        ProductId = 5,
        SupplierId = 11,
        SupplierProductName = "Wheel Set",
        SupplierProductNumber = "P-005",
        UnitPrice = 12.5,
        Amount = 2
    };

    var vm = new StockOrderProductDialogViewModel(model);

    var confirmed = vm.TryConfirm(out var errorMessage);

    Assert.IsTrue(confirmed);
    Assert.IsNull(errorMessage);
    Assert.AreEqual(25.0, vm.Model.RowTotal);
}
```

- [ ] **Step 2: Run the dialog test to verify it fails**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderProductDialogViewModelTests"
```

Expected:
- FAIL because `StockOrderProductDialogViewModel` does not exist.

- [ ] **Step 3: Implement the dialog viewmodel, dialog view, and StockOrder integration**

Create `Modelbouwer/ViewModels/StockOrderProductDialogViewModel.cs`:

```csharp
namespace Modelbouwer.ViewModels;

public partial class StockOrderProductDialogViewModel : ObservableObject
{
    public StockOrderProductDialogModel Model { get; }

    public StockOrderProductDialogViewModel(StockOrderProductDialogModel model)
    {
        Model = model;
    }

    public bool TryConfirm(out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(Model.SupplierProductName))
        {
            errorMessage = "Supplier product name is verplicht.";
            return false;
        }

        if (Model.Amount <= 0)
        {
            errorMessage = "Aantal moet groter zijn dan nul.";
            return false;
        }

        if (Model.UnitPrice <= 0)
        {
            errorMessage = "Prijs moet groter zijn dan nul.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
```

Create `Modelbouwer/Views/StockOrderProductDialog.xaml.cs`:

```csharp
namespace Modelbouwer.Views;

public partial class StockOrderProductDialog : Window
{
    public StockOrderProductDialog(StockOrderProductDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ConfirmClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not StockOrderProductDialogViewModel vm)
            return;

        if (!vm.TryConfirm(out var errorMessage))
        {
            MessageBox.Show(errorMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }
}
```

Create `Modelbouwer/Views/StockOrderProductDialog.xaml` with fields for:
- `SupplierProductNumber`
- `SupplierProductName`
- `UnitPrice`
- `Amount`
- read-only `RowTotal`

Use bindings like:

```xml
<TextBox Text="{Binding Model.SupplierProductNumber, UpdateSourceTrigger=PropertyChanged}" />
<TextBox Text="{Binding Model.SupplierProductName, UpdateSourceTrigger=PropertyChanged}" />
<TextBox Text="{Binding Model.UnitPrice, UpdateSourceTrigger=PropertyChanged, StringFormat={}{0:N4}}" />
<TextBox Text="{Binding Model.Amount, UpdateSourceTrigger=PropertyChanged, StringFormat={}{0:N2}}" />
<TextBox IsReadOnly="True" Text="{Binding Model.RowTotal, StringFormat={}{0:N2}}" />
```

Then hook the popup into `StockOrderViewModel`:

```csharp
private async Task AddSelectedProductAsync()
{
    if (!CanEditOrder || SelectedProduct == null)
        return;

    var supplier = Suppliers.FirstOrDefault(s => s.Id == EditableOrder.SupplierId);
    if (supplier == null)
    {
        MessageBox.Show("Selecteer eerst een leverancier.", Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    var existingProductSupplier = await _supplierService.GetProductSupplierAsync(supplier.Id, SelectedProduct.ProductId);
    var dialogModel = StockOrderProductDialogModel.Create(SelectedProduct, supplier, existingProductSupplier);
    var dialogVm = new StockOrderProductDialogViewModel(dialogModel);
    var dialog = new StockOrderProductDialog(dialogVm);

    if (dialog.ShowDialog() != true)
        return;

    await UpsertProductSupplierAsync(dialogVm.Model);

    var line = new StockOrderLineModel
    {
        SupplyOrderId = EditableOrder.Id,
        SupplierId = supplier.Id,
        ProductId = SelectedProduct.ProductId,
        ProductCode = SelectedProduct.ProductCode,
        ProductName = SelectedProduct.ProductName,
        SupplierProductName = dialogVm.Model.SupplierProductName,
        Amount = dialogVm.Model.Amount,
        OpenAmount = dialogVm.Model.Amount,
        Price = dialogVm.Model.UnitPrice,
        RealRowTotal = dialogVm.Model.RowTotal
    };

    if (IsNewOrder)
        PendingOrderLines.Add(line);
    else
        line.Id = await _stockOrderService.InsertOrderLineAsync(line);

    RecalculateTotals();
    HasUnsavedChanges = true;
}
```

- [ ] **Step 4: Re-run the dialog tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrderProductDialogViewModelTests"
```

Expected:
- PASS.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/ViewModels/StockOrderProductDialogViewModel.cs Modelbouwer/Views/StockOrderProductDialog.xaml Modelbouwer/Views/StockOrderProductDialog.xaml.cs Modelbouwer/ViewModels/StockOrderViewModel.cs Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs
git commit -m "feat: add stock order product popup workflow"
```

---

### Task 7: Rebuild `StockOrderView` And Wire It Like The Other Screens

**Files:**
- Modify: `Modelbouwer/Views/StockOrderView.xaml`
- Modify: `Modelbouwer/Views/StockOrderView.xaml.cs`
- Modify: `Modelbouwer/App.xaml.cs`

- [ ] **Step 1: Write the compile-time target for the view wiring**

Use the existing app build as the failing guard:

```powershell
dotnet build Modelbouwer/Modelbouwer.csproj
```

Expected:
- The build will fail or the screen will remain functionally incomplete until the XAML and constructor wiring match the new viewmodel.

- [ ] **Step 2: Update the view constructor to match the established project pattern**

Replace `Modelbouwer/Views/StockOrderView.xaml.cs` with:

```csharp
namespace Modelbouwer.Views;

public partial class StockOrderView : UserControl
{
    public StockOrderView(StockOrderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

- [ ] **Step 3: Rebuild the XAML around the final layout**

Replace the current content in `Modelbouwer/Views/StockOrderView.xaml` with a layout that contains:
- top toolbar: `Nieuw`, `Opslaan`, `Verwijderen`, `Reset`
- tabbed detail block: `Orderinformatie`, `Orderregels`, `Memo`
- order grid for existing orders
- product grid at the bottom for catalog selection

Use bindings like:

```xml
<Button Command="{Binding NewOrderCommand}" Content="Nieuw" />
<Button Command="{Binding SaveOrderCommand}" Content="Opslaan" />
<Button Command="{Binding DeleteOrderCommand}" Content="Verwijderen" />
<Button Command="{Binding ResetOrderCommand}" Content="Reset" />

<syncfusion:ComboBoxAdv
    ItemsSource="{Binding Suppliers}"
    DisplayMemberPath="Name"
    SelectedValuePath="Id"
    SelectedValue="{Binding EditableOrder.SupplierId, Mode=TwoWay}" />

<DatePicker
    SelectedDate="{Binding EditableOrder.OrderDate, Mode=TwoWay}" />

<syncfusion:ComboBoxAdv
    ItemsSource="{Binding Currencies}"
    DisplayMemberPath="CurrencyName"
    SelectedValuePath="CurrencyId"
    SelectedValue="{Binding EditableOrder.CurrencyId, Mode=TwoWay}" />

<TextBox Text="{Binding EditableOrder.OrderNumber, UpdateSourceTrigger=PropertyChanged}" />
<TextBox Text="{Binding EditableOrder.ShippingCosts, UpdateSourceTrigger=PropertyChanged, StringFormat={}{0:N2}}" />
<TextBox Text="{Binding EditableOrder.OrderCosts, UpdateSourceTrigger=PropertyChanged, StringFormat={}{0:N2}}" />
<CheckBox IsChecked="{Binding EditableOrder.Closed}" IsEnabled="False" />
```

Make the editability explicit:

```xml
<Grid IsEnabled="{Binding CanEditOrder}">
```

Use the order lines grid against:

```xml
<sfGrid:SfDataGrid ItemsSource="{Binding VisibleOrderLines}" SelectedItem="{Binding SelectedOrderLine, Mode=TwoWay}" />
```

Use the existing orders grid against:

```xml
<sfGrid:SfDataGrid ItemsSource="{Binding Orders}" SelectedItem="{Binding SelectedOrder, Mode=TwoWay}" />
```

Use the products grid against:

```xml
<sfGrid:SfDataGrid ItemsSource="{Binding AvailableProducts}" SelectedItem="{Binding SelectedProduct, Mode=TwoWay}" />
<Button Command="{Binding AddProductToOrderCommand}" Content="Product toevoegen" />
```

- [ ] **Step 4: Build the WPF project**

Run:

```powershell
dotnet build Modelbouwer/Modelbouwer.csproj
```

Expected:
- `Build succeeded`.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/Views/StockOrderView.xaml Modelbouwer/Views/StockOrderView.xaml.cs
git commit -m "feat: rebuild stock order screen and wire viewmodel injection"
```

---

### Task 8: Final Verification, Manual UX Pass, And Supplier Filter Bonus

**Files:**
- Modify: `Modelbouwer/ViewModels/StockOrderViewModel.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs`

- [ ] **Step 1: Add the optional supplier filter behavior**

Extend `StockOrderViewModel` with:

```csharp
partial void OnEnableSupplierOrderFilterChanged(bool value)
{
    ApplySupplierFilter();
}

private List<StockOrderModel> _allOrders = [];

private void ApplySupplierFilter()
{
    Orders.Clear();

    var filtered = _allOrders.AsEnumerable();

    if (EnableSupplierOrderFilter && EditableOrder.SupplierId > 0)
        filtered = filtered.Where(o => o.SupplierId == EditableOrder.SupplierId);

    foreach (var order in filtered)
        Orders.Add(order);
}
```

Update `LoadOrdersAsync()` so it fills `_allOrders` first and then calls `ApplySupplierFilter()`.

- [ ] **Step 2: Add the final viewmodel test for supplier filtering**

Append to `Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs`:

```csharp
[TestMethod]
public void ApplySupplierFilter_WhenEnabled_ShowsOnlyMatchingSupplierOrders()
{
    _viewModel.EditableOrder.SupplierId = 5;
    _viewModel.ReplaceOrdersForTest(new List<StockOrderModel>
    {
        new() { Id = 1, SupplierId = 5, OrderNumber = "SO-1" },
        new() { Id = 2, SupplierId = 8, OrderNumber = "SO-2" }
    });

    _viewModel.EnableSupplierOrderFilter = true;

    Assert.AreEqual(1, _viewModel.Orders.Count);
    Assert.AreEqual(5, _viewModel.Orders[0].SupplierId);
}
```

Add a tiny test hook in the viewmodel:

```csharp
internal void ReplaceOrdersForTest(IEnumerable<StockOrderModel> orders)
{
    _allOrders = orders.ToList();
    ApplySupplierFilter();
}
```

- [ ] **Step 3: Run all focused StockOrder tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrder"
```

Expected:
- PASS for `StockOrderServiceTests`
- PASS for `StockOrderViewModelTests`
- PASS for `StockOrderProductDialogViewModelTests`

- [ ] **Step 4: Manual verification in the running app**

Run:

```powershell
dotnet run --project Modelbouwer/Modelbouwer.csproj
```

Manual checklist:
- Open `StockOrder` from navigation.
- Confirm the detail form opens empty in new-order mode.
- Enter supplier, currency, order number, order date.
- Add a product through the popup and confirm a new line appears only in-memory.
- Save the new order and confirm the header and lines are persisted and reloaded.
- Select an existing open order and edit memo or costs, then save.
- Select a closed order and confirm all edit areas are disabled.
- Verify the popup loads existing `productsupplier` data when present.
- Verify the popup falls back to `product` values when supplier-specific fields are empty.
- Verify a missing `productsupplier` record is created on popup confirm.
- Toggle the supplier filter and confirm the order grid narrows to matching supplier orders.

- [ ] **Step 5: Commit**

```bash
git add Modelbouwer/ViewModels/StockOrderViewModel.cs Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs
git commit -m "feat: finish stock order verification and supplier filter"
```

---

## Self-Review

**Spec coverage**
- New-first screen: covered in Task 5 (`BeginNewOrder`, delayed header insert until `SaveOrderAsync`).
- Existing order selection and editing: covered in Task 5 (`ApplySelectedOrder`) and Task 7 (existing orders grid).
- Closed orders read-only: covered in Task 5 (`CanEditOrder`) and Task 7 (`IsEnabled="{Binding CanEditOrder}"`).
- Tabs `Orderinformatie`, `Orderregels`, `Memo`: covered in Task 7.
- Order grid and product grid: covered in Task 7.
- Product popup and productsupplier-first fallback: covered in Task 2 and Task 6.
- Create missing `productsupplier` on confirm and update immediately: covered in Task 4 and Task 6.
- `DefaultSupplier` out of scope: respected by leaving it untouched except storing `''` on insert.
- Save new order only on explicit save: covered in Task 5.
- Supplier filter as bonus: covered in Task 8.

**Placeholder scan**
- No `TODO`, `TBD`, or “handle later” instructions remain in the implementation tasks.

**Type consistency**
- `StockOrderModel`, `StockOrderLineModel`, and `StockOrderProductDialogModel` are referenced consistently across service, popup, and screen tasks.
- `IStockOrderService` is the only new service contract for order persistence.
- `ISupplierService` only gains the extra `productsupplier` methods needed by the popup workflow.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-04-30-stock-order-implementation.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
