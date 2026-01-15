using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;

using sfGridColumn = Syncfusion.UI.Xaml.Grid.GridColumn;

namespace Modelbouwer.Services;

public class CsvExportService : IExportService
{
	public string Separator { get; set; } = ";";
	public Encoding Encoding { get; set; } = Encoding.UTF8;
	public bool IncludeBom { get; set; } = true;
	public bool IncludeHeaders { get; set; } = true;

	private readonly ILanguageProvider? _languageProvider;

	public CsvExportService( ILanguageProvider? languageProvider = null )
	{
		_languageProvider = languageProvider;
	}

	#region Public Export Methods

	public Task ExportToCsvAsync<T>(
		SfDataGrid dataGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides = null,
		Func<T, sfGridColumn, string>? customValueFormatter = null )
	{
		return ExportSfDataGridAsync( dataGrid, filePath, columnHeaderOverrides, customValueFormatter );
	}

	public Task ExportToCsvAsync<T>(
		SfTreeGrid treeGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides = null,
		Func<T, TreeGridColumn, string>? customValueFormatter = null )
	{
		return ExportSfTreeGridAsync( treeGrid, filePath, columnHeaderOverrides, customValueFormatter );
	}

	#endregion

	#region Internal Export Implementations

	private async Task ExportSfDataGridAsync<T>(
		SfDataGrid dataGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides,
		Func<T, sfGridColumn, string>? customValueFormatter )
	{
		ExportData<T>? exportData = null;

		await dataGrid.Dispatcher.InvokeAsync( () =>
		{
			exportData = PrepareDataGridExportData<T>( dataGrid, columnHeaderOverrides );
		} );

		if ( exportData == null )
			return;

		await Task.Run( () =>
		{
			var csv = GenerateCsvContent<T, sfGridColumn>(exportData, customValueFormatter);
			File.WriteAllText( filePath, csv, Encoding );
		} );

		ShowSuccessMessage( filePath, exportData.Items.Count );
	}

	private async Task ExportSfTreeGridAsync<T>(
		SfTreeGrid treeGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides,
		Func<T, TreeGridColumn, string>? customValueFormatter )
	{
		ExportData<T>? exportData = null;

		await treeGrid.Dispatcher.InvokeAsync( () =>
		{
			exportData = PrepareTreeGridExportData<T>( treeGrid, columnHeaderOverrides );
		} );

		if ( exportData == null )
			return;

		await Task.Run( () =>
		{
			var csv = GenerateCsvContent<T, TreeGridColumn>(exportData, customValueFormatter);
			File.WriteAllText( filePath, csv, Encoding );
		} );

		ShowSuccessMessage( filePath, exportData.Items.Count );
	}

	#endregion

	#region Prepare Export Data

	private ExportData<T> PrepareDataGridExportData<T>(
		SfDataGrid grid,
		Dictionary<string, string>? columnHeaderOverrides )
	{
		var exportData = new ExportData<T>();

		foreach ( var column in grid.Columns )
		{
			if ( !column.IsHidden && column.MappingName != null )
			{
				exportData.ColumnInfos.Add( new ColumnInfo
				{
					MappingName = column.MappingName,
					HeaderText = GetColumnHeader( column, columnHeaderOverrides ),
					Column = column
				} );
				exportData.Headers.Add( GetColumnHeader( column, columnHeaderOverrides ) );
			}
		}

		foreach ( var record in grid.View.Records )
		{
			if ( record.Data is T item )
				exportData.Items.Add( item );
		}

		return exportData;
	}

	private ExportData<T> PrepareTreeGridExportData<T>(
		SfTreeGrid grid,
		Dictionary<string, string>? columnHeaderOverrides )
	{
		var exportData = new ExportData<T>();

		foreach ( var column in grid.Columns )
		{
			if ( !column.IsHidden && column.MappingName != null )
			{
				exportData.ColumnInfos.Add( new ColumnInfo
				{
					MappingName = column.MappingName,
					HeaderText = GetColumnHeader( column, columnHeaderOverrides ),
					Column = column
				} );
				exportData.Headers.Add( GetColumnHeader( column, columnHeaderOverrides ) );
			}
		}

		foreach ( var node in grid.View.Nodes )
		{
			if ( node.Item is T item )
				exportData.Items.Add( item );
		}

		return exportData;
	}

