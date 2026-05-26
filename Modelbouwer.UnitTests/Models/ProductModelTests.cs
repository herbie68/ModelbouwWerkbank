namespace Modelbouwer.UnitTests.Models;

[TestClass]
public class ProductModelTests
{
	[TestMethod]
	public void ProductModel_DefaultConstructor_InitializesProperties()
	{
		// Arrange & Act
		var product = new ProductModel();

		// Assert
		Assert.AreEqual( 0, product.ProductId );
		Assert.AreEqual( 0.0, product.ProductPrice );
		Assert.AreEqual( 0.0, product.ProductPackagePrice );
		Assert.AreEqual( 0.0, product.ProductStandardQuantity );
	}

	[TestMethod]
	public void ProductModel_CopyConstructor_CopiesPriceProperties()
	{
		// Arrange
		var original = new ProductModel
		{
			ProductPrice = 10.5,
			ProductPackagePrice = 52.5,
			ProductStandardQuantity = 5
		};

		// Act
		var copy = new ProductModel(original);

		// Assert
		Assert.AreEqual( 10.5, copy.ProductPrice );
		Assert.AreEqual( 52.5, copy.ProductPackagePrice );
		Assert.AreEqual( 5, copy.ProductStandardQuantity );
	}

	[TestMethod]
	public void OnProductPriceChanged_WithPositiveStandardQuantity_CalculatesPackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 5
		};

		// Act
		product.ProductPrice = 10.0;

		// Assert
		Assert.AreEqual( 50.0, product.ProductPackagePrice );
	}

	[TestMethod]
	public void OnProductPriceChanged_WithZeroStandardQuantity_DoesNotCalculatePackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 0,
			ProductPackagePrice = 100.0
		};

		// Act
		product.ProductPrice = 10.0;

		// Assert
		Assert.AreEqual( 100.0, product.ProductPackagePrice ); // Should remain unchanged
	}

	[TestMethod]
	public void OnProductPriceChanged_RoundsPackagePriceToTwoDecimals()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 3
		};

		// Act
		product.ProductPrice = 10.555;

		// Assert
		Assert.AreEqual( 31.67, product.ProductPackagePrice );
	}

	[TestMethod]
	public void OnProductPackagePriceChanged_WithPositiveStandardQuantity_CalculatesPrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 5
		};

		// Act
		product.ProductPackagePrice = 50.0;

		// Assert
		Assert.AreEqual( 10.0, product.ProductPrice );
	}

	[TestMethod]
	public void OnProductPackagePriceChanged_WithZeroStandardQuantity_DoesNotCalculatePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 0,
			ProductPrice = 15.0
		};

		// Act
		product.ProductPackagePrice = 100.0;

		// Assert
		Assert.AreEqual( 15.0, product.ProductPrice ); // Should remain unchanged
	}

	[TestMethod]
	public void OnProductPackagePriceChanged_RoundsPriceToSixDecimals()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 3
		};

		// Act
		product.ProductPackagePrice = 31.67;

		// Assert
		Assert.AreEqual( 10.556667, product.ProductPrice );
	}

	[TestMethod]
	public void OnProductStandardQuantityChanged_WithExistingPrice_CalculatesPackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductPrice = 10.0
		};

		// Act
		product.ProductStandardQuantity = 5;

		// Assert
		Assert.AreEqual( 50.0, product.ProductPackagePrice );
	}

	[TestMethod]
	public void OnProductStandardQuantityChanged_WithExistingPackagePrice_CalculatesPrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductPackagePrice = 50.0
		};

		// Act
		product.ProductStandardQuantity = 5;

		// Assert
		Assert.AreEqual( 10.0, product.ProductPrice );
	}

	[TestMethod]
	public void OnProductStandardQuantityChanged_WithBothPricesZero_DoesNotCalculate()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductPrice = 0.0,
			ProductPackagePrice = 0.0
		};

		// Act
		product.ProductStandardQuantity = 5;

		// Assert
		Assert.AreEqual( 0.0, product.ProductPrice );
		Assert.AreEqual( 0.0, product.ProductPackagePrice );
	}

	[TestMethod]
	public void OnProductStandardQuantityChanged_PrefersProductPriceOverPackagePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductPrice = 10.0,
			ProductPackagePrice = 100.0
		};

		// Act
		product.ProductStandardQuantity = 5;

		// Assert
		// ProductPrice takes precedence, so PackagePrice should be recalculated
		Assert.AreEqual( 50.0, product.ProductPackagePrice );
	}

	[TestMethod]
	public void OnProductStandardQuantityChanged_ToZeroWithExistingPackagePrice_DoesNotCreateInfinitePrice()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 5,
			ProductPrice = 10.0
		};

		// Act
		product.ProductStandardQuantity = 0;

		// Assert
		Assert.AreEqual( 10.0, product.ProductPrice );
		Assert.IsFalse( double.IsInfinity( product.ProductPrice ) );
		Assert.IsFalse( double.IsNaN( product.ProductPrice ) );
	}

	[TestMethod]
	public void Name_Property_ReturnsProductName()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductName = "Test Product"
		};

		// Act
		var name = product.Name;

		// Assert
		Assert.AreEqual( "Test Product", name );
	}

	[TestMethod]
	public void ProductModel_PriceCalculations_HandleVerySmallNumbers()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 1000
		};

		// Act
		product.ProductPrice = 0.001;

		// Assert
		Assert.AreEqual( 1.0, product.ProductPackagePrice );
	}

	[TestMethod]
	public void ProductModel_PriceCalculations_HandleVeryLargeNumbers()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductStandardQuantity = 10000
		};

		// Act
		product.ProductPrice = 999.999;

		// Assert
		Assert.AreEqual( 9999990.0, product.ProductPackagePrice );
	}

	[TestMethod]
	public void HeaderToPropertyMap_ContainsAllExpectedMappings()
	{
		// Assert
		Assert.IsTrue( ProductModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductFieldNameBrandId ) );
		Assert.IsTrue( ProductModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductFieldNameCategoryId ) );
		Assert.IsTrue( ProductModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductFieldNameCode ) );
		Assert.IsTrue( ProductModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductFieldNameName ) );
		Assert.IsTrue( ProductModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductFieldNamePrice ) );
	}
}
