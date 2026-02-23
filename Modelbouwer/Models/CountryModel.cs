namespace Modelbouwer.Models;

public partial class CountryModel : ObservableObject
{
	[ObservableProperty] public int _countryCurrencyId;

	[ObservableProperty] public int _countryId;

	[ObservableProperty] public string? _countryCode;

	[ObservableProperty] public string? _countryCurrencySymbol;

	[ObservableProperty] public string? _countryName;

	private CurrencyModel? _defaultCurrency;

	public CurrencyModel? DefaultCurrency
	{
		get => _defaultCurrency;
		set
		{
			if ( SetProperty( ref _defaultCurrency, value ) )
			{
				CountryCurrencyId = value?.CurrencyId ?? 0;
				CountryCurrencySymbol = value?.CurrencySymbol;
			}
		}
	}

	/// <summary>
	/// Gives the mapping between CSV column headers and model property names, for 3 languages Dutch, English, German.
	/// </summary>
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(CountryCurrencyId)] =
		[
			"Valuta ID",
			"Currency ID",
			"Währungs ID" ],

		[nameof(CountryId)] = [ "ID" ],

		[nameof(CountryCode)] = [
			"Landcode",
			"Country Code",
			"Ländercode" ],

		[nameof(CountryName)] = [
			"Land",
			"Country",
			"Land (DE)" ],

		[nameof(CountryCurrencySymbol)] = [
			"Valuta",
			"Currency",
			"Währung" ]
	};
}