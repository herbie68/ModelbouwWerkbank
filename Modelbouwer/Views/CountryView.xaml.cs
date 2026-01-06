using System.Windows.Controls;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Modelbouwer.Services;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.XlsIO;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for CountryView.xaml
/// </summary>
public partial class CountryView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;

	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	public CountryView( CountryPageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;
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

				grid.View.Filter = vm.FilterCountry;
				grid.View.RefreshFilter();
				vm.VisibleCountryCount = grid.View.Records.Count;
			} ),
			DispatcherPriority.Loaded
		);
	}

	private void ButtonNew( object sender, RoutedEventArgs e )
	{
		if ( DataContext is CountryPageViewModel vm )
		{
			vm.AddCountryCommand.Execute( null );
		}
	}

	private void ButtonDelete( object sender, RoutedEventArgs e )
	{
		if ( DataContext is CountryPageViewModel vm )
		{
			vm.DeleteCountryCommand.Execute( null );
		}
	}

	private async void ButtonSave( object sender, RoutedEventArgs e )
	{
		if ( DataContext is CountryPageViewModel vm )
		{
			await vm.SaveCountryCommand.ExecuteAsync( null );
		}
	}

	private async void ButtonImport( object sender, RoutedEventArgs e )
	{
		if ( DataContext is CountryPageViewModel vm )
		{
			await vm.ImportCountriesCommand.ExecuteAsync( null );
		}
	}

	private async void ButtonCSVExport( object sender, RoutedEventArgs e )
	{
		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>
		{
			{ "CountryCode", Lang.ExportCountriesHeaderCountryCode },
			{ "CountryName", Lang.ExportCountriesHeaderCountryName },
			{ "CountryCurrencySymbol", Lang.ExportCountriesHeaderCountryCurrencySymbol }
		};

		await _csvExportService.ExportToCsvAsync<CountryModel>(
			SfDataGrid,
			$"{Lang.ExportCountriesFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
			columnHeaders );
	}

	private async void ButtonExcelExport( object sender, RoutedEventArgs e )
	{
		var columnHeaders = new Dictionary<string, string>
	{
		{ "CountryCode", Lang.ExportCountriesHeaderCountryCode },
		{ "CountryName", Lang.ExportCountriesHeaderCountryName },
		{ "CountryCurrencySymbol", Lang.ExportCountriesHeaderCountryCurrencySymbol }
	};

		await _excelExportService.ExportToExcelAsync<CountryModel>(
			SfDataGrid,
			$"{Lang.ExportCountriesFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
			columnHeaders );
	}
}
