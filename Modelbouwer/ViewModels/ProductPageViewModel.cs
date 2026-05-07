using System.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Modelbouwer.Model;

namespace Modelbouwer.ViewModels;

public partial class ProductPageViewModel : EntityPageViewModel<ProductModel>
{
	private readonly IProductService _dataService;
	private readonly IUnitService _unitService;
	private readonly IBrandService _brandService;
	private readonly ICategoryService _categoryService;
	private readonly IStorageLocationService _storageLocationService;
	private readonly ISupplierService _supplierService;

	private int? _lastSelectedProductId;
	private void UpdateSelectedCategoryFromProduct()
	{
		if ( SelectedItem?.ProductCategoryId == null )
		{
			SelectedCategory = null;
			return;
		}

		SelectedCategory = AllCategories
			.FirstOrDefault( c => c.CategoryId == SelectedItem.ProductCategoryId );
	}

	/// <summary>
	/// Since suppliers is a nested part inside the productpage, it has to have
	/// its own "unsaved changes" tracking, otherwise when you change a
	/// supplier, it would mark the entire product as having unsaved changes,
	/// which is not ideal UX
	/// </summary>
	private bool _hasUnsavedSupplierChanges;
	public bool HasUnsavedSupplierChanges
	{
		get => _hasUnsavedSupplierChanges;
		set => SetProperty( ref _hasUnsavedSupplierChanges, value );
	}

	private void UpdateSelectedStorageLocationFromProduct()
	{
		if ( SelectedItem?.ProductStorageId == null )
		{
			SelectedStorageLocation = null;
			return;
		}

		SelectedStorageLocation = AllStorageLocations
			.FirstOrDefault( c => c.StorageId == SelectedItem.ProductStorageId );
	}

	private void HookSupplierPropertyChanged( ProductSupplierModel ps )
	{
		ps.PropertyChanged += ( s, e ) =>
		{
			// Only mark supplier changes, not product changes
			HasUnsavedSupplierChanges = true;
		};
	}

	// Constructor
	public ProductPageViewModel
		(
			IProductService dataService,
			IUnitService unitService,
			IBrandService brandService,
			ICategoryService categoryService,
			IStorageLocationService storageLocationService,
			ISupplierService supplierService,
			IEntityValidator<ProductModel> validator
		) : base( validator )
	{
		_dataService = dataService;
		_brandService = brandService;
		_unitService = unitService;
		_categoryService = categoryService ?? throw new ArgumentNullException( nameof( categoryService ) );
		_storageLocationService = storageLocationService ?? throw new ArgumentNullException( nameof( storageLocationService ) );
		_supplierService = supplierService;

		OpenCategoryPickerCommand = new AsyncRelayCommand( OpenCategoryPickerAsync );
		OpenStorageLocationPickerCommand = new AsyncRelayCommand( OpenStorageLocationPickerAsync );

		_ = InitializeAsync();
	}

