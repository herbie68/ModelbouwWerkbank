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

		_mockProductService
			.Setup(x => x.GetAllProductsAsync())
			.ReturnsAsync(new List<ProductModel>());

		_mockBrandService
			.Setup(x => x.GetAllBrandsAsync())
			.ReturnsAsync(new List<BrandModel>());

		_mockUnitService
			.Setup(x => x.GetAllUnitsAsync())
			.ReturnsAsync(new List<UnitModel>());

		_mockCategoryService
			.Setup(x => x.GetAllCategoriesAsync())
			.ReturnsAsync(new List<CategoryModel>());

		_viewModel = new ProductPageViewModel(
			_mockProductService.Object,
			_mockUnitService.Object,
			_mockBrandService.Object,
			_mockCategoryService.Object,
			_mockValidator.Object);
	}

	[TestMethod]
	public void Constructor_ShouldInitializeCollections()
	{
		// Assert
		Assert.IsNotNull(_viewModel.Products);
		Assert.IsNotNull(_viewModel.ProductBrand);
		Assert.IsNotNull(_viewModel.ProductUnit);
		Assert.IsNotNull(_viewModel.ProductCategory);
	}

	[TestMethod]
	public void Constructor_ShouldThrowWhenCategoryServiceIsNull()
	{
		// Arrange & Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => new ProductPageViewModel(
			_mockProductService.Object,
			_mockUnitService.Object,
			_mockBrandService.Object,
			null!,
			_mockValidator.Object));
	}

	[TestMethod]
	public void Products_ShouldReturnItemsCollection()
	{
		// Arrange & Act
		var products = _viewModel.Products;

		// Assert
		Assert.IsNotNull(products);
		Assert.AreSame(_viewModel.Items, products);
	}

	[TestMethod]
	public void TotalProductCount_ShouldReturnItemCount()
	{
		// Arrange
		_viewModel.Items.Add(new ProductModel { ProductId = 1 });
		_viewModel.Items.Add(new ProductModel { ProductId = 2 });

		// Act
		var count = _viewModel.TotalProductCount;

		// Assert
		Assert.AreEqual(2, count);
	}

	[TestMethod]
	public void SelectedBrand_WhenSet_ShouldUpdateSelectedItemBrandId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 1 };
		var brand = new BrandModel { BrandId = 5, BrandName = "Test Brand" };
		_viewModel.SelectedItem = product;

		// Act
		_viewModel.SelectedBrand = brand;

		// Assert
		Assert.AreEqual(5, product.ProductBrandId);
	}

	[TestMethod]
	public void SelectedUnit_WhenSet_ShouldUpdateSelectedItemUnitId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 1 };
		var unit = new UnitModel { UnitId = 3, UnitName = "Test Unit" };
		_viewModel.SelectedItem = product;

		// Act
		_viewModel.SelectedUnit = unit;

		// Assert
		Assert.AreEqual(3, product.ProductUnitId);
	}

	[TestMethod]
	public void SelectedCategory_WhenSet_ShouldUpdateSelectedItemCategoryId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 1 };
		var category = new CategoryModel { CategoryId = 7, CategoryName = "Test Category" };
		_viewModel.SelectedItem = product;

		// Act
		_viewModel.SelectedCategory = category;

		// Assert
		Assert.AreEqual(7, product.ProductCategoryId);
	}

	[TestMethod]
	public void SelectedBrand_WhenSelectedItemIsNull_ShouldNotThrow()
	{
		// Arrange
		var brand = new BrandModel { BrandId = 5 };
		_viewModel.SelectedItem = null;

		// Act & Assert
		_viewModel.SelectedBrand = brand;
		// No exception should be thrown
	}

	[TestMethod]
	public void FilterProduct_WhenSearchTextIsEmpty_ShouldReturnTrue()
	{
		// Arrange
		_viewModel.SearchText = "";
		var product = new ProductModel { ProductName = "Test Product" };

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilterProduct_WhenSearchTextMatches_ShouldReturnTrue()
	{
		// Arrange
		_viewModel.SearchText = "Test";
		var product = new ProductModel { ProductName = "Test Product" };

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilterProduct_WhenSearchTextDoesNotMatch_ShouldReturnFalse()
	{
		// Arrange
		_viewModel.SearchText = "XYZ";
		var product = new ProductModel { ProductName = "Test Product" };

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FilterProduct_ShouldBeCaseInsensitive()
	{
		// Arrange
		_viewModel.SearchText = "test";
		var product = new ProductModel { ProductName = "TEST PRODUCT" };

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilterProduct_WhenObjectIsNotProduct_ShouldReturnFalse()
	{
		// Arrange
		_viewModel.SearchText = "Test";
		var notAProduct = new object();

		// Act
		var result = _viewModel.FilterProduct(notAProduct);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FilterProduct_WhenProductNameIsNull_ShouldReturnFalse()
	{
		// Arrange
		_viewModel.SearchText = "Test";
		var product = new ProductModel { ProductName = null };

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GetId_ShouldReturnProductId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 42 };

		// Act
		var id = typeof(ProductPageViewModel)
			.GetMethod("GetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, new object[] { product });

		// Assert
		Assert.AreEqual(42, id);
	}

	[TestMethod]
	public void SetId_ShouldSetProductId()
	{
		// Arrange
		var product = new ProductModel { ProductId = 0 };

		// Act
		typeof(ProductPageViewModel)
			.GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, new object[] { product, 99 });

		// Assert
		Assert.AreEqual(99, product.ProductId);
	}

	[TestMethod]
	public void CreateNewItem_ShouldReturnNewProductWithDefaultValues()
	{
		// Act
		var newProduct = typeof(ProductPageViewModel)
			.GetMethod("CreateNewItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.Invoke(_viewModel, null) as ProductModel;

		// Assert
		Assert.IsNotNull(newProduct);
		Assert.AreEqual(0, newProduct.ProductId);
		Assert.AreEqual(string.Empty, newProduct.ProductName);
	}

	[TestMethod]
	public void AddProductCommand_ShouldReturnAddCommand()
	{
		// Act
		var command = _viewModel.AddProductCommand;

		// Assert
		Assert.IsNotNull(command);
		Assert.AreSame(_viewModel.AddCommand, command);
	}

	[TestMethod]
	public void SaveProductCommand_ShouldReturnSaveCommand()
	{
		// Act
		var command = _viewModel.SaveProductCommand;

		// Assert
		Assert.IsNotNull(command);
		Assert.AreSame(_viewModel.SaveCommand, command);
	}

	[TestMethod]
	public void DeleteProductCommand_ShouldReturnDeleteCommand()
	{
		// Act
		var command = _viewModel.DeleteProductCommand;

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
	public void VisibleProductCount_ShouldGetAndSetValue()
	{
		// Act
		_viewModel.VisibleProductCount = 5;

		// Assert
		Assert.AreEqual(5, _viewModel.VisibleProductCount);
	}

	[TestMethod]
	public async Task LoadItemsAsync_ShouldCallProductService()
	{
		// Arrange
		var products = new List<ProductModel>
		{
			new ProductModel { ProductId = 1, ProductName = "Product 1" }
		};

		_mockProductService
			.Setup(x => x.GetAllProductsAsync())
			.ReturnsAsync(products);

		// Act
		var loadItemsMethod = typeof(ProductPageViewModel)
			.GetMethod("LoadItemsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var task = loadItemsMethod?.Invoke(_viewModel, null) as Task<List<ProductModel>>;
		var result = await task!;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		_mockProductService.Verify(x => x.GetAllProductsAsync(), Times.Once);
	}

	[TestMethod]
	public void SelectedItem_WhenChanged_ShouldUpdateSelectedBrandUnitAndCategory()
	{
		// Arrange
		var brand = new BrandModel { BrandId = 1, BrandName = "Brand 1" };
		var unit = new UnitModel { UnitId = 2, UnitName = "Unit 1" };
		var category = new CategoryModel { CategoryId = 3, CategoryName = "Category 1" };

		_viewModel.ProductBrand.Add(brand);
		_viewModel.ProductUnit.Add(unit);
		_viewModel.ProductCategory.Add(category);

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
		Assert.AreEqual(brand, _viewModel.SelectedBrand);
		Assert.AreEqual(unit, _viewModel.SelectedUnit);
		Assert.AreEqual(category, _viewModel.SelectedCategory);
	}

	[TestMethod]
	public void FilterProduct_WithPartialMatch_ShouldReturnTrue()
	{
		// Arrange
		_viewModel.SearchText = "rod";
		var product = new ProductModel { ProductName = "Test Product" };

		// Act
		var result = _viewModel.FilterProduct(product);

		// Assert
		Assert.IsTrue(result);
	}
}