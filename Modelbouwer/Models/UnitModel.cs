using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Models;

public class UnitModel
{
	public string? UnitName { get; set; }
	public int UnitId { get; set; }

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
