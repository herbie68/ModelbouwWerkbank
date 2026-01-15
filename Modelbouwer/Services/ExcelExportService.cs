using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Modelbouwer.Services;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.Services;

public class ExcelExportService : IExportService
{
	private readonly ILanguageProvider? _languageProvider;

	public ExcelExportService( ILanguageProvider? languageProvider = null )
	{
		_languageProvider = languageProvider;
	}

	#region CSV Methods (noop)

	public Task ExportToCsvAsync<T>( SfDataGrid dataGrid, string filePath, Dictionary<string, string>? columnHeaderOverrides = null, Func<T, GridColumn, string>? customValueFormatter = null )
	{
		// Not supported for Excel service
		return Task.CompletedTask;
	}

	public Task ExportToCsvAsync<T>( SfTreeGrid treeGrid, string filePath, Dictionary<string, string>? columnHeaderOverrides = null, Func<T, TreeGridColumn, string>? customValueFormatter = null )
	{
		// Not supported for Excel service
		return Task.CompletedTask;
	}

	#endregion

	#region Excel Methods

	public async Task ExportToExcelAsync<T>( SfDataGrid dataGrid, string filePath, Dictionary<string, string>? columnHeaderOverrides = null, Func<T, GridColumn, string>? customValueFormatter = null )
	{
		if ( dataGrid == null )
			return;

		await dataGrid.Dispatcher.InvokeAsync( () =>
		{
			var exportData = PrepareDataGridExportData<T>(dataGrid, columnHeaderOverrides);
			WriteExcelFile( exportData, filePath, customValueFormatter );
		} );
	}

	public async Task ExportToExcelAsync<T>( SfTreeGrid treeGrid, string filePath, Dictionary<string, string>? columnHeaderOverrides = null, Func<T, TreeGridColumn, string>? customValueFormatter = null )
	{
		if ( treeGrid == null )
			return;

		await treeGrid.Dispatcher.InvokeAsync( () =>
		{
			var exportData = PrepareTreeGridExportData<T>(treeGrid, columnHeaderOverrides);
			WriteExcelFile( exportData, filePath, customValueFormatter );
		} );
	}

	#endregion

	#region Internal Helpers

	private ExportData<T> PrepareDataGridExportData<T>( SfDataGrid grid, Dictionary<string, string>? columnHeaderOverrides )
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

	private ExportData<T> PrepareTreeGridExportData<T>( SfTreeGrid treeGrid, Dictionary<string, string>? columnHeaderOverrides )
	{
		var exportData = new ExportData<T>();

		foreach ( var column in treeGrid.Columns )
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

		foreach ( var node in treeGrid.View.Nodes )
		{
			if ( node.Item is T item )
				exportData.Items.Add( item );
		}

		return exportData;
	}

	private void WriteExcelFile<T, TColumn>( ExportData<T> exportData, string filePath, Func<T, TColumn, string>? customValueFormatter )
	{
		foreach ( var item in exportData.Items )
		{
			foreach ( var colInfo in exportData.ColumnInfos )
			{
				if ( colInfo.Column == null )
				{
					continue; // skip or handle default when null
				}

				var value = customValueFormatter != null
				? customValueFormatter(item, (TColumn)colInfo.Column)
				: ""; // fallback
			}
		}
	}

	private string GetColumnHeader( GridColumn column, Dictionary<string, string>? columnHeaderOverrides )
	{
		var name = column.MappingName ?? string.Empty;

		if ( columnHeaderOverrides?.ContainsKey( name ) == true )
			return columnHeaderOverrides [ name ];

		if ( _languageProvider != null )
		{
			var translated = _languageProvider.GetTranslation($"ExportHeader_{name}");
			if ( !string.IsNullOrEmpty( translated ) )
				return translated;
		}

		return !string.IsNullOrWhiteSpace( column.HeaderText ) ? column.HeaderText : name;
	}

	private string GetColumnHeader( TreeGridColumn column, Dictionary<string, string>? columnHeaderOverrides )
	{
		var name = column.MappingName ?? string.Empty;

		if ( columnHeaderOverrides?.ContainsKey( name ) == true )
			return columnHeaderOverrides [ name ];

		if ( _languageProvider != null )
		{
			var translated = _languageProvider.GetTranslation($"ExportHeader_{name}");
			if ( !string.IsNullOrEmpty( translated ) )
				return translated;
		}

		return !string.IsNullOrWhiteSpace( column.HeaderText ) ? column.HeaderText : name;
	}

	private void ShowSuccessMessage( string filePath, int recordCount )
	{
		try
		{
			var message = _languageProvider?.GetTranslation("ExportGeneralSuccess")
						  ?.Replace("{count}", recordCount.ToString())
						  ?.Replace("{file}", Path.GetFileName(filePath))
						  ?? $"Exported {recordCount} records to {Path.GetFileName(filePath)}";

			Application.Current?.Dispatcher.BeginInvoke( new Action( () =>
			{
				MessageBox.Show( message, "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information );
			} ) );
		}
		catch { }
	}

	#endregion
}
