using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for CountryView.xaml
/// </summary>
public partial class CountryView : UserControl
{
	private readonly CountryService _countryService;
	private readonly GenericDataService _genericDataService;

	//readonly string _exportFolder = SettingsService.Instance.Settings.ExportFolder;
	public CountryView()
	{
		InitializeComponent();

		_genericDataService = new GenericDataService();
		_countryService = new CountryService( _genericDataService );

		Loaded += CountryView_Loaded;
	}

	private void CountryView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is CountryPageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfDataGrid.View?.RefreshFilter();
				SfDataGrid.UpdateLayout();

				vm.VisibleCountryCount = SfDataGrid.View?.Records.Count ?? 0;
			};

			// Dispatcher voor ComboBoxAdv
			Dispatcher.BeginInvoke( new Action( () =>
			{
				CurrencyComboBox.ItemsSource = vm.Currencies;
				CurrencyComboBox.SelectedValue = vm.SelectedCountry?.CountryCurrencyId;
			} ), System.Windows.Threading.DispatcherPriority.Loaded );
		}
	}

	private void CountryDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfDataGrid grid )
			return;

		if ( DataContext is not CountryPageViewModel vm )
			return;

		grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				// Search forthe entered text in the Serach box
				grid.View.Filter = vm.FilterCountry;
				grid.View.RefreshFilter();

				vm.VisibleCountryCount = grid.View.Records.Count;// Update the visible country count
			} ),
			System.Windows.Threading.DispatcherPriority.Loaded
		);
	}

	private void ButtonNew( object sender, RoutedEventArgs e )
	{
		if ( DataContext is not CountryPageViewModel vm )
			return;

		vm.AddCountryCommand.Execute( null );

		Dispatcher.BeginInvoke( () =>
		{
			var rowIndex = SfDataGrid.ResolveToRowIndex(vm.SelectedCountry);
			SfDataGrid.ScrollInView( new RowColumnIndex( rowIndex, 0 ) );

			SfDataGrid.SelectedItem = vm.SelectedCountry;
		}, DispatcherPriority.Background );
	}

	private void ButtonDelete( object sender, RoutedEventArgs e )
	{

	}

	private async void ButtonSave( object sender, RoutedEventArgs e )
	{
		if ( DataContext is not CountryPageViewModel vm )
			return;

		var country = vm.SelectedCountry;
		if ( country == null )
			return;

		var _queryParameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.CountryFieldNameName}", country.CountryName ?? string.Empty},
			{ $"@{DBNames.CountryFieldNameCode}", country.CountryCode ?? string.Empty},
			{ $"@{DBNames.CountryFieldNameCurrencyId}", country.CountryCurrencyId },
			{ $"@{DBNames.CountryFieldNameCurrencySymbol}", (( Modelbouwer.Models.CurrencyModel ) CurrencyComboBox.SelectedItem ).CurrencySymbol ?? string.Empty},
			{ $"@{DBNames.CountryFieldNameId}", country.CountryId }
		};
		if (country.CountryId == 0 )
		{
			await _countryService.InsertNewCountryAsync( _queryParameters );

			country.CountryId = ( int ) await _genericDataService.GetLastInsertIdAsync( );
		}
		else
		{
			await _countryService.UpdateCountryAsync( _queryParameters );
		}
	}

	private void ButtonImport( object sender, RoutedEventArgs e )
	{
		var dialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = $"{Lang.ImportCSVFilter}",
		};

		if ( dialog.ShowDialog() == true )
		{
			// Haal de lijst op uit de DataGrid
			if ( SfDataGrid.ItemsSource is List<CountryModel> countries )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: countries,
				columnMappings: CountryModel.ColumnMappings, // mapping van UI naar property
                uniqueProperty: nameof(CountryModel.CountryName) // unieke kolom
            );

				MessageBox.Show(
					$"{Lang.ImportMessagboxCompletedRead}: {result.TotalRows}\n" +
					$"{Lang.ImportMessagboxCompletedImported}: {result.Imported}\n" +
					$"{Lang.ImportMessagboxCompletedSkipped}: {result.Skipped}\n" +
					$"{Lang.ImportMessagboxCompletedModified}: {result.Updated}",
					$"{Lang.ImportMessagboxCompletedTitle}",
					MessageBoxButton.OK,
					MessageBoxImage.Information
				);

				// Forceer datagrid refresh
				SfDataGrid.View.Refresh();
			}
			else
			{
				MessageBox.Show( "De ItemsSource van de DataGrid is geen List<CountryModel>.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
	}

	private void ButtonExport( object sender, RoutedEventArgs e )
	{
		var dialog = new Microsoft.Win32.SaveFileDialog
		{
			Filter = $"{Lang.ExportCSVFilter}",
			FileName = $"{Lang.ExportCountriesFileName} - {DateTime.Now:yyyyMMdd-HHmmss}.csv"
		};

		if ( dialog.ShowDialog() == true )
		{
			DataGridExportService.ExportToCsv( SfDataGrid, dialog.FileName );
			MessageBox.Show( Lang.AppMessageExportComplete, "Export", MessageBoxButton.OK, MessageBoxImage.Information );
		}
	}
}
