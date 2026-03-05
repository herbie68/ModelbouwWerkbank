using Syncfusion.Data;

namespace Modelbouwer.Converters;

public class CaptionSummaryRowConverter : IValueConverter
{
	public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
	{
		if ( value is Group summaryRecordEntry )
		{
			string? columnName = parameter.ToString();
			if ( columnName != "" )
			{
				ISummaryRow summaryRow = summaryRecordEntry.SummaryDetails.SummaryRow;
				ISummaryColumn? summaryCol = summaryRow.SummaryColumns.FirstOrDefault( s => s.MappingName == columnName );
				List<SummaryValue> summaryItems = summaryRecordEntry.SummaryDetails.SummaryValues;
				if ( summaryItems != null && summaryCol != null )
				{
					SummaryValue? item = summaryItems.FirstOrDefault( s => s.Name == summaryCol.Name );
					if ( item != null )
					{
						if ( columnName == "ProductInventoryValue" )
						{
							return string.Format( "{0:c}", item.AggregateValues.Values.ToArray() );
						}

						if ( columnName == "ProductVirtualInventoryValue" )
						{
							return string.Format( "{0:c}", item.AggregateValues.Values.ToArray() );
						}
					}
				}
			}
		}

		return "Foutieve waarde";
	}

	public object? ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => null;
}
