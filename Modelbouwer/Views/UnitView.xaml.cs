using System.Windows.Threading;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for UnitView.xaml
/// </summary>
public partial class UnitView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;
	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	public UnitView( UnitPageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;
		Loaded += UnitView_Loaded;
	}

	private void UnitView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is UnitPageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfDataGrid.View?.RefreshFilter();
				SfDataGrid.UpdateLayout();
				vm.VisibleUnitCount = SfDataGrid.View?.Records.Count ?? 0;
			};
		}
	}

	private void UnitDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfDataGrid grid )
			return;

		if ( DataContext is not UnitPageViewModel vm )
			return;

		_ = grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterUnit;
				grid.View.RefreshFilter();
				vm.VisibleUnitCount = grid.View.Records.Count;
			} ),
			DispatcherPriority.Loaded
		);
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
			if ( SfDataGrid.ItemsSource is List<UnitModel> units )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: units,
				columnMappings: UnitModel.ColumnMappings, // mapping van UI naar property
                uniqueProperty: nameof(UnitModel.UnitName) // unieke kolom
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
				MessageBox.Show( "The ItemsSource of the DataGrid is not a List<UnitModel>.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
	}

	private async void ButtonCSVExport( object sender, RoutedEventArgs e )
	{
		try
		{
			var dialog = new SaveFileDialog
			{
				Filter = Lang.ExportGeneralCSVFilter ?? "CSV files (*.csv)|*.csv",
				DefaultExt = ".csv",
				FileName = $"{Lang.ExportUnitFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
			};

			if ( dialog.ShowDialog() != true )
				return;

			var columnHeaders = new Dictionary<string, string>();

			foreach ( var mapping in UnitModel.ColumnMappings )
			{
				columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
			}

			using ( new UiBusyScope( CustomCursors.Exporting ) )
			{
				await _csvExportService.ExportToCsvAsync<UnitModel>(
				SfDataGrid,
				dialog.FileName,
				columnHeaders );
			}
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.Message, Lang.ExportGeneralFailedMessageboxTitle, MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}

	private async void ButtonExcelExport( object sender, RoutedEventArgs e )
	{
		try
		{
			var dialog = new SaveFileDialog
			{
				Filter = Lang.ExportGeneralExcelFilter ?? "Excel Bestanden (*.xlsx)|*.xlsx|Alle Bestanden (*.*)|*.*",
				DefaultExt = ".xlsx",
				FileName = $"{Lang.ExportUnitFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
			};

			if ( dialog.ShowDialog() != true )
				return;

			var columnHeaders = new Dictionary<string, string>
			{
				{ "UnitName", Lang.ExportUnitHeaderName }
			};

			using ( new UiBusyScope( CustomCursors.Exporting ) )
			{
				await _excelExportService.ExportToExcelAsync<UnitModel>(
				SfDataGrid,
				dialog.FileName,
				columnHeaders );
			}
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.Message, Lang.ExportGeneralFailedMessageboxTitle, MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}
}