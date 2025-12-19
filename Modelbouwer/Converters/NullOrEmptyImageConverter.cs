namespace Modelbouwer.Converters;
public class NullOrEmptyImageConverter : IValueConverter
{
	public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
	{
		if ( value is ImageSource imageSource )
		{
			// Haal de "noimage" op uit de resources
			DrawingImage? noImage = System.Windows.Application.Current.TryFindResource( "noimage" ) as DrawingImage;

			// Vergelijk de huidige afbeelding met de "noimage"
			if ( imageSource == noImage )
			{
				return true; // Dit betekent dat het de "noimage" afbeelding is
			}
		}

		return false; // Er is een echte afbeelding
	}

	public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
	{
		throw new NotImplementedException();
	}
}