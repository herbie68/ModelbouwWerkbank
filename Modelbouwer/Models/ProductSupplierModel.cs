namespace Modelbouwer.Model;

public class ProductSupplierModel : ObservableObject
{
	public int ProductSupplierId { get; set; }
	public int ProductId { get; set; }
	public int SupplierId { get; set; }
	public int CurrencyId { get; set; }
	public string? SupplierName { get; set; }
	public string? ProductNumber { get; set; }
	public string? ProductName { get; set; }
	public double Price { get; set; }
	public string? URL { get; set; }
	public bool? DefaultSupplier { get; set; }
	public bool? DefaultSupplierCheck { get; set; }
	public string? CurrencySymbol { get; set; }

	// Define the property that you want to use in TLists (for example in the errorList
	public string Name => ProductName;

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