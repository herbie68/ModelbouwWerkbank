using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for CategoryView.xaml
/// </summary>
public partial class CategoryView : UserControl
{
	private readonly CsvExportService _csvExportService;
	private readonly ExcelExportService _excelExportService;
	public bool ExportIds { get; set; } = true;
	public string CsvSeparator { get; set; } = ";";
	public bool IncludeHeaders { get; set; } = true;
	public Encoding CsvEncoding { get; set; } = Encoding.UTF8;

	private bool _wasFiltering;

	public CategoryView( CategoryPageViewModel viewModel, CsvExportService csvExportService, ExcelExportService excelExportService )
	{
		InitializeComponent();
		DataContext = viewModel;
		_csvExportService = csvExportService;
		_excelExportService = excelExportService;

		if ( DataContext is CategoryPageViewModel vm )
		{
			vm.filterChanged += () =>
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

		Loaded += CategoryView_Loaded;
	}

	private void CategoryView_Loaded( object sender, RoutedEventArgs e )
	{
		if ( DataContext is CategoryPageViewModel vm )
		{
			vm.RefreshGridFilter = () =>
			{
				SfGridTree.View?.RefreshFilter();
				SfGridTree.UpdateLayout();
			};
		}
	}

	private void CategoryDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfTreeGrid grid )
			return;

		if ( DataContext is not CategoryPageViewModel vm )
			return;

		grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterCategory;
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
			if ( SfGridTree.ItemsSource is List<CategoryModel> currencies )
			{
				// Voer de import uit
				var result = CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: currencies,
				columnMappings: CategoryModel.ColumnMappings, // mapping van UI naar property
                uniqueProperty: nameof(CategoryModel.CategoryName) // unieke kolom
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
				MessageBox.Show( "The ItemsSource of the DataGrid is not a List<CategoryModel>.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
	}

	private async void ButtonCSVExport( object sender, RoutedEventArgs e )
	{
		var dialog = new SaveFileDialog
		{
			Filter = Lang.ExportGeneralCSVFilter ?? "CSV files (*.csv)|*.csv",
			DefaultExt = ".csv",
			FileName = $"{Lang.ExportCategoryFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
		};

		if ( dialog.ShowDialog() != true )
			return;

		// Defineer custom headers voor deze view
		var columnHeaders = new Dictionary<string, string>
		{
			{ "CategoryName", Lang.ExportCurrenciesHeaderName }
		};

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _csvExportService.ExportToCsvAsync<CategoryModel>(
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
			FileName = $"{Lang.ExportCategoryFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
		};

		if ( dialog.ShowDialog() != true )
			return;

		var columnHeaders = new Dictionary<string, string>
		{
			{ "CategoryName", Lang.ExportCategoryHeaderName }
		};

		using ( new UiBusyScope( CustomCursors.Exporting ) )
		{
			await _excelExportService.ExportToExcelAsync<CategoryModel>(
			SfGridTree,
			dialog.FileName,
			columnHeaders );
		}
	}
}
