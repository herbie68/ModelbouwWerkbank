namespace Modelbouwer.Models;

public class CountryModel : ObservableObject
{
	public int CountryCurrencyId { get; set; }
	public int CountryId { get; set; }
	public string? CountryCode { get; set; }
	public string? CountryCurrencySymbol { get; set; }
	public string? CountryName { get; set; }

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

		[nameof(CountryName)] =	[
			"Land",
			"Country",
			"Land (DE)"	],

		[nameof(CountryCurrencySymbol)] = [
			"Valuta",
			"Currency",
			"Währung" ]
	};
}