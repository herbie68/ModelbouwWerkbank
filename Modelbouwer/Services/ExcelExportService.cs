using System.Collections;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.XlsIO;

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

	public Task ExportToCsvAsync<T>( SfTreeGrid treeGrid, string filePath, Dictionary<string, string>? columnHeaderOverrides = null, Func<T, TreeGridColumn, string>? customValueFormatter = null ) =>
		// Not supported for Excel service
		Task.CompletedTask;

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


	private object? GetPropertyValue<T>( T item, string propertyName ) => item?.GetType().GetProperty( propertyName )?.GetValue( item );

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

		// ✅ Recursively get ALL nodes, including collapsed children
		if ( treeGrid.ItemsSource is IEnumerable rootItems )
		{
			var childPropertyName = treeGrid.ChildPropertyName ?? "Children";

			foreach ( var rootItem in rootItems )
			{
				if ( rootItem is T item )
				{
					AddItemAndChildren( item, exportData, childPropertyName );
				}
			}
		}

		return exportData;
	}

	private void AddItemAndChildren<T>( T item, ExportData<T> exportData, string childPropertyName )
	{
		// Add current item
		exportData.Items.Add( item );

		// Get the children collection using reflection
		var childProperty = typeof(T).GetProperty(childPropertyName);
		if ( childProperty != null )
		{
			var childrenValue = childProperty.GetValue(item);

			if ( childrenValue is IEnumerable children )
			{
				foreach ( var child in children )
				{
					if ( child is T childItem )
					{
						AddItemAndChildren( childItem, exportData, childPropertyName );
					}
				}
			}
		}
	}

	private void WriteExcelFile<T, TColumn>( ExportData<T> exportData, string filePath, Func<T, TColumn, string>? customValueFormatter )
	{
		using ( ExcelEngine excelEngine = new ExcelEngine() )
		{
			IApplication application = excelEngine.Excel;
			application.DefaultVersion = ExcelVersion.Xlsx;

			IWorkbook workbook = application.Workbooks.Create(1);
			IWorksheet worksheet = workbook.Worksheets[0];

			// Write headers
			for ( int col = 0; col < exportData.Headers.Count; col++ )
			{
				worksheet.Range [ 1, col + 1 ].Text = exportData.Headers [ col ];
			}

			// Write data
			int row = 2;
			foreach ( var item in exportData.Items )
			{
				int col = 1;
				foreach ( var colInfo in exportData.ColumnInfos )
				{
					if ( colInfo.Column != null )
					{
						var value = customValueFormatter != null
						? customValueFormatter(item, (TColumn)colInfo.Column)
						: GetPropertyValue(item, colInfo.MappingName);

						worksheet.Range [ row, col ].Text = value?.ToString() ?? "";
					}
					col++;
				}
				row++;
			}

			// Save the file
			workbook.SaveAs( filePath );

			ShowSuccessMessageAsync( filePath, exportData.Items.Count ).GetAwaiter().GetResult();
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

	private async Task ShowSuccessMessageAsync( string filePath, int recordCount )
	{
		try
		{
			var message = _languageProvider?.GetTranslation("ExportGeneralSuccess")
						  ?.Replace("{count}", recordCount.ToString())
						  ?.Replace("{file}", Path.GetFileName(filePath))
						  ?? $"Exported {recordCount} records to {Path.GetFileName(filePath)}";

			if ( Application.Current != null )
			{
				await Application.Current.Dispatcher.InvokeAsync( () =>
				{
					MessageBox.Show( message, "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information );
				} );
			}
		}
		catch { }
	}

	#endregion
}
