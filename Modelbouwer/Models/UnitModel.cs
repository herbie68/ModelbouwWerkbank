namespace Modelbouwer.Models;

public partial class UnitModel : ObservableObject
{
	[ObservableProperty] public string? _unitName;
	[ObservableProperty] public int _unitId;

	/// <summary>
	/// Gives the mapping between CSV column headers and model property names, for 3 languages Dutch, English, German.
	/// </summary>
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(UnitId)] = [ "ID" ],

		[nameof(UnitName)] = [
			"Eenheid",
			"Unit",
			"Einheit" ]
	};
}
