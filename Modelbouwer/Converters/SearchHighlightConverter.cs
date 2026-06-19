using System;
using System.Globalization;
using System.Windows.Data;

namespace Modelbouwer.Converters
{
	public class SearchHighlightConverter : IMultiValueConverter
	{
		// value[0] = cell text
		// value[1] = search text
		public object Convert( object [ ] values, Type targetType, object parameter, CultureInfo culture )
		{
			if ( values.Length < 2 )
				return false;

			var cellText = values[0]?.ToString();
			var searchText = values[1]?.ToString();

			if ( string.IsNullOrWhiteSpace( cellText ) || string.IsNullOrWhiteSpace( searchText ) )
				return false;

			return cellText.Contains( searchText, StringComparison.OrdinalIgnoreCase );
		}

		public object [ ] ConvertBack( object value, Type [ ] targetTypes, object parameter, CultureInfo culture ) => throw new NotImplementedException();
	}
}