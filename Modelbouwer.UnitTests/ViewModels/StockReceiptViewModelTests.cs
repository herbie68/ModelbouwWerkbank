namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockReceiptViewModelTests
{
	private Mock<IStockOrderService> _mockStockOrderService = null!;
	private StockReceiptViewModel _viewModel = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockStockOrderService = new Mock<IStockOrderService>();
		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync() ).ReturnsAsync( new List<StockOrderModel>() );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( It.IsAny<int>() ) ).ReturnsAsync( new List<StockOrderLineModel>() );
		_mockStockOrderService.Setup( s => s.RegisterReceiptAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>(), It.IsAny<DateTime?>() ) ).Returns( Task.CompletedTask );
		_mockStockOrderService.Setup( s => s.UpdateOrderAsync( It.IsAny<StockOrderModel>() ) ).Returns( Task.CompletedTask );

		_viewModel = new StockReceiptViewModel( _mockStockOrderService.Object );
	}

	[TestMethod]
	public void ApplySelectedOrder_ShowsOnlyOpenOrderLines()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025" };
		var lines = new List<StockOrderLineModel>
		{
			new() { Id = 40, SupplyOrderId = 25, ProductId = 5, ProductCode = "P-005", ProductName = "Wheel Set", Amount = 5, OpenAmount = 3, Closed = false },
			new() { Id = 41, SupplyOrderId = 25, ProductId = 6, ProductCode = "P-006", ProductName = "Closed Item", Amount = 2, OpenAmount = 0, Closed = true }
		};

		_viewModel.ApplySelectedOrder( order, lines );

		Assert.AreEqual( 1, _viewModel.OpenOrderLines.Count );
		Assert.AreEqual( 40, _viewModel.OpenOrderLines[ 0 ].Id );
	}

	[TestMethod]
	public void ApplySelectedOrder_WhenClosedOrdersAreShown_ShowsClosedOrderLines()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = true };
		var lines = new List<StockOrderLineModel>
		{
			new() { Id = 40, SupplyOrderId = 25, ProductId = 5, ProductCode = "P-005", ProductName = "Wheel Set", Amount = 5, OpenAmount = 0, Closed = true },
			new() { Id = 41, SupplyOrderId = 25, ProductId = 6, ProductCode = "P-006", ProductName = "Axle", Amount = 2, OpenAmount = 0, Closed = true }
		};

		_viewModel.ShowClosedOrders = true;
		_viewModel.ApplySelectedOrder( order, lines );

		Assert.AreEqual( 2, _viewModel.OpenOrderLines.Count );
		Assert.IsTrue( _viewModel.OpenOrderLines.All( line => line.Closed ) );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenLineIsClosed_DoesNotOpenDialogOrPersist()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = true };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 0,
			Received = 5,
			Closed = true
		};
		var dialogOpened = false;
		_viewModel.ShowClosedOrders = true;
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = _ =>
		{
			dialogOpened = true;
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		Assert.IsFalse( dialogOpened );
		_mockStockOrderService.Verify( s => s.RegisterReceiptAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>(), It.IsAny<DateTime?>() ), Times.Never );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenSaved_PersistsReceivedDeltaAndRefreshesOpenRows()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025" };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			SupplierProductName = "Supplier Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false
		};
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { line } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 4;
			vm.Model.DeliveryDate = new DateTime( 2026, 5, 4 );
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.RegisterReceiptAsync(
			It.Is<StockOrderLineModel>( updated =>
				updated.Id == 40 &&
				updated.OpenAmount == 1d &&
				updated.Received == 4d &&
				updated.Closed == false ),
			2d,
			new DateTime( 2026, 5, 4 ) ),
			Times.Once );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenFullyReceived_AutomaticallyClosesLine()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025" };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false
		};
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { line } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 5;
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.RegisterReceiptAsync(
			It.Is<StockOrderLineModel>( updated => updated.Id == 40 && updated.OpenAmount == 0d && updated.Closed ),
			3d,
			It.IsAny<DateTime?>() ),
			Times.Once );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenLastOrderLineCloses_ClosesOrder()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = false };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false
		};
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { line } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 5;
			vm.Model.DeliveryDate = new DateTime( 2026, 5, 4 );
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.UpdateOrderAsync(
			It.Is<StockOrderModel>( updated =>
				updated.Id == 25 &&
				updated.Closed &&
				updated.ClosedDate == new DateTime( 2026, 5, 4 ) ) ),
			Times.Once );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenLastOrderLineCloses_RemovesOrderFromOpenOrderList()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = false };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false
		};
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { line } );
		_viewModel.ReplaceOrdersForTest( new List<StockOrderModel> { order } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 5;
			vm.Model.DeliveryDate = new DateTime( 2026, 5, 4 );
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		Assert.AreEqual( 0, _viewModel.Orders.Count );
		Assert.IsTrue( order.Closed );
		Assert.AreEqual( new DateTime( 2026, 5, 4 ), order.ClosedDate );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenLastOrderLineClosesAndClosedOrdersAreShown_UpdatesOrderInList()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = false };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false
		};
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { line } );
		_viewModel.ShowClosedOrders = true;
		_viewModel.ReplaceOrdersForTest( new List<StockOrderModel> { order } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 5;
			vm.Model.DeliveryDate = new DateTime( 2026, 5, 4 );
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		Assert.AreEqual( 1, _viewModel.Orders.Count );
		Assert.IsTrue( _viewModel.Orders[ 0 ].Closed );
		Assert.AreEqual( new DateTime( 2026, 5, 4 ), _viewModel.Orders[ 0 ].ClosedDate );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenAnotherOrderLineStillOpen_DoesNotCloseOrder()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = false };
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false
		};
		var otherLine = new StockOrderLineModel
		{
			Id = 41,
			SupplyOrderId = 25,
			ProductId = 6,
			ProductCode = "P-006",
			ProductName = "Axle",
			Amount = 2,
			OpenAmount = 1,
			Received = 1,
			Closed = false
		};
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { line, otherLine } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line, otherLine } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 5;
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.UpdateOrderAsync( It.IsAny<StockOrderModel>() ), Times.Never );
	}
}
