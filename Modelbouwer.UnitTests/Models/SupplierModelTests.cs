namespace Modelbouwer.UnitTests.Models;

[TestClass]
public class SupplierModelTests
{
	[TestMethod]
	public void SupplierModel_DefaultConstructor_ShouldInitializeProperties()
	{
		// Arrange & Act
		var supplier = new SupplierModel();

		// Assert
		Assert.IsNotNull(supplier);
		Assert.AreEqual(0, supplier.Id);
		Assert.AreEqual(0, supplier.CountryId);
		Assert.AreEqual(0, supplier.CurrencyId);
	}

	[TestMethod]
	public void SupplierModel_AllGeneralProperties_ShouldBeSettable()
	{
		// Arrange & Act
		var supplier = new SupplierModel
		{
			Id = 1,
			CountryId = 2,
			CurrencyId = 3,
			Code = "SUP001",
			Name = "Test Supplier",
			Address1 = "123 Main St",
			Address2 = "Suite 100",
			City = "Amsterdam",
			Zip = "1000 AA",
			Country = "Netherlands",
			Currency = "EUR",
			Mail = "test@supplier.com",
			Phone = "+31 20 1234567",
			Url = "https://www.supplier.com",
			Memo = "Test memo",
			CurrencyRate = 1.0,
			MinOrderCosts = 10.0,
			MinShippingCosts = 5.0,
			OrderCosts = 15.0,
			ShippingCosts = 7.5
		};

		// Assert
		Assert.AreEqual(1, supplier.Id);
		Assert.AreEqual(2, supplier.CountryId);
		Assert.AreEqual(3, supplier.CurrencyId);
		Assert.AreEqual("SUP001", supplier.Code);
		Assert.AreEqual("Test Supplier", supplier.Name);
		Assert.AreEqual("123 Main St", supplier.Address1);
		Assert.AreEqual("Suite 100", supplier.Address2);
		Assert.AreEqual("Amsterdam", supplier.City);
		Assert.AreEqual("1000 AA", supplier.Zip);
		Assert.AreEqual("Netherlands", supplier.Country);
		Assert.AreEqual("EUR", supplier.Currency);
		Assert.AreEqual("test@supplier.com", supplier.Mail);
		Assert.AreEqual("+31 20 1234567", supplier.Phone);
		Assert.AreEqual("https://www.supplier.com", supplier.Url);
		Assert.AreEqual("Test memo", supplier.Memo);
		Assert.AreEqual(1.0, supplier.CurrencyRate);
		Assert.AreEqual(10.0, supplier.MinOrderCosts);
		Assert.AreEqual(5.0, supplier.MinShippingCosts);
		Assert.AreEqual(15.0, supplier.OrderCosts);
		Assert.AreEqual(7.5, supplier.ShippingCosts);
	}

	[TestMethod]
	public void CountryName_WhenCountryListIsNull_ShouldReturnNull()
	{
		// Arrange
		var supplier = new SupplierModel
		{
			CountryId = 1,
			CountryList = null
		};

		// Act
		var countryName = supplier.CountryName;

		// Assert
		Assert.IsNull(countryName);
	}

	[TestMethod]
	public void CountryName_WhenCountryExists_ShouldReturnCountryName()
	{
		// Arrange
		var countries = new List<CountryModel>
		{
			new CountryModel { CountryId = 1, CountryName = "Netherlands" },
			new CountryModel { CountryId = 2, CountryName = "Belgium" }
		};

		var supplier = new SupplierModel
		{
			CountryId = 1,
			CountryList = countries
		};

		// Act
		var countryName = supplier.CountryName;

		// Assert
		Assert.AreEqual("Netherlands", countryName);
	}

