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
	public void ProductSupplierQueries_UseSupplierCurrencyInsteadOfProductSupplierCurrency()
	{
		StringAssert.Contains( _service.CompleteProductSupplierList, $"s.{DBNames.SupplierFieldNameCurrencyId} AS {DBNames.SupplierFieldNameCurrencyId}" );
		StringAssert.Contains( _service.CompleteProductSupplierList, $"c ON s.{DBNames.SupplierFieldNameCurrencyId} = c.{DBNames.CurrencyFieldNameId}" );
		Assert.IsFalse( _service.CompleteProductSupplierList.Contains( $"ps.{DBNames.ProductSupplierFieldNameCurrencyId}, " ) );
		StringAssert.Contains( _service.CompleteProductSupplierList, $"ps.{DBNames.ProductSupplierFieldNameDefaultSupplier}" );

		StringAssert.Contains( _service.ProductSupplierBySupplierAndProductQuery, $"s.{DBNames.SupplierFieldNameCurrencyId} AS {DBNames.SupplierFieldNameCurrencyId}" );
		StringAssert.Contains( _service.ProductSupplierBySupplierAndProductQuery, $"c ON s.{DBNames.SupplierFieldNameCurrencyId} = c.{DBNames.CurrencyFieldNameId}" );
		Assert.IsFalse( _service.ProductSupplierBySupplierAndProductQuery.Contains( $"ps.{DBNames.ProductSupplierFieldNameCurrencyId}, " ) );
		StringAssert.Contains( _service.ProductSupplierBySupplierAndProductQuery, $"ps.{DBNames.ProductSupplierFieldNameDefaultSupplier}" );
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
		Assert.IsFalse( _service.InsertProductSupplierQuery.Contains( DBNames.ProductSupplierFieldNameCurrencyId ) );
		Assert.IsFalse( _service.UpdateProductSupplierQuery.Contains( DBNames.ProductSupplierFieldNameCurrencyId ) );
		Assert.IsFalse( _service.InsertProductSupplierQuery.Contains( DBNames.ProductSupplierFieldNameDefaultSupplier ) );
		Assert.IsFalse( _service.UpdateProductSupplierQuery.Contains( DBNames.ProductSupplierFieldNameDefaultSupplier ) );
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.Is<string>( q => q != null && q.Contains( "INSERT INTO" ) ),
			It.Is<Dictionary<string, object>>( p =>
				( int ) p[DBNames.ProductSupplierFieldNameProductId] == 5 &&
				( int ) p[DBNames.ProductSupplierFieldNameSupplierId] == 11 &&
				!p.ContainsKey( DBNames.ProductSupplierFieldNameCurrencyId ) &&
				!p.ContainsKey( DBNames.ProductSupplierFieldNameDefaultSupplier ) ) ),
			Times.Once );
	}
}
