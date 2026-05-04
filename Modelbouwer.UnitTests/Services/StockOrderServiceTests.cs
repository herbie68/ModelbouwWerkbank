using System.Data.Common;

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
	public async Task GetAllOrdersAsync_ReturnsMappedOrders()
	{
		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<DbDataReader, StockOrderModel>>(), null ) )
			.ReturnsAsync( new List<StockOrderModel> { new() { Id = 9, OrderNumber = "SO-9", Closed = true } } );

		var result = await _service.GetAllOrdersAsync();

		Assert.AreEqual( 1, result.Count );
		Assert.AreEqual( 9, result[ 0 ].Id );
		Assert.AreEqual( "SO-9", result[ 0 ].OrderNumber );
		Assert.IsTrue( result[ 0 ].Closed );
	}

	[TestMethod]
	public void CompleteOrderListQuery_UsesActualHasStockLogColumnName()
	{
		StringAssert.Contains( _service.CompleteOrderListQuery, DBNames.OrderViewFieldNameHasStackLog );
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
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 44u );

		var result = await _service.InsertOrderLineAsync( line );

		Assert.AreEqual( 44, result );

		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( p =>
				( int ) p [ $"@{DBNames.OrderLineFieldNameSupplierOrderId}" ] == 4 &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameAmount}" ] == 5d &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameOpenAmount}" ] == 5d &&
				!p.ContainsKey( $"@{DBNames.OrderLineFieldNameSupplierId}" ) ) ),
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
			.Setup( s => s.ExecuteInTransactionAsync<int>( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task<int>>>() ) )
			.ReturnsAsync( 25 );

		var result = await _service.InsertOrderWithLinesAsync( order, lines );

		Assert.AreEqual( 25, result );
		_mockDataService.Verify( s => s.ExecuteInTransactionAsync<int>( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task<int>>>() ), Times.Once );
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
			.Setup( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>() ) )
			.Returns( Task.CompletedTask );

		await _service.UpdateOrderLineWithStockCorrectionAsync( line, 1d );

		_mockDataService.Verify( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>() ), Times.Once );
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
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		await _service.UpdateOrderLineAsync( line );

		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( p =>
				( int ) p [ $"@{DBNames.OrderLineFieldNameId}" ] == 40 &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameOpenAmount}" ] == 0d &&
				( bool ) p [ $"@{DBNames.OrderLineFieldNameClosed}" ] ) ),
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
			.Setup( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>() ) )
			.Returns( Task.CompletedTask );

		await _service.DeleteOrderWithLinesAsync( 25, lines );

		_mockDataService.Verify( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>() ), Times.Once );
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
			.Setup( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>() ) )
			.Returns( Task.CompletedTask );

		await _service.RegisterReceiptAsync( line, 2d, new DateTime( 2026, 5, 4 ) );

		_mockDataService.Verify( s => s.ExecuteInTransactionAsync( It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>() ), Times.Once );
	}
}
