using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class ProductPageViewModel : EntityPageViewModel<ProductModel>
{
	private readonly IProductService _dataService;
	private readonly IUnitService _unitService;
	private readonly IBrandService _brandService;
	private readonly ICategoryService _categoryService;

	private int? _lastSelectedProductId;

	/// <summary>
	/// Initializes a ProductPageViewModel with the services required for product, unit, brand, and category operations and initiates loading of lookup data and items.
	/// </summary>
	/// <param name="dataService">Service used for product CRUD operations.</param>
	/// <param name="unitService">Service providing unit lookup data.</param>
	/// <param name="brandService">Service providing brand lookup data.</param>
	/// <param name="categoryService">Service providing category lookup data; required and cannot be null.</param>
	/// <param name="validator">Validator for ProductModel instances.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="categoryService"/> is null.</exception>
	public ProductPageViewModel
		(
			IProductService dataService,
			IUnitService unitService,
			IBrandService brandService,
			ICategoryService categoryService,
			IEntityValidator<ProductModel> validator
		) : base( validator )
	{
		_dataService = dataService;
		_brandService = brandService;
		_unitService = unitService;
		_categoryService = categoryService ?? throw new ArgumentNullException( nameof( categoryService ) );

		_ = LoadComboBoxesContentAsync();

		_ = ReloadCommand.ExecuteAsync( null );
	}

	#region Collections & Selected Items
	public ObservableCollection<BrandModel> ProductBrand { get; } = [ ];
	public ObservableCollection<UnitModel> ProductUnit { get; } = [ ];
	public ObservableCollection<CategoryModel> ProductCategory { get; } = [ ];
	public ObservableCollection<CategoryModel> Categorys { get; } = [ ];

	private BrandModel? _selectedBrand;
	public BrandModel? SelectedBrand
	{
		get => _selectedBrand;
		set
		{
			if ( SetProperty( ref _selectedBrand, value ) && SelectedItem != null && value != null )
			{
				SelectedItem.ProductBrandId = value.BrandId;
			}
		}
	}

	private UnitModel? _selectedUnit;
	public UnitModel? SelectedUnit
	{
		get => _selectedUnit;
		set
		{
			if ( SetProperty( ref _selectedUnit, value ) && SelectedItem != null && value != null )
			{
				SelectedItem.ProductUnitId = value.UnitId;
			}
		}
	}

	private CategoryModel? _selectedCategory;
	public CategoryModel? SelectedCategory
	{
		get => _selectedCategory;
		set
		{
			if ( SetProperty( ref _selectedCategory, value ) && SelectedItem != null && value != null )
			{
				SelectedItem.ProductCategoryId = value.CategoryId;
			}
		}
	}
	#endregion

	#region Load Methods
	/// <summary>
	/// Reloads the ProductBrand and ProductUnit collections used by the UI combo boxes.
	/// </summary>
	/// <remarks>
	/// Clears the existing collections and repopulates them from the brand and unit services.
	/// </remarks>
	/// <returns>A task that completes when the collections have been refreshed.</returns>
	private async Task LoadComboBoxesContentAsync()
	{
		ProductBrand.Clear();
		ProductUnit.Clear();

		var brands = await _brandService.GetAllBrandsAsync();
		foreach ( var brand in brands )
		{
			ProductBrand.Add( brand );
		}

		var units = await _unitService.GetAllUnitsAsync();
		foreach ( var unit in units )
		{
			ProductUnit.Add( unit );
		}
	}
	#endregion

	#region SelectedProduct + Filter
	private ProductModel? _previousProduct;
	#endregion


	// Commands
	public IRelayCommand AddProductCommand => AddCommand;
	public IAsyncRelayCommand SaveProductCommand => SaveCommand;
	public IRelayCommand DeleteProductCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );


	private IRelayCommand? _clearSearchCommand;


	/// <summary>
	/// Handles a change to the currently selected product and synchronizes the SelectedUnit, SelectedBrand, and SelectedCategory properties to match the new product.
	/// </summary>
	/// <param name="value">The newly selected ProductModel, or null if the selection was cleared.</param>
	protected override void OnSelectedItemChanged( ProductModel? value )
	{
		base.OnSelectedItemChanged( value );

		SelectedUnit = ProductUnit.FirstOrDefault( c => c.UnitId == value?.ProductUnitId );

		SelectedBrand = ProductBrand.FirstOrDefault( c => c.BrandId == value?.ProductBrandId );
		SelectedCategory = ProductCategory.FirstOrDefault( c => c.CategoryId == value?.ProductCategoryId );

		_previousProduct = value;
	}


	// Properties voor UI binding
	public ObservableCollection<ProductModel> Products => Items;
	public int TotalProductCount => TotalItemCount;

	public int VisibleProductCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	/// <summary>
	/// Determines whether the provided object is a ProductModel whose Name matches the view model's current SearchText (case-insensitive).
	/// </summary>
	/// <param name="obj">The object to test; expected to be a ProductModel.</param>
	/// <returns>`true` if <paramref name="obj"/> is a ProductModel and its Name contains the current SearchText using case-insensitive comparison, `false` otherwise.</returns>
	public bool FilterProduct( object obj )
	{
		if ( obj is not ProductModel Product )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return Product.Name?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	/// <summary>
/// Retrieves all product models for the view model.
/// </summary>
/// <returns>A list containing all ProductModel instances.</returns>
	protected override Task<List<ProductModel>> LoadItemsAsync() => _dataService.GetAllProductsAsync();
	/// <summary>
/// Insert a new product into the data store.
/// </summary>
/// <param name="item">The product to insert.</param>
/// <returns>The database identifier assigned to the inserted product.</returns>
protected override Task<int> InsertAsync( ProductModel item ) => _dataService.InsertNewProductAsync( CreateParameters( item ) );
	/// <summary>
	/// Updates the currently selected product via the product data service and records its id for selection restoration after reload.
	/// </summary>
	/// <param name="item">This parameter is ignored; the method updates the currently selected product instead of the provided instance.</param>
	/// <returns>A task that completes when the selected product has been sent to the data service for update.</returns>
	protected override Task UpdateAsync( ProductModel item )
	{
		if ( SelectedItem == null )
			return Task.CompletedTask;

		_lastSelectedProductId = SelectedItem.ProductId;

		return _dataService.UpdateProductAsync( CreateParameters( SelectedItem ) );
	}
	/// <summary>
	/// Prompts the user for confirmation and deletes the specified product when confirmed.
	/// </summary>
	/// <param name="item">The product to delete; if null, the method returns without action.</param>
	protected override async Task DeleteAsync( ProductModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.Name}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteProductAsync( item.ProductId );
		}
		catch ( EntityInUseException ex )
		{
			MessageBox.Show(
				ex.Message,
				Lang.generalMessageboxWarningTitle,
				MessageBoxButton.OK,
				MessageBoxImage.Information
			);
		}
	}

	/// <summary>
