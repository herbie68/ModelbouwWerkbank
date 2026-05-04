namespace Modelbouwer.Models;

public partial class StockOrderLineModel : ObservableObject
{
	[ObservableProperty] private int _id;
	[ObservableProperty] private int _supplyOrderId;
	[ObservableProperty] private int _supplierId;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private string? _productCode;
	[ObservableProperty] private string? _productName;
	[ObservableProperty] private string? _supplierProductNumber;
	[ObservableProperty] private string? _supplierProductName;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _openAmount;
	[ObservableProperty] private double _price;
	[ObservableProperty] private double _realRowTotal;
	[ObservableProperty] private double _received;
	[ObservableProperty] private double _expected;
	[ObservableProperty] private bool _closed;
	[ObservableProperty] private DateTime? _closedDate;

	public string DisplayProductNumber => string.IsNullOrWhiteSpace( SupplierProductNumber ) ? ProductCode ?? string.Empty : SupplierProductNumber;
	public string DisplayProductDescription => string.IsNullOrWhiteSpace( SupplierProductName ) ? ProductName ?? string.Empty : SupplierProductName;

	partial void OnProductCodeChanged( string? value )
	{
		OnPropertyChanged( nameof( DisplayProductNumber ) );
	}

	partial void OnProductNameChanged( string? value )
	{
		OnPropertyChanged( nameof( DisplayProductDescription ) );
	}

	partial void OnSupplierProductNumberChanged( string? value )
	{
		OnPropertyChanged( nameof( DisplayProductNumber ) );
	}

	partial void OnSupplierProductNameChanged( string? value )
	{
		OnPropertyChanged( nameof( DisplayProductDescription ) );
	}
}
