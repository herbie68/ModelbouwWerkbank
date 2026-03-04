namespace Modelbouwer.Models;

public partial class StockManagementModel : ObservableObject
{
	[ObservableProperty] private double _productInOrder;
	[ObservableProperty] private double _productInventory;
	[ObservableProperty] private double _productInventoryValue;
	[ObservableProperty] private double _productOriginalInventory;
	[ObservableProperty] private double _productMinimalStock;
	[ObservableProperty] private double _productPrice;
	[ObservableProperty] private double _productShortInventory;
	[ObservableProperty] private double _productTempShortInventory;
	[ObservableProperty] private double _productVirtualInventory;
	[ObservableProperty] private double _productVirtualInventoryValue;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private string? _productCategory;
	[ObservableProperty] private string? _productCode;
	[ObservableProperty] private string? _productName;
	[ObservableProperty] private string? _productStorageLocation;

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ProductInventoryViewFieldNameProductId, "Product_Id" },
		{ DBNames.ProductInventoryViewFieldNameProductCode, "Code" },
		{ DBNames.ProductInventoryViewFieldNameProductName, "Name" },
		{ DBNames.ProductInventoryViewFieldNameProductPrice, "Price" },
		{ DBNames.ProductInventoryViewFieldNameProductMinimalStock, "MinimalStock" },
		{ DBNames.ProductInventoryViewFieldNameProductCategory, "Category" },
		{ DBNames.ProductInventoryViewFieldNameProductStorage, "Location" },
		{ DBNames.ProductInventoryViewFieldNameProductMinimalInventory, "InventoryOrder" },
		{ DBNames.ProductInventoryViewFieldNameProductMinimalValue, "Value" },
		{ DBNames.ProductInventoryViewFieldNameProductMinimalOrder, "InOrder" },
		{ DBNames.ProductInventoryViewFieldNameProductMinimalVirtualInventory, "VirtualInventory" },
		{ DBNames.ProductInventoryViewFieldNameProductMinimalVirtualValue, "VirtualValue" },
		{ DBNames.ProductInventoryViewFieldNameProductShort, "Short" },
		{ DBNames.ProductInventoryViewFieldNameProductTempShort, "TempShort" }
	};
}