/// Gets the identifier for the specified product.
/// </summary>
/// <param name="item">The product whose identifier to retrieve.</param>
/// <returns>The product's ProductId.</returns>
protected override int GetId( ProductModel item ) => item.ProductId;
	/// <summary>
/// Set the ProductId of the specified product.
/// </summary>
/// <param name="item">The product whose ProductId will be updated.</param>
/// <param name="id">The value to assign to the product's ProductId.</param>
protected override void SetId( ProductModel item, int id ) => item.ProductId = id;

	/// <summary>
	/// Creates a new ProductModel initialized with default values.
	/// </summary>
	/// <returns>A ProductModel with ProductId set to 0 and ProductName set to an empty string.</returns>
	protected override ProductModel CreateNewItem() => new()
	{
		ProductId = 0,
		ProductName = string.Empty
	};

	/// <summary>
	/// Update UI state after the product list is loaded by refreshing counts and restoring or choosing the selected product.
	/// </summary>
	/// <remarks>
	/// Notifies that TotalProductCount changed, attempts to re-select the product with the previously stored product id (if any), and if no matching product is found selects the product with the highest ProductId.
	/// </remarks>
	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		OnPropertyChanged( nameof( TotalProductCount ) );

		if ( _lastSelectedProductId.HasValue )
		{
			var match = Products.FirstOrDefault( p => p.ProductId == _lastSelectedProductId.Value );

			if ( match != null )
			{
				SelectedItem = match;
				return;
			}

			_lastSelectedProductId = null;
		}

		// Default Product selection (Highest Id)
		SelectProductWithHighestId();
	}

	/// <summary>
	/// Selects the product that has the largest ProductId, or clears the current selection if no products exist.
	/// </summary>
	private void SelectProductWithHighestId()
	{
		if ( Products.Count == 0 )
		{
			SelectedItem = null;
			return;
		}

		SelectedItem = Products
			.OrderByDescending( p => p.ProductId )
			.First();
	}

	/// <summary>
	/// Builds a dictionary of database parameters for persisting a ProductModel.
	/// </summary>
	/// <param name="c">The product whose fields are converted into parameter values.</param>
	/// <returns>
	/// A dictionary mapping parameter names (DB field name constants prefixed with '@') to values:
	/// ProductId, ProductCode, ProductName (trimmed), and ProductMemo.
	/// </returns>
	private static Dictionary<string, object?> CreateParameters( ProductModel c ) => new()
	{
		{ $"@{DBNames.ProductFieldNameId}", c.ProductId },
		{ $"@{DBNames.ProductFieldNameCode}", c.ProductCode },
		{ $"@{DBNames.ProductFieldNameName}", c.ProductName?.Trim() },
		{ $"@{DBNames.ProductFieldNameMemo}", c.ProductMemo }
	};

}