	#endregion

	#region CSV Generation

	private string GenerateCsvContent<T, TColumn>(
		ExportData<T> exportData,
		Func<T, TColumn, string>? customValueFormatter )
		where TColumn : class
	{
		var csvBuilder = new StringBuilder();

		if ( IncludeBom )
			csvBuilder.Append( '\uFEFF' );

		if ( IncludeHeaders )
			csvBuilder.AppendLine( string.Join( Separator, exportData.Headers ) );

		foreach ( var item in exportData.Items )
		{
			var rowValues = new List<string>();

			foreach ( var columnInfo in exportData.ColumnInfos )
			{
				string value;
				try
				{
					if ( customValueFormatter != null && columnInfo.Column is TColumn col )
						value = customValueFormatter( item, col );
					else
						value = GetCellValueSafely( item, columnInfo.MappingName );
				}
				catch
				{
					value = GetCellValueSafely( item, columnInfo.MappingName );
				}

				rowValues.Add( FormatValueForCsv( value ) );
			}

			csvBuilder.AppendLine( string.Join( Separator, rowValues ) );
		}

		return csvBuilder.ToString();
	}

	private string GetCellValueSafely<T>( T item, string mappingName )
	{
		try
		{
			var prop = typeof(T).GetProperty(mappingName);
			if ( prop != null )
			{
				var val = prop.GetValue(item);
				return val?.ToString() ?? string.Empty;
			}
		}
		catch { }

		return string.Empty;
	}

	#endregion

	#region Column Headers

	private string GetColumnHeader( sfGridColumn column, Dictionary<string, string>? columnHeaderOverrides )
	{
		var name = column.MappingName;
		if ( columnHeaderOverrides?.ContainsKey( name ) == true )
			return columnHeaderOverrides [ name ];

		if ( _languageProvider != null )
		{
			var translated = _languageProvider.GetTranslation($"ExportHeader_{name}");
			if ( !string.IsNullOrEmpty( translated ) )
				return translated;
		}

		return !string.IsNullOrWhiteSpace( column.HeaderText ) ? column.HeaderText : name ?? string.Empty;
	}

	private string GetColumnHeader( TreeGridColumn column, Dictionary<string, string>? columnHeaderOverrides )
	{
		var name = column.MappingName;
		if ( columnHeaderOverrides?.ContainsKey( name ) == true )
			return columnHeaderOverrides [ name ];

		if ( _languageProvider != null )
		{
			var translated = _languageProvider.GetTranslation($"ExportHeader_{name}");
			if ( !string.IsNullOrEmpty( translated ) )
				return translated;
		}

		return !string.IsNullOrWhiteSpace( column.HeaderText ) ? column.HeaderText : name ?? string.Empty;
	}

	#endregion

	#region Helpers

	private string FormatValueForCsv( string value )
	{
		if ( string.IsNullOrEmpty( value ) )
			return string.Empty;

		if ( value.Contains( Separator ) || value.Contains( "\"" ) || value.Contains( "\n" ) || value.Contains( "\r" ) )
			return $"\"{value.Replace( "\"", "\"\"" )}\"";

		return value;
	}

	private void ShowSuccessMessage( string filePath, int recordCount )
	{
		try
		{
			var message = _languageProvider?.GetTranslation("ExportGeneralSuccess")
						  ?.Replace("{count}", recordCount.ToString())
						  ?.Replace("{file}", Path.GetFileName(filePath))
						  ?? $"Exported {recordCount} records to {Path.GetFileName(filePath)}";

			if ( Application.Current != null )
			{
				Application.Current.Dispatcher.BeginInvoke( new Action( () =>
				{
					MessageBox.Show( message, "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information );
				} ) );
			}
		}
		catch { }
	}

	#endregion
}