namespace Modelbouwer.Converters;
public class TooltipValidationConverter : IMultiValueConverter
{
	public object Convert( object [ ] values, Type targetType, object parameter, CultureInfo culture )
	{
		string? validationError = values[0] as string; // Show validation error as tooltip
		string? existingTooltip = values[1] as string; // Show existing tooltip tooltip

		// Combineer beide, indien aanwezig
		if ( !string.IsNullOrEmpty( validationError ) )
		{
			return string.IsNullOrEmpty( existingTooltip )
				? validationError
				: $"{validationError}\n{existingTooltip}";
		}

		return existingTooltip;
	}

	public object [ ] ConvertBack( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
	{
		throw new NotImplementedException();
	}
}
