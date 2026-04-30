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

		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync() ).ReturnsAsync( new List<StockOrderModel>() );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( It.IsAny<int>() ) ).ReturnsAsync( new List<StockOrderLineModel>() );
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>() );
		_mockSupplierService.Setup( s => s.GetAllSuppliersAsync() ).ReturnsAsync( new List<SupplierModel>() );
		_mockSupplierService.Setup( s => s.GetAllCurrenciesAsync() ).ReturnsAsync( new List<CurrencyModel>() );

		_viewModel = new StockOrderViewModel(
			_mockStockOrderService.Object,
			_mockProductService.Object,
			_mockSupplierService.Object );
	}

	[TestMethod]
	public void Constructor_BeginsInNewOrderModeWithEmptyEditableOrder()
	{
		Assert.IsTrue( _viewModel.IsNewOrder );
		Assert.IsNotNull( _viewModel.EditableOrder );
		Assert.AreEqual( 0, _viewModel.EditableOrder.Id );
		Assert.IsNull( _viewModel.SelectedOrder );
		Assert.AreEqual( 0, _viewModel.VisibleOrderLines.Count );
	}

	[TestMethod]
	public void ApplySelectedOrder_CopiesOrderAndLoadsLines()
	{
		var order = new StockOrderModel
		{
			Id = 4,
			SupplierId = 11,
			OrderNumber = "SO-004",
			Closed = false
		};

		var lines = new List<StockOrderLineModel>
		{
			new() { Id = 8, SupplyOrderId = 4, ProductId = 5, Amount = 2, Price = 10, RealRowTotal = 20 }
		};

		_viewModel.ApplySelectedOrder( order, lines );

		Assert.IsFalse( _viewModel.IsNewOrder );
		Assert.IsNotNull( _viewModel.SelectedOrder );
		Assert.AreEqual( 4, _viewModel.EditableOrder.Id );
		Assert.AreEqual( "SO-004", _viewModel.EditableOrder.OrderNumber );
		Assert.AreEqual( 1, _viewModel.VisibleOrderLines.Count );
	}

	[TestMethod]
	public void ValidateOrderForSave_WithMissingSupplier_ReturnsMessage()
	{
		_viewModel.EditableOrder.OrderNumber = "SO-123";
		_viewModel.EditableOrder.OrderDate = DateTime.Today;
		_viewModel.EditableOrder.SupplierId = 0;

		var result = _viewModel.ValidateOrderForSave();

		Assert.AreEqual( "Supplier is verplicht.", result );
	}

	[TestMethod]
	public async Task SaveOrderAsync_WithNewOrder_InsertsOrderAndPendingLines()
	{
		_viewModel.EditableOrder.SupplierId = 11;
		_viewModel.EditableOrder.CurrencyId = 2;
		_viewModel.EditableOrder.OrderNumber = "SO-025";
		_viewModel.EditableOrder.OrderDate = DateTime.Today;

		_viewModel.PendingOrderLines.Add( new StockOrderLineModel
		{
			ProductId = 5,
			SupplierId = 11,
			Amount = 3,
			OpenAmount = 3,
			Price = 12.5,
			RealRowTotal = 37.5
		} );

		_mockStockOrderService.Setup( s => s.InsertOrderAsync( It.IsAny<StockOrderModel>() ) ).ReturnsAsync( 25 );
		_mockStockOrderService.Setup( s => s.InsertOrderLineAsync( It.IsAny<StockOrderLineModel>() ) ).ReturnsAsync( 40 );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel>
		{
			new() { Id = 40, SupplyOrderId = 25, ProductId = 5, Amount = 3, OpenAmount = 3, Price = 12.5, RealRowTotal = 37.5 }
		} );
		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync() ).ReturnsAsync( new List<StockOrderModel>
		{
			new() { Id = 25, SupplierId = 11, CurrencyId = 2, OrderNumber = "SO-025", OrderDate = DateTime.Today }
		} );

		await _viewModel.SaveOrderAsync();

		_mockStockOrderService.Verify( s => s.InsertOrderAsync( It.IsAny<StockOrderModel>() ), Times.Once );
		_mockStockOrderService.Verify( s => s.InsertOrderLineAsync( It.Is<StockOrderLineModel>( line => line.SupplyOrderId == 25 ) ), Times.Once );
		Assert.IsFalse( _viewModel.IsNewOrder );
		Assert.IsNotNull( _viewModel.SelectedOrder );
		Assert.AreEqual( 25, _viewModel.SelectedOrder.Id );
	}

	[TestMethod]
	public async Task AddSelectedProductAsync_WithNewOrder_AddsPendingLineAndUpsertsProductSupplier()
	{
		var supplier = new SupplierModel
		{
			Id = 11,
			Name = "Supplier 11",
			CurrencyId = 2
		};

		var product = new ProductModel
		{
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			ProductPrice = 12.5,
			ProductStandardQuantity = 3
		};

		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( supplier );
		_viewModel.EditableOrder.SupplierId = 11;
		_viewModel.SelectedProduct = product;
		_viewModel.ShowProductDialog = _ => true;

		_mockSupplierService
			.Setup( s => s.GetProductSupplierAsync( 11, 5 ) )
			.ReturnsAsync( ( Modelbouwer.Model.ProductSupplierModel? ) null );

		_mockSupplierService
			.Setup( s => s.UpsertProductSupplierAsync( It.IsAny<Modelbouwer.Model.ProductSupplierModel>() ) )
			.ReturnsAsync( 70 );

		await _viewModel.AddSelectedProductAsync();

		Assert.AreEqual( 1, _viewModel.PendingOrderLines.Count );
		Assert.AreEqual( "Wheel Set", _viewModel.PendingOrderLines[ 0 ].SupplierProductName );
		Assert.AreEqual( 3d, _viewModel.PendingOrderLines[ 0 ].Amount );
		Assert.AreEqual( 3d, _viewModel.PendingOrderLines[ 0 ].OpenAmount );
		Assert.AreEqual( 37.5, _viewModel.PendingOrderLines[ 0 ].RealRowTotal );
		_mockSupplierService.Verify( s => s.UpsertProductSupplierAsync(
			It.Is<Modelbouwer.Model.ProductSupplierModel>( ps => ps.SupplierId == 11 && ps.ProductId == 5 ) ),
			Times.Once );
	}

	[TestMethod]
	public void ApplySupplierFilter_WhenEnabled_ShowsOnlyMatchingSupplierOrders()
	{
		_viewModel.EditableOrder.SupplierId = 5;
		_viewModel.ReplaceOrdersForTest( new List<StockOrderModel>
		{
			new() { Id = 1, SupplierId = 5, OrderNumber = "SO-1" },
			new() { Id = 2, SupplierId = 8, OrderNumber = "SO-2" }
		} );

		_viewModel.EnableSupplierOrderFilter = true;

		Assert.AreEqual( 1, _viewModel.Orders.Count );
		Assert.AreEqual( 5, _viewModel.Orders[ 0 ].SupplierId );
	}
}