	private async Task InitializeAsync()
	{
		try
		{
			await LoadComboBoxesContentAsync();

			await LoadSuppliersAsync();
			await LoadProductSuppliersAsync();

			// Then load products (this will trigger OnSelectedItemChanged with populated combo boxes)
			await ReloadCommand.ExecuteAsync( null );
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.ToString() );
		}
	}
	#region Collections & Selected Items
	public ObservableCollection<BrandModel> ProductBrand { get; } = [ ];
	public ObservableCollection<UnitModel> ProductUnit { get; } = [ ];
	public ObservableCollection<CategoryModel> ProductCategory { get; private set; } = [ ];
	public ObservableCollection<StorageLocationModel> ProductStorageLocation { get; private set; } = [ ];
	public ObservableCollection<SupplierModel> Suppliers { get; } = [ ];
	public ObservableCollection<ProductSupplierModel> ProductSuppliers { get; set; } = [ ];
	public ObservableCollection<ProductSupplierModel> FilteredSuppliers { get; } = [ ];
	//Filter suplliers list voor Suppliers per product tab, to filter out suppliers alteide in the datagrid
	public IEnumerable<SupplierModel> AvailableSuppliers => Suppliers.Where( s => !ProductSuppliers.Any( ps => ps.SupplierId == s.Id ) );

	private ProductSupplierModel? _selectedSupplier;

	public ProductSupplierModel? SelectedSupplier
	{
		get => _selectedSupplier;
		set
		{
			if ( SetProperty( ref _selectedSupplier, value ) )
			{
				HasUnsavedSupplierChanges = false;

				OnPropertyChanged( nameof( SelectedSupplierSupplier ) );
				OpenWebsiteCommand.NotifyCanExecuteChanged();
			}
		}
	}

	public SupplierModel? SelectedSupplierSupplier
	{
		get
		{
			if ( SelectedSupplier == null )
				return null;

			return Suppliers.FirstOrDefault( s => s.Id == SelectedSupplier.SupplierId );
		}
		set
		{
			if ( SelectedSupplier != null && value != null )
			{
				SelectedSupplier.SupplierId = value.Id;
				SelectedSupplier.SupplierName = value.Name;

				OnPropertyChanged( nameof( SelectedSupplier ) );
			}
		}
	}

	private void Supplier_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName == nameof( ProductSupplierModel.URL ) )
		{
			OpenWebsiteCommand.NotifyCanExecuteChanged();
		}
	}

	public List<CategoryModel> AllCategories
	{
		get;
		private set
		{
			if ( SetProperty( ref field, value ) )
			{
				// When categories change, remap selected category for the current product
				UpdateSelectedCategoryFromProduct();
			}
		}
	} = [ ];

	public List<StorageLocationModel> AllStorageLocations
	{
		get;
		private set
		{
			if ( SetProperty( ref field, value ) )
			{
				// When storage location change, remap selected storagelocation for the current product
				UpdateSelectedStorageLocationFromProduct();
			}
		}
	} = [ ];

	public IRelayCommand RotateCommand => _rotateCommand ??= new RelayCommand( RotateImage );
	public IRelayCommand AddImageCommand => _addImageCommand ??= new RelayCommand( AddImage );

	private IRelayCommand? _rotateCommand;
	private IRelayCommand? _addImageCommand;


	public CategoryModel? SelectedCategory
	{
		get;
		set
		{
			if ( SetProperty( ref field, value ) )
			{
				if ( SelectedItem != null )
				{
					SelectedItem?.ProductCategoryId = value?.CategoryId ?? 0;
				}
			}
		}
	}

	public StorageLocationModel? SelectedStorageLocation
	{
		get;
		set
		{
			if ( SetProperty( ref field, value ) )
			{
				if ( SelectedItem != null )
				{
					SelectedItem?.ProductStorageId = value?.StorageId ?? 0;
				}
			}
		}
	}
	#endregion

	#region Load Methods
	private async Task LoadComboBoxesContentAsync()
	{
		ProductBrand.Clear();
		ProductUnit.Clear();
		ProductCategory.Clear();
		ProductStorageLocation.Clear();

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

		var categories = await _categoryService.GetAllCategorysAsync();
		AllCategories = categories;
		foreach ( var category in categories )
		{
			ProductCategory.Add( category );
		}

		var storageLocations = await _storageLocationService.GetAllStorageLocationsAsync();
		AllStorageLocations = storageLocations;
		foreach ( var location in storageLocations )
		{
			ProductStorageLocation.Add( location );
		}


		// If a product is already selected, map its category now that AllCategories is available
		if ( SelectedItem != null )
		{
			UpdateSelectedCategoryFromProduct();
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
	public IAsyncRelayCommand OpenCategoryPickerCommand { get; }
	public IAsyncRelayCommand OpenStorageLocationPickerCommand { get; }

	#region CRUD Contacts
	private IRelayCommand? _addSupplierCommand;
	public IRelayCommand AddSupplierCommand => _addSupplierCommand ??= new RelayCommand( AddSupplier );
	private IRelayCommand? _saveSupplierCommand;
	public IRelayCommand SaveSupplierCommand => _saveSupplierCommand ??= new RelayCommand( SaveSupplier );
	private IRelayCommand? _deleteSupplierCommand;
	public IRelayCommand DeleteSupplierCommand => _deleteSupplierCommand ??= new RelayCommand( DeleteSupplier );

	#region Relay command for going to the supplier website
	[RelayCommand( CanExecute = nameof( CanOpenWebsite ) )]
	private void OpenWebsite()
	{
		if ( string.IsNullOrWhiteSpace( SelectedSupplier?.URL ) )
			return;

		ProcessStartInfo startInfo = new()
		{
			FileName = SelectedSupplier.URL,
			UseShellExecute = true
		};

		Process.Start( startInfo );
	}

	private bool CanOpenWebsite()
	{
		return !string.IsNullOrWhiteSpace( SelectedSupplier?.URL );
	}
	#endregion


	private void AddSupplier()
	{
		if ( SelectedItem == null )
			return;

		var newSupplier = new ProductSupplierModel
		{
			ProductSupplierId = 0,
			SupplierId = 0,
			SupplierName = string.Empty,
			ProductId = SelectedItem.ProductId,
			ProductName = string.Empty,
			ProductNumber = string.Empty,
			CurrencyId = 0,
			CurrencySymbol = string.Empty,
			Price = 0,
			URL = string.Empty,
			DefaultSupplier = false
		};
		ProductSuppliers.Add( newSupplier );
		FilteredSuppliers.Add( newSupplier );

		HookSupplierPropertyChanged( newSupplier );

		SelectedSupplier = newSupplier;

		HasUnsavedSupplierChanges = true;

		OnPropertyChanged( nameof( AvailableSuppliers ) );

		RaiseSupplierCounters();
	}

	private void DeleteSupplier()
	{
		if ( SelectedSupplier == null )
			return;

		ProductSuppliers.Remove( SelectedSupplier );
		FilteredSuppliers.Remove( SelectedSupplier );

		OnPropertyChanged( nameof( AvailableSuppliers ) );

		HasUnsavedSupplierChanges = false;

		RaiseSupplierCounters();
	}

	private void SaveSupplier()
	{
		// Implement contact save logic here
		// This would typically call a service method to persist the contact

		HasUnsavedSupplierChanges = false;
	}

	private void UpdateFilteredSuppliers()
	{
		FilteredSuppliers.Clear();

		if ( SelectedItem == null )
		{
			RaiseSupplierCounters();
			return;
		}

		foreach ( var c in ProductSuppliers.Where( c => c.ProductId == SelectedItem.ProductId ) )
			FilteredSuppliers.Add( c );


		RaiseSupplierCounters();
	}

	private void RaiseSupplierCounters()
	{
		OnPropertyChanged( nameof( TotalSupplierCount ) );
	}

	public int TotalSupplierCount => FilteredSuppliers.Count;
	#endregion


	// Commands


	public async Task OpenCategoryPickerAsync()
	{
		var vm = new CategoryPickerViewModel(_categoryService, SelectedCategory);

		var dlg = new CategoryPickerDialog(vm);
		bool? result = dlg.ShowDialog();

		if ( result == true && vm.SelectedCategory != null )
		{
			SelectedCategory = vm.SelectedCategory;
		}
	}

	public async Task OpenStorageLocationPickerAsync()
	{
		var vm = new StorageLocationPickerViewModel(_storageLocationService, SelectedStorageLocation);

		var dlg = new StorageLocationPickerDialog(vm);
		bool? result = dlg.ShowDialog();

		if ( result == true && vm.SelectedStorageLocation != null )
		{
			SelectedStorageLocation = vm.SelectedStorageLocation;
		}
	}

	// Override SelectedItem changed om DefaultProduct te zetten
	protected override void OnSelectedItemChanged( ProductModel? oldValue, ProductModel? newValue )
	{
		base.OnSelectedItemChanged( oldValue, newValue );

		HasUnsavedSupplierChanges = false;

		SelectedCategory = ProductCategory.FirstOrDefault( c => c.CategoryId == newValue?.ProductCategoryId );
		SelectedStorageLocation = ProductStorageLocation.FirstOrDefault( c => c.StorageId == newValue?.ProductStorageId );

		UpdateFilteredSuppliers();

		_previousProduct = newValue;
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
		OnPropertyChanged( nameof( TotalSupplierCount ) );

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

		// Select the first product in the list by default (better UX than selecting the last/highest id)
		SelectedItem = Products.First();
	}

	private void RotateImage()
	{
		if ( SelectedItem == null )
			return;

		SelectedItem.ProductImageRotationAngle = ( SelectedItem.ProductImageRotationAngle + 90 ) % 360;
		Debug.WriteLine( SelectedItem.ProductImageRotationAngle );
	}

	private void AddImage()
	{

		if ( SelectedItem == null )
			return;

		var dialog = new OpenFileDialog
		{
			Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp",
			Title = "Select image"
		};

		if ( dialog.ShowDialog() != true )
			return;

		SelectedItem.ProductImage = File.ReadAllBytes( dialog.FileName );
		SelectedItem.ProductImageRotationAngle = 0;
	}

	private async Task LoadSuppliersAsync()
	{
		var allSuppliers = await _supplierService.GetAllSuppliersAsync();
		Suppliers.Clear();
		foreach ( var c in allSuppliers )
			Suppliers.Add( c );
		OnPropertyChanged( nameof( AvailableSuppliers ) );
	}

	private async Task LoadProductSuppliersAsync()
	{
		// Load contact types
		var allProductSuppliers = await _supplierService.GetAllProductSuppliersAsync();
		ProductSuppliers.Clear();
		foreach ( var c in allProductSuppliers )
		{
			ProductSuppliers.Add( c );
			HookSupplierPropertyChanged( c );
		}

		UpdateFilteredSuppliers();
	}

	private int _totalProductSupplierCount;

	public int TotalProductSupplierCount
	{
		get => _totalProductSupplierCount;
		set
		{
			_totalProductSupplierCount = value;
			OnPropertyChanged();
		}
	}

	public BrandModel? SelectedBrand { get; set; }
	public UnitModel? SelectedUnit { get; set; }


	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( ProductModel c ) => new()
	{
		{ $"@{DBNames.ProductFieldNameBrandId}", c.ProductBrandId },
		{ $"@{DBNames.ProductFieldNameCategoryId}", c.ProductCategoryId },
		{ $"@{DBNames.ProductFieldNameCode}", c.ProductCode },
		{ $"@{DBNames.ProductFieldNameDimensions}", c.ProductDimensions },
		{ $"@{DBNames.ProductFieldNameHide}", c.ProductHide },
		{ $"@{DBNames.ProductFieldNameImage}", c.ProductImage },
		{ $"@{DBNames.ProductFieldNameImageRotationAngle}", c.ProductImageRotationAngle },
		{ $"@{DBNames.ProductFieldNameMemo}", c.ProductMemo },
		{ $"@{DBNames.ProductFieldNameMinimalStock}", c.ProductMinimalStock },
		{ $"@{DBNames.ProductFieldNameName}" , c.ProductName },
		{ $"@{DBNames.ProductFieldNamePrice}", c.ProductPrice },
		{ $"@{DBNames.ProductFieldNameProjectCosts}", c.ProductProjectCosts },
		{ $"@{DBNames.ProductFieldNameStandardOrderQuantity}", c.ProductStandardQuantity },
		{ $"@{DBNames.ProductFieldNameStorageId}", c.ProductStorageId },
		{ $"@{DBNames.ProductFieldNameUnitId}", c.ProductUnitId }
	};

}
