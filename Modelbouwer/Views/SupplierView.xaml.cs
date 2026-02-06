using System.Windows.Threading;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for SupplierView.xaml
/// </summary>
public partial class SupplierView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;
	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	public SupplierView( SupplierPageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;
		Loaded += SupplierView_Loaded;
	}

	private void SupplierView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is SupplierPageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfDataGrid.View?.RefreshFilter();
				SfDataGrid.UpdateLayout();
				vm.VisibleSupplierCount = SfDataGrid.View?.Records.Count ?? 0;
			};
		}
	}

	private void SupplierDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfDataGrid grid )
			return;

		if ( DataContext is not SupplierPageViewModel vm )
			return;

		grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterSupplier;
				grid.View.RefreshFilter();
				vm.VisibleSupplierCount = grid.View.Records.Count;
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
			if ( SfDataGrid.ItemsSource is List<SupplierModel> Suppliers )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: Suppliers,
				columnMappings: SupplierModel.ColumnMappings, // mapping van UI naar property
                uniqueProperty: nameof(SupplierModel.SupplierName) // unieke kolom
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
				MessageBox.Show( "The ItemsSource of the DataGrid is not a List<SupplierModel>.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
	}

	private async void ButtonCSVExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralCSVFilter ?? "CSV files (*.csv)|*.csv",
			DefaultExt = ".csv",
			FileName = $"{Lang.ExportSuppliersFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
		};

		if ( dialog.ShowDialog() != true )
			return;

		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>();

		foreach ( var mapping in SupplierModel.ColumnMappings )
		{
			// Use the first header from the array (usually the English/default one)
			columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
		}

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _csvExportService.ExportToCsvAsync<SupplierModel>(
			SfDataGrid,
			dialog.FileName,
			columnHeaders );
		}
	}

	private async void ButtonExcelExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralExcelFilter ?? "Excel Bestanden (*.xlsx)|*.xlsx|Alle Bestanden (*.*)|*.*",
			DefaultExt = ".xlsx",
			FileName = $"{Lang.ExportSuppliersFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
		};

		if ( dialog.ShowDialog() != true )
			return;

		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>();

		foreach ( var mapping in SupplierModel.ColumnMappings )
		{
			// Use the first header from the array (usually the English/default one)
			columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
		}

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _excelExportService.ExportToExcelAsync<SupplierModel>(
			SfDataGrid,
			dialog.FileName,
			columnHeaders );
		}
	}

	private void ButtonWeb( object sender, RoutedEventArgs e )
	{

	}

	private void ContactDataGrid_Loaded( object sender, RoutedEventArgs e )
	{

	}

	private void SupplierMemoEditor_Loaded( object sender, RoutedEventArgs e )
	{

	}
}
