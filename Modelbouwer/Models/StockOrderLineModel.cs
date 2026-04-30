namespace Modelbouwer.Models;

public partial class StockOrderLineModel : ObservableObject
{
	[ObservableProperty] private int _id;
	[ObservableProperty] private int _supplyOrderId;
	[ObservableProperty] private int _supplierId;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private string? _productCode;
	[ObservableProperty] private string? _productName;
	[ObservableProperty] private string? _supplierProductName;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _openAmount;
	[ObservableProperty] private double _price;
	[ObservableProperty] private double _realRowTotal;
	[ObservableProperty] private double _received;
	[ObservableProperty] private double _expected;
	[ObservableProperty] private bool _closed;
	[ObservableProperty] private DateTime? _closedDate;
}
