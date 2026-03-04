namespace Modelbouwer.Converters;
public class MappingNameToAlignmentConverter : IValueConverter
{
	public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
	{
		if ( value == null )
		{
			return System.Windows.HorizontalAlignment.Left;
		}

		string mappingName = value.ToString();

		if ( mappingName.Contains( "Value" ) || mappingName.Contains( "Price" ) || mappingName.Contains( "Inventory" ) || mappingName.Contains( "Order" ) )
		{
			return System.Windows.HorizontalAlignment.Right;
		}

		return System.Windows.HorizontalAlignment.Left;
	}
	public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
	{
		throw new NotImplementedException();
	}
}
