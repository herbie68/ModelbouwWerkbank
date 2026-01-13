using ClosedXML.Excel;

using Syncfusion.UI.Xaml.Grid;

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace Modelbouwer.Services;
public class ExcelExportService : IExportService
{
	public bool FreezeHeaderRow { get; set; } = true;
	public string WorksheetName { get; set; } = "Data";

	private readonly ILanguageProvider? _languageProvider;

	public ExcelExportService( ILanguageProvider? languageProvider = null )
	{
		_languageProvider = languageProvider;
	}

	// CSV is niet van toepassing hier
	public Task ExportToCsvAsync<T>(
		SfDataGrid dataGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides = null,
		Func<T, GridColumn, string>? customValueFormatter = null )
		=> Task.CompletedTask;

	public async Task ExportToExcelAsync<T>(
		SfDataGrid dataGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides = null,
		Func<T, GridColumn, string>? customValueFormatter = null )
	{
		try
		{
			ExcelExportData<T>? exportData = null;

			await dataGrid.Dispatcher.InvokeAsync( () =>
			{
				exportData = PrepareExportData<T>( dataGrid, columnHeaderOverrides );
			} );

			if ( exportData == null )
				return;

			await Task.Run( () =>
			{
				GenerateExcel( exportData, filePath, customValueFormatter );
			} );

			ShowSuccessMessage( filePath, exportData.Items.Count );
		}
		catch ( Exception ex )
		{
			ShowErrorMessage( ex );
		}
	}

	// ------------------------
	// Data preparation
	// ------------------------

	private ExcelExportData<T> PrepareExportData<T>(
		SfDataGrid grid,
		Dictionary<string, string>? columnHeaderOverrides )
	{
		var data = new ExcelExportData<T>();

		foreach ( var column in grid.Columns )
		{
			if ( column.IsHidden || string.IsNullOrWhiteSpace( column.MappingName ) )
				continue;

			data.Columns.Add( new ExcelExportColumn
			{
				MappingName = column.MappingName,
				HeaderText = GetColumnHeader( column, columnHeaderOverrides ),
				Column = column,
				Width = column.ActualWidth
			} );

			data.Headers.Add(
				GetColumnHeader( column, columnHeaderOverrides ) );
		}

		foreach ( var record in grid.View.Records )
		{
			if ( record.Data is T item )
				data.Items.Add( item );
		}

		Debug.WriteLine( $"Excel export: {data.Items.Count} rows, {data.Columns.Count} columns" );
		return data;
	}

	// ------------------------
	// Excel generation
	// ------------------------

	private void GenerateExcel<T>(
		ExcelExportData<T> data,
		string filePath,
		Func<T, GridColumn, string>? formatter )
	{
		using var workbook = new XLWorkbook();
		var ws = workbook.Worksheets.Add(WorksheetName);

		// Headers
		for ( int c = 0; c < data.Headers.Count; c++ )
		{
			var cell = ws.Cell(1, c + 1);
			cell.Value = data.Headers [ c ];
			cell.Style.Font.Bold = true;
			cell.Style.Fill.BackgroundColor = XLColor.LightGray;
			cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

			var width = data.Columns[c].Width;
			ws.Column( c + 1 ).Width = width > 0 ? width / 7.5 : 12;
		}

		// Rows
		for ( int r = 0; r < data.Items.Count; r++ )
		{
			var item = data.Items[r];

			for ( int c = 0; c < data.Columns.Count; c++ )
			{
				var col = data.Columns[c];
				var cell = ws.Cell(r + 2, c + 1);

				string value = formatter != null
					? formatter(item, col.Column)
					: GetValue(item, col.MappingName);

				SetCellValue( cell, value, item, col.MappingName );

				if ( r % 2 == 1 )
					cell.Style.Fill.BackgroundColor = XLColor.AliceBlue;
			}
		}

		if ( FreezeHeaderRow )
			ws.SheetView.FreezeRows( 1 );

		workbook.SaveAs( filePath );
	}

	// ------------------------
	// Helpers
	// ------------------------

	private static string GetValue<T>( T item, string mapping )
	{
		var prop = typeof(T).GetProperty(mapping);
		var value = prop?.GetValue(item);
		return value?.ToString() ?? string.Empty;
	}

	private static void SetCellValue(
		IXLCell cell,
		string text,
		object item,
		string mapping )
	{
		var prop = item.GetType().GetProperty(mapping);
		var value = prop?.GetValue(item);

		if ( value is DateTime dt )
		{
			cell.Value = dt;
			cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
		}
		else if ( value is bool b )
		{
			cell.Value = b;
		}
		else if ( value is IFormattable f )
		{
			cell.Value = Convert.ToDouble( f, CultureInfo.InvariantCulture );
		}
		else
		{
			cell.Value = text;
		}
	}

	private string GetColumnHeader(
		GridColumn column,
		Dictionary<string, string>? overrides )
	{
		if ( overrides?.TryGetValue( column.MappingName, out var header ) == true )
			return header;

		return _languageProvider?.GetTranslation( $"ExportHeader_{column.MappingName}" )
			   ?? column.HeaderText
			   ?? column.MappingName;
	}

	private void ShowSuccessMessage( string file, int count )
	{
		MessageBox.Show(
			$"Exported {count} records to {Path.GetFileName( file )}",
			"Excel export",
			MessageBoxButton.OK,
			MessageBoxImage.Information );
	}

	private void ShowErrorMessage( Exception ex )
	{
		MessageBox.Show(
			ex.Message,
			"Excel export failed",
			MessageBoxButton.OK,
			MessageBoxImage.Error );
	}
}

// ------------------------
// Internal models
// ------------------------

internal sealed class ExcelExportData<T>
{
	public List<T> Items { get; } = new();
	public List<string> Headers { get; } = new();
	public List<ExcelExportColumn> Columns { get; } = new();
}

internal sealed class ExcelExportColumn
{
	public string MappingName { get; set; } = string.Empty;
	public string HeaderText { get; set; } = string.Empty;
	public GridColumn Column { get; set; } = null!;
	public double Width { get; set; }
}