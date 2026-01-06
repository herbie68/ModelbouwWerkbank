using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Services
{
	using sfGridColumn = Syncfusion.UI.Xaml.Grid.GridColumn;
	public interface IExportService
	{
		Task ExportToCsvAsync<T>( SfDataGrid dataGrid, string defaultFileName,
			Dictionary<string, string> columnHeaderOverrides = null,
			Func<T, GridColumn, string> customValueFormatter = null );

		Task ExportToExcelAsync<T>( SfDataGrid dataGrid, string defaultFileName,
			Dictionary<string, string> columnHeaderOverrides = null,
			Func<T, GridColumn, string> customValueFormatter = null );
	}
}