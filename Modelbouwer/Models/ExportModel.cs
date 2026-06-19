using System.Collections.Generic;

namespace Modelbouwer.Services;

public class ColumnInfo
{
	public string MappingName { get; set; } = string.Empty;
	public string HeaderText { get; set; } = string.Empty;
	public dynamic? Column { get; set; } // SfDataGrid of TreeGridColumn
}

public class ExportData<T>
{
	public List<ColumnInfo> ColumnInfos { get; set; } = new();
	public List<string> Headers { get; set; } = new();
	public List<T> Items { get; set; } = new();
}