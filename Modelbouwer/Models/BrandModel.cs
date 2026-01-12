using Modelbouwer.Models;

namespace Modelbouwer.Models;

public class BrandModel
{
	public string? BrandName { get; set; }
	public int BrandId { get; set; }

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