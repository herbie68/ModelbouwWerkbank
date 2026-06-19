namespace Modelbouwer.Converters;

public class IntToBoolConverter : IValueConverter
{
	public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) => ( int ) value == 1;

	public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) => ( bool ) value ? 1 : 0;
}