using Syncfusion.UI.Xaml.Grid;
using Microsoft.Win32;
using System.Windows;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;
using System.Reflection;

using sfGridColumn = Syncfusion.UI.Xaml.Grid.GridColumn;

namespace Modelbouwer.Services
{
	public class ExcelExportService : IExportService
	{
		// Configuratie properties
		public bool AutoFilter { get; set; } = true;
		public bool FreezeHeaderRow { get; set; } = true;
		public string WorksheetName { get; set; } = "Data";

		// Language provider
		private readonly ILanguageProvider? _languageProvider;

		public ExcelExportService( ILanguageProvider? languageProvider = null )
		{
			_languageProvider = languageProvider;
		}

		public async Task ExportToCsvAsync<T>( SfDataGrid? dataGrid, string? defaultFileName,
			Dictionary<string, string>? columnHeaderOverrides = null,
			Func<T, sfGridColumn, string>? customValueFormatter = null )
		{
			// Doe niets voor CSV in Excel service
			await Task.CompletedTask;
		}

		public async Task ExportToExcelAsync<T>( SfDataGrid dataGrid, string defaultFileName,
			Dictionary<string, string>? columnHeaderOverrides = null,
			Func<T, sfGridColumn, string>? customValueFormatter = null )
		{
			var dialog = new SaveFileDialog
			{
				Filter = GetFilterString(),
				FileName = defaultFileName,
				DefaultExt = ".xlsx"
			};

			if ( dialog.ShowDialog() != true )
				return;

			try
			{
				ExportData<T>? exportData = null;
				await dataGrid.Dispatcher.InvokeAsync( () =>
				{
					exportData = PrepareExportData<T>( dataGrid, columnHeaderOverrides );
				} );

				if ( exportData == null ) return;
				
				await Task.Run( () =>
				{
					GenerateExcelFile<T>( exportData, dialog.FileName, customValueFormatter );
				} );

				ShowSuccessMessage( dialog.FileName, exportData.Items.Count );
			}
			catch ( Exception ex )
			{
				ShowErrorMessage( ex, "Excel" );
			}
		}

		private ExportData<T> PrepareExportData<T>( SfDataGrid dataGrid,
			Dictionary<string, string>? columnHeaderOverrides )
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
						ColumnType = column.GetType().Name,
						Width = column.ActualWidth
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

			Debug.WriteLine( $"Prepared {exportData.Items.Count} items, {exportData.ColumnInfos.Count} columns for Excel" );

			return exportData;
		}

		private void GenerateExcelFile<T>( ExportData<T> exportData, string? filePath,
			Func<T, sfGridColumn, string>? customValueFormatter )
		{
			using ( var workbook = new XLWorkbook() )
			{
				var worksheet = workbook.Worksheets.Add(WorksheetName);

				// Headers (rij 1)
				for ( int col = 0; col < exportData.Headers.Count; col++ )
				{
					var cell = worksheet.Cell(1, col + 1);
					cell.Value = exportData.Headers [ col ];

					// Stijl voor headers
					cell.Style.Font.Bold = true;
					cell.Style.Fill.BackgroundColor = XLColor.LightGray;
					cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
					cell.Style.Border.BottomBorderColor = XLColor.Black;

					// Kolombreedte instellen
					if ( exportData.ColumnInfos [ col ].Width > 0 )
					{
						var widthInChars = exportData.ColumnInfos[col].Width / 7.5;
						worksheet.Column( col + 1 ).Width = Math.Max( widthInChars, 10 );
					}
					else
					{
						worksheet.Column( col + 1 ).AdjustToContents();
					}
				}

				// Data rows
				for ( int row = 0; row < exportData.Items.Count; row++ )
				{
					var item = exportData.Items[row];

					for ( int col = 0; col < exportData.ColumnInfos.Count; col++ )
					{
						var columnInfo = exportData.ColumnInfos[col];
						var cell = worksheet.Cell(row + 2, col + 1);

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

						SetCellValueWithFormatting( cell, value, item, columnInfo.MappingName );

						// Alternerende rij kleuren
						if ( row % 2 == 0 )
						{
							cell.Style.Fill.BackgroundColor = XLColor.White;
						}
						else
						{
							cell.Style.Fill.BackgroundColor = XLColor.AliceBlue;
						}

						// Randen voor cellen
						cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
						cell.Style.Border.BottomBorderColor = XLColor.LightGray;
					}
				}

				// Bevries header row
				if ( FreezeHeaderRow )
				{
					worksheet.SheetView.FreezeRows( 1 );
				}

				// Maak Excel tabel
				if ( exportData.Items.Count > 0 )
				{
					var tableRange = worksheet.Range(1, 1, exportData.Items.Count + 1, exportData.ColumnInfos.Count);
					var table = tableRange.CreateTable();

					table.Theme = XLTableTheme.TableStyleMedium9;
					table.ShowTotalsRow = false;
				}

				workbook.SaveAs( filePath );
			}
		}

		private void SetCellValueWithFormatting( IXLCell cell, string stringValue, object? item, string mappingName )
		{
			if ( item == null || string.IsNullOrEmpty( mappingName ) )
			{
				cell.Value = stringValue;
				return;
			}

			try
			{
				var property = item.GetType().GetProperty(mappingName);
				if ( property != null )
				{
					var originalValue = property.GetValue(item);

					if ( originalValue == null )
					{
						cell.Value = stringValue;
					}
					else if ( originalValue is DateTime dateTime )
					{
						cell.Value = dateTime;
						cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
						cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
					}
					else if ( originalValue is bool boolValue )
					{
						cell.Value = boolValue;
						cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					}
					else if ( IsNumericType( originalValue.GetType() ) )
					{
						if ( double.TryParse( stringValue, out double numericValue ) )
						{
							cell.Value = numericValue;
							cell.Style.NumberFormat.Format = "#,##0.00";
							cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
						}
						else
						{
							cell.Value = stringValue;
						}
					}
					else
					{
						cell.Value = stringValue;
					}
				}
				else
				{
					cell.Value = stringValue;
				}
			}
			catch
			{
				cell.Value = stringValue;
			}
		}

		private bool IsNumericType( Type? type )
		{
			return type == typeof( int ) || type == typeof( double ) || type == typeof( decimal ) ||
				   type == typeof( long ) || type == typeof( float ) || type == typeof( short ) ||
				   type == typeof( byte ) || type == typeof( uint ) || type == typeof( ulong ) ||
				   type == typeof( ushort ) || type == typeof( sbyte );
		}

		private string GetCellValueSafely<T>( T? item, string? mappingName )
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

		private string GetColumnHeader( sfGridColumn column, Dictionary<string, string>? columnHeaderOverrides )
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

		private string FormatValueToString( object? value )
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

			if ( value is IFormattable formattable )
				return formattable.ToString( null, CultureInfo.InvariantCulture );

			return value.ToString() ?? string.Empty;
		}

		private bool ShouldExportColumn( sfGridColumn column )
		{
			return !column.IsHidden && column.MappingName != null;
		}

		private string GetFilterString()
		{
			return _languageProvider?.GetTranslation( "ExportGeneralExcelFilter" ) ?? "Excel Files (*.xlsx)|*.xlsx";
		}

		private void ShowSuccessMessage( string? filePath, int? recordCount )
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

		private void ShowErrorMessage( Exception ex, string? exportType )
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