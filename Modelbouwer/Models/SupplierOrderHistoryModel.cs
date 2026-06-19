namespace Modelbouwer.Models;

public partial class SupplierOrderHistoryModel : ObservableObject
{
	[ObservableProperty] private int _orderId;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private int _supplierId;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _currencyConversionRate;
	[ObservableProperty] private double _orderCosts;
	[ObservableProperty] private double _orderTotal;
	[ObservableProperty] private double _price;
	[ObservableProperty] private double _rowTotal;
	[ObservableProperty] private double _shippingCosts;
	[ObservableProperty] private string? _orderNumber;
	[ObservableProperty] private string? _productDescription;
	[ObservableProperty] private string? _productNumber;
	[ObservableProperty] private string? _received;
	[ObservableProperty] private DateOnly? _orderDate;
}