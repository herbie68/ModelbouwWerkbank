namespace Modelbouwer.Converters;

[ValueConversion( typeof( bool ), typeof( Visibility ) )]
public sealed class BooleanToVisibilityConverter : IValueConverter
{
	public bool IsInverted { get; set; }

	private static bool TryGetBool( object? value, out bool result )
	{
		result = false;

		// Handles boxed bool (covers bool and boxed nullable<bool> with value)
		if ( value is bool b )
		{
			result = b;
			return true;
		}

		if ( value is int i )
		{
			result = i != 0;
			return true;
		}

		if ( value is string s )
		{
			if ( bool.TryParse( s, out var parsed ) )
			{
				result = parsed;
				return true;
			}

			if ( int.TryParse( s, out var parsedInt ) )
			{
				result = parsedInt != 0;
				return true;
			}
		}

		result = false;
		return false;
	}

	private static bool IsParameterInverted( object? parameter )
	{
		if ( parameter is null )
			return false;

		if ( parameter is bool pb )
			return pb;

		var text = parameter.ToString();
		if ( string.IsNullOrWhiteSpace( text ) )
			return false;

		return text.Equals( "invert", StringComparison.OrdinalIgnoreCase )
			|| text.Equals( "inverse", StringComparison.OrdinalIgnoreCase )
			|| text.Equals( "true", StringComparison.OrdinalIgnoreCase )
			|| text.Equals( "1", StringComparison.OrdinalIgnoreCase );
	}

	public object Convert( object value, Type targetType, object? parameter, CultureInfo culture )
	{
		// Normalize input to bool (robust against strings and ints)
		TryGetBool( value, out var boolValue );

		// Allow inversion either through parameter or property (parameter wins)
		var inverted = IsParameterInverted( parameter ) || IsInverted;

		return inverted
			? ( object ) ( boolValue ? Visibility.Collapsed : Visibility.Visible )
			: ( object ) ( boolValue ? Visibility.Visible : Visibility.Collapsed );
	}

	public object ConvertBack( object value, Type targetType, object? parameter, CultureInfo culture )
	{
		if ( value is Visibility visibility )
		{
			var isVisible = visibility == Visibility.Visible;
			var inverted = IsParameterInverted( parameter ) || IsInverted;
			return inverted ? !isVisible : isVisible;
		}

		// If value is already a bool, return it (defensive)
		if ( value is bool bb )
			return bb;

		return DependencyProperty.UnsetValue;
	}
}