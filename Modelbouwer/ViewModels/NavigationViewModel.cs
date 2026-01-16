using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Extensions.DependencyInjection;

using Modelbouwer.Helpers;
using Modelbouwer.Models;
using Modelbouwer.Views;

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

	public NavigationViewModel( IServiceProvider serviceProvider )
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
		NavigationItems = [ ];

		LoadNavigationItemsAsync();
	}

	private async void LoadNavigationItemsAsync()
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
			NavigationIcon = CreateNavigationImage( "Currencies" ),
			NavigationTooltip = $"{Lang.navigation_Resources_SubItem_Currency_Tooltip}",
			Command = new SimpleCommand( () => LoadCurrencyView() )
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

	private void BuildNavigationItems()
	{
		NavigationItems.Clear();

		#region Time section
		#region Subitems
		TimeSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Time_SubItem_Import_Label,
			NavigationIcon = CreateNavigationImage( "Import" ),
			NavigationTooltip = Language.navigation_Time_SubItem_Import_Tooltip
		} );

		TimeSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Time_SubItem_Export_Label,
			NavigationIcon = CreateNavigationImage( "Export" ),
			NavigationTooltip = Language.navigation_Time_SubItem_Export_Tooltip
		} );

		TimeSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Time_SubItem_Report_Label,
			NavigationIcon = CreateNavigationImage( "Report" ),
			NavigationTooltip = Language.navigation_Time_SubItem_Report_Tooltip
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
			NavigationItem = Language.navigation_Inventory_SubItem_Order_Label,
			NavigationIcon = CreateNavigationImage( "Order" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Order_Tooltip
		} );

		InventorySubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Receipt_Label,
			NavigationIcon = CreateNavigationImage( "Recieve" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Receipt_Tooltip
		} );

		InventorySubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Report_Label,
			NavigationIcon = CreateNavigationImage( "Report" ),
			NavigationTooltip = Language.navigation_Inventory_SubItem_Report_Tooltip
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
		#region Subitems
		ProjectSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Projects_SubItem_Import_Label,
			NavigationIcon = CreateNavigationImage( "Import" ),
			NavigationTooltip = Language.navigation_Projects_SubItem_Import_Tooltip
		} );

		ProjectSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Projects_SubItem_Export_Label,
			NavigationIcon = CreateNavigationImage( "Export" ),
			NavigationTooltip = Language.navigation_Projects_SubItem_Export_Tooltip
		} );

		ProjectSubItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Projects_SubItem_Report_Label,
			NavigationIcon = CreateNavigationImage( "Report" ),
			NavigationTooltip = Language.navigation_Projects_SubItem_Report_Tooltip
		} );
		#endregion

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Projects_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Projects" ),
			NavigationTooltip = Language.navigation_Projects_MainItem_Tooltip,
			SubItems = ProjectSubItems
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

		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Resources_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Resources" ),
			NavigationTooltip = Language.navigation_Resources_MainItem_Tooltip,
			SubItems = MetadataSubItems
		} );
		#endregion

		#region Settings section
		NavigationItems.Add( new NavigationModel
		{
			NavigationItem = Language.navigation_Settings_MainItem_Label,
			NavigationIcon = CreateNavigationImage( "Settings" ),
			NavigationTooltip = Language.navigation_Settings_MainItem_Tooltip
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

		public SimpleCommand( Action execute )
		{
			_execute = execute ?? throw new ArgumentNullException( nameof( execute ) );
		}

		public bool CanExecute( object? parameter ) => true;

		public void Execute( object? parameter ) => _execute();
	}
}