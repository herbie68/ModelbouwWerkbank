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

	public double LinesTotal { get; set; }

	public double GrandTotal => Math.Round( LinesTotal + ShippingCosts + OrderCosts, 2, MidpointRounding.AwayFromZero );
}
