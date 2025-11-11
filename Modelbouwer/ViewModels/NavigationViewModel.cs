using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modelbouwer.ViewModels;

public class NavigationViewModel : INotifyPropertyChanged
{
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

	public event PropertyChangedEventHandler PropertyChanged;

	// Raises PropertyChanged event
	protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
	{
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
	}

	public NavigationViewModel()
	{
		NavigationItems = [];

		#region Time section
		#region Subitems
		TimeSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Time_SubItem_Import_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Import" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Time_SubItem_Import_Tooltip
		}
		);
		TimeSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Time_SubItem_Export_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Export" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Time_SubItem_Export_Tooltip
		}
		);
		TimeSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Time_SubItem_Report_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Report" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Time_SubItem_Report_Tooltip
		}
		);
		#endregion

		NavigationItems.Add(new()
		{
			NavigationItem = Language.navigation_Time_MainItem_Label ,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource("NavigationIconStyle") as Style,
				Source = Application.Current.FindResource("Time") as ImageSource
			},
			NavigationTooltip = Language.navigation_Time_MainItem_Label,
			SubItems = TimeSubItems
		} );
		#endregion

		#region Inventory section
		#region Subitems
		InventorySubItems.Add( new()
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Order_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Order" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Inventory_SubItem_Order_Tooltip
		}
		);
		InventorySubItems.Add( new()
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Receipt_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Recieve" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Inventory_SubItem_Receipt_Tooltip
		}
		);
		InventorySubItems.Add( new()
		{
			NavigationItem = Language.navigation_Inventory_SubItem_Report_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Report" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Inventory_SubItem_Report_Tooltip
		}
		);
		#endregion

		NavigationItems.Add( new()
		{
			NavigationItem = Language.navigation_Inventory_MainItem_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Inventory" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Inventory_MainItem_Tooltip,
			SubItems = InventorySubItems
		} );
		#endregion

		#region Projects section
		#region Subitems
		ProjectSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Projects_SubItem_Import_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Import" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Projects_SubItem_Import_Tooltip
		}
		);
		ProjectSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Projects_SubItem_Export_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Export" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Projects_SubItem_Export_Tooltip
		}
		);
		ProjectSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Projects_SubItem_Report_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Report" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Projects_SubItem_Report_Tooltip
		}
		);
		#endregion

		NavigationItems.Add( new()
		{
			NavigationItem = Language.navigation_Projects_MainItem_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Projects" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Projects_MainItem_Tooltip,
			SubItems = ProjectSubItems
		} );
		#endregion

		#region Metadata section
		#region Subitems
		MetadataSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_SubItem_Currency_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Currency" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_SubItem_Currency_Tooltip,
			SubItems = MetadataCurrenciesSubItems
		} );

		MetadataSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_SubItem_Country_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Countries" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_SubItem_Country_Tooltip,
			Command = new RelayCommand( _ => CurrentView = new CountryView() ),
			SubItems = MetadataCountriesSubItems
		} );

		#endregion

		#region CountrySubitems
		MetadataCountriesSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_SubItem_Country_SubItem_Import_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Import" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_SubItem_Country_SubItem_Import_Tooltip,
			Command = new RelayCommand( _ => CurrentView = new CountryView() )
		}
		);
		MetadataCountriesSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_SubItem_Country_SubItem_Export_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Export" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_SubItem_Country_SubItem_Export_Tooltip
		}
		);
		#endregion

		#region CurrencySubitems
		MetadataCurrenciesSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_SubItem_Currency_SubItem_Import_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Import" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_SubItem_Currency_SubItem_Import_Tooltip
		}
		);
		MetadataCurrenciesSubItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_SubItem_Currency_SubItem_Export_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Export" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_SubItem_Currency_SubItem_Export_Tooltip
		}
		);
		#endregion

		NavigationItems.Add( new()
		{
			NavigationItem = Language.navigation_Resources_MainItem_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Resources" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Resources_MainItem_Tooltip,
			SubItems = MetadataSubItems
		} );
		#endregion

		#region Settings section
		NavigationItems.Add( new()
		{
			NavigationItem = Language.navigation_Settings_MainItem_Label,
			NavigationIcon = new Image()
			{
				Style = Application.Current.FindResource( "NavigationIconStyle" ) as Style,
				Source = Application.Current.FindResource( "Settings" ) as ImageSource
			},
			NavigationTooltip = Language.navigation_Settings_MainItem_Tooltip
		} );
		#endregion
	}
}
