using System.ComponentModel;

namespace Modelbouwer.Models;

public class StorageLocationModel
{
	public int StorageId { get; set; }
	public int? ParentId { get; set; }
	public ObservableCollection<StorageLocationModel> Children { get; set; } = [ ];

	private string _storageName;

	public string StorageName
	{
		get => _storageName;
		set
		{
			if ( _storageName != value )
			{
				_storageName = value;
				OnPropertyChanged( nameof( StorageName ) );
			}
		}
	}

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
		{ DBNames.StorageFieldNameParentId, "ParentId" },
		{ DBNames.StorageFieldNameName, "StorageName" },
	};

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(StorageId)] = [ "ID" ],
		[nameof(ParentId)] = [ "Parent" ],

		[nameof(StorageName)] = [
			"Voorraad locatie",
			"Stock location",
			"Lagerort" ]
	};

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged( string propertyName )
	{
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
	}

}
