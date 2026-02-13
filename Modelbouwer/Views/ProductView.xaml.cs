using System.Windows.Threading;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for ProductViewxaml.xaml
/// </summary>
public partial class ProductView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;
	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	/// <summary>
	/// Initializes a new instance of ProductView with the specified view model and export services.
	/// </summary>
	/// <param name="viewModel">The view model to set as the control's DataContext.</param>
	/// <param name="csvExportService">The service used to export data to CSV.</param>
	/// <param name="excelExportService">The service used to export data to Excel.</param>
	public ProductView( ProductPageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;
		Loaded += ProductView_Loaded;
	}

	/// <summary>
	/// On control load, assigns the view model's RefreshGridFilter action to refresh the data grid's filter, update layout, and set the view model's VisibleProductCount.
	/// </summary>
	/// <param name="sender">The source of the Loaded event.</param>
	/// <param name="e">Event data for the Loaded event.</param>
	private void ProductView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is ProductPageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfDataGrid.View?.RefreshFilter();
				SfDataGrid.UpdateLayout();
				vm.VisibleProductCount = SfDataGrid.View?.Records.Count ?? 0;
			};
		}
	}

	/// <summary>
	/// Applies the view model's product filter to the data grid and updates the view model's VisibleProductCount after the grid's view is available.
	/// </summary>
	/// <remarks>
	/// Invoked on the grid's Loaded event; the filter application and count update are dispatched on the grid's dispatcher with Loaded priority.
	/// </remarks>
	private void ProductDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfDataGrid grid )
			return;

		if ( DataContext is not ProductPageViewModel vm )
			return;

		grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterProduct;
				grid.View.RefreshFilter();
				vm.VisibleProductCount = grid.View.Records.Count;

			} ),
			DispatcherPriority.Loaded
		);
	}

	/// <summary>
	/// Prompts the user to select a CSV file and imports its rows into the DataGrid's item list when that list is a List&lt;ProductModel&gt;.
	/// </summary>
	/// <remarks>
	/// Uses ProductModel.ColumnMappings to map CSV columns and uses ProductModel.Name as the unique key; displays an import summary message box and refreshes the grid view. If the DataGrid's ItemsSource is not a List&lt;ProductModel&gt;, displays an error message.
	/// </remarks>
	private void ButtonImport( object sender, RoutedEventArgs e )
	{
		var dialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = $"{Lang.ImportCSVFilter}",
		};

		if ( dialog.ShowDialog() == true )
		{
			// Haal de lijst op uit de DataGrid
			if ( SfDataGrid.ItemsSource is List<ProductModel> Products )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: Products,
				columnMappings: ProductModel.ColumnMappings, // mapping van UI naar property
				uniqueProperty: nameof(ProductModel.Name) // unieke kolom
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
				MessageBox.Show( "The ItemsSource of the DataGrid is not a List<ProductModel>.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
	}

	/// <summary>
	/// Prompts the user to choose a file and exports the current product grid to a CSV file.
	/// </summary>
	/// <remarks>
	/// Builds column headers from ProductModel.ColumnMappings (takes the first header for each mapping),
	/// then writes the grid contents to the selected CSV file using the CSV export service while showing an exporting cursor.
	/// If the user cancels the save dialog, no action is taken.
	/// </remarks>
	private async void ButtonCSVExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralCSVFilter ?? "CSV files (*.csv)|*.csv",
			DefaultExt = ".csv",
			FileName = $"{Lang.ExportProductsFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
		};

		if ( dialog.ShowDialog() != true )
			return;

		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>();

		foreach ( var mapping in ProductModel.ColumnMappings )
		{
			// Use the first header from the array (usually the English/default one)
			columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
		}

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _csvExportService.ExportToCsvAsync<ProductModel>(
			SfDataGrid,
			dialog.FileName,
			columnHeaders );
		}
	}

	/// <summary>
	/// Prompts the user to select a destination file and exports the current product grid to an Excel (.xlsx) file using ProductModel column mappings for the column headers.
	/// </summary>
	private async void ButtonExcelExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralExcelFilter ?? "Excel Bestanden (*.xlsx)|*.xlsx|Alle Bestanden (*.*)|*.*",
			DefaultExt = ".xlsx",
			FileName = $"{Lang.ExportProductsFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
		};

		if ( dialog.ShowDialog() != true )
			return;

		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>();

		foreach ( var mapping in ProductModel.ColumnMappings )
		{
			// Use the first header from the array (usually the English/default one)
			columnHeaders [ mapping.Key ] = mapping.Value [ 0 ];
		}

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _excelExportService.ExportToExcelAsync<ProductModel>(
			SfDataGrid,
			dialog.FileName,
			columnHeaders );
		}
	}

	private void ContactDataGrid_Loaded( object sender, RoutedEventArgs e )
	{

	}

	/// <summary>
	/// Handles the Loaded event for the product memo editor control.
	/// </summary>
	/// <param name="sender">The control that raised the Loaded event (the memo editor).</param>
	/// <param name="e">Event data for the Loaded event.</param>
	private void ProductMemoEditor_Loaded( object sender, RoutedEventArgs e )
	{

	}
}