#nullable enable

namespace Modelbouwer.Models;

public class CurrencyModel
{
	public double CurrencyConversionRate { get; set; } = 1.0;
	public int CurrencyId { get; set; }
	public string? CurrencyCode { get; set; } = string.Empty;
	public string? CurrencyName { get; set; } = string.Empty;
	public string? CurrencySymbol { get; set; } = string.Empty;
	public override string? ToString() => CurrencySymbol;

	/// <summary>
	/// Gives the mapping between CSV column headers and model property names, for 3 languages Dutch, English, German.
	/// </summary>
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(CurrencyId)] = [ "ID" ],

		[nameof(CurrencyCode)] = [
			"Valuta code",
			"Currency Code",
			"Währungscode" ],

		[nameof(CurrencyName)] = [
			"Valuta naam",
			"Currency name",
			"Währungsname" ],

		[nameof(CurrencySymbol)] = [
			"Valutateken",
			"Currency symbol",
			"Währungszeichen" ],

		[nameof(CurrencyConversionRate)] = [
			"Wisselkoers",
			"ExchangeRate",
			"Wechselkurs" ]
	};
}