namespace Modelbouwer.UnitTests.Models;

[TestClass]
public class ProductModelTests
{
	[TestMethod]
	public void ProductModel_DefaultConstructor_ShouldInitializeProperties()
	{
		// Arrange & Act
		var product = new ProductModel();

		// Assert
		Assert.IsNotNull(product);
		Assert.AreEqual(0, product.ProductId);
		Assert.AreEqual(0.0, product.ProductPrice);
		Assert.AreEqual(0.0, product.ProductPackagePrice);
		Assert.AreEqual(0.0, product.ProductStandardQuantity);
	}

	[TestMethod]
	public void ProductModel_CopyConstructor_ShouldCopyPriceProperties()
	{
		// Arrange
		var original = new ProductModel
		{
			ProductPrice = 10.5,
			ProductPackagePrice = 52.5,
			ProductStandardQuantity = 5.0
		};

		// Act
		var copy = new ProductModel(original);

		// Assert
		Assert.AreEqual(original.ProductPrice, copy.ProductPrice);
		Assert.AreEqual(original.ProductPackagePrice, copy.ProductPackagePrice);
		Assert.AreEqual(original.ProductStandardQuantity, copy.ProductStandardQuantity);
	}

	[TestMethod]
	public void ProductPrice_WhenChanged_ShouldUpdatePackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 10.0
		};

		// Act
		product.ProductPrice = 5.0;

		// Assert
		Assert.AreEqual(50.0, product.ProductPackagePrice);
	}

	[TestMethod]
	public void ProductPrice_WhenStandardQuantityIsZero_ShouldNotUpdatePackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 0.0,
			ProductPackagePrice = 100.0
		};

		// Act
		product.ProductPrice = 5.0;

		// Assert
		Assert.AreEqual(100.0, product.ProductPackagePrice);
	}

	[TestMethod]
	public void ProductPackagePrice_WhenChanged_ShouldUpdatePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 10.0
		};

		// Act
		product.ProductPackagePrice = 100.0;

		// Assert
		Assert.AreEqual(10.0, product.ProductPrice);
	}

	[TestMethod]
	public void ProductPackagePrice_WhenStandardQuantityIsZero_ShouldNotUpdatePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 0.0,
			ProductPrice = 5.0
		};

		// Act
		product.ProductPackagePrice = 100.0;

		// Assert
		Assert.AreEqual(5.0, product.ProductPrice);
	}

	[TestMethod]
	public void ProductStandardQuantity_WhenChangedWithPrice_ShouldUpdatePackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductPrice = 10.0
		};

		// Act
		product.ProductStandardQuantity = 5.0;

		// Assert
		Assert.AreEqual(50.0, product.ProductPackagePrice);
	}

	[TestMethod]
	public void ProductStandardQuantity_WhenChangedWithPackagePrice_ShouldUpdatePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductPackagePrice = 100.0,
			ProductPrice = 0.0
		};

		// Act
		product.ProductStandardQuantity = 10.0;

		// Assert
		Assert.AreEqual(10.0, product.ProductPrice);
	}

	[TestMethod]
	public void ProductPrice_Calculation_ShouldRoundToSixDecimals()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 3.0
		};

		// Act
		product.ProductPackagePrice = 10.0;

		// Assert
		Assert.AreEqual(3.333333, product.ProductPrice);
	}

	[TestMethod]
	public void ProductPackagePrice_Calculation_ShouldRoundToTwoDecimals()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 3.0
		};

		// Act
		product.ProductPrice = 3.333333;

		// Assert
		Assert.AreEqual(10.0, product.ProductPackagePrice);
	}

	[TestMethod]
	public void Name_Property_ShouldReturnProductName()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductName = "Test Product"
		};

		// Act
		var name = product.Name;

		// Assert
		Assert.AreEqual("Test Product", name);
	}

	[TestMethod]
	public void ProductModel_AllProperties_ShouldBeSettable()
	{
		// Arrange & Act
		var product = new ProductModel
		{
			ProductId = 1,
			ProductName = "Test Product",
			ProductCode = "TP001",
			ProductPrice = 10.5,
			ProductMinimalStock = 5.0,
			ProductBrandId = 2,
			ProductCategoryId = 3,
			ProductUnitId = 4,
			ProductStorageId = 5,
			ProductProjectCosts = 1,
			ProductHide = 0,
			ProductDimensions = "10x10x10",
			ProductMemo = "Test memo"
		};

		// Assert
		Assert.AreEqual(1, product.ProductId);
		Assert.AreEqual("Test Product", product.ProductName);
		Assert.AreEqual("TP001", product.ProductCode);
		Assert.AreEqual(10.5, product.ProductPrice);
		Assert.AreEqual(5.0, product.ProductMinimalStock);
		Assert.AreEqual(2, product.ProductBrandId);
		Assert.AreEqual(3, product.ProductCategoryId);
		Assert.AreEqual(4, product.ProductUnitId);
		Assert.AreEqual(5, product.ProductStorageId);
		Assert.AreEqual(1, product.ProductProjectCosts);
		Assert.AreEqual(0, product.ProductHide);
		Assert.AreEqual("10x10x10", product.ProductDimensions);
		Assert.AreEqual("Test memo", product.ProductMemo);
	}

	[TestMethod]
	public void ProductImage_ShouldAcceptByteArray()
	{
		// Arrange
		var product = new ProductModel();
		var imageBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		product.ProductImage = imageBytes;

		// Assert
		Assert.IsNotNull(product.ProductImage);
		Assert.AreEqual(4, product.ProductImage.Length);
		CollectionAssert.AreEqual(imageBytes, product.ProductImage);
	}

	[TestMethod]
	public void ProductImageRotationAngle_ShouldBeSettable()
	{
		// Arrange
		var product = new ProductModel();

		// Act
		product.ProductImageRotationAngle = "90";

		// Assert
		Assert.AreEqual("90", product.ProductImageRotationAngle);
	}

	[TestMethod]
	public void PriceCalculation_ComplexScenario_ShouldMaintainConsistency()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 12.0
		};

		// Act - Set price, should update package price
		product.ProductPrice = 2.5;
		var packagePrice1 = product.ProductPackagePrice; // Should be 30.0

		// Change standard quantity, should recalculate package price
		product.ProductStandardQuantity = 6.0;
		var packagePrice2 = product.ProductPackagePrice; // Should be 15.0

		// Assert
		Assert.AreEqual(30.0, packagePrice1);
		Assert.AreEqual(15.0, packagePrice2);
		Assert.AreEqual(2.5, product.ProductPrice); // Price should remain unchanged
	}

	[TestMethod]
	public void ColumnMappings_ShouldContainExpectedKeys()
	{
		// Arrange & Act
		var mappings = ProductModel.ColumnMappings;

		// Assert
		Assert.IsNotNull(mappings);
		Assert.IsTrue(mappings.ContainsKey("Id"));
		Assert.IsTrue(mappings.ContainsKey("ProductCode"));
		Assert.IsTrue(mappings.ContainsKey("ProductName"));
	}

	[TestMethod]
	public void HeaderToPropertyMap_ShouldNotBeNull()
	{
		// Arrange & Act
		var map = ProductModel.HeaderToPropertyMap;

		// Assert
		Assert.IsNotNull(map);
		Assert.IsTrue(map.Count > 0);
	}
}