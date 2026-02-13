using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class ProductPageViewModel : EntityPageViewModel<ProductModel>
{
	private readonly IProductService _dataService;
	private readonly IUnitService _unitService;
	private readonly IBrandService _brandService;
	private readonly ICategoryService _categoryService;

	private int? _lastSelectedProductId;

	// Constructor
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


	// Override SelectedItem changed om DefaultProduct te zetten
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

	// Filtering
	public bool FilterProduct( object obj )
	{
		if ( obj is not ProductModel Product )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return Product.Name?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<ProductModel>> LoadItemsAsync() => _dataService.GetAllProductsAsync();
	protected override Task<int> InsertAsync( ProductModel item ) => _dataService.InsertNewProductAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( ProductModel item )
	{
		if ( SelectedItem == null )
			return Task.CompletedTask;

		_lastSelectedProductId = SelectedItem.ProductId;

		return _dataService.UpdateProductAsync( CreateParameters( SelectedItem ) );
	}
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

	protected override int GetId( ProductModel item ) => item.ProductId;
	protected override void SetId( ProductModel item, int id ) => item.ProductId = id;

	protected override ProductModel CreateNewItem() => new()
	{
		ProductId = 0,
		ProductName = string.Empty
	};

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

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( ProductModel c ) => new()
	{
		{ $"@{DBNames.ProductFieldNameId}", c.ProductId },
		{ $"@{DBNames.ProductFieldNameCode}", c.ProductCode },
		{ $"@{DBNames.ProductFieldNameName}", c.ProductName?.Trim() },
		{ $"@{DBNames.ProductFieldNameMemo}", c.ProductMemo }
	};

}
