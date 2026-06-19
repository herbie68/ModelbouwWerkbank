namespace Modelbouwer.Models;

public partial class StockOrderProductDialogModel : ObservableObject
{
	[ObservableProperty] private int _productSupplierId;
	[ObservableProperty] private int _supplierId;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private int _currencyId;
	[ObservableProperty] private string? _productCode;
	[ObservableProperty] private string? _productName;
	[ObservableProperty] private string? _supplierProductNumber;
	[ObservableProperty] private string? _supplierProductName;
	[ObservableProperty] private string? _productUrl;
	[ObservableProperty] private double _unitPrice;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private bool _productSupplierExists;

	public double RowTotal => Math.Round( UnitPrice * Amount, 2, MidpointRounding.AwayFromZero );

	partial void OnUnitPriceChanged( double value )
	{
		OnPropertyChanged( nameof( RowTotal ) );
	}

	partial void OnAmountChanged( double value )
	{
		OnPropertyChanged( nameof( RowTotal ) );
	}

	public static StockOrderProductDialogModel Create(
		ProductModel product,
		SupplierModel supplier,
		Modelbouwer.Model.ProductSupplierModel? productSupplier )
	{
		return new StockOrderProductDialogModel
		{
			ProductSupplierId = productSupplier?.ProductSupplierId ?? 0,
			ProductSupplierExists = productSupplier != null,
			ProductId = product.ProductId,
			SupplierId = supplier.Id,
			CurrencyId = productSupplier?.CurrencyId > 0 ? productSupplier.CurrencyId : supplier.CurrencyId,
			ProductCode = product.ProductCode,
			ProductName = product.ProductName,
			SupplierProductNumber = string.IsNullOrWhiteSpace( productSupplier?.ProductNumber ) ? product.ProductCode : productSupplier.ProductNumber,
			SupplierProductName = string.IsNullOrWhiteSpace( productSupplier?.ProductName ) ? product.ProductName : productSupplier.ProductName,
			ProductUrl = productSupplier?.URL ?? string.Empty,
			UnitPrice = productSupplier is { Price: > 0 } ? productSupplier.Price : product.ProductPrice,
			Amount = product.ProductStandardQuantity > 0 ? product.ProductStandardQuantity : 1
		};
	}
}