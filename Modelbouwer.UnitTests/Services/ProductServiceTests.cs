namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class ProductServiceTests
{
	private Mock<GenericDataService> _mockDataService;
	private ProductService _productService;

	[TestInitialize]
	public void Setup()
	{
		_mockDataService = new Mock<GenericDataService>();
		_productService = new ProductService(_mockDataService.Object);
	}

	[TestMethod]
	public async Task GetAllProductsAsync_ShouldReturnListOfProducts()
	{
		// Arrange
		var expectedProducts = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Product 1" },
			new ProductModel { ProductId = 2, ProductName = "Product 2" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProductModel>>()))
			.ReturnsAsync(expectedProducts);

		// Act
		var result = await _productService.GetAllProductsAsync();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.AreEqual("Product 1", result[0].ProductName);
		Assert.AreEqual("Product 2", result[1].ProductName);
	}

	[TestMethod]
	public async Task GetAllBrandsAsync_ShouldReturnListOfBrands()
	{
		// Arrange
		var expectedBrands = new List<BrandModel>
		{
			new BrandModel { BrandId = 1, BrandName = "Brand 1" },
			new BrandModel { BrandId = 2, BrandName = "Brand 2" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, BrandModel>>()))
			.ReturnsAsync(expectedBrands);

		// Act
		var result = await _productService.GetAllBrandsAsync();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
	}

	[TestMethod]
	public async Task GetAllUnitsAsync_ShouldReturnListOfUnits()
	{
		// Arrange
		var expectedUnits = new List<UnitModel>
		{
			new UnitModel { UnitId = 1, UnitName = "Unit 1" },
			new UnitModel { UnitId = 2, UnitName = "Unit 2" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, UnitModel>>()))
			.ReturnsAsync(expectedUnits);

		// Act
		var result = await _productService.GetAllUnitsAsync();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
	}

	[TestMethod]
	public async Task GetAllCategoriesAsync_ShouldReturnListOfCategories()
	{
		// Arrange
		var expectedCategories = new List<CategoryModel>
		{
			new CategoryModel { CategoryId = 1, CategoryName = "Category 1" },
			new CategoryModel { CategoryId = 2, CategoryName = "Category 2" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, CategoryModel>>()))
			.ReturnsAsync(expectedCategories);

		// Act
		var result = await _productService.GetAllCategoriesAsync();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
	}

	[TestMethod]
	public async Task InsertNewProductAsync_ShouldReturnNewProductId()
	{
		// Arrange
		var parameters = new Dictionary<string, object?>
		{
			{ "@ProductName", "New Product" }
		};

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(123);

		// Act
		var result = await _productService.InsertNewProductAsync(parameters);

		// Assert
		Assert.AreEqual(123, result);
	}

	[TestMethod]
	public async Task UpdateProductAsync_ShouldCallDataService()
	{
		// Arrange
		var parameters = new Dictionary<string, object?>
		{
			{ "@ProductId", 1 },
			{ "@ProductName", "Updated Product" }
		};

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		await _productService.UpdateProductAsync(parameters);

		// Assert
		_mockDataService.Verify(x => x.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.IsAny<Dictionary<string, object>>()), Times.Once);
	}

	[TestMethod]
	public async Task DeleteProductAsync_ShouldCallDataService()
	{
		// Arrange
		int productId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		await _productService.DeleteProductAsync(productId);

		// Assert
		_mockDataService.Verify(x => x.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>(d => d.ContainsKey("@ProductId"))), Times.Once);
	}

	[TestMethod]
	[ExpectedException(typeof(EntityInUseException))]
	public async Task DeleteProductAsync_WhenProductInUse_ShouldThrowEntityInUseException()
	{
		// Arrange
		int productId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ThrowsAsync(new MySqlException("Foreign key constraint", 1451));

		// Act
		await _productService.DeleteProductAsync(productId);

		// Assert - ExpectedException
	}

	[TestMethod]
	public async Task IsProductUsedAsync_WhenProductIsUsed_ShouldReturnTrue()
	{
		// Arrange
		int productId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<int>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(1);

		// Act
		var result = await _productService.IsProductUsedAsync(productId);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsProductUsedAsync_WhenProductIsNotUsed_ShouldReturnFalse()
	{
		// Arrange
		int productId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<int>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		var result = await _productService.IsProductUsedAsync(productId);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameIsNull_ShouldReturnFalse()
	{
		// Arrange
		string? productName = null;

		// Act
		var result = await _productService.NameExistsAsync(productName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameIsEmpty_ShouldReturnFalse()
	{
		// Arrange
		string productName = "";

		// Act
		var result = await _productService.NameExistsAsync(productName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameIsWhitespace_ShouldReturnFalse()
	{
		// Arrange
		string productName = "   ";

		// Act
		var result = await _productService.NameExistsAsync(productName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameExists_ShouldReturnTrue()
	{
		// Arrange
		string productName = "Existing Product";
		var products = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Existing Product" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProductModel>>()))
			.ReturnsAsync(products);

		// Act
		var result = await _productService.NameExistsAsync(productName);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameDoesNotExist_ShouldReturnFalse()
	{
		// Arrange
		string productName = "Non-Existing Product";
		var products = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Existing Product" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProductModel>>()))
			.ReturnsAsync(products);

		// Act
		var result = await _productService.NameExistsAsync(productName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_ShouldBeCaseInsensitive()
	{
		// Arrange
		string productName = "EXISTING PRODUCT";
		var products = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "existing product" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProductModel>>()))
			.ReturnsAsync(products);

		// Act
		var result = await _productService.NameExistsAsync(productName);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void ProductUsed_Property_ShouldBeSettable()
	{
		// Arrange
		var service = new ProductService(_mockDataService.Object);

		// Act
		service.ProductUsed = true;

		// Assert
		Assert.IsTrue(service.ProductUsed);
	}

	[TestMethod]
	public void QueryStrings_ShouldNotBeNullOrEmpty()
	{
		// Arrange & Act & Assert
		Assert.IsFalse(string.IsNullOrEmpty(_productService.CompleteProductList));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.CompleteBrandList));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.CompleteUnitList));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.CompleteCategoryList));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.AddNewProductQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.UpdateProductQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.DeleteProductQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.ProductNameExistsQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_productService.ProductUsedQuery));
	}
}