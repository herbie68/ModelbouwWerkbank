using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

namespace Modelbouwer.ViewModels;

public class NavigationViewModel : INotifyPropertyChanged
{
	private readonly IServiceProvider _serviceProvider;

	public ObservableCollection<NavigationModel> NavigationItems { get; set; }

	readonly ObservableCollection<NavigationModel> TimeSubItems = [];
	readonly ObservableCollection<NavigationModel> InventorySubItems = [];
	readonly ObservableCollection<NavigationModel> ProjectSubItems = [];
	readonly ObservableCollection<NavigationModel> MetadataSubItems = [];
	readonly ObservableCollection<NavigationModel> MetadataCountriesSubItems = [];
	readonly ObservableCollection<NavigationModel> MetadataCurrenciesSubItems = [];

	private object? _currentView;
	public object? CurrentView
	{
		get => _currentView;
		set
		{
			_currentView = value;
			OnPropertyChanged();
		}
	}

	private bool _isNavigationLoaded;
	public bool IsNavigationLoaded
	{
		get => _isNavigationLoaded;
		set
		{
			if ( _isNavigationLoaded != value )
			{
				_isNavigationLoaded = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged( [CallerMemberName] string? propertyName = null ) =>
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

	public NavigationViewModel( IServiceProvider serviceProvider, SettingsService? settingsService = null )
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
		NavigationItems = [ ];
		if ( settingsService != null )
			settingsService.SettingsChanged += OnSettingsChanged;

		_ = LoadNavigationItemsAsync();
	}

	private void OnSettingsChanged( object? sender, EventArgs e )
	{
		if ( Application.Current == null )
			return;

		Application.Current.Dispatcher.Invoke( () =>
		{
			BuildNavigationItems();
			LoadSettingsView();
		} );
	}

	private async Task LoadNavigationItemsAsync()
	{
		try
		{
			// Wacht tot de applicatie volledig is geladen
			await Task.Delay( 100 );

			if ( Application.Current == null )
				return;

			await Application.Current.Dispatcher.InvokeAsync( () =>
			{
				try
				{
					BuildNavigationItems();
					IsNavigationLoaded = true;
				}
				catch ( Exception ex )
				{
					Debug.WriteLine( $"Error loading navigation: {ex.Message}" );
					// Fallback: minimaal navigatie-item
					AddFallbackNavigation();
					IsNavigationLoaded = true;
				}
			} );
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Unexpected error loading navigation: {ex}" );
			AddFallbackNavigation();
			IsNavigationLoaded = true;
		}
	}

	private void AddFallbackNavigation()
	{
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Brand_Label}",
			NavigationIcon = CreateNavigationImage( "Brands" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Brand_Tooltip}",
			Command = new SimpleCommand( () => LoadBrandView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Category_Label}",
			NavigationIcon = CreateNavigationImage( "Category" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Category_Tooltip}",
			Command = new SimpleCommand( () => LoadCategoryView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_ContactType_Label}",
			NavigationIcon = CreateNavigationImage( "contacttype" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_ContactType_Tooltip}",
			Command = new SimpleCommand( () => LoadContactTypeView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Country_Label}",
			NavigationIcon = CreateNavigationImage( "Countries" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Country_Tooltip}",
			Command = new SimpleCommand( () => LoadCountryView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Currency_Label}",
			NavigationIcon = CreateNavigationImage( "Currency" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Currency_Tooltip}",
			Command = new SimpleCommand( () => LoadCurrencyView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_StorageLocation_Label}",
			NavigationIcon = CreateNavigationImage( "StorageLocation" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_StorageLocation_Tooltip}",
			Command = new SimpleCommand( () => LoadStorageLocationView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Unit_Label}",
			NavigationIcon = CreateNavigationImage( "Unit" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Unit_Tooltip}",
			Command = new SimpleCommand( () => LoadUnitView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Worktype_Label}",
			NavigationIcon = CreateNavigationImage( "Worktype" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Worktype_Tooltip}",
			Command = new SimpleCommand( () => LoadWorktypeView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Project_Label}",
			NavigationIcon = CreateNavigationImage( "Project" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Project_Tooltip}",
			Command = new SimpleCommand( () => LoadProjectView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Supplier_Label}",
			NavigationIcon = CreateNavigationImage( "supplier" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Supplier_Tooltip}",
			Command = new SimpleCommand( () => LoadSupplierView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Product_Label}",
			NavigationIcon = CreateNavigationImage( "product" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Product_Tooltip}",
			Command = new SimpleCommand( () => LoadProductView() )
		} );

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Report_Label,
			NavigationIcon = CreateNavigationImage( "Inventory" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Report_Tooltip,
			Command = new SimpleCommand( () => LoadStockManagementView() )
		} );
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Order_Label,
			NavigationIcon = CreateNavigationImage( "Order" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Order_Tooltip,
			Command = new SimpleCommand( () => LoadStockOrderManagementView() )
		} );
	}

	private static Image? CreateNavigationImage( string resourceKey )
	{
		try
		{
			if ( Application.Current == null )
				return null;

			var style = Application.Current.FindResource("NavigationIconStyle") as Style;
			var source = Application.Current.FindResource(resourceKey) as ImageSource;

			if ( source == null )
			{
				Debug.WriteLine( $"Resource not found: {resourceKey}" );
				return null;
			}

			return new Image
			{
				Style = style,
				Source = source
			};
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error creating image for {resourceKey}: {ex.Message}" );
			return null;
		}
	}

	private void LoadBrandView()
	{
		try
		{
			var brandView = _serviceProvider.GetRequiredService<BrandView>();
			CurrentView = brandView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading BrandView: {ex.Message}" );
		}
	}

	private void LoadCategoryView()
	{
		try
		{
			var categoryView = _serviceProvider.GetRequiredService<CategoryView>();
			CurrentView = categoryView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading CategoryView: {ex.Message}" );
		}
	}

	private void LoadContactTypeView()
	{
		try
		{
			var contacttypeView = _serviceProvider.GetRequiredService<ContactTypeView>();
			CurrentView = contacttypeView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading ContactTypeView: {ex.Message}" );
		}
	}

	private void LoadCountryView()
	{
		try
		{
			var countryView = _serviceProvider.GetRequiredService<CountryView>();
			CurrentView = countryView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading CountryView: {ex.Message}" );
		}
	}

	private void LoadCurrencyView()
	{
		try
		{
			var currencyView = _serviceProvider.GetRequiredService<CurrencyView>();
			CurrentView = currencyView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading CurrencyView: {ex.Message}" );
		}
	}

	private void LoadStorageLocationView()
	{
		try
		{
			var storagelocationView = _serviceProvider.GetRequiredService<StorageLocationView>();
			CurrentView = storagelocationView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading StorageLocationView: {ex.Message}" );
		}
	}

	private void LoadUnitView()
	{
		try
		{
			var unitView = _serviceProvider.GetRequiredService<UnitView>();
			CurrentView = unitView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading UnitView: {ex.Message}" );
		}
	}

	private void LoadWorktypeView()
	{
		try
		{
			var worktypeView = _serviceProvider.GetRequiredService<WorktypeView>();
			CurrentView = worktypeView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading WorktypeView: {ex.Message}" );
		}
	}

	private void LoadProjectView()
	{
		try
		{
			var projectView = _serviceProvider.GetRequiredService<ProjectView>();
			CurrentView = projectView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading ProjectView: {ex.Message}" );
		}
	}

	private void LoadSupplierView()
	{
		try
		{
			var supplierView = _serviceProvider.GetRequiredService<SupplierView>();
			CurrentView = supplierView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading SupplierView: {ex.Message}" );
		}
	}

	private void LoadProductView()
	{
		try
		{
			var productView = _serviceProvider.GetRequiredService<ProductView>();
			CurrentView = productView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading ProductView: {ex.Message}" );
		}
	}

	private void LoadStockManagementView()
	{
		try
		{
			var stockmanagementView = _serviceProvider.GetRequiredService<StockManagementView>();
			CurrentView = stockmanagementView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading StockManagementView: {ex.Message}" );
		}
	}

	private void LoadStockOrderManagementView()
	{
		try
		{
			var stockorderView = _serviceProvider.GetRequiredService<StockOrderView>();
			CurrentView = stockorderView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading StockOrderView: {ex.Message}" );
		}
	}

	private void LoadStockReceiptView()
	{
		try
		{
			var stockReceiptView = _serviceProvider.GetRequiredService<StockReceiptView>();
			CurrentView = stockReceiptView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading StockReceiptView: {ex.Message}" );
		}
	}

	private void LoadTimeRegistrationView()
	{
		try
		{
			var timeRegistrationView = _serviceProvider.GetRequiredService<TimeRegistrationView>();
			CurrentView = timeRegistrationView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading TimeRegistrationView: {ex.Message}" );
		}
	}

	private void LoadProjectReportsView( int selectedTabIndex = 0 )
	{
		try
		{
			var projectReportsView = _serviceProvider.GetRequiredService<ProjectReportsView>();
			if ( projectReportsView.DataContext is ProjectReportsViewModel viewModel )
				viewModel.SelectReportTab( selectedTabIndex );

			CurrentView = projectReportsView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading ProjectReportsView: {ex.Message}" );
		}
	}

	private void LoadSettingsView()
	{
		try
		{
			var settingsView = _serviceProvider.GetRequiredService<SettingsView>();
			CurrentView = settingsView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading SettingsView: {ex.Message}" );
		}
	}

	private void LoadAboutView()
	{
		try
		{
			var aboutView = _serviceProvider.GetRequiredService<AboutView>();
			CurrentView = aboutView;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error loading AboutView: {ex.Message}" );
		}
	}

	private void BuildNavigationItems()
	{
		NavigationItems.Clear();
		TimeSubItems.Clear();
		InventorySubItems.Clear();
		ProjectSubItems.Clear();
		MetadataSubItems.Clear();
		MetadataCountriesSubItems.Clear();
		MetadataCurrenciesSubItems.Clear();

		#region Time section
		#region Subitems
		TimeSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.TimeRegistrationHeaderTitle,
			NavigationIcon = CreateNavigationImage( "Time" ),
			NavigationTooltip = Language.TimeRegistrationHeaderSubtitle,
			Command = new SimpleCommand( () => LoadTimeRegistrationView() )
		} );

		TimeSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Time_SubItem_Report_Label,
			NavigationIcon = CreateNavigationImage( "Report" ),
			NavigationTooltip = Language.navigation_Time_SubItem_Report_Tooltip,
			Command = new SimpleCommand( () => LoadProjectReportsView( 0 ) )
		} );
		#endregion

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Time_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Time" ),
			NavigationTooltip = Language.navigation_Time_MainItem_Label,
			SubItems = TimeSubItems
		} );
		#endregion

		#region Inventory section
		#region Subitems
		InventorySubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Report_Label,
			NavigationIcon = CreateNavigationImage( "Inventory" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Report_Tooltip,
			Command = new SimpleCommand( () => LoadStockManagementView() )
		} );

		InventorySubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Order_Label,
			NavigationIcon = CreateNavigationImage( "Order" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Order_Tooltip,
			Command = new SimpleCommand( () => LoadStockOrderManagementView() )
		} );

		InventorySubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Receipt_Label,
			NavigationIcon = CreateNavigationImage( "Recieve" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Receipt_Tooltip,
			Command = new SimpleCommand( () => LoadStockReceiptView() )
		} );
		#endregion

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Inventory" ),
			NavigationTooltip = Language.navigation_Inventory_MainItem_Tooltip,
			SubItems = InventorySubItems
		} );
		#endregion

		#region Projects section
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Projects_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Projects" ),
			NavigationTooltip = Language.navigation_Projects_MainItem_Tooltip,
			Command = new SimpleCommand( () => LoadProjectView() )
		} );
		#endregion

		#region Metadata section
		#region Subitems
		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = "Voorbeeld",
			NavigationIcon = CreateNavigationImage( "Resources" ),
			NavigationTooltip = "Een voorbeeld van een menu met sub menu's",
			SubItems = MetadataCurrenciesSubItems
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Brand_Label,
			NavigationIcon = CreateNavigationImage( "Brands" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Brand_Tooltip,
			Command = new SimpleCommand( () => LoadBrandView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = $"{Lang.navigation_Resources_SubItem_Category_Label}",
			NavigationIcon = CreateNavigationImage( "Category" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Category_Tooltip}",
			Command = new SimpleCommand( () => LoadCategoryView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_ContactType_Label,
			NavigationIcon = CreateNavigationImage( "contacttype" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_ContactType_Tooltip,
			Command = new SimpleCommand( () => LoadContactTypeView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Country_Label,
			NavigationIcon = CreateNavigationImage( "Countries" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Country_Tooltip,
			Command = new SimpleCommand( () => LoadCountryView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Currency_Label,
			NavigationIcon = CreateNavigationImage( "Currency" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Currency_Tooltip,
			Command = new SimpleCommand( () => LoadCurrencyView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_StorageLocation_Label,
			NavigationIcon = CreateNavigationImage( "StorageLocation" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_StorageLocation_Tooltip,
			Command = new SimpleCommand( () => LoadStorageLocationView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Supplier_Label,
			NavigationIcon = CreateNavigationImage( "supplier" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Supplier_Tooltip,
			Command = new SimpleCommand( () => LoadSupplierView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Unit_Label,
			NavigationIcon = CreateNavigationImage( "Unit" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Unit_Tooltip,
			Command = new SimpleCommand( () => LoadUnitView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Worktype_Label,
			NavigationIcon = CreateNavigationImage( "Worktype" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Worktype_Tooltip,
			Command = new SimpleCommand( () => LoadWorktypeView() )
		} );

		MetadataSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Product_Label,
			NavigationIcon = CreateNavigationImage( "product" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Product_Tooltip,
			Command = new SimpleCommand( () => LoadProductView() )
		} );
		#endregion

		#region CurrencySubitems
		MetadataCurrenciesSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Currency_SubItem_Import_Label,
			NavigationIcon = CreateNavigationImage( "Import" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Currency_SubItem_Import_Tooltip
		} );

		MetadataCurrenciesSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_SubItem_Currency_SubItem_Export_Label,
			NavigationIcon = CreateNavigationImage( "Export" ),
			NavigationTooltip = Language.navigation_Resources_SubItem_Currency_SubItem_Export_Tooltip
		} );
		#endregion

		#region Resources main item
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Resources" ),
			NavigationTooltip = Language.navigation_Resources_MainItem_Tooltip,
			SubItems = MetadataSubItems
		} );
		#endregion
		#endregion

		#region Settings section
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Settings_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Settings" ),
			NavigationTooltip = Language.navigation_Settings_MainItem_Tooltip,
			Command = new SimpleCommand( () => LoadSettingsView() )
		} );
		#endregion

		#region About section
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Lang.ResourceManager.GetString( "AboutNavigationLabel", Lang.Culture ) ?? "Over Modelbouwer",
			NavigationIcon = CreateNavigationImage( "AppLogo" ),
			NavigationTooltip = Lang.ResourceManager.GetString( "AboutNavigationTooltip", Lang.Culture ) ?? "Informatie over Modelbouwer en de release historie",
			Command = new SimpleCommand( () => LoadAboutView() )
		} );
		#endregion
	}

	// Simple command implementation zonder parameter conflicts
	private class SimpleCommand : ICommand
	{
		private readonly Action _execute;

#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

		public SimpleCommand( Action execute ) => _execute = execute ?? throw new ArgumentNullException( nameof( execute ) );

		public bool CanExecute( object? parameter ) => true;

		public void Execute( object? parameter ) => _execute();
	}

	// Get the Assembly File Version for display in the UI
	public static string AppVersion =>
	$"Modelbouwer v{Assembly
		.GetExecutingAssembly()
		.GetCustomAttribute<AssemblyFileVersionAttribute>()?
		.Version ?? Language.generalUnknownVersion}";
}
