namespace Modelbouwer.Models;

public partial class BrandModel : ObservableObject
{
	[ObservableProperty] private int _brandId;

	[ObservableProperty] private string? _brandName = string.Empty;

	/// <summary>
	/// Gives the mapping between CSV column headers and model property names, for 3 languages Dutch, English, German.
	/// </summary>
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(BrandId)] = [ "ID" ],

		[nameof(BrandName)] = [
			"Merk",
			"Brand",
			"Marke" ]
	};
}