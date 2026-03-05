namespace Modelbouwer.Converters;

public class MappingNameToAlignmentConverter : IValueConverter
{
	public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
	{
		if ( value is string mappingName )
		{
			if ( mappingName.Contains( "Value" ) ||
				mappingName.Contains( "Price" ) ||
				mappingName.Contains( "Inventory" ) ||
				mappingName.Contains( "Order" ) )
			{
				return HorizontalAlignment.Right;
			}
		}

		return HorizontalAlignment.Left;
	}
	public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) => throw new NotImplementedException();
}
