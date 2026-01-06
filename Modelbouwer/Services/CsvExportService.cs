using Syncfusion.UI.Xaml.Grid;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using sfGridColumn = Syncfusion.UI.Xaml.Grid.GridColumn;

namespace Modelbouwer.Services
{
	public class CsvExportService : IExportService
	{
		// Configuratie properties
		public string Separator { get; set; } = ";";
		public Encoding Encoding { get; set; } = Encoding.UTF8;
		public bool IncludeBom { get; set; } = true;
		public bool IncludeHeaders { get; set; } = true;

		// Language provider
		private readonly ILanguageProvider _languageProvider;

		public CsvExportService( ILanguageProvider languageProvider = null )
		{
			_languageProvider = languageProvider;
		}

		public async Task ExportToCsvAsync<T>( SfDataGrid dataGrid, string defaultFileName,
			Dictionary<string, string> columnHeaderOverrides = null,
			Func<T,sfGridColumn, string> customValueFormatter = null )
		{
			var dialog = new SaveFileDialog
			{
				Filter = GetFilterString(),
				FileName = defaultFileName,
				DefaultExt = ".csv"
			};

			if ( dialog.ShowDialog() != true )
				return;

			try
			{
				Debug.WriteLine( "Starting CSV export process..." );

				// Alle UI operaties in één Dispatcher.Invoke
				ExportData<T> exportData = null;
				await dataGrid.Dispatcher.InvokeAsync( () =>
				{
					Debug.WriteLine( "Preparing export data in UI thread..." );
					exportData = PrepareExportData<T>( dataGrid, columnHeaderOverrides );
				} );

				Debug.WriteLine( $"Export data prepared: {exportData?.Items?.Count ?? 0} items" );

				// CSV genereren en opslaan in background thread
				await Task.Run( () =>
				{
					Debug.WriteLine( "Generating CSV in background thread..." );
					var csvContent = GenerateCsvContent<T>(exportData, customValueFormatter);
					File.WriteAllText( dialog.FileName, csvContent, Encoding );
					Debug.WriteLine( $"CSV saved to: {dialog.FileName}" );
				} );

				Debug.WriteLine( "Showing success message..." );
				ShowSuccessMessage( dialog.FileName, exportData.Items.Count );
				Debug.WriteLine( "CSV export completed successfully." );
			}
			catch ( Exception ex )
			{
				Debug.WriteLine( $"CSV export failed with error: {ex}" );
				ShowErrorMessage( ex, "CSV" );
			}
		}

		public async Task ExportToExcelAsync<T>( SfDataGrid dataGrid, string defaultFileName,
			Dictionary<string, string> columnHeaderOverrides = null,
			Func<T,sfGridColumn, string> customValueFormatter = null )
		{
			// Doe niets voor Excel in CSV service
			await Task.CompletedTask;

			// Optioneel: toon een bericht
			// MessageBox.Show("Please use ExcelExportService for Excel exports.", 
			//     "Export Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private ExportData<T> PrepareExportData<T>( SfDataGrid dataGrid,
			Dictionary<string, string> columnHeaderOverrides )
		{
			var exportData = new ExportData<T>();

			// 1. Haal kolom informatie op
			var columnInfos = new List<ColumnInfo>();
			foreach ( var column in dataGrid.Columns )
			{
				if ( ShouldExportColumn( column ) )
				{
					var columnInfo = new ColumnInfo
					{
						MappingName = column.MappingName,
						HeaderText = column.HeaderText,
						Column = column,
						ColumnType = column.GetType().Name
					};
					columnInfos.Add( columnInfo );

					// 2. Haal header op voor deze kolom
					string header = GetColumnHeader(column, columnHeaderOverrides);
					exportData.Headers.Add( header );
				}
			}
			exportData.ColumnInfos = columnInfos;

			// 3. Haal items op
			if ( dataGrid.ItemsSource != null )
			{
				var items = new List<T>();

				if ( dataGrid.ItemsSource is IEnumerable enumerable )
				{
					foreach ( var item in enumerable )
					{
						if ( item is T typedItem )
						{
							items.Add( typedItem );
						}
						else
						{
							try
							{
								if ( item != null )
								{
									var converted = (T)Convert.ChangeType(item, typeof(T));
									items.Add( converted );
								}
							}
							catch
							{
								Debug.WriteLine( $"Warning: Could not convert item of type {item?.GetType().Name} to {typeof( T ).Name}" );
							}
						}
					}
				}
				exportData.Items = items;
			}

			Debug.WriteLine( $"Prepared {exportData.Items.Count} items, {exportData.ColumnInfos.Count} columns" );

			return exportData;
		}

