namespace Modelbouwer.Models;

public class CornwallDowelImportItem
{
	public string Name { get; init; } = string.Empty;
	public string ProductNumber { get; init; } = string.Empty;
	public double Price { get; init; }
	public string RelativeProductUrl { get; init; } = string.Empty;
	public string AbsoluteProductUrl { get; init; } = string.Empty;
	public string AbsoluteImageUrl { get; init; } = string.Empty;
	public string MaterialCode { get; init; } = string.Empty;
	public string GeneratedProductCode { get; set; } = string.Empty;
}
