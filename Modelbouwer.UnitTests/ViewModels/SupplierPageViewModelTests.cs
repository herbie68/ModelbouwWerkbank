namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class SupplierPageViewModelTests
{
	private Mock<ISupplierService> _mockSupplierService;
	private Mock<ICountryService> _mockCountryService;
	private Mock<ICurrencyService> _mockCurrencyService;
	private Mock<IContactService> _mockContactService;
	private Mock<IContactTypeService> _mockContactTypeService;
	private Mock<IEntityValidator<SupplierModel>> _mockValidator;
	private SupplierPageViewModel _viewModel;

	[TestInitialize]
	public void Setup()
	{
		_mockSupplierService = new Mock<ISupplierService>();
		_mockCountryService = new Mock<ICountryService>();
		_mockCurrencyService = new Mock<ICurrencyService>();
		_mockContactService = new Mock<IContactService>();
		_mockContactTypeService = new Mock<IContactTypeService>();
		_mockValidator = new Mock<IEntityValidator<SupplierModel>>();

		_mockSupplierService
			.Setup(x => x.GetAllSuppliersAsync())
			.ReturnsAsync(new List<SupplierModel>());

		_mockCountryService
			.Setup(x => x.GetAllCountriesAsync())
			.ReturnsAsync(new List<CountryModel>());

		_mockCurrencyService
			.Setup(x => x.GetAllCurrenciesAsync())
			.ReturnsAsync(new List<CurrencyModel>());

		_mockContactService
			.Setup(x => x.GetAllContactsAsync())
			.ReturnsAsync(new List<SupplierContactModel>());

		_mockContactTypeService
			.Setup(x => x.GetAllContactTypesAsync())
			.ReturnsAsync(new List<ContactTypeModel>());

		_viewModel = new SupplierPageViewModel(
			_mockSupplierService.Object,
			_mockCountryService.Object,
			_mockCurrencyService.Object,
			_mockContactService.Object,
			_mockContactTypeService.Object,
			_mockValidator.Object);
	}

	[TestMethod]
	public void Constructor_ShouldInitializeCollections()
	{
		// Assert
		Assert.IsNotNull(_viewModel.Suppliers);
		Assert.IsNotNull(_viewModel.SupplierCountry);
		Assert.IsNotNull(_viewModel.SupplierCurrency);
		Assert.IsNotNull(_viewModel.SupplierContactFunctions);
		Assert.IsNotNull(_viewModel.Contacts);
		Assert.IsNotNull(_viewModel.FilteredContacts);
	}

	[TestMethod]
	public void Constructor_ShouldThrowWhenContactServiceIsNull()
	{
		// Arrange & Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => new SupplierPageViewModel(
			_mockSupplierService.Object,
			_mockCountryService.Object,
			_mockCurrencyService.Object,
			null!,
			_mockContactTypeService.Object,
			_mockValidator.Object));
	}

	[TestMethod]
	public void Constructor_ShouldThrowWhenContactTypeServiceIsNull()
	{
		// Arrange & Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => new SupplierPageViewModel(
			_mockSupplierService.Object,
			_mockCountryService.Object,
			_mockCurrencyService.Object,
			_mockContactService.Object,
			null!,
			_mockValidator.Object));
	}

	[TestMethod]
	public void Suppliers_ShouldReturnItemsCollection()
	{
		// Arrange & Act
		var suppliers = _viewModel.Suppliers;

		// Assert
		Assert.IsNotNull(suppliers);
		Assert.AreSame(_viewModel.Items, suppliers);
	}

	[TestMethod]
	public void TotalSupplierCount_ShouldReturnItemCount()
	{
		// Arrange
		_viewModel.Items.Add(new SupplierModel { Id = 1 });
		_viewModel.Items.Add(new SupplierModel { Id = 2 });

		// Act
		var count = _viewModel.TotalSupplierCount;

		// Assert
		Assert.AreEqual(2, count);
	}

	[TestMethod]
	public void SelectedCountry_WhenSet_ShouldUpdateSelectedItemCountryId()
	{
		// Arrange
		var supplier = new SupplierModel { Id = 1 };
		var country = new CountryModel { CountryId = 5, CountryName = "Test Country" };
		_viewModel.SelectedItem = supplier;

		// Act
		_viewModel.SelectedCountry = country;

		// Assert
		Assert.AreEqual(5, supplier.CountryId);
	}

	[TestMethod]
	public void SelectedCurrency_WhenSet_ShouldUpdateSelectedItemCurrencyId()
	{
		// Arrange
		var supplier = new SupplierModel { Id = 1 };
		var currency = new CurrencyModel { CurrencyId = 3, CurrencyName = "EUR" };
		_viewModel.SelectedItem = supplier;

		// Act
		_viewModel.SelectedCurrency = currency;

		// Assert
		Assert.AreEqual(3, supplier.CurrencyId);
	}

	[TestMethod]
	public void SelectedContact_WhenSet_ShouldUpdateSelectedContactFunction()
	{
		// Arrange
		var contactType = new ContactTypeModel { ContactTypeId = 2, ContactTypeName = "Manager" };
		_viewModel.SupplierContactFunctions.Add(contactType);

		var contact = new SupplierContactModel
		{
			SupplierContactId = 1,
			ContactTypeId = 2
		};

		// Act
		_viewModel.SelectedContact = contact;

		// Assert
		Assert.AreEqual(contactType, _viewModel.SelectedContactFunction);
	}

	[TestMethod]
	public void SelectedContactFunction_WhenSet_ShouldUpdateSelectedContactTypeId()
	{
		// Arrange
		var contact = new SupplierContactModel { SupplierContactId = 1 };
		var contactType = new ContactTypeModel { ContactTypeId = 5, ContactTypeName = "Sales" };
		_viewModel.SelectedContact = contact;

		// Act
		_viewModel.SelectedContactFunction = contactType;

		// Assert
		Assert.AreEqual(5, contact.ContactTypeId);
	}

	[TestMethod]
	public void FilterSupplier_WhenSearchTextIsEmpty_ShouldReturnTrue()
	{
		// Arrange
		_viewModel.SearchText = "";
		var supplier = new SupplierModel { Name = "Test Supplier" };

		// Act
		var result = _viewModel.FilterSupplier(supplier);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilterSupplier_WhenSearchTextMatches_ShouldReturnTrue()
	{
		// Arrange
		_viewModel.SearchText = "Test";
		var supplier = new SupplierModel { Name = "Test Supplier" };

		// Act
		var result = _viewModel.FilterSupplier(supplier);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilterSupplier_WhenSearchTextDoesNotMatch_ShouldReturnFalse()
	{
		// Arrange
		_viewModel.SearchText = "XYZ";
		var supplier = new SupplierModel { Name = "Test Supplier" };

		// Act
		var result = _viewModel.FilterSupplier(supplier);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FilterSupplier_ShouldBeCaseInsensitive()
	{
		// Arrange
		_viewModel.SearchText = "test";
		var supplier = new SupplierModel { Name = "TEST SUPPLIER" };

		// Act
		var result = _viewModel.FilterSupplier(supplier);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilterSupplier_WhenObjectIsNotSupplier_ShouldReturnFalse()
	{
		// Arrange
		_viewModel.SearchText = "Test";
		var notASupplier = new object();

		// Act
		var result = _viewModel.FilterSupplier(notASupplier);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FilterSupplier_WhenSupplierNameIsNull_ShouldReturnFalse()
	{
		// Arrange
		_viewModel.SearchText = "Test";
		var supplier = new SupplierModel { Name = null };

		// Act
		var result = _viewModel.FilterSupplier(supplier);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GetId_ShouldReturnSupplierId()
	{
		// Arrange
		var supplier = new SupplierModel { Id = 42 };

		// Act
		var id = typeof(SupplierPageViewModel)
			.GetMethod("GetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, new object[] { supplier });

		// Assert
		Assert.AreEqual(42, id);
	}

	[TestMethod]
	public void SetId_ShouldSetSupplierId()
	{
		// Arrange
		var supplier = new SupplierModel { Id = 0 };

		// Act
		typeof(SupplierPageViewModel)
			.GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, new object[] { supplier, 99 });

		// Assert
		Assert.AreEqual(99, supplier.Id);
	}

	[TestMethod]
	public void CreateNewItem_ShouldReturnNewSupplierWithDefaultValues()
	{
		// Act
		var newSupplier = typeof(SupplierPageViewModel)
			.GetMethod("CreateNewItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, null) as SupplierModel;

		// Assert
		Assert.IsNotNull(newSupplier);
		Assert.AreEqual(0, newSupplier.Id);
		Assert.AreEqual(string.Empty, newSupplier.Name);
	}

	[TestMethod]
	public void AddSupplierCommand_ShouldReturnAddCommand()
	{
		// Act
		var command = _viewModel.AddSupplierCommand;

		// Assert
		Assert.IsNotNull(command);
		Assert.AreSame(_viewModel.AddCommand, command);
	}

	[TestMethod]
	public void SaveSupplierCommand_ShouldReturnSaveCommand()
	{
		// Act
		var command = _viewModel.SaveSupplierCommand;

		// Assert
		Assert.IsNotNull(command);
		Assert.AreSame(_viewModel.SaveCommand, command);
	}

	[TestMethod]
	public void DeleteSupplierCommand_ShouldReturnDeleteCommand()
	{
		// Act
		var command = _viewModel.DeleteSupplierCommand;

		// Assert
		Assert.IsNotNull(command);
		Assert.AreSame(_viewModel.DeleteCommand, command);
	}

	[TestMethod]
	public void ClearSearchCommand_ShouldClearSearchText()
	{
		// Arrange
		_viewModel.SearchText = "Some search text";

		// Act
		_viewModel.ClearSearchCommand.Execute(null);

		// Assert
		Assert.AreEqual(string.Empty, _viewModel.SearchText);
	}

	[TestMethod]
	public void VisibleSupplierCount_ShouldGetAndSetValue()
	{
		// Act
		_viewModel.VisibleSupplierCount = 5;

		// Assert
		Assert.AreEqual(5, _viewModel.VisibleSupplierCount);
	}

	[TestMethod]
	public void TotalContactCount_WhenNoContactsFiltered_ShouldReturnZero()
	{
		// Act
		var count = _viewModel.TotalContactCount;

		// Assert
		Assert.AreEqual(0, count);
	}

	[TestMethod]
	public void TotalContactCount_WithFilteredContacts_ShouldReturnCount()
	{
		// Arrange
		_viewModel.FilteredContacts.Add(new SupplierContactModel { SupplierContactId = 1 });
		_viewModel.FilteredContacts.Add(new SupplierContactModel { SupplierContactId = 2 });

		// Act
		var count = _viewModel.TotalContactCount;

		// Assert
		Assert.AreEqual(2, count);
	}

	[TestMethod]
	public void AddContactCommand_ShouldBeInitialized()
	{
		// Act
		var command = _viewModel.AddContactCommand;

		// Assert
		Assert.IsNotNull(command);
	}

	[TestMethod]
	public void DeleteContactCommand_ShouldBeInitialized()
	{
		// Act
		var command = _viewModel.DeleteContactCommand;

		// Assert
		Assert.IsNotNull(command);
	}

	[TestMethod]
	public void SaveContactCommand_ShouldBeInitialized()
	{
		// Act
		var command = _viewModel.SaveContactCommand;

		// Assert
		Assert.IsNotNull(command);
	}

	[TestMethod]
	public void OpenWebsiteCommand_ShouldBeInitialized()
	{
		// Act
		var command = _viewModel.OpenWebsiteCommand;

		// Assert
		Assert.IsNotNull(command);
	}

	[TestMethod]
	public async Task LoadItemsAsync_ShouldCallSupplierService()
	{
		// Arrange
		var suppliers = new List<SupplierModel>
		{
			new SupplierModel { Id = 1, Name = "Supplier 1" }
		};

		_mockSupplierService
			.Setup(x => x.GetAllSuppliersAsync())
			.ReturnsAsync(suppliers);

		// Act
		var loadItemsMethod = typeof(SupplierPageViewModel)
			.GetMethod("LoadItemsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var task = loadItemsMethod?.Invoke(_viewModel, null) as Task<List<SupplierModel>>;
		var result = await task!;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		_mockSupplierService.Verify(x => x.GetAllSuppliersAsync(), Times.AtLeastOnce);
	}

	[TestMethod]
	public void SelectedItem_WhenChanged_ShouldUpdateSelectedCountryAndCurrency()
	{
		// Arrange
		var country = new CountryModel { CountryId = 1, CountryName = "Country 1" };
		var currency = new CurrencyModel { CurrencyId = 2, CurrencyName = "EUR" };

		_viewModel.SupplierCountry.Add(country);
		_viewModel.SupplierCurrency.Add(currency);

		var supplier = new SupplierModel
		{
			Id = 1,
			CountryId = 1,
			CurrencyId = 2
		};

		// Act
		_viewModel.SelectedItem = supplier;

		// Assert
		Assert.AreEqual(country, _viewModel.SelectedCountry);
		Assert.AreEqual(currency, _viewModel.SelectedCurrency);
	}

	[TestMethod]
	public void FilterSupplier_WithPartialMatch_ShouldReturnTrue()
	{
		// Arrange
		_viewModel.SearchText = "ppl";
		var supplier = new SupplierModel { Name = "Test Supplier" };

		// Act
		var result = _viewModel.FilterSupplier(supplier);

		// Assert
		Assert.IsTrue(result);
	}
}