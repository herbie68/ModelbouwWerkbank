namespace Modelbouwer.Model;

public partial class ProductSupplierModel : ObservableObject
{
	[ObservableProperty] public int _productSupplierId;

	[ObservableProperty] public int _productId;

	[ObservableProperty] public int _supplierId;

	[ObservableProperty] public int _currencyId;

	[ObservableProperty] public string? _supplierName;

	[ObservableProperty] public string? _productNumber;

	[ObservableProperty] public string? _productName;

	[ObservableProperty] public double _price;

	[ObservableProperty] public string? _uRL;

	[ObservableProperty] public bool? _defaultSupplier;

	[ObservableProperty] public bool? _defaultSupplierCheck;

	[ObservableProperty] public string? _currencySymbol;

	// Define the property that you want to use in TLists (for example in the errorList
	public string? Name => ProductName;

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ProductSupplierFieldNameId, "ProductSupplierId" },
		{ DBNames.ProductSupplierFieldNameProductId, "ProductId" },
		{ DBNames.ProductSupplierFieldNameSupplierId, "SupplierId" },
		{ DBNames.ProductSupplierFieldNameCurrencyId, "CurrencyId" },
		{ DBNames.ProductSupplierFieldNameProductNumber, "ProductNumber" },
		{ DBNames.ProductSupplierFieldNameProductName, "ProductName" },
		{ DBNames.ProductSupplierFieldNameSupplierName, "SupplierName" },
		{ DBNames.ProductSupplierFieldNamePrice, "Price" },
		{ DBNames.ProductSupplierFieldNameProductUrl, "URL" },
		{ DBNames.ProductSupplierFieldNameDefaultSupplier, "ProductSupplierDefaultSupplier" }
	};
}