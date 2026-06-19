using System.Data.Common;
using System.Reflection;

using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySqlTransaction = MySql.Data.MySqlClient.MySqlTransaction;

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
		_service = new StockOrderService( _mockDataService.Object );
	}

	[TestMethod]
	public void IStockOrderService_ExposesCancellationTokenOverloads()
	{
		var methods = typeof( IStockOrderService )
			.GetMethods()
			.ToArray();

		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.GetAllOrdersAsync ), 1 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.GetOrderLinesAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.InsertOrderAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.InsertOrderWithLinesAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.UpdateOrderAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.DeleteOrderAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.DeleteOrderWithLinesAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.InsertOrderLineAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.InsertOrderLineWithStockCorrectionAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.UpdateOrderLineAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.UpdateOrderLineWithStockCorrectionAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.RegisterReceiptAsync ), 4 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.DeleteOrderLineAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( IStockOrderService.DeleteOrderLineWithStockCorrectionAsync ), 3 );
	}

	[TestMethod]
	public async Task GetAllOrdersAsync_ReturnsMappedOrders()
	{
		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<DbDataReader, StockOrderModel>>(), null, CancellationToken.None ) )
			.ReturnsAsync( new List<StockOrderModel> { new() { Id = 9, OrderNumber = "SO-9", Closed = true } } );

		var result = await _service.GetAllOrdersAsync();

		Assert.AreEqual( 1, result.Count );
		Assert.AreEqual( 9, result [ 0 ].Id );
		Assert.AreEqual( "SO-9", result [ 0 ].OrderNumber );
		Assert.IsTrue( result [ 0 ].Closed );
	}

	[TestMethod]
	public async Task GetAllOrdersAsync_WithCancellationToken_PassesTokenToDataService()
	{
		using var cts = new CancellationTokenSource();

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<DbDataReader, StockOrderModel>>(),
				null,
				cts.Token ) )
			.ReturnsAsync( new List<StockOrderModel>() );

		await _service.GetAllOrdersAsync( cts.Token );

		_mockDataService.Verify( s => s.ExecuteQueryAsync(
			It.IsAny<string>(),
			It.IsAny<Func<DbDataReader, StockOrderModel>>(),
			null,
			cts.Token ), Times.Once );
	}

	[TestMethod]
	public void CompleteOrderListQuery_UsesActualHasStockLogColumnName()
	{
		StringAssert.Contains( _service.CompleteOrderListQuery, DBNames.OrderViewFieldNameHasStockLog );
		Assert.IsFalse( _service.CompleteOrderListQuery.Contains( "HasStackLog" ) );
	}

	[TestMethod]
	public async Task InsertOrderLineAsync_PassesOpenAmountEqualToAmount()
	{
		var line = new StockOrderLineModel
		{
			SupplyOrderId = 4,
			ProductId = 12,
			SupplierProductName = "Axle",
			Amount = 5,
			Price = 3.5,
			RealRowTotal = 17.5
		};

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None ) )
			.ReturnsAsync( 44u );

		var result = await _service.InsertOrderLineAsync( line );

		Assert.AreEqual( 44, result );

		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( p =>
				( int ) p [ $"@{DBNames.OrderLineFieldNameSupplierOrderId}" ] == 4 &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameAmount}" ] == 5d &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameOpenAmount}" ] == 5d &&
				!p.ContainsKey( $"@{DBNames.OrderLineFieldNameSupplierId}" ) ),
			CancellationToken.None ),
			Times.Once );
	}

	[TestMethod]
	public async Task InsertOrderLineAsync_WithCancellationToken_PassesTokenToDataService()
	{
		using var cts = new CancellationTokenSource();
		var line = new StockOrderLineModel
		{
			SupplyOrderId = 4,
			ProductId = 12,
			SupplierProductName = "Axle",
			Amount = 5,
			Price = 3.5,
			RealRowTotal = 17.5
		};

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>(),
				cts.Token ) )
			.ReturnsAsync( 44u );

		var result = await _service.InsertOrderLineAsync( line, cts.Token );

		Assert.AreEqual( 44, result );
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.IsAny<Dictionary<string, object>>(),
			cts.Token ), Times.Once );
	}

	[TestMethod]
	public async Task InsertOrderLineAsync_PreservesZeroOpenAmount()
	{
		var line = new StockOrderLineModel
		{
			SupplyOrderId = 4,
			ProductId = 12,
			SupplierProductName = "Axle",
			Amount = 5,
			OpenAmount = 0,
			Price = 3.5,
			RealRowTotal = 17.5,
			Closed = true
		};

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None ) )
			.ReturnsAsync( 44u );

		await _service.InsertOrderLineAsync( line );

		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( p =>
				( double ) p [ $"@{DBNames.OrderLineFieldNameOpenAmount}" ] == 0d &&
				( bool ) p [ $"@{DBNames.OrderLineFieldNameClosed}" ] ),
			CancellationToken.None ),
			Times.Once );
	}

	[TestMethod]
	public async Task InsertOrderWithLinesAsync_UsesTransactionalDataService()
	{
		var order = new StockOrderModel { SupplierId = 11, CurrencyId = 2, OrderNumber = "SO-25", OrderDate = DateTime.Today };
		var lines = new List<StockOrderLineModel>
		{
			new() { ProductId = 5, Amount = 3, OpenAmount = 3, Price = 12.5, RealRowTotal = 37.5 }
		};

		_mockDataService
			.Setup( s => s.ExecuteInTransactionAsync<int>( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task<int>>>(), CancellationToken.None ) )
			.ReturnsAsync( 25 );

		var result = await _service.InsertOrderWithLinesAsync( order, lines );

		Assert.AreEqual( 25, result );
		_mockDataService.Verify( s => s.ExecuteInTransactionAsync<int>( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task<int>>>(), CancellationToken.None ), Times.Once );
	}

	[TestMethod]
	public async Task InsertOrderWithLinesAsync_WithCancellationToken_PassesTokenToTransaction()
	{
		using var cts = new CancellationTokenSource();
		var order = new StockOrderModel { SupplierId = 11, CurrencyId = 2, OrderNumber = "SO-25", OrderDate = DateTime.Today };
		var lines = new List<StockOrderLineModel>
		{
			new() { ProductId = 5, Amount = 3, OpenAmount = 3, Price = 12.5, RealRowTotal = 37.5 }
		};

		_mockDataService
			.Setup( s => s.ExecuteInTransactionAsync<int>(
				It.IsAny<Func<MySqlConnection, MySqlTransaction, Task<int>>>(),
				cts.Token ) )
			.ReturnsAsync( 25 );

		var result = await _service.InsertOrderWithLinesAsync( order, lines, cts.Token );

		Assert.AreEqual( 25, result );
		_mockDataService.Verify( s => s.ExecuteInTransactionAsync<int>(
			It.IsAny<Func<MySqlConnection, MySqlTransaction, Task<int>>>(),
			cts.Token ), Times.Once );
	}

	[TestMethod]
	public async Task UpdateOrderLineWithStockCorrectionAsync_UsesTransactionalDataService()
	{
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			Amount = 6,
			OpenAmount = 4,
			Price = 15,
			RealRowTotal = 90
		};

		_mockDataService
			.Setup( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>(), CancellationToken.None ) )
			.Returns( Task.CompletedTask );

		await _service.UpdateOrderLineWithStockCorrectionAsync( line, 1d );

		_mockDataService.Verify( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>(), CancellationToken.None ), Times.Once );
	}

	[TestMethod]
	public async Task UpdateOrderLineAsync_PassesZeroOpenAmount()
	{
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			Amount = 5,
			OpenAmount = 0,
			Price = 12.5,
			RealRowTotal = 62.5,
			Closed = true
		};

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None ) )
			.ReturnsAsync( 0u );

		await _service.UpdateOrderLineAsync( line );

		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( p =>
				( int ) p [ $"@{DBNames.OrderLineFieldNameId}" ] == 40 &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameOpenAmount}" ] == 0d &&
				( bool ) p [ $"@{DBNames.OrderLineFieldNameClosed}" ] ),
			CancellationToken.None ),
			Times.Once );
	}

	[TestMethod]
	public async Task DeleteOrderWithLinesAsync_UsesTransactionalDataService()
	{
		var lines = new List<StockOrderLineModel>
		{
			new() { Id = 40, SupplyOrderId = 25, ProductId = 5, Amount = 2 },
			new() { Id = 41, SupplyOrderId = 25, ProductId = 8, Amount = 3 }
		};

		_mockDataService
			.Setup( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>(), CancellationToken.None ) )
			.Returns( Task.CompletedTask );

		await _service.DeleteOrderWithLinesAsync( 25, lines );

		_mockDataService.Verify( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>(), CancellationToken.None ), Times.Once );
	}

	[TestMethod]
	public async Task RegisterReceiptAsync_UsesTransactionalDataService()
	{
		var line = new StockOrderLineModel
		{
			Id = 40,
			SupplyOrderId = 25,
			ProductId = 5,
			Amount = 5,
			OpenAmount = 1,
			Received = 4
		};

		_mockDataService
			.Setup( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>(), CancellationToken.None ) )
			.Returns( Task.CompletedTask );

		await _service.RegisterReceiptAsync( line, 2d, new DateTime( 2026, 5, 4 ) );

		_mockDataService.Verify( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>(), CancellationToken.None ), Times.Once );
	}

	private static void AssertHasCancellationTokenOverload( MethodInfo [ ] methods, string methodName, int parameterCount )
	{
		var hasOverload = methods.Any( method =>
		{
			if ( method.Name != methodName )
				return false;

			var parameters = method.GetParameters();
			return parameters.Length == parameterCount
				&& parameters[^1].ParameterType == typeof( CancellationToken );
		} );

		Assert.IsTrue( hasOverload, $"{methodName} should expose a {parameterCount}-parameter overload ending in CancellationToken." );
	}
}