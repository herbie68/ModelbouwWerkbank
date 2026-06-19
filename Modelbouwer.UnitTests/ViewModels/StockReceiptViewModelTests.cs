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
		_mockStockOrderService.Setup( s => s.GetAllOrdersAsync( It.IsAny<CancellationToken>() ) ).ReturnsAsync( new List<StockOrderModel>() );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( It.IsAny<int>() ) ).ReturnsAsync( new List<StockOrderLineModel>() );
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( It.IsAny<int>(), It.IsAny<CancellationToken>() ) ).ReturnsAsync( new List<StockOrderLineModel>() );
		_mockStockOrderService.Setup( s => s.RegisterReceiptAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>(), It.IsAny<DateTime?>() ) ).Returns( Task.CompletedTask );
		_mockStockOrderService.Setup( s => s.RegisterReceiptAsync( It.IsAny<StockOrderLineModel>(), It.IsAny<double>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>() ) ).Returns( Task.CompletedTask );
		_mockStockOrderService.Setup( s => s.UpdateOrderAsync( It.IsAny<StockOrderModel>() ) ).Returns( Task.CompletedTask );
		_mockStockOrderService.Setup( s => s.UpdateOrderAsync( It.IsAny<StockOrderModel>(), It.IsAny<CancellationToken>() ) ).Returns( Task.CompletedTask );

		_viewModel = new StockReceiptViewModel( _mockStockOrderService.Object );
	}

	[TestMethod]
	public async Task InitializeAsync_WithCancellationToken_PassesTokenToStockOrderService()
	{
		using var cts = new CancellationTokenSource();
		_mockStockOrderService
			.Setup( s => s.GetAllOrdersAsync( cts.Token ) )
			.ReturnsAsync( new List<StockOrderModel>
			{
				new() { Id = 25, OrderNumber = "SO-025" }
			} );

		await _viewModel.InitializeAsync( cts.Token );

		_mockStockOrderService.Verify( s => s.GetAllOrdersAsync( cts.Token ), Times.Once );
		Assert.AreEqual( 1, _viewModel.Orders.Count );
		Assert.AreEqual( 25, _viewModel.Orders [ 0 ].Id );
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
		Assert.AreEqual( 40, _viewModel.OpenOrderLines [ 0 ].Id );
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
	public async Task SelectedOrder_WhenBackgroundLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "receipt lines failed" );
		_mockStockOrderService
			.Setup( s => s.GetOrderLinesAsync( 25 ) )
			.ThrowsAsync( expected );

		_viewModel.SelectedOrder = new StockOrderModel { Id = 25, OrderNumber = "SO-025" };

		await WaitUntilAsync( () => ReferenceEquals( expected, _viewModel.LastAsyncError ) );

		Assert.AreSame( expected, _viewModel.LastAsyncError );
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
	public async Task EditSelectedOrderLineAsync_WithCancellationToken_PassesTokenToStockOrderService()
	{
		using var cts = new CancellationTokenSource();
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

		_mockStockOrderService
			.Setup( s => s.RegisterReceiptAsync( It.IsAny<StockOrderLineModel>(), 2d, new DateTime( 2026, 5, 4 ), cts.Token ) )
			.Returns( Task.CompletedTask );
		_mockStockOrderService
			.Setup( s => s.GetOrderLinesAsync( 25, cts.Token ) )
			.ReturnsAsync( new List<StockOrderLineModel> { line } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { line } );
		_viewModel.SelectedOrderLine = line;
		_viewModel.ShowReceiptDialog = vm =>
		{
			vm.Model.ReceivedAmount = 4;
			vm.Model.DeliveryDate = new DateTime( 2026, 5, 4 );
			return true;
		};

		await _viewModel.EditSelectedOrderLineAsync( cts.Token );

		_mockStockOrderService.Verify( s => s.RegisterReceiptAsync(
			It.Is<StockOrderLineModel>( updated => updated.Id == 40 ),
			2d,
			new DateTime( 2026, 5, 4 ),
			cts.Token ), Times.Once );
		_mockStockOrderService.Verify( s => s.GetOrderLinesAsync( 25, cts.Token ), Times.Once );
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
		Assert.IsTrue( _viewModel.Orders [ 0 ].Closed );
		Assert.AreEqual( new DateTime( 2026, 5, 4 ), _viewModel.Orders [ 0 ].ClosedDate );
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

	[TestMethod]
	public void AreAllOrderLinesSelected_SelectsOnlyOpenOrderLines()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025" };
		var openLine = new StockOrderLineModel { Id = 40, SupplyOrderId = 25, ProductId = 5, Amount = 5, OpenAmount = 3, Closed = false };
		var closedLine = new StockOrderLineModel { Id = 41, SupplyOrderId = 25, ProductId = 6, Amount = 2, OpenAmount = 0, Closed = true };

		_viewModel.ShowClosedOrders = true;
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { openLine, closedLine } );
		_viewModel.AreAllOrderLinesSelected = true;

		Assert.IsTrue( openLine.IsSelected );
		Assert.IsFalse( closedLine.IsSelected );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenLinesAreSelected_BulkReceivesSelectedLinesAndKeepsOrderSelected()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = false };
		var selectedLine = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			Amount = 5,
			OpenAmount = 3,
			Received = 2,
			Closed = false,
			IsSelected = true
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
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { selectedLine, otherLine } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { selectedLine, otherLine } );
		selectedLine.IsSelected = true;
		_viewModel.SelectedOrderLine = otherLine;
		_viewModel.ShowReceiptDialog = _ => throw new AssertFailedException( "The single-line dialog should not open for selected rows." );
		_viewModel.ShowReceiptDateDialog = vm =>
		{
			vm.DeliveryDate = new DateTime( 2026, 5, 6 );
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.RegisterReceiptAsync(
			It.Is<StockOrderLineModel>( updated =>
				updated.Id == 40 &&
				updated.Received == 5d &&
				updated.OpenAmount == 0d &&
				updated.Closed &&
				updated.ClosedDate == new DateTime( 2026, 5, 6 ) ),
			3d,
			new DateTime( 2026, 5, 6 ) ),
			Times.Once );
		_mockStockOrderService.Verify( s => s.RegisterReceiptAsync( It.Is<StockOrderLineModel>( updated => updated.Id == 41 ), It.IsAny<double>(), It.IsAny<DateTime?>() ), Times.Never );
		Assert.AreSame( order, _viewModel.SelectedOrder );
	}

	[TestMethod]
	public async Task EditSelectedOrderLineAsync_WhenBulkReceivesLastOpenLines_ClosesOrder()
	{
		var order = new StockOrderModel { Id = 25, OrderNumber = "SO-025", Closed = false };
		var firstLine = new StockOrderLineModel { Id = 40, SupplyOrderId = 25, ProductId = 5, Amount = 5, OpenAmount = 3, Received = 2, Closed = false, IsSelected = true };
		var secondLine = new StockOrderLineModel { Id = 41, SupplyOrderId = 25, ProductId = 6, Amount = 2, OpenAmount = 1, Received = 1, Closed = false, IsSelected = true };
		_mockStockOrderService.Setup( s => s.GetOrderLinesAsync( 25 ) ).ReturnsAsync( new List<StockOrderLineModel> { firstLine, secondLine } );
		_viewModel.ReplaceOrdersForTest( new List<StockOrderModel> { order } );
		_viewModel.ApplySelectedOrder( order, new List<StockOrderLineModel> { firstLine, secondLine } );
		firstLine.IsSelected = true;
		secondLine.IsSelected = true;
		_viewModel.ShowReceiptDateDialog = vm =>
		{
			vm.DeliveryDate = new DateTime( 2026, 5, 6 );
			return true;
		};

		await _viewModel.EditReceiptCommand.ExecuteAsync( null );

		_mockStockOrderService.Verify( s => s.UpdateOrderAsync(
			It.Is<StockOrderModel>( updated =>
				updated.Id == 25 &&
				updated.Closed &&
				updated.ClosedDate == new DateTime( 2026, 5, 6 ) ) ),
			Times.Once );
	}

	[TestMethod]
	public void StockReceiptViewModel_EditReceiptCommandIsGuardedAgainstParallelExecution()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "StockReceiptViewModel.cs" );

		StringAssert.Contains( source, "[ObservableProperty] private bool _isEditingReceipt;" );
		StringAssert.Contains( source, "EditReceiptCommand = new AsyncRelayCommand( () => EditSelectedOrderLineAsync(), () => !IsEditingReceipt );" );
		StringAssert.Contains( source, "partial void OnIsEditingReceiptChanged( bool value ) => EditReceiptCommand.NotifyCanExecuteChanged();" );
		AssertMethodContains( source, "private async Task EditSelectedOrderLineCoreAsync", "if ( IsEditingReceipt )" );
		AssertMethodContains( source, "private async Task EditSelectedOrderLineCoreAsync", "IsEditingReceipt = true;" );
		AssertMethodContains( source, "private async Task EditSelectedOrderLineCoreAsync", "finally" );
	}

	private static string LoadSource( params string [ ] relativeSegments )
	{
		var directory = AppContext.BaseDirectory;
		while ( directory != null && !File.Exists( Path.Combine( directory, "ModelbouwWerkbank.slnx" ) ) )
		{
			directory = Directory.GetParent( directory )?.FullName;
		}

		var repositoryRoot = directory ?? throw new DirectoryNotFoundException( "Could not locate repository root." );
		var path = Path.Combine( [ repositoryRoot, .. relativeSegments ] );

		return File.ReadAllText( path );
	}

	private static void AssertMethodContains( string source, string methodSignature, string expectedContent )
	{
		var methodStart = source.IndexOf( methodSignature, StringComparison.Ordinal );
		Assert.IsTrue( methodStart >= 0, $"Method '{methodSignature}' was not found." );

		var nextMethod = source.IndexOf( "\n\tprivate ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.Length;

		var methodBody = source.Substring( methodStart, nextMethod - methodStart );
		StringAssert.Contains( methodBody, expectedContent );
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