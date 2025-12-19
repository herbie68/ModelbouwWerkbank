namespace Modelbouwer.Models;

public class CountryModel
{
	public int CountryCurrencyId { get; set; }
	public int CountryId { get; set; }
	public string? CountryCode { get; set; }
	public string? CountryCurrencySymbol { get; set; }
	public string? CountryName { get; set; }

	public CurrencyModel? DefaultCurrency { get; set; }

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