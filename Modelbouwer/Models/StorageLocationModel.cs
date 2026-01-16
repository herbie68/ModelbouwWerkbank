using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Models;

public class StorageLocationModel
{
	public int StorageId { get; set; }
	public int? StorageParentId { get; set; }
	public string? StorageName { get; set; }
	public ObservableCollection<StorageLocationModel> Children { get; set; } = [ ];

	private StorageLocationModel? _parent;

	public StorageLocationModel Parent
	{
		get => _parent;
		set
		{
			_parent = value;
		}
	}

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.StorageFieldNameId, "StorageId" },
		{ DBNames.StorageFieldNameParentId, "StorageParentId" },
		{ DBNames.StorageFieldNameName, "StorageName" },
	};

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(StorageId)] = [ "ID" ],
		[nameof(StorageParentId)] = [ "Parent" ],

		[nameof(StorageName)] = [
			"Voorraad locatie",
			"Stock location",
			"Lagerort" ]
	};

}
