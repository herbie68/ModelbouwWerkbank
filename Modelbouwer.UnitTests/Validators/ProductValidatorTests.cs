namespace Modelbouwer.UnitTests.Validators;

[TestClass]
public class ProductValidatorTests
{
	private Mock<IProductService> _mockProductService;
	private ProductValidator _validator;

	[TestInitialize]
	public void Setup()
	{
		_mockProductService = new Mock<IProductService>();
		_validator = new ProductValidator(_mockProductService.Object);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameIsNull_ShouldReturnError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = null
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse(result.IsValid);
		Assert.IsTrue(result.Errors.Count > 0);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameIsEmpty_ShouldReturnError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = ""
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse(result.IsValid);
		Assert.IsTrue(result.Errors.Count > 0);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameIsWhitespace_ShouldReturnError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "   "
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse(result.IsValid);
		Assert.IsTrue(result.Errors.Count > 0);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameExceedsMaxLength_ShouldReturnError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = new string('A', 101) // 101 characters
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse(result.IsValid);
		Assert.IsTrue(result.Errors.Count > 0);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameIsExactly100Characters_ShouldPass()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = new string('A', 100) // 100 characters
		};

		_mockProductService
			.Setup(x => x.NameExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(false);

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(0, result.Errors.Count);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNewProductWithValidName_ShouldPass()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "Valid Product Name"
		};

		_mockProductService
			.Setup(x => x.NameExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(false);

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(0, result.Errors.Count);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNewProductWithDuplicateName_ShouldReturnError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "Duplicate Product"
		};

		_mockProductService
			.Setup(x => x.NameExistsAsync("Duplicate Product"))
			.ReturnsAsync(true);

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse(result.IsValid);
		Assert.IsTrue(result.Errors.Count > 0);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenExistingProductWithDuplicateName_ShouldNotCheckDuplicate()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "Existing Product"
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		_mockProductService.Verify(x => x.NameExistsAsync(It.IsAny<string>()), Times.Never);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenExistingProductWithValidName_ShouldPass()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "Valid Product Name"
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(0, result.Errors.Count);
	}

	[TestMethod]
	public async Task ValidateAsync_WithMultipleErrors_ShouldReturnAllErrors()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "" // Empty name
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse(result.IsValid);
		Assert.IsTrue(result.Errors.Count >= 1);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameHasLeadingOrTrailingSpaces_ShouldPass()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "  Valid Name  "
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue(result.IsValid);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameIs99Characters_ShouldPass()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = new string('B', 99)
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue(result.IsValid);
	}

	[TestMethod]
	public async Task ValidateAsync_NewProduct_ShouldCallNameExistsAsync()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "New Product"
		};

		_mockProductService
			.Setup(x => x.NameExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(false);

		// Act
		await _validator.ValidateAsync(product);

		// Assert
		_mockProductService.Verify(x => x.NameExistsAsync("New Product"), Times.Once);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenNameIsMinimalValidLength_ShouldPass()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "A"
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue(result.IsValid);
	}

	[TestMethod]
	public async Task ValidateAsync_WhenProductIdIsNegative_DuplicateCheckShouldNotRun()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = -1,
			ProductName = "Test Product"
		};

		// Act
		await _validator.ValidateAsync(product);

		// Assert
		_mockProductService.Verify(x => x.NameExistsAsync(It.IsAny<string>()), Times.Never);
	}
}