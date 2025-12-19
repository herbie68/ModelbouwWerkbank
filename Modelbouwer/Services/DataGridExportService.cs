using Syncfusion.UI.Xaml.Grid;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Modelbouwer.Services;

public static class DataGridExportService
{
	public static void ExportToCsv( SfDataGrid dataGrid, string filePath )
	{
		if ( dataGrid == null || dataGrid.View == null )
			return;

		var sb = new StringBuilder();

		// --- HEADER ROW ---
		var exportColumns = dataGrid.Columns
			.Where(c => !string.IsNullOrEmpty(c.MappingName))
			.ToList();

		// Write header line
		sb.AppendLine( string.Join( ";", exportColumns.Select( c => c.HeaderText ) ) );

		// --- DATA ROWS ---
		foreach ( var record in dataGrid.View.Records )
		{
			var data = record.Data;
			var values = exportColumns.Select(col =>
			{
				string mappingName = col.MappingName;

                // Get property by reflection
                var prop = data.GetType().GetProperty(mappingName, BindingFlags.Public | BindingFlags.Instance);

				if (prop == null)
					return string.Empty;

				var val = prop.GetValue(data);
				return EscapeCsv(val?.ToString() ?? "");
			});

			sb.AppendLine( string.Join( ";", values ) );
		}

		File.WriteAllText( filePath, sb.ToString(), Encoding.UTF8 );
	}

	// Escapes CSV fields that contain separators or quotes
	private static string EscapeCsv( string input )
	{
		if ( input.Contains( ';' ) || input.Contains( '"' ) )
			return $"\"{input.Replace( "\"", "\"\"" )}\"";

		return input;
	}
}