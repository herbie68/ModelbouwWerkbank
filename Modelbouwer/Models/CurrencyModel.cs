#nullable enable

namespace Modelbouwer.Models;

public partial class CurrencyModel : ObservableObject
{
	[ObservableProperty] public double _currencyConversionRate = 1.0;

	[ObservableProperty] public int _currencyId;

	[ObservableProperty] public string? _currencyCode = string.Empty;

	[ObservableProperty] public string? _currencyName = string.Empty;

	[ObservableProperty] public string? _currencySymbol = string.Empty;

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