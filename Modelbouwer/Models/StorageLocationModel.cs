using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modelbouwer.Models;

public class StorageLocationModel : INotifyPropertyChanged
{
	public int StorageId { get; set; }
	public int? ParentId { get; set; }

	private string? _storageLocationName;
	public string? StorageLocationName
	{
		get => _storageLocationName;
		set
		{
			if ( _storageLocationName != value )
			{
				_storageLocationName = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected void OnPropertyChanged( [CallerMemberName] string? name = null )
		=> PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );


	public ObservableCollection<StorageLocationModel> Children { get; set; } = [ ];

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.StorageFieldNameId, "StorageId" },
		{ DBNames.StorageFieldNameParentId, "ParentId" },
		{ DBNames.StorageFieldNameName, "StorageLocationName" },
	};

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(StorageId)] = [ "ID" ],
		[nameof(ParentId)] = [ "Parent" ],

		[nameof(StorageLocationName)] = [
			"Voorraad locatie",
			"Stock location",
			"Lagerort" ]
	};

}
