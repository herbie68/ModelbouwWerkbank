using System.ComponentModel;

namespace Modelbouwer.Models;

public partial class MaterialUsageModel : ObservableObject
{
	public enum RecordState
	{
		Unchanged,
		Added,
		Modified,
		Deleted
	}

	[ObservableProperty] private int _productUsageId;
	[ObservableProperty] private int _projectId;
	[ObservableProperty] private string? _projectName;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private string? _productName;
	[ObservableProperty] private int _categoryId;
	[ObservableProperty] private string? _categoryName;
	[ObservableProperty] private DateTime _usageDate = DateTime.Today;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _price;
	[ObservableProperty] private double _costs;
	[ObservableProperty] private string? _comment;

	private RecordState _state = RecordState.Unchanged;
	public RecordState State
	{
		get => _state;
		set => SetProperty( ref _state, value );
	}

	public string StatusMarker => State == RecordState.Unchanged ? string.Empty : "*";

	public MaterialUsageModel()
	{
		PropertyChanged += MaterialUsageModel_PropertyChanged;
	}

	private void MaterialUsageModel_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName == nameof( State ) ||
			e.PropertyName == nameof( StatusMarker ) ||
			e.PropertyName == nameof( Costs ) )
			return;

		if ( e.PropertyName == nameof( Amount ) || e.PropertyName == nameof( Price ) )
			Costs = Amount * Price;

		if ( State == RecordState.Unchanged )
		{
			_state = RecordState.Modified;
			OnPropertyChanged( nameof( State ) );
			OnPropertyChanged( nameof( StatusMarker ) );
		}
	}
}