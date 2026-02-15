using Modelbouwer.Validators;

using Moq;

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
		_validator = new ProductValidator( _mockProductService.Object );
	}

	[TestMethod]
	public async Task ValidateAsync_WithValidProduct_ReturnsValidResult()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "Valid Product"
		};
		_mockProductService.Setup( s => s.NameExistsAsync( It.IsAny<string>() ) ).ReturnsAsync( false );

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue( result.IsValid );
		Assert.AreEqual( 0, result.Errors.Count );
	}

	[TestMethod]
	public async Task ValidateAsync_WithNullName_ReturnsError()
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
		Assert.IsFalse( result.IsValid );
		Assert.IsTrue( result.Errors.Count > 0 );
		Assert.IsTrue( result.Errors.Any( e => e.Contains( Lang.ExportValidationMessageNameRequirered ) ) );
	}

	[TestMethod]
	public async Task ValidateAsync_WithEmptyName_ReturnsError()
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
		Assert.IsFalse( result.IsValid );
		Assert.IsTrue( result.Errors.Count > 0 );
		Assert.IsTrue( result.Errors.Any( e => e.Contains( Lang.ExportValidationMessageNameRequirered ) ) );
	}

	[TestMethod]
	public async Task ValidateAsync_WithWhitespaceName_ReturnsError()
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
		Assert.IsFalse( result.IsValid );
		Assert.IsTrue( result.Errors.Count > 0 );
		Assert.IsTrue( result.Errors.Any( e => e.Contains( Lang.ExportValidationMessageNameRequirered ) ) );
	}

	[TestMethod]
	public async Task ValidateAsync_WithNameTooLong_ReturnsError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = new string('A', 101) // 101 characters, exceeds limit of 100
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse( result.IsValid );
		Assert.IsTrue( result.Errors.Count > 0 );
		Assert.IsTrue( result.Errors.Any( e => e.Contains( Lang.ExportValidationMessageNameLength ) ) );
	}

	[TestMethod]
	public async Task ValidateAsync_WithNameExactly100Characters_IsValid()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = new string('A', 100) // Exactly 100 characters
		};
		_mockProductService.Setup( s => s.NameExistsAsync( It.IsAny<string>() ) ).ReturnsAsync( false );

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue( result.IsValid );
		Assert.AreEqual( 0, result.Errors.Count );
	}

	[TestMethod]
	public async Task ValidateAsync_NewProductWithDuplicateName_ReturnsError()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0, // New product
			ProductName = "Duplicate Product"
		};
		_mockProductService.Setup( s => s.NameExistsAsync( "Duplicate Product" ) ).ReturnsAsync( true );

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse( result.IsValid );
		Assert.IsTrue( result.Errors.Count > 0 );
		Assert.IsTrue( result.Errors.Any( e => e.Contains( Lang.ExportValidationProductNameExists ) ) );
	}

	[TestMethod]
	public async Task ValidateAsync_ExistingProductWithDuplicateName_DoesNotCheckDuplicates()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 5, // Existing product
			ProductName = "Duplicate Product"
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		// Should not call NameExistsAsync for existing products
		_mockProductService.Verify( s => s.NameExistsAsync( It.IsAny<string>() ), Times.Never );
	}

	[TestMethod]
	public async Task ValidateAsync_NewProductWithUniqueName_IsValid()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "Unique Product"
		};
		_mockProductService.Setup( s => s.NameExistsAsync( "Unique Product" ) ).ReturnsAsync( false );

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue( result.IsValid );
		Assert.AreEqual( 0, result.Errors.Count );
	}

	[TestMethod]
	public async Task ValidateAsync_WithMultipleErrors_ReturnsAllErrors()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = new string('A', 101) // Too long
		};

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsFalse( result.IsValid );
		Assert.IsTrue( result.Errors.Count > 0 );
		// Should have length error
		Assert.IsTrue( result.Errors.Any( e => e.Contains( Lang.ExportValidationMessageNameLength ) ) );
	}

	[TestMethod]
	public async Task ValidateAsync_WithValidNameBoundaryTest_IsValid()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "A" // Minimum valid name
		};
		_mockProductService.Setup( s => s.NameExistsAsync( It.IsAny<string>() ) ).ReturnsAsync( false );

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		Assert.IsTrue( result.IsValid );
	}

	[TestMethod]
	public async Task ValidateAsync_TrimsWhitespaceBeforeValidation()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "  Valid Product  "
		};
		_mockProductService.Setup( s => s.NameExistsAsync( It.IsAny<string>() ) ).ReturnsAsync( false );

		// Act
		var result = await _validator.ValidateAsync(product);

		// Assert
		// Name has content after trimming, should be valid
		Assert.IsTrue( result.IsValid );
	}

	[TestMethod]
	public async Task ValidateAsync_CallsNameExistsWithCorrectParameter()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 0,
			ProductName = "Test Product"
		};
		_mockProductService.Setup( s => s.NameExistsAsync( "Test Product" ) ).ReturnsAsync( false );

		// Act
		await _validator.ValidateAsync( product );

		// Assert
		_mockProductService.Verify( s => s.NameExistsAsync( "Test Product" ), Times.Once );
	}
}