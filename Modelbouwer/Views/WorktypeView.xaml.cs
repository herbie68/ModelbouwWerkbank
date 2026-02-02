using System.Windows.Threading;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for WorktypeView.xaml
/// </summary>
public partial class WorktypeView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;
	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	public WorktypeView( WorktypePageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;

		if ( DataContext is WorktypePageViewModel vm )
		{
			vm._filterChanged += () =>
			{
				if ( SfGridTree.View == null )
					return;

				if ( string.IsNullOrWhiteSpace( vm.SearchText ) )
					SfGridTree.View.Filter = null;
				else
					SfGridTree.View.Filter = vm.FilterRecords;

				SfGridTree.View.RefreshFilter();
			};
		}

		Loaded += WorktypeView_Loaded;
	}

	private void WorktypeView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is WorktypePageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfGridTree.View?.RefreshFilter();
				SfGridTree.UpdateLayout();
			};
		}
	}

	private void WorktypeDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfTreeGrid grid )
			return;

		if ( DataContext is not WorktypePageViewModel vm )
			return;

		grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterWorkType;
				grid.View.RefreshFilter();
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
			if ( SfGridTree.ItemsSource is List<WorktypeModel> currencies )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: currencies,
				columnMappings: WorktypeModel.ColumnMappings, // mapping van UI naar property
                uniqueProperty: nameof(WorktypeModel.WorktypeName) // unieke kolom
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
				SfGridTree.View.Refresh();
			}
			else
			{
				MessageBox.Show( "The ItemsSource of the DataGrid is not a List<WorktypeModel>.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
	}

	private async void ButtonCSVExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralCSVFilter ?? "CSV files (*.csv)|*.csv",
			DefaultExt = ".csv",
			FileName = $"{Lang.ExportWorkTypeFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
		};

		if ( dialog.ShowDialog() != true )
			return;

		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>();

		foreach ( var mapping in WorktypeModel.ColumnMappings )
		{
			// Use the first header from the array (usually the English/default one)
			columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
		}

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _csvExportService.ExportToCsvAsync<WorktypeModel>(
			SfGridTree,
			dialog.FileName,
			columnHeaders,
			null );
		}
	}

	private async void ButtonExcelExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralExcelFilter ?? "Excel Bestanden (*.xlsx)|*.xlsx|Alle Bestanden (*.*)|*.*",
			DefaultExt = ".xlsx",
			FileName = $"{Lang.ExportWorkTypeFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
		};

		if ( dialog.ShowDialog() != true )
			return;

		var columnHeaders = new Dictionary<string, string>
		{
			{ "WorktypeName", Lang.ExportWorkTypeHeaderName }
		};

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _excelExportService.ExportToExcelAsync<WorktypeModel>(
			SfGridTree,
			dialog.FileName,
			columnHeaders );
		}
	}
}
