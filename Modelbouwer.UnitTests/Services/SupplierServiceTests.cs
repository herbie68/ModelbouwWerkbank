using Modelbouwer.Model;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class SupplierServiceTests
{
	private Mock<GenericDataService> _mockDataService = null!;
	private SupplierService _service = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockDataService = new Mock<GenericDataService>();
		_service = new SupplierService( _mockDataService.Object );
	}

	[TestMethod]
	public async Task GetProductSupplierAsync_ReturnsMatchForSupplierAndProduct()
	{
		var expected = new ProductSupplierModel
		{
			ProductSupplierId = 3,
			SupplierId = 11,
			ProductId = 5,
			ProductNumber = "SUP-005"
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProductSupplierModel>>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( new List<ProductSupplierModel> { expected } );

		var result = await _service.GetProductSupplierAsync( 11, 5 );

		Assert.IsNotNull( result );
		Assert.AreEqual( 3, result.ProductSupplierId );
		Assert.AreEqual( "SUP-005", result.ProductNumber );
	}

	[TestMethod]
	public async Task UpsertProductSupplierAsync_WithNewRecord_InsertsAndReturnsId()
	{
		var productSupplier = new ProductSupplierModel
		{
			ProductSupplierId = 0,
			ProductId = 5,
			SupplierId = 11,
			CurrencyId = 2,
			ProductNumber = "SUP-005",
			ProductName = "Wheel Set",
			Price = 12.5,
			URL = "https://supplier.example/item"
		};

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.Is<string>( q => q != null && q.Contains( "INSERT INTO" ) ), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 41u );

		var result = await _service.UpsertProductSupplierAsync( productSupplier );

		Assert.AreEqual( 41, result );
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.Is<string>( q => q != null && q.Contains( "INSERT INTO" ) ),
			It.Is<Dictionary<string, object>>( p =>
				( int ) p[DBNames.ProductSupplierFieldNameProductId] == 5 &&
				( int ) p[DBNames.ProductSupplierFieldNameSupplierId] == 11 ) ),
			Times.Once );
	}
}