		private string GenerateCsvContent<T>( ExportData<T> exportData,
			Func<T,sfGridColumn, string> customValueFormatter )
		{
			if ( exportData == null || exportData.Items == null || exportData.ColumnInfos == null )
			{
				return string.Empty;
			}

			var csvBuilder = new StringBuilder();

			if ( IncludeBom )
				csvBuilder.Append( '\uFEFF' );

			// Headers
			if ( IncludeHeaders && exportData.Headers != null )
			{
				csvBuilder.AppendLine( FormatCsvLine( exportData.Headers ) );
			}

			// Data rows
			foreach ( var item in exportData.Items )
			{
				var rowValues = new List<string>();

				foreach ( var columnInfo in exportData.ColumnInfos )
				{
					string value;
					if ( customValueFormatter != null )
					{
						try
						{
							value = customValueFormatter( item, columnInfo.Column );
						}
						catch ( InvalidOperationException )
						{
							value = GetCellValueSafely( item, columnInfo.MappingName );
						}
					}
					else
					{
						value = GetCellValueSafely( item, columnInfo.MappingName );
					}

					rowValues.Add( FormatValueForCsv( value ) );
				}

				csvBuilder.AppendLine( FormatCsvLine( rowValues ) );
			}

			return csvBuilder.ToString();
		}

		private string GetCellValueSafely<T>( T item, string mappingName )
		{
			try
			{
				if ( !string.IsNullOrWhiteSpace( mappingName ) )
				{
					var property = typeof(T).GetProperty(mappingName);
					if ( property != null )
					{
						var value = property.GetValue(item);
						return FormatValueToString( value );
					}
				}

				return string.Empty;
			}
			catch ( Exception ex )
			{
				Debug.WriteLine( $"Error getting cell value for {mappingName}: {ex.Message}" );
				return string.Empty;
			}
		}

		private string GetColumnHeader(sfGridColumn column, Dictionary<string, string> columnHeaderOverrides )
		{
			var mappingName = column.MappingName;

			if ( columnHeaderOverrides?.ContainsKey( mappingName ) == true )
				return columnHeaderOverrides [ mappingName ];

			if ( _languageProvider != null )
			{
				var translatedHeader = _languageProvider.GetTranslation($"ExportHeader_{mappingName}");
				if ( !string.IsNullOrEmpty( translatedHeader ) )
					return translatedHeader;
			}

			if ( !string.IsNullOrWhiteSpace( column.HeaderText ) )
				return column.HeaderText;

			return mappingName ?? string.Empty;
		}

		private string FormatValueToString( object value )
		{
			if ( value == null )
				return string.Empty;

			if ( value is DependencyObject )
			{
				Debug.WriteLine( "Warning: Attempting to export UI element" );
				return string.Empty;
			}

			if ( value is DateTime dateTime )
				return dateTime.ToString( "yyyy-MM-dd HH:mm:ss" );

			if ( value is bool boolValue )
				return boolValue ? "1" : "0";

			if ( value is decimal decimalValue )
				return decimalValue.ToString( System.Globalization.CultureInfo.InvariantCulture );

			if ( value is double || value is float || value is int || value is long )
				return value.ToString();

			return value.ToString() ?? string.Empty;
		}

		private bool ShouldExportColumn(sfGridColumn column )
		{
			return !column.IsHidden && column.MappingName != null;
		}

		private string FormatValueForCsv( string value )
		{
			if ( string.IsNullOrEmpty( value ) )
				return string.Empty;

			if ( NeedsEscaping( value ) )
			{
				return $"\"{value.Replace( "\"", "\"\"" )}\"";
			}

			return value;
		}

		private bool NeedsEscaping( string value )
		{
			return value.Contains( Separator ) ||
				   value.Contains( "\"" ) ||
				   value.Contains( "\n" ) ||
				   value.Contains( "\r" ) ||
				   value.StartsWith( " " ) ||
				   value.EndsWith( " " );
		}

		private string FormatCsvLine( List<string> values )
		{
			return string.Join( Separator, values );
		}

		private string GetFilterString()
		{
			return _languageProvider?.GetTranslation( "ExportGeneralCSVFilter" ) ?? "CSV Files (*.csv)|*.csv";
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
						MessageBox.Show( message, "Export Complete",
							MessageBoxButton.OK, MessageBoxImage.Information );
					} ) );
				}
				else
				{
					MessageBox.Show( message, "Export Complete",
						MessageBoxButton.OK, MessageBoxImage.Information );
				}
			}
			catch ( Exception ex )
			{
				Debug.WriteLine( $"Error showing success message: {ex}" );
			}
		}

		private void ShowErrorMessage( Exception ex, string exportType )
		{
			try
			{
				var message = $"{exportType} export failed: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}";

				if ( Application.Current != null )
				{
					Application.Current.Dispatcher.BeginInvoke( new Action( () =>
					{
						MessageBox.Show( message, "Export Error",
							MessageBoxButton.OK, MessageBoxImage.Error );
					} ) );
				}
				else
				{
					MessageBox.Show( message, "Export Error",
						MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
			catch ( Exception showEx )
			{
				Debug.WriteLine( $"Error showing error message: {showEx}" );
			}
		}
	}
}