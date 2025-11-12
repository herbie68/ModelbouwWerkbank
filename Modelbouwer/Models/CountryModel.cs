namespace Modelbouwer.Models;

public class CountryModel
{
	public int CountryCurrencyId { get; set; }
	public int CountryId { get; set; }
	public string? CountryCode { get; set; }
	public string? CountryCurrencySymbol { get; set; }
	public string? CountryName { get; set; }

	public CurrencyModel? DefaultCurrency { get; set; }
}