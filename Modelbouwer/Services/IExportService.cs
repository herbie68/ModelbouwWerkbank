using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Windows.Controls.Grid;

namespace Modelbouwer.Services
{
	using sfGridColumn = Syncfusion.UI.Xaml.Grid.GridColumn;
	public interface IExportService
	{
		Task ExportToCsvAsync<T>(
		SfDataGrid dataGrid,
		string filePath,
		Dictionary<string, string>? columnHeaderOverrides = null,
		Func<T, GridColumn, string>? customValueFormatter = null );

		Task ExportToCsvAsync<T>(
			SfTreeGrid treeGrid,
			string filePath,
			Dictionary<string, string>? columnHeaderOverrides = null,
			Func<T, TreeGridColumn, string>? customValueFormatter = null );
	}
}