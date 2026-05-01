namespace Modelbouwer.Models;

public partial class StockOrderModel : ObservableObject
{
	[ObservableProperty] private int _id;
	[ObservableProperty] private int _supplierId;
	[ObservableProperty] private int _currencyId;
	[ObservableProperty] private string? _supplierName;
	[ObservableProperty] private string? _currencySymbol;
	[ObservableProperty] private string? _orderNumber;
	[ObservableProperty] private DateTime? _orderDate;
	[ObservableProperty] private double _shippingCosts;
	[ObservableProperty] private double _orderCosts;
	[ObservableProperty] private string? _memo;
	[ObservableProperty] private bool _closed;
	[ObservableProperty] private DateTime? _closedDate;
	[ObservableProperty] private bool _hasStockLog;

	private double _linesTotal;
	public double LinesTotal
	{
		get => _linesTotal;
		set
		{
			if ( SetProperty( ref _linesTotal, value ) )
			{
				OnPropertyChanged( nameof( GrandTotal ) );
			}
		}
	}

	public double GrandTotal => Math.Round( LinesTotal + ShippingCosts + OrderCosts, 2, MidpointRounding.AwayFromZero );

	partial void OnShippingCostsChanged( double value )
	{
		OnPropertyChanged( nameof( GrandTotal ) );
	}

	partial void OnOrderCostsChanged( double value )
	{
		OnPropertyChanged( nameof( GrandTotal ) );
	}
}
