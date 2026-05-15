namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockOrderViewModelTests
{
	private Mock<IStockOrderService> _mockStockOrderService = null!;
	private Mock<IProductService> _mockProductService = null!;
	private Mock<IStockService> _mockStockService = null!;
	private Mock<ISupplierService> _mockSupplierService = null!;
	private StockOrderViewModel _viewModel = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockStockOrderService = new Mock<IStockOrderService>();
		_mockProductService = new Mock<IProductService>();
		_mockStockService = new Mock<IStockService>();
		_mockSupplierService = new Mock<ISupplierService>();

		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync() ).ReturnsAsync( new List<StockOrderModel>() );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( It.IsAny<int>() ) ).ReturnsAsync( new List<StockOrderLineModel>() );
		_mockStockOrderService.Setup( s => s.UpdateOrderLineWithStockCorrectionAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>() ) ).Returns( Task.CompletedTask );
		_mockStockOrderService.Setup( s => s.DeleteOrderLineWithStockCorrectionAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>() ) ).Returns( Task.CompletedTask );
		_mockStockOrderService.Setup( s => s.DeleteOrderWithLinesAsync( It.IsAny<int>(), It.IsAny<IEnumerable<StockOrderLineModel>>() ) ).Returns( Task.CompletedTask );
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>() );
		_mockStockService.Setup( s => s.GetCompleteInventoryAsync() ).ReturnsAsync( new List<StockManagementModel>() );
		_mockSupplierService.Setup( s => s.GetAllSuppliersAsync() ).ReturnsAsync( new List<SupplierModel>() );
		_mockSupplierService.Setup( s => s.GetAllCurrenciesAsync() ).ReturnsAsync( new List<CurrencyModel>() );

		_viewModel = new StockOrderViewModel(
			_mockStockOrderService.Object,
			_mockProductService.Object,
			_mockStockService.Object,
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
	public void EditableOrder_WhenSupplierSelectedOnNewOrder_FillsCurrencyAndSupplierCosts()
	{
		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( new SupplierModel
		{
			Id = 11,
			Name = "Supplier 11",
			CurrencyId = 2,
			ShippingCosts = 7.5,
			OrderCosts = 3.25
		} );

		_viewModel.EditableOrder.SupplierId = 11;

		Assert.AreEqual( 2, _viewModel.EditableOrder.CurrencyId );
		Assert.AreEqual( 7.5d, _viewModel.EditableOrder.ShippingCosts );
		Assert.AreEqual( 3.25d, _viewModel.EditableOrder.OrderCosts );
	}

	[TestMethod]
	public void EditableOrder_WhenCostsManuallyOverridden_KeepsManualValuesOnSupplierChange()
	{
		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( new SupplierModel
		{
			Id = 11,
			Name = "Supplier 11",
			CurrencyId = 2,
			ShippingCosts = 7.5,
			OrderCosts = 3.25
		} );
		_viewModel.Suppliers.Add( new SupplierModel
		{
			Id = 12,
			Name = "Supplier 12",
			CurrencyId = 3,
			ShippingCosts = 12.5,
			OrderCosts = 6.5
		} );

		_viewModel.EditableOrder.SupplierId = 11;
		_viewModel.EditableOrder.ShippingCosts = 99d;
		_viewModel.EditableOrder.OrderCosts = 55d;

		_viewModel.EditableOrder.SupplierId = 12;

		Assert.AreEqual( 3, _viewModel.EditableOrder.CurrencyId );
		Assert.AreEqual( 99d, _viewModel.EditableOrder.ShippingCosts );
		Assert.AreEqual( 55d, _viewModel.EditableOrder.OrderCosts );
	}

	[TestMethod]
	public void ApplySelectedOrder_WhenSupplierChanges_PreservesLoadedCosts()
	{
		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( new SupplierModel
		{
			Id = 11,
			Name = "Supplier 11",
			CurrencyId = 2,
			ShippingCosts = 7.5,
			OrderCosts = 3.25
		} );
		_viewModel.Suppliers.Add( new SupplierModel
		{
			Id = 12,
			Name = "Supplier 12",
			CurrencyId = 3,
			ShippingCosts = 12.5,
			OrderCosts = 6.5
		} );

		_viewModel.ApplySelectedOrder( new StockOrderModel
		{
			Id = 25,
			SupplierId = 11,
			CurrencyId = 2,
			OrderNumber = "SO-025",
			ShippingCosts = 18d,
			OrderCosts = 9d
		}, new List<StockOrderLineModel>() );

		_viewModel.EditableOrder.SupplierId = 12;

		Assert.AreEqual( 3, _viewModel.EditableOrder.CurrencyId );
		Assert.AreEqual( 18d, _viewModel.EditableOrder.ShippingCosts );
		Assert.AreEqual( 9d, _viewModel.EditableOrder.OrderCosts );
	}

	[TestMethod]
	public async Task ConstructorInitialization_PopulatesAvailableProductsWithInventoryFields()
	{
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>
		{
			new()
			{
				ProductId = 5,
				ProductCode = "P-005",
				ProductName = "Wheel Set",
				ProductPrice = 12.5,
				ProductMinimalStock = 10,
				ProductStandardQuantity = 4
			}
		} );

		_mockStockService.Setup( s => s.GetCompleteInventoryAsync() ).ReturnsAsync( new List<StockManagementModel>
		{
			new()
			{
				ProductId = 5,
				ProductInventory = 6,
				ProductInOrder = 3,
				ProductMinimalStock = 10
			}
		} );

		_viewModel = new StockOrderViewModel(
			_mockStockOrderService.Object,
			_mockProductService.Object,
			_mockStockService.Object,
			_mockSupplierService.Object );

		for ( int attempt = 0; attempt < 20 && _viewModel.AvailableProducts.Count == 0; attempt++ )
		{
			await Task.Delay( 25 );
		}

		Assert.AreEqual( 1, _viewModel.AvailableProducts.Count );
		Assert.AreEqual( 6d, _viewModel.AvailableProducts[ 0 ].CurrentInventory );
		Assert.AreEqual( 3d, _viewModel.AvailableProducts[ 0 ].InOrder );
	}

	[TestMethod]
	public async Task InitializeAsync_PopulatesAvailableProductsWithInventoryFields()
	{
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>
		{
			new()
			{
				ProductId = 5,
				ProductCode = "P-005",
				ProductName = "Wheel Set",
				ProductPrice = 12.5,
				ProductMinimalStock = 10,
				ProductStandardQuantity = 4
			}
		} );

		_mockStockService.Setup( s => s.GetCompleteInventoryAsync() ).ReturnsAsync( new List<StockManagementModel>
		{
			new()
			{
				ProductId = 5,
				ProductInventory = 6,
				ProductInOrder = 3,
				ProductMinimalStock = 10
			}
		} );

		_viewModel = new StockOrderViewModel(
			_mockStockOrderService.Object,
			_mockProductService.Object,
			_mockStockService.Object,
			_mockSupplierService.Object );

		await _viewModel.InitializeAsync();

		Assert.AreEqual( 1, _viewModel.AvailableProducts.Count );
		Assert.AreEqual( 6d, _viewModel.AvailableProducts[ 0 ].CurrentInventory );
		Assert.AreEqual( 3d, _viewModel.AvailableProducts[ 0 ].InOrder );
		Assert.AreEqual( 10d, _viewModel.AvailableProducts[ 0 ].ProductMinimalStock );
		Assert.AreEqual( 4d, _viewModel.AvailableProducts[ 0 ].ProductStandardQuantity );
	}

	[TestMethod]
	public async Task SearchText_FiltersAvailableProductsByCodeAndName()
	{
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>
		{
			new() { ProductId = 5, ProductCode = "AX-005", ProductName = "Wheel Set" },
			new() { ProductId = 6, ProductCode = "BR-006", ProductName = "Brake Hose" },
			new() { ProductId = 7, ProductCode = "LN-007", ProductName = "Lantern" }
		} );

		await _viewModel.InitializeAsync();

		_viewModel.SearchText = "BR";

		Assert.AreEqual( 1, _viewModel.AvailableProducts.Count );
		Assert.AreEqual( "BR-006", _viewModel.AvailableProducts[ 0 ].ProductCode );

		_viewModel.SearchText = "wheel";

		Assert.AreEqual( 1, _viewModel.AvailableProducts.Count );
		Assert.AreEqual( "Wheel Set", _viewModel.AvailableProducts[ 0 ].ProductName );
	}

	[TestMethod]
	public async Task ClearSearchCommand_RestoresAvailableProducts()
	{
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>
		{
			new() { ProductId = 5, ProductCode = "AX-005", ProductName = "Wheel Set" },
			new() { ProductId = 6, ProductCode = "BR-006", ProductName = "Brake Hose" }
		} );

		await _viewModel.InitializeAsync();

		_viewModel.SearchText = "wheel";
		_viewModel.ClearSearchCommand.Execute( null );

		Assert.AreEqual( string.Empty, _viewModel.SearchText );
		Assert.AreEqual( 2, _viewModel.AvailableProducts.Count );
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

		_mockStockOrderService.Setup( s => s.InsertOrderWithLinesAsync( It.IsAny<StockOrderModel>(), It.IsAny<IEnumerable<StockOrderLineModel>>() ) ).ReturnsAsync( 25 );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel>
		{
			new() { Id = 40, SupplyOrderId = 25, ProductId = 5, Amount = 3, OpenAmount = 3, Price = 12.5, RealRowTotal = 37.5 }
		} );
		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync() ).ReturnsAsync( new List<StockOrderModel>
		{
			new() { Id = 25, SupplierId = 11, CurrencyId = 2, OrderNumber = "SO-025", OrderDate = DateTime.Today }
		} );

		await _viewModel.SaveOrderAsync();

		_mockStockOrderService.Verify( s => s.InsertOrderWithLinesAsync(
			It.IsAny<StockOrderModel>(),
			It.Is<IEnumerable<StockOrderLineModel>>( lines => lines.Count() == 1 && lines.First().ProductId == 5 && lines.First().Amount == 3d ) ),
			Times.Once );
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
			ProductStandardQuantity = 3,
			ProductMinimalStock = 10
		};

		_mockStockService.Setup( s => s.GetCompleteInventoryAsync() ).ReturnsAsync( new List<StockManagementModel>
		{
			new() { ProductId = 5, ProductInventory = 6, ProductMinimalStock = 10 }
		} );

		_viewModel = new StockOrderViewModel(
			_mockStockOrderService.Object,
			_mockProductService.Object,
			_mockStockService.Object,
			_mockSupplierService.Object );

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
		Assert.AreEqual( 4d, _viewModel.PendingOrderLines[ 0 ].Amount );
		Assert.AreEqual( 4d, _viewModel.PendingOrderLines[ 0 ].OpenAmount );
		Assert.AreEqual( 50d, _viewModel.PendingOrderLines[ 0 ].RealRowTotal );
		_mockSupplierService.Verify( s => s.UpsertProductSupplierAsync(
			It.Is<Modelbouwer.Model.ProductSupplierModel>( ps => ps.SupplierId == 11 && ps.ProductId == 5 ) ),
			Times.Once );
	}

	[TestMethod]
	public async Task AddSelectedProductAsync_WhenMinimumStockAlreadyCovered_DefaultsAmountToOne()
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
			ProductMinimalStock = 10
		};

		_mockStockService.Setup( s => s.GetCompleteInventoryAsync() ).ReturnsAsync( new List<StockManagementModel>
		{
			new() { ProductId = 5, ProductInventory = 10, ProductMinimalStock = 10 }
		} );

		_viewModel = new StockOrderViewModel(
			_mockStockOrderService.Object,
			_mockProductService.Object,
			_mockStockService.Object,
			_mockSupplierService.Object );

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

		Assert.AreEqual( 1d, _viewModel.PendingOrderLines[ 0 ].Amount );
		Assert.AreEqual( 12.5d, _viewModel.PendingOrderLines[ 0 ].RealRowTotal );
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

	[TestMethod]
	public void ApplySupplierFilter_WhenSupplierChanges_RefreshesFilteredOrders()
	{
		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( new SupplierModel { Id = 5, CurrencyId = 1 } );
		_viewModel.Suppliers.Add( new SupplierModel { Id = 8, CurrencyId = 2 } );
		_viewModel.EditableOrder.SupplierId = 5;
		_viewModel.ReplaceOrdersForTest( new List<StockOrderModel>
		{
			new() { Id = 1, SupplierId = 5, OrderNumber = "SO-1" },
			new() { Id = 2, SupplierId = 8, OrderNumber = "SO-2" }
		} );
		_viewModel.EnableSupplierOrderFilter = true;

		_viewModel.EditableOrder.SupplierId = 8;

		Assert.AreEqual( 1, _viewModel.Orders.Count );
		Assert.AreEqual( 8, _viewModel.Orders[ 0 ].SupplierId );
	}

	[TestMethod]
	public async Task SelectedOrder_WhenOlderLoadCompletesAfterNewerSelection_KeepsLatestOrder()
	{
		var firstOrderLines = new TaskCompletionSource<List<StockOrderLineModel>>();
		var secondOrder = new StockOrderModel { Id = 2, SupplierId = 8, OrderNumber = "SO-2" };

		_mockStockOrderService
			.Setup( s => s.GetOrderLinesAsync( 1 ) )
			.Returns( firstOrderLines.Task );
		_mockStockOrderService
			.Setup( s => s.GetOrderLinesAsync( 2 ) )
			.ReturnsAsync( new List<StockOrderLineModel>
			{
				new() { Id = 20, SupplyOrderId = 2, ProductId = 200, Amount = 1, OpenAmount = 1 }
			} );

		_viewModel.SelectedOrder = new StockOrderModel { Id = 1, SupplierId = 5, OrderNumber = "SO-1" };
		_viewModel.SelectedOrder = secondOrder;
		await WaitUntilAsync( () => _viewModel.EditableOrder.Id == 2 );

		firstOrderLines.SetResult( new List<StockOrderLineModel>
		{
			new() { Id = 10, SupplyOrderId = 1, ProductId = 100, Amount = 1, OpenAmount = 1 }
		} );
		await Task.Delay( 25 );

		Assert.AreEqual( 2, _viewModel.EditableOrder.Id );
		Assert.AreEqual( "SO-2", _viewModel.EditableOrder.OrderNumber );
		Assert.AreEqual( 20, _viewModel.OrderLines[ 0 ].Id );
	}

	[TestMethod]
	public void Orders_DefaultToOpenOnly_AndCanIncludeClosedOrders()
	{
		_viewModel.ReplaceOrdersForTest( new List<StockOrderModel>
		{
			new() { Id = 1, SupplierId = 5, OrderNumber = "SO-1", Closed = false },
			new() { Id = 2, SupplierId = 8, OrderNumber = "SO-2", Closed = true }
		} );

		Assert.AreEqual( 1, _viewModel.Orders.Count );
		Assert.IsFalse( _viewModel.Orders[ 0 ].Closed );

		_viewModel.ShowClosedOrders = true;

		Assert.AreEqual( 2, _viewModel.Orders.Count );
		Assert.IsTrue( _viewModel.Orders.Any( o => o.Closed ) );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WithPendingLine_UpdatesLineFromDialog()
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
			ProductPrice = 12.5
		};

		var line = new StockOrderLineModel
		{
			ProductId = 5,
			SupplierId = 11,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			SupplierProductName = "Wheel Set",
			Amount = 2,
			OpenAmount = 2,
			Price = 12.5,
			RealRowTotal = 25
		};

		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( supplier );
		_viewModel.AvailableProducts.Clear();
		_viewModel.AvailableProducts.Add( product );
		_viewModel.EditableOrder.SupplierId = 11;
		_viewModel.PendingOrderLines.Add( line );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowProductDialog = vm =>
		{
			vm.Model.SupplierProductName = "Wheel Set Updated";
			vm.Model.Amount = 4;
			vm.Model.UnitPrice = 15;
			return true;
		};

		_mockSupplierService
			.Setup( s => s.GetProductSupplierAsync( 11, 5 ) )
			.ReturnsAsync( ( Modelbouwer.Model.ProductSupplierModel? ) null );

		_mockSupplierService
			.Setup( s => s.UpsertProductSupplierAsync( It.IsAny<Modelbouwer.Model.ProductSupplierModel>() ) )
			.ReturnsAsync( 70 );

		await _viewModel.EditOrderLineCommand.ExecuteAsync( null );

		Assert.AreEqual( "Wheel Set Updated", line.SupplierProductName );
		Assert.AreEqual( 4d, line.Amount );
		Assert.AreEqual( 4d, line.OpenAmount );
		Assert.AreEqual( 15d, line.Price );
		Assert.AreEqual( 60d, line.RealRowTotal );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WithExistingLine_UpdatesLineAndPersists()
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
			ProductPrice = 12.5
		};

		var order = new StockOrderModel
		{
			Id = 25,
			SupplierId = 11,
			OrderNumber = "SO-025"
		};

		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			SupplierId = 11,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			SupplierProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Price = 12.5,
			RealRowTotal = 62.5
		};

		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( supplier );
		_viewModel.AvailableProducts.Clear();
		_viewModel.AvailableProducts.Add( product );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowProductDialog = vm =>
		{
			vm.Model.SupplierProductName = "Wheel Set Updated";
			vm.Model.Amount = 6;
			vm.Model.UnitPrice = 15;
			return true;
		};

		_mockSupplierService
			.Setup( s => s.GetProductSupplierAsync( 11, 5 ) )
			.ReturnsAsync( ( Modelbouwer.Model.ProductSupplierModel? ) null );

		_mockSupplierService
			.Setup( s => s.UpsertProductSupplierAsync( It.IsAny<Modelbouwer.Model.ProductSupplierModel>() ) )
			.ReturnsAsync( 70 );

		await _viewModel.EditOrderLineCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.UpdateOrderLineWithStockCorrectionAsync(
			It.Is<StockOrderLineModel>( updated =>
				updated.Id == 40 &&
				updated.SupplierProductName == "Wheel Set Updated" &&
				updated.Amount == 6d &&
				updated.OpenAmount == 4d &&
				updated.Price == 15d &&
				updated.RealRowTotal == 90d ),
			1d ),
			Times.Once );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WithExistingLineAndLowerAmount_WritesNegativeDeltaCorrection()
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
			ProductPrice = 12.5
		};

		var order = new StockOrderModel
		{
			Id = 25,
			SupplierId = 11,
			OrderNumber = "SO-025"
		};

		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			SupplierId = 11,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			SupplierProductName = "Wheel Set",
			Amount = 2,
			OpenAmount = 2,
			Received = 0,
			Price = 12.5,
			RealRowTotal = 25
		};

		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( supplier );
		_viewModel.AvailableProducts.Clear();
		_viewModel.AvailableProducts.Add( product );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowProductDialog = vm =>
		{
			vm.Model.Amount = 1;
			vm.Model.UnitPrice = 12.5;
			return true;
		};

		_mockSupplierService
			.Setup( s => s.GetProductSupplierAsync( 11, 5 ) )
			.ReturnsAsync( ( Modelbouwer.Model.ProductSupplierModel? ) null );

		_mockSupplierService
			.Setup( s => s.UpsertProductSupplierAsync( It.IsAny<Modelbouwer.Model.ProductSupplierModel>() ) )
			.ReturnsAsync( 70 );

		await _viewModel.EditOrderLineCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.UpdateOrderLineWithStockCorrectionAsync(
			It.Is<StockOrderLineModel>( updated => updated.Id == 40 && updated.Amount == 1d && updated.OpenAmount == 1d ),
			-1d ),
			Times.Once );
	}

	[TestMethod]
	public async Task AddSelectedProductAsync_WithExistingOrder_InsertsLineAndWritesPositiveStockCorrection()
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
			ProductPrice = 12.5
		};

		var order = new StockOrderModel
		{
			Id = 25,
			SupplierId = 11,
			OrderNumber = "SO-025"
		};

		_viewModel.Suppliers.Clear();
		_viewModel.Suppliers.Add( supplier );
		_viewModel.AvailableProducts.Clear();
		_viewModel.AvailableProducts.Add( product );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel>() );
		_viewModel.SelectedProduct = product;
		_viewModel.ShowProductDialog = vm =>
		{
			vm.Model.Amount = 4;
			vm.Model.UnitPrice = 15;
			return true;
		};

		_mockSupplierService
			.Setup( s => s.GetProductSupplierAsync( 11, 5 ) )
			.ReturnsAsync( ( Modelbouwer.Model.ProductSupplierModel? ) null );

		_mockSupplierService
			.Setup( s => s.UpsertProductSupplierAsync( It.IsAny<Modelbouwer.Model.ProductSupplierModel>() ) )
			.ReturnsAsync( 70 );

		_mockStockOrderService
			.Setup( s => s.InsertOrderLineWithStockCorrectionAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>() ) )
			.ReturnsAsync( 40 );

		await _viewModel.AddSelectedProductAsync();

		_mockStockOrderService.Verify( s => s.InsertOrderLineWithStockCorrectionAsync(
			It.Is<StockOrderLineModel>( line => line.SupplyOrderId == 25 && line.ProductId == 5 && line.Amount == 4d ),
			4d ),
			Times.Once );
	}

	[TestMethod]
	public async Task DeleteSelectedOrderLineAsync_WithPendingLine_RemovesLineFromMemoryOnly()
	{
		var line = new StockOrderLineModel
		{
			ProductId = 5,
			SupplierId = 11,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			SupplierProductName = "Wheel Set",
			Amount = 2,
			OpenAmount = 2,
			Price = 12.5,
			RealRowTotal = 25
		};

		_viewModel.PendingOrderLines.Add( line );
		_viewModel.SelectedOrderLine = line;

		await _viewModel.DeleteOrderLineCommand.ExecuteAsync( null );

		Assert.AreEqual( 0, _viewModel.PendingOrderLines.Count );
		Assert.IsNull( _viewModel.SelectedOrderLine );
		_mockStockOrderService.Verify( s => s.DeleteOrderLineWithStockCorrectionAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>() ), Times.Never );
	}

	[TestMethod]
	public async Task DeleteSelectedOrderLineAsync_WithExistingLine_DeletesLineAndWritesNegativeStockCorrection()
	{
		var order = new StockOrderModel
		{
			Id = 25,
			SupplierId = 11,
			OrderNumber = "SO-025"
		};

		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			SupplierId = 11,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			SupplierProductName = "Wheel Set",
			Amount = 2,
			OpenAmount = 2,
			Price = 12.5,
			RealRowTotal = 25
		};

		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;

		await _viewModel.DeleteOrderLineCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.DeleteOrderLineWithStockCorrectionAsync(
			It.Is<StockOrderLineModel>( deleted => deleted.Id == 40 && deleted.ProductId == 5 && deleted.Amount == 2d ),
			-2d ),
			Times.Once );
		Assert.AreEqual( 0, _viewModel.OrderLines.Count );
		Assert.IsNull( _viewModel.SelectedOrderLine );
		Assert.AreEqual( 0d, _viewModel.EditableOrder.LinesTotal );
	}

	[TestMethod]
	public async Task DeleteOrderAsync_WithExistingOrder_DeletesLinesWritesStockCorrectionsDeletesOrderAndResetsScreen()
	{
		var order = new StockOrderModel
		{
			Id = 25,
			SupplierId = 11,
			OrderNumber = "SO-025"
		};

		var lines = new List<StockOrderLineModel>
		{
			new()
			{
				Id = 40,
				SupplyOrderId = 25,
				ProductId = 5,
				Amount = 2,
				OpenAmount = 2,
				Price = 12.5,
				RealRowTotal = 25
			},
			new()
			{
				Id = 41,
				SupplyOrderId = 25,
				ProductId = 8,
				Amount = 3,
				OpenAmount = 3,
				Price = 10,
				RealRowTotal = 30
			}
		};

		_viewModel.ApplySelectedOrder( order, lines );
		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync() ).ReturnsAsync( new List<StockOrderModel>() );

		await _viewModel.DeleteOrderCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.DeleteOrderWithLinesAsync(
			25,
			It.Is<IEnumerable<StockOrderLineModel>>( deletedLines =>
				deletedLines.Count() == 2 &&
				deletedLines.Any( line => line.Id == 40 && line.ProductId == 5 && line.Amount == 2d ) &&
				deletedLines.Any( line => line.Id == 41 && line.ProductId == 8 && line.Amount == 3d ) ) ),
			Times.Once );
		Assert.IsTrue( _viewModel.IsNewOrder );
		Assert.IsNull( _viewModel.SelectedOrder );
		Assert.AreEqual( 0, _viewModel.OrderLines.Count );
		Assert.AreEqual( 0, _viewModel.Orders.Count );
		Assert.AreEqual( 0, _viewModel.EditableOrder.Id );
	}

	private static async Task WaitUntilAsync( Func<bool> condition )
	{
		for ( int attempt = 0; attempt < 40; attempt++ )
		{
			if ( condition() )
				return;

			await Task.Delay( 25 );
		}

		Assert.Fail( "Condition was not met before the timeout." );
	}
}
