using System.ComponentModel;
using System.Runtime.CompilerServices;

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
		set => SetProperty( ref _storageName, value );
	}

	private StorageLocationModel? _parent;

	public StorageLocationModel Parent
	{
		get => _parent;
		set => _parent = value;
	}

	public enum RecordState
	{
		Unchanged,
		Added,
		Modified,
		Deleted
	}

	private RecordState _state = RecordState.Unchanged;
	public RecordState State
	{
		get => _state;
		set => SetProperty( ref _state, value );
	}

	public string StatusMarker => State == RecordState.Unchanged ? string.Empty : "*" ;

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

	protected bool SetProperty<T>( ref T field, T value, [ CallerMemberName] string? propertyName = null )
	{
		if ( EqualityComparer<T>.Default.Equals( field, value ) )
		{
			return false;
		}

		field = value;

		// Mark record as modified when property has been changed
		if ( State == RecordState.Unchanged && propertyName != nameof( State ) )
		{
			_state = RecordState.Modified; // avoid recursion to SetProperty
			OnPropertyChanged( nameof( StatusMarker ) );
		}

		OnPropertyChanged( propertyName ?? string.Empty );
		return true;
	}

}
