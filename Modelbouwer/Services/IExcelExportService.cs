using Syncfusion.UI.Xaml.Grid;

public interface IExcelExportService
{
	Task ExportToExcelAsync<T>( SfDataGrid dataGrid, string defaultFileName,
		Dictionary<string, string> columnHeaderOverrides = null,
		Func<T, GridColumn, string> customValueFormatter = null );

	bool AutoFilter { get; set; }
	bool FreezeHeaderRow { get; set; }
	string WorksheetName { get; set; }
}