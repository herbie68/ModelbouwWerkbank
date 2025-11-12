namespace Modelbouwer.Models;

public class CurrencyModel
{
	public double CurrencyConversionRate { get; set; }
	public int CurrencyId { get; set; }
	public string? CurrencyCode { get; set; }
	public string? CurrencyName { get; set; }
	public string? CurrencySymbol { get; set; }
}