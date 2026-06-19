using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modelbouwer.Model;

public partial class ProductUsageModel : ObservableObject
{
	[ObservableProperty] public int _productUsageId;

	[ObservableProperty] public int _productUsageProjectId;

	[ObservableProperty] public int _productUsageProductId;

	[ObservableProperty] public int _productUsageCategoryId;

	private string? _productUsageProjectName;
	public string? ProductUsageProjectName { get => _productUsageProjectName; set => SetProperty( ref _productUsageProjectName, value ); }


	private string? _productUsageProductName;
	public string? ProductUsageProductName { get => _productUsageProductName; set => SetProperty( ref _productUsageProductName, value ); }
	private string? _productUsageUsageDate;
	public string? ProductUsageUsageDate { get => _productUsageUsageDate; set => SetProperty( ref _productUsageUsageDate, value ); }
	private string? _productUsageCategoryName;
	public string? ProductUsageCategoryName { get => _productUsageCategoryName; set => SetProperty( ref _productUsageCategoryName, value ); }
	private double _productUsageAmount;
	public double ProductUsageAmount { get => _productUsageAmount; set => SetProperty( ref _productUsageAmount, value ); }
	private double _productUsageProductPrice;
	public double ProductUsageProductPrice { get => _productUsageProductPrice; set => SetProperty( ref _productUsageProductPrice, value ); }
	private double _productUsageCosts;
	public double ProductUsageCosts { get => _productUsageCosts; set => SetProperty( ref _productUsageCosts, value ); }
	private string? _productUsageComment;
	public string? ProductUsageComment { get => _productUsageComment; set => SetProperty( ref _productUsageComment, value ); }

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

	public new event PropertyChangedEventHandler? PropertyChanged;

	protected void NotifyPropertyChanged( [CallerMemberName] string? propertyName = null )
	{
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
	}

	protected new bool SetProperty<T>( ref T field, T value, [CallerMemberName] string? propertyName = null )
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