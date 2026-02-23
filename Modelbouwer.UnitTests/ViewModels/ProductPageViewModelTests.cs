namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class ProductPageViewModelTests
{
	private Mock<IProductService> _mockProductService;
	private Mock<IUnitService> _mockUnitService;
	private Mock<IBrandService> _mockBrandService;
	private Mock<ICategoryService> _mockCategoryService;
	private Mock<IEntityValidator<ProductModel>> _mockValidator;
	private ProductPageViewModel _viewModel;

	[TestInitialize]
	public void Setup()
	{
		_mockProductService = new Mock<IProductService>();
		_mockUnitService = new Mock<IUnitService>();
		_mockBrandService = new Mock<IBrandService>();
		_mockCategoryService = new Mock<ICategoryService>();
		_mockValidator = new Mock<IEntityValidator<ProductModel>>();

		// Setup default returns for async methods
		_mockProductService.Setup( s => s.GetAllProductsAsync() ).ReturnsAsync( new List<ProductModel>() );
		_mockUnitService.Setup( s => s.GetAllUnitsAsync() ).ReturnsAsync( new List<UnitModel>() );
		_mockBrandService.Setup( s => s.GetAllBrandsAsync() ).ReturnsAsync( new List<BrandModel>() );
		_mockCategoryService.Setup( s => s.GetAllCategorysAsync() ).ReturnsAsync( new List<CategoryModel>() );

		_viewModel = new ProductPageViewModel(
			_mockProductService.Object,
			_mockUnitService.Object,
			_mockBrandService.Object,
			_mockCategoryService.Object,
			_mockValidator.Object
		);
	}

	[TestMethod]
	public void Constructor_InitializesCollections()
	{
		// Assert
		object value = Assert.IsNotNull( _viewModel.ProductBrand );
		object value1 = Assert.IsNotNull( _viewModel.ProductUnit );
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
	public void SelectedBrand_UpdatesProductBrandId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 1 };
		var brand = new BrandModel { BrandId = 5, BrandName = "Test Brand" };
		_viewModel.SelectedItem = product;

		// Act
		_viewModel.SelectedBrand = brand;

		// Assert
		Assert.AreEqual( 5, product.ProductBrandId );
	}

	[TestMethod]
	public void SelectedUnit_UpdatesProductUnitId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 1 };
		var unit = new UnitModel { UnitId = 3, UnitName = "Test Unit" };
		_viewModel.SelectedItem = product;

		// Act
		_viewModel.SelectedUnit = unit;

		// Assert
		Assert.AreEqual( 3, product.ProductUnitId );
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
		_mockProductService.Verify( s => s.GetAllProductsAsync(), Times.Once );
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
	public void OnSelectedItemChanged_UpdatesSelectedBrandUnitCategory()
	{
		// Arrange
		var brand = new BrandModel { BrandId = 1, BrandName = "Brand 1" };
		var unit = new UnitModel { UnitId = 2, UnitName = "Unit 1" };
		var category = new CategoryModel { CategoryId = 3, CategoryName = "Category 1" };

		_viewModel.ProductBrand.Add( brand );
		_viewModel.ProductUnit.Add( unit );
		_viewModel.ProductCategory.Add( category );

		var product = new ProductModel
		{
			ProductId = 1,
			ProductBrandId = 1,
			ProductUnitId = 2,
			ProductCategoryId = 3
		};

		// Act
		_viewModel.SelectedItem = product;

		// Assert
		Assert.AreEqual( brand, _viewModel.SelectedBrand );
		Assert.AreEqual( unit, _viewModel.SelectedUnit );
		Assert.AreEqual( category, _viewModel.SelectedCategory );
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
}