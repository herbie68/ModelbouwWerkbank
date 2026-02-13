using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modelbouwer.Model;

public class ProductUsageModel
{
	public int ProductUsageId { get; set; }
	public int ProductUsageProjectId { get; set; }
	public string? ProductUsageProjectName { get; set; }
	public int ProductUsageProductId { get; set; }
	public string? ProductUsageProductName { get; set; }
	public string? ProductUsageUsageDate { get; set; }
	public int ProductUsageCategoryId { get; set; }
	public string? ProductUsageCategoryName { get; set; }
	public double ProductUsageAmount { get; set; }
	public double ProductUsageProductPrice { get; set; }
	public double ProductUsageCosts { get; set; }
	public string? ProductUsageComment { get; set; }

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

	public string StatusMarker
	{
		get
		{
			return State == RecordState.Unchanged ? string.Empty : "*";
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Raises the <see cref="PropertyChanged"/> event for a specified property.
	/// </summary>
	/// <param name="propertyName">The name of the property that changed. If omitted, the caller member name is used.</param>
	protected void NotifyPropertyChanged( [CallerMemberName] string? propertyName = null )
	{
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
	}

	/// <summary>
	/// Sets a backing field to a new value, raises PropertyChanged for the property, and updates the record state and status marker when appropriate.
	/// </summary>
	/// <typeparam name="T">Type of the backing field.</typeparam>
	/// <param name="field">Reference to the backing field to update.</param>
	/// <param name="value">New value to assign to the field.</param>
	/// <param name="propertyName">Name of the property that changed; automatically supplied by the compiler when omitted.</param>
	/// <returns>`true` if the field was changed, `false` if the new value equals the current value.</returns>
	protected bool SetProperty<T>( ref T field, T value, [CallerMemberName] string? propertyName = null )
	{
		if ( EqualityComparer<T>.Default.Equals( field, value ) )
		{
			return false;
		}

		field = value;

		// Mark record as modified when property has been changed
		if ( State == RecordState.Unchanged && propertyName != nameof( State ) )
		{
			State = RecordState.Modified;

			if ( propertyName != nameof( State ) )
			{
				NotifyPropertyChanged( nameof( StatusMarker ) );
			}
		}

		NotifyPropertyChanged( propertyName );
		return true;
	}


	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ProductUsageViewFieldNameProductName, "ProductUsageProductName" },
		{ DBNames.ProductUsageViewFieldTypeCategoryName, "ProductUsageCategoryName" },
		{ DBNames.ProductUsageViewFieldNameAmountUsed, "ProductUsageAmount" },
		{ DBNames.ProductUsageViewFieldNamePrice, "ProductUsageProductPrice" },
		{ DBNames.ProductUsageViewFieldNameTotalCosts, "ProductUsageCosts" },
		{ DBNames.ProductUsageViewFieldNameComment, "ProductUsageComment" }
	};
}