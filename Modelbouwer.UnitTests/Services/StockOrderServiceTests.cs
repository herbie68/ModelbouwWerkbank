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
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 44u );

		var result = await _service.InsertOrderLineAsync( line );

		Assert.AreEqual( 44, result );

		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( p =>
				( double ) p [ $"@{DBNames.OrderLineFieldNameAmount}" ] == 5d &&
				( double ) p [ $"@{DBNames.OrderLineFieldNameOpenAmount}" ] == 5d ) ),
			Times.Once );
	}
}
