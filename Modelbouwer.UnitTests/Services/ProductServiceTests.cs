using Moq;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class ProductServiceTests
{
	private Mock<GenericDataService> _mockDataService = null!;
	private ProductService _productService = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockDataService = new Mock<GenericDataService>();
		_productService = new ProductService( _mockDataService.Object );
	}

	[TestMethod]
	public async Task GetAllProductsAsync_ReturnsProductList()
	{
		// Arrange
		var expectedProducts = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Product 1" },
			new ProductModel { ProductId = 2, ProductName = "Product 2" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProductModel>>() ) )
			.ReturnsAsync( expectedProducts );

		// Act
		var result = await _productService.GetAllProductsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.HasCount( 2, result );
		Assert.AreEqual( "Product 1", result [ 0 ].ProductName );
		Assert.AreEqual( "Product 2", result [ 1 ].ProductName );
	}

	[TestMethod]
	public async Task GetAllProductsAsync_WithEmptyDatabase_ReturnsEmptyList()
	{
		// Arrange
		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProductModel>>() ) )
			.ReturnsAsync( new List<ProductModel>() );

		// Act
		var result = await _productService.GetAllProductsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.IsEmpty( result );
	}

	[TestMethod]
	public async Task GetAllBrandsAsync_ReturnsBrandList()
	{
		// Arrange
		var expectedBrands = new List<BrandModel>
		{
			new BrandModel { BrandId = 1, BrandName = "Brand 1" },
			new BrandModel { BrandId = 2, BrandName = "Brand 2" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, BrandModel>>() ) )
			.ReturnsAsync( expectedBrands );

		// Act
		var result = await _productService.GetAllBrandsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.HasCount( 2, result );
		Assert.AreEqual( "Brand 1", result [ 0 ].BrandName );
	}

	[TestMethod]
	public async Task GetAllUnitsAsync_ReturnsUnitList()
	{
		// Arrange
		var expectedUnits = new List<UnitModel>
		{
			new UnitModel { UnitId = 1, UnitName = "Unit 1" },
			new UnitModel { UnitId = 2, UnitName = "Unit 2" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, UnitModel>>() ) )
			.ReturnsAsync( expectedUnits );

		// Act
		var result = await _productService.GetAllUnitsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.HasCount( 2, result );
		Assert.AreEqual( "Unit 1", result [ 0 ].UnitName );
	}

	[TestMethod]
	public async Task GetAllCategoriesAsync_ReturnsCategoryList()
	{
		// Arrange
		var expectedCategories = new List<CategoryModel>
		{
			new CategoryModel { CategoryId = 1, CategoryName = "Category 1" },
			new CategoryModel { CategoryId = 2, CategoryName = "Category 2" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, CategoryModel>>() ) )
			.ReturnsAsync( expectedCategories );

		// Act
		var result = await _productService.GetAllCategoriesAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.HasCount( 2, result );
		Assert.AreEqual( "Category 1", result [ 0 ].CategoryName );
	}

	[TestMethod]
	public async Task InsertNewProductAsync_ReturnsNewProductId()
	{
		// Arrange
		var parameters = CreateValidProductParameters();

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 42u );

		// Act
		var result = await _productService.InsertNewProductAsync(parameters);

		// Assert
		Assert.AreEqual( 42, result );
	}

	[TestMethod]
	public async Task InsertNewProductAsync_PassesCorrectParameters()
	{
		// Arrange
		var parameters = CreateValidProductParameters();

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1u );

		// Act
		await _productService.InsertNewProductAsync( parameters );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( DBNames.ProductFieldNameCategoryId ) &&
				d.ContainsKey( DBNames.ProductFieldNameCode ) &&
				d.ContainsKey( DBNames.ProductFieldNameDimensions ) &&
				d.ContainsKey( DBNames.ProductFieldNameHide ) &&
				d.ContainsKey( DBNames.ProductFieldNameImage ) &&
				d.ContainsKey( DBNames.ProductFieldNameImageRotationAngle ) &&
				d.ContainsKey( DBNames.ProductFieldNameMemo ) &&
				d.ContainsKey( DBNames.ProductFieldNameMinimalStock ) &&
				d.ContainsKey( DBNames.ProductFieldNameName ) &&
				d.ContainsKey( DBNames.ProductFieldNameBrandId ) &&
				d.ContainsKey( DBNames.ProductFieldNamePrice ) &&
				d.ContainsKey( DBNames.ProductFieldNameProjectCosts ) &&
				d.ContainsKey( DBNames.ProductFieldNameStandardOrderQuantity ) &&
				d.ContainsKey( DBNames.ProductFieldNameStorageId ) &&
				d.ContainsKey( DBNames.ProductFieldNameUnitId ) &&
				( string ) d [ DBNames.ProductFieldNameName ] == "New Product" &&
				( string ) d [ DBNames.ProductFieldNameCode ] == "NP001" &&
				( double ) d [ DBNames.ProductFieldNamePrice ] == 10.50 &&
				( double ) d [ DBNames.ProductFieldNameStandardOrderQuantity ] == 5d &&
				( int ) d [ DBNames.ProductFieldNameStorageId ] == 4 &&
				( int ) d [ DBNames.ProductFieldNameUnitId ] == 2 &&
				( int ) d [ DBNames.ProductFieldNameBrandId ] == 5
			) ), Times.Once );
	}

	[TestMethod]
	public async Task UpdateProductAsync_CallsDataService()
	{
		// Arrange
		var parameters = CreateValidProductParameters( 1, "Updated Product", "UP001" );

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		// Act
		await _productService.UpdateProductAsync( parameters );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( DBNames.ProductFieldNameId ) &&
				d.ContainsKey( DBNames.ProductFieldNameBrandId ) &&
				d.ContainsKey( DBNames.ProductFieldNameCategoryId ) &&
				d.ContainsKey( DBNames.ProductFieldNameCode ) &&
				d.ContainsKey( DBNames.ProductFieldNameDimensions ) &&
				d.ContainsKey( DBNames.ProductFieldNameHide ) &&
				d.ContainsKey( DBNames.ProductFieldNameImage ) &&
				d.ContainsKey( DBNames.ProductFieldNameImageRotationAngle ) &&
				d.ContainsKey( DBNames.ProductFieldNameMemo ) &&
				d.ContainsKey( DBNames.ProductFieldNameMinimalStock ) &&
				d.ContainsKey( DBNames.ProductFieldNameName ) &&
				d.ContainsKey( DBNames.ProductFieldNamePrice ) &&
				d.ContainsKey( DBNames.ProductFieldNameProjectCosts ) &&
				d.ContainsKey( DBNames.ProductFieldNameStandardOrderQuantity ) &&
				d.ContainsKey( DBNames.ProductFieldNameStorageId ) &&
				d.ContainsKey( DBNames.ProductFieldNameUnitId ) &&
				( int ) d [ DBNames.ProductFieldNameId ] == 1 &&
				( string ) d [ DBNames.ProductFieldNameName ] == "Updated Product" &&
				( string ) d [ DBNames.ProductFieldNameCode ] == "UP001" &&
				( double ) d [ DBNames.ProductFieldNamePrice ] == 10.50 &&
				( double ) d [ DBNames.ProductFieldNameStandardOrderQuantity ] == 5d &&
				( int ) d [ DBNames.ProductFieldNameStorageId ] == 4 &&
				( int ) d [ DBNames.ProductFieldNameUnitId ] == 2 &&
				( int ) d [ DBNames.ProductFieldNameBrandId ] == 5
			) ), Times.Once );
	}

	[TestMethod]
	public async Task DeleteProductAsync_CallsDataServiceWithCorrectId()
	{
		// Arrange
		var productId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		// Act
		await _productService.DeleteProductAsync( productId );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.ProductFieldNameId}" ) &&
				( int ) d [ $"@{DBNames.ProductFieldNameId}" ] == productId
			) ), Times.Once );
	}

	[TestMethod]
	public async Task DeleteProductAsync_WithConstraintViolation_ThrowsEntityInUseException()
	{
		// Arrange
		var productId = 123;

		// Moq will throw a lightweight test exception that exposes a Number property.
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ThrowsAsync( new TestMySqlException( 1451, "Foreign key constraint" ) );

		// Act / Assert: use try/catch because Assert.ThrowsExceptionAsync is not available
		try
		{
			await _productService.DeleteProductAsync( productId );
			Assert.Fail( "Expected EntityInUseException was not thrown." );
		}
		catch ( EntityInUseException )
		{
			// expected
		}
	}

	[TestMethod]
	public async Task IsProductUsedAsync_WhenProductIsUsed_ReturnsTrue()
	{
		// Arrange
		var productId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1 );

		// Act
		var result = await _productService.IsProductUsedAsync(productId);

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public async Task IsProductUsedAsync_WhenProductIsNotUsed_ReturnsFalse()
	{
		// Arrange
		var productId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0 );

		// Act
		var result = await _productService.IsProductUsedAsync(productId);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithNullName_ReturnsFalse()
	{
		// Act
		var result = await _productService.NameExistsAsync(null);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithWhitespaceName_ReturnsFalse()
	{
		// Act
		var result = await _productService.NameExistsAsync("   ");

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithExistingName_ReturnsTrue()
	{
		// Arrange
		var existingProducts = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Existing Product" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProductModel>>() ) )
			.ReturnsAsync( existingProducts );

		// Act
		var result = await _productService.NameExistsAsync("Existing Product");

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithNonExistingName_ReturnsFalse()
	{
		// Arrange
		var existingProducts = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Product A" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProductModel>>() ) )
			.ReturnsAsync( existingProducts );

		// Act
		var result = await _productService.NameExistsAsync("Product B");

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_IsCaseInsensitive()
	{
		// Arrange
		var existingProducts = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Test Product" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProductModel>>() ) )
			.ReturnsAsync( existingProducts );

		// Act
		var result = await _productService.NameExistsAsync("TEST PRODUCT");

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public void ProductUsed_Property_CanBeSetAndRetrieved()
	{
		// Arrange
		var service = new ProductService(_mockDataService.Object);

		// Act
		service.ProductUsed = true;

		// Assert
		Assert.IsTrue( service.ProductUsed );
	}

	private static Dictionary<string, object?> CreateValidProductParameters(
		int? productId = null,
		string productName = "New Product",
		string productCode = "NP001" )
	{
		var parameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.ProductFieldNameBrandId}", 5 },
			{ $"@{DBNames.ProductFieldNameCategoryId}", 3 },
			{ $"@{DBNames.ProductFieldNameCode}", productCode },
			{ $"@{DBNames.ProductFieldNameDimensions}", "10x20" },
			{ $"@{DBNames.ProductFieldNameHide}", 0 },
			{ $"@{DBNames.ProductFieldNameImage}", null },
			{ $"@{DBNames.ProductFieldNameImageRotationAngle}", 0d },
			{ $"@{DBNames.ProductFieldNameMemo}", "Test memo" },
			{ $"@{DBNames.ProductFieldNameMinimalStock}", 2d },
			{ $"@{DBNames.ProductFieldNameName}", productName },
			{ $"@{DBNames.ProductFieldNamePrice}", 10.50 },
			{ $"@{DBNames.ProductFieldNameProjectCosts}", 0 },
			{ $"@{DBNames.ProductFieldNameStandardOrderQuantity}", 5d },
			{ $"@{DBNames.ProductFieldNameStorageId}", 4 },
			{ $"@{DBNames.ProductFieldNameUnitId}", 2 }
		};

		if ( productId.HasValue )
		{
			parameters.Add( $"@{DBNames.ProductFieldNameId}", productId.Value );
		}

		return parameters;
	}

	// Small test exception that mimics MySqlException's Number property
	private class TestMySqlException : Exception
	{
		public int Number { get; }
		public TestMySqlException( int number, string? message = null ) : base( message )
		{
			Number = number;
		}
	}
}