	[TestMethod]
	public void CountryName_WhenCountryNotFound_ShouldReturnNull()
	{
		// Arrange
		var countries = new List<CountryModel>
		{
			new CountryModel { CountryId = 1, CountryName = "Netherlands" },
			new CountryModel { CountryId = 2, CountryName = "Belgium" }
		};

		var supplier = new SupplierModel
		{
			CountryId = 99,
			CountryList = countries
		};

		// Act
		var countryName = supplier.CountryName;

		// Assert
		Assert.IsNull(countryName);
	}

	[TestMethod]
	public void CountryName_WhenCountryListIsEmpty_ShouldReturnNull()
	{
		// Arrange
		var supplier = new SupplierModel
		{
			CountryId = 1,
			CountryList = new List<CountryModel>()
		};

		// Act
		var countryName = supplier.CountryName;

		// Assert
		Assert.IsNull(countryName);
	}

	[TestMethod]
	public void ColumnMappings_ShouldContainExpectedKeys()
	{
		// Arrange & Act
		var mappings = SupplierModel.ColumnMappings;

		// Assert
		Assert.IsNotNull(mappings);
		Assert.IsTrue(mappings.ContainsKey("Id"));
		Assert.IsTrue(mappings.ContainsKey("Code"));
		Assert.IsTrue(mappings.ContainsKey("Name"));
	}

	[TestMethod]
	public void ColumnMappings_ShouldContainMultipleLanguages()
	{
		// Arrange & Act
		var nameMappings = SupplierModel.ColumnMappings["Name"];

		// Assert
		Assert.IsNotNull(nameMappings);
		Assert.IsTrue(nameMappings.Length >= 3);
	}

	[TestMethod]
	public void HeaderToPropertyMap_ShouldNotBeNull()
	{
		// Arrange & Act
		var map = SupplierModel.HeaderToPropertyMap;

		// Assert
		Assert.IsNotNull(map);
		Assert.IsTrue(map.Count > 0);
	}

	[TestMethod]
	public void HeaderToPropertyMap_ShouldContainExpectedMappings()
	{
		// Arrange & Act
		var map = SupplierModel.HeaderToPropertyMap;

		// Assert
		Assert.IsTrue(map.ContainsValue("Id"));
		Assert.IsTrue(map.ContainsValue("Code"));
		Assert.IsTrue(map.ContainsValue("Name"));
		Assert.IsTrue(map.ContainsValue("CountryId"));
		Assert.IsTrue(map.ContainsValue("CurrencyId"));
	}

	[TestMethod]
	public void SupplierModel_NumericProperties_ShouldAcceptDoubleValues()
	{
		// Arrange
		var supplier = new SupplierModel();

		// Act
		supplier.CurrencyRate = 1.25;
		supplier.MinOrderCosts = 10.50;
		supplier.MinShippingCosts = 5.75;
		supplier.OrderCosts = 15.25;
		supplier.ShippingCosts = 7.99;

		// Assert
		Assert.AreEqual(1.25, supplier.CurrencyRate);
		Assert.AreEqual(10.50, supplier.MinOrderCosts);
		Assert.AreEqual(5.75, supplier.MinShippingCosts);
		Assert.AreEqual(15.25, supplier.OrderCosts);
		Assert.AreEqual(7.99, supplier.ShippingCosts);
	}

	[TestMethod]
	public void SupplierModel_ContactInfo_ShouldAcceptNullValues()
	{
		// Arrange & Act
		var supplier = new SupplierModel
		{
			Mail = null,
			Phone = null,
			Url = null
		};

		// Assert
		Assert.IsNull(supplier.Mail);
		Assert.IsNull(supplier.Phone);
		Assert.IsNull(supplier.Url);
	}

	[TestMethod]
	public void SupplierModel_AddressFields_ShouldStoreMultipleLines()
	{
		// Arrange & Act
		var supplier = new SupplierModel
		{
			Address1 = "Building A",
			Address2 = "Floor 3, Room 301"
		};

		// Assert
		Assert.AreEqual("Building A", supplier.Address1);
		Assert.AreEqual("Floor 3, Room 301", supplier.Address2);
	}
}