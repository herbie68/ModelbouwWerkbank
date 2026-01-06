using System;
using System.Collections.Generic;
using System.Text;

using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Services;

public class ExportData<T>
{
	public List<string> Headers { get; set; } = [];
	public List<ColumnInfo> ColumnInfos { get; set; } = [];
	public List<T> Items { get; set; } = [];
}

public class ColumnInfo
{
	public string MappingName { get; set; }
	public string HeaderText { get; set; }
	public GridColumn Column { get; set; }
	public string ColumnType { get; set; }
	public double Width { get; set; }
	public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
}
