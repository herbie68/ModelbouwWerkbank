namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class ProductPageViewModelTests
{
	private Mock<IProductService> _mockProductService = null!;
	private Mock<IUnitService> _mockUnitService = null!;
	private Mock<IBrandService> _mockBrandService = null!;
	private Mock<ICategoryService> _mockCategoryService = null!;
	private Mock<IEntityValidator<ProductModel>> _mockValidator = null!;
	private ProductPageViewModel _viewModel = null!;
	private Mock<IStorageLocationService> _mockStorageLocationService = null!;
	private Mock<ISupplierService> _mockSupplierService = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockProductService = new Mock<IProductService>();
    	_mockUnitService = new Mock<IUnitService>();
    	_mockBrandService = new Mock<IBrandService>();
    	_mockCategoryService = new Mock<ICategoryService>();
    	_mockStorageLocationService = new Mock<IStorageLocationService>();
    	_mockSupplierService = new Mock<ISupplierService>();
    	_mockValidator = new Mock<IEntityValidator<ProductModel>>();

		// Setup default returns for async methods
		_mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<ProductModel>());
    	_mockUnitService.Setup(s => s.GetAllUnitsAsync()).ReturnsAsync(new List<UnitModel>());
   		_mockBrandService.Setup(s => s.GetAllBrandsAsync()).ReturnsAsync(new List<BrandModel>());
		_mockCategoryService.Setup(s => s.GetAllCategorysAsync()).ReturnsAsync(new List<CategoryModel>());
		_mockStorageLocationService.Setup(s => s.GetAllStorageLocationsAsync()).ReturnsAsync(new List<StorageLocationModel>());
		_mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(new List<SupplierModel>());
		_mockSupplierService.Setup(s => s.GetAllProductSuppliersAsync()).ReturnsAsync(new List<Modelbouwer.Model.ProductSupplierModel>());

		_viewModel = new ProductPageViewModel(
			_mockProductService.Object,
			_mockUnitService.Object,
			_mockBrandService.Object,
			_mockCategoryService.Object,
			_mockStorageLocationService.Object,
			_mockSupplierService.Object,
			_mockValidator.Object);
	}

	[TestMethod]
	public void Constructor_InitializesCollections()
	{
		// Assert
		Assert.IsNotNull( _viewModel.ProductBrand );
		Assert.IsNotNull( _viewModel.ProductUnit );
		Assert.IsNotNull( _viewModel.ProductCategory );
		Assert.IsNotNull( _viewModel.Products );
	}

	[TestMethod]
	public void Products_ReturnsItemsCollection()
	{
		// Assert
		Assert.AreSame( _viewModel.Items, _viewModel.Products );
	}

	[TestMethod]
	public void TotalProductCount_ReturnsTotalItemCount()
	{
		// Arrange
		_viewModel.Items.Add( new ProductModel { ProductId = 1 } );
		_viewModel.Items.Add( new ProductModel { ProductId = 2 } );

		// Act
		var count = _viewModel.TotalProductCount;

		// Assert
		Assert.AreEqual( 2, count );
	}

	[TestMethod]
	public void SelectedBrand_CanBeSetAndRetrieved()
	{
		// Arrange
		var brand = new BrandModel { BrandId = 5, BrandName = "Test Brand" };

		// Act
		_viewModel.SelectedBrand = brand;

		// Assert
		Assert.AreEqual( brand, _viewModel.SelectedBrand );
	}

	[TestMethod]
	public void SelectedUnit_CanBeSetAndRetrieved()
	{
		// Arrange
		var unit = new UnitModel { UnitId = 3, UnitName = "Test Unit" };

		// Act
		_viewModel.SelectedUnit = unit;

		// Assert
		Assert.AreEqual( unit, _viewModel.SelectedUnit );
	}

	[TestMethod]
	public void SelectedCategory_UpdatesProductCategoryId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 1 };
		var category = new CategoryModel { CategoryId = 7, CategoryName = "Test Category" };
		_viewModel.SelectedItem = product;

		// Act
		_viewModel.SelectedCategory = category;

		// Assert
		Assert.AreEqual( 7, product.ProductCategoryId );
	}

	[TestMethod]
	public void FilterProduct_WithEmptySearchText_ReturnsTrue()
	{
		// Arrange
		var product = new ProductModel { ProductName = "Test Product" };
		_viewModel.SearchText = "";

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public void FilterProduct_WithMatchingSearchText_ReturnsTrue()
	{
		// Arrange
		var product = new ProductModel { ProductName = "Test Product" };
		_viewModel.SearchText = "Test";

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public void FilterProduct_WithNonMatchingSearchText_ReturnsFalse()
	{
		// Arrange
		var product = new ProductModel { ProductName = "Test Product" };
		_viewModel.SearchText = "XYZ";

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public void FilterProduct_IsCaseInsensitive()
	{
		// Arrange
		var product = new ProductModel { ProductName = "Test Product" };
		_viewModel.SearchText = "test";

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public void FilterProduct_WithNullProductName_ReturnsFalse()
	{
		// Arrange
		var product = new ProductModel { ProductName = null };
		_viewModel.SearchText = "Test";

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public void FilterProduct_WithNonProductObject_ReturnsFalse()
	{
		// Arrange
		var notAProduct = new object();
		_viewModel.SearchText = "Test";

		// Act
		var result = _viewModel.FilterProduct(notAProduct);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public void GetId_ReturnsProductId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 42 };

		// Act
		var id = _viewModel.GetType()
			.GetMethod("GetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, new object[] { product });

		// Assert
		Assert.AreEqual( 42, id );
	}

	[TestMethod]
	public void SetId_SetsProductId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 0 };

		// Act
		_viewModel.GetType()
			.GetMethod( "SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance )
			?.Invoke( _viewModel, new object [ ] { product, 99 } );

		// Assert
		Assert.AreEqual( 99, product.ProductId );
	}

	[TestMethod]
	public void CreateNewItem_ReturnsProductWithDefaultValues()
	{
		// Act
		var method = _viewModel.GetType()
			.GetMethod("CreateNewItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var product = method?.Invoke(_viewModel, null) as ProductModel;

		// Assert
		Assert.IsNotNull( product );
		Assert.AreEqual( 0, product.ProductId );
		Assert.AreEqual( string.Empty, product.ProductName );
	}

	[TestMethod]
	public async Task LoadItemsAsync_CallsGetAllProductsAsync()
	{
		// Arrange
		var products = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Product 1" }
		};
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( products );

		// Act
		var method = _viewModel.GetType()
			.GetMethod("LoadItemsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var task = method?.Invoke(_viewModel, null) as Task<List<ProductModel>>;
		var result = await task!;

		// Assert
		Assert.IsNotNull( result );
		Assert.AreEqual( 1, result.Count );
		_mockProductService.Verify( s => s.GetAllProductsAsync(), Times.AtLeastOnce );
	}

	[TestMethod]
	public async Task InsertAsync_CallsInsertNewProductAsync()
	{
		// Arrange
		var product = new ProductModel { ProductId = 0, ProductName = "New Product" };
		_mockProductService.Setup( s => s.InsertNewProductAsync( It.IsAny<Dictionary<string, object?>>() ) )
			.ReturnsAsync( 123 );

		// Act
		var method = _viewModel.GetType()
			.GetMethod("InsertAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var task = method?.Invoke(_viewModel, new object[] { product }) as Task<int>;
		var result = await task!;

		// Assert
		Assert.AreEqual( 123, result );
		_mockProductService.Verify( s => s.InsertNewProductAsync( It.IsAny<Dictionary<string, object?>>() ), Times.Once );
	}

	[TestMethod]
	public void AddProductCommand_ReturnsAddCommand()
	{
		// Assert
		Assert.IsNotNull( _viewModel.AddProductCommand );
		Assert.AreSame( _viewModel.AddCommand, _viewModel.AddProductCommand );
	}

	[TestMethod]
	public void SaveProductCommand_ReturnsSaveCommand()
	{
		// Assert
		Assert.IsNotNull( _viewModel.SaveProductCommand );
		Assert.AreSame( _viewModel.SaveCommand, _viewModel.SaveProductCommand );
	}

	[TestMethod]
	public void DeleteProductCommand_ReturnsDeleteCommand()
	{
		// Assert
		Assert.IsNotNull( _viewModel.DeleteProductCommand );
		Assert.AreSame( _viewModel.DeleteCommand, _viewModel.DeleteProductCommand );
	}

	[TestMethod]
	public void VisibleProductCount_CanBeSetAndRetrieved()
	{
		// Act
		_viewModel.VisibleProductCount = 10;

		// Assert
		Assert.AreEqual( 10, _viewModel.VisibleProductCount );
	}

	[TestMethod]
	public void OnSelectedItemChanged_UpdatesSelectedCategoryAndStorageLocation()
	{
		// Arrange
		var category = new CategoryModel { CategoryId = 3, CategoryName = "Category 1" };
		var storageLocation = new StorageLocationModel { StorageId = 4, StorageName = "Shelf A" };

		_viewModel.ProductCategory.Add( category );
		_viewModel.ProductStorageLocation.Add( storageLocation );

		var product = new ProductModel
		{
			ProductId = 1,
			ProductCategoryId = 3,
			ProductStorageId = 4
		};

		// Act
		_viewModel.SelectedItem = product;

		// Assert
		Assert.AreEqual( category, _viewModel.SelectedCategory );
		Assert.AreEqual( storageLocation, _viewModel.SelectedStorageLocation );
	}

	[TestMethod]
	public void SearchText_PropertyChangedRaisesNotification()
	{
		// Arrange
		var propertyChanged = false;
		_viewModel.PropertyChanged += ( s, e ) =>
		{
			if ( e.PropertyName == nameof( _viewModel.SearchText ) )
				propertyChanged = true;
		};

		// Act
		_viewModel.SearchText = "Test";

		// Assert
		Assert.IsTrue( propertyChanged );
	}

	[TestMethod]
	public async Task UpdateAsync_PassesProductIdToProductService()
	{
		// Arrange
		var product = new ProductModel
		{
			ProductId = 42,
			ProductName = "Existing Product"
		};
		_viewModel.SelectedItem = product;

		Dictionary<string, object?>? capturedParameters = null;
		_mockProductService
			.Setup( s => s.UpdateProductAsync( It.IsAny<Dictionary<string, object?>>() ) )
			.Callback<Dictionary<string, object?>>( p => capturedParameters = p )
			.Returns( Task.CompletedTask );

		// Act
		var method = _viewModel.GetType()
			.GetMethod( "UpdateAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
		var task = method?.Invoke( _viewModel, new object[] { product } ) as Task;
		await task!;

		// Assert
		Assert.IsNotNull( capturedParameters );
		Assert.IsTrue( capturedParameters.ContainsKey( $"@{DBNames.ProductFieldNameId}" ) );
		Assert.AreEqual( 42, capturedParameters [ $"@{DBNames.ProductFieldNameId}" ] );
	}

	[TestMethod]
	public void SelectedSupplierSupplier_UpdatesSupplierWithoutReplacingAvailableSuppliersCollection()
	{
		// Arrange
		var product = new ProductModel { ProductId = 10, ProductName = "Product" };
		var supplier = new SupplierModel { Id = 5, Name = "Supplier" };
		_viewModel.SelectedItem = product;
		_viewModel.Suppliers.Add( supplier );

		_viewModel.AddSupplierCommand.Execute( null );
		var selectedProductSupplier = _viewModel.SelectedSupplier;
		var availableSuppliers = _viewModel.AvailableSuppliers;

		// Act
		_viewModel.SelectedSupplierSupplier = supplier;

		// Assert
		Assert.IsNotNull( selectedProductSupplier );
		Assert.AreSame( availableSuppliers, _viewModel.AvailableSuppliers );
		Assert.AreEqual( supplier.Id, selectedProductSupplier.SupplierId );
		Assert.AreEqual( supplier.Name, selectedProductSupplier.SupplierName );
	}
}
