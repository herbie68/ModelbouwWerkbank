using System.Windows.Threading;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for StorageLocationView.xaml
/// </summary>
public partial class StorageLocationView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;
	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	public StorageLocationView( StorageLocationPageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;

		if ( DataContext is StorageLocationPageViewModel vm )
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

		Loaded += StorageLocationView_Loaded;
	}

	private void StorageLocationView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is StorageLocationPageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfGridTree.View?.RefreshFilter();
				SfGridTree.UpdateLayout();
			};
		}
	}

	private void StorageLocationDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfTreeGrid grid )
			return;

		if ( DataContext is not StorageLocationPageViewModel vm )
			return;

		_ = grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterStorageLocation;
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
			if ( SfGridTree.ItemsSource is List<StorageLocationModel> currencies )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: currencies,
				columnMappings: StorageLocationModel.ColumnMappings,
				uniqueProperty: nameof(StorageLocationModel.StorageName)
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
				MessageBox.Show( "The ItemsSource of the DataGrid is not a List<StorageLocationModel>.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
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
				FileName = $"{Lang.ExportStorageLocationFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
			};

			if ( dialog.ShowDialog() != true )
				return;

			var columnHeaders = new Dictionary<string, string>();

			foreach ( var mapping in StorageLocationModel.ColumnMappings )
			{
				columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
			}

			using ( new UiBusyScope( CustomCursors.Exporting ) )
			{
				await _csvExportService.ExportToCsvAsync<StorageLocationModel>(
				SfGridTree,
				dialog.FileName,
				columnHeaders,
				null );
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
				FileName = $"{Lang.ExportStorageLocationFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
			};

			if ( dialog.ShowDialog() != true )
				return;

			var columnHeaders = new Dictionary<string, string>
			{
				{ "StorageName", Lang.ExportStorageLocationHeaderName }
			};

			using ( new UiBusyScope( CustomCursors.Exporting ) )
			{
				await _excelExportService.ExportToExcelAsync<StorageLocationModel>(
				SfGridTree,
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