namespace Modelbouwer.Models;

public partial class SupplierModel : ObservableObject
{
	#region General supplier information
	[ObservableProperty]
	private int _supplierId;

	[ObservableProperty]
	private string? _supplierCode;

	[ObservableProperty]
	private string? _supplierName;

	[ObservableProperty]
	private string? _supplierAddress1;

	[ObservableProperty]
	private string? _supplierAddress2;

	[ObservableProperty]
	private string? _supplierZip;

	[ObservableProperty]
	private string? _supplierCity;

	[ObservableProperty]
	private string? _supplierUrl;

	[ObservableProperty]
	private double _supplierShippingCosts;

	[ObservableProperty]
	private double _supplierMinShippingCosts;

	[ObservableProperty]
	private double _supplierOrderCosts;

	[ObservableProperty]
	private double _supplierMinOrderCosts;

	[ObservableProperty]
	private string? _supplierMemo;

	[ObservableProperty]
	private string? _supplierMail;

	[ObservableProperty]
	private string? _supplierPhone;

	[ObservableProperty]
	private int _supplierCurrencyId;

	[ObservableProperty]
	private string? _supplierCurrency;

	[ObservableProperty]
	private double _supplierCurrencyRate;

	[ObservableProperty]
	private int _supplierCountryId;

	[ObservableProperty]
	private string? _supplierCountry;
	#endregion

	#region Supplier Contacts
	[ObservableProperty]
	private int _supplierContactId;

	[ObservableProperty]
	private int _supplierContactSuppplierId;

	[ObservableProperty]
	private string? _supplierContactName;

	[ObservableProperty]
	private int _supplierContactContactTypeId;

	[ObservableProperty]
	private string? _supplierContactContactType;

	[ObservableProperty]
	private string? _supplierContactMail;

	[ObservableProperty]
	private string? _supplierContactPhone;
	#endregion

	#region Supplier Conacts functions
	[ObservableProperty]
	private string? _supplierContactTypeName;

	[ObservableProperty]
	private int _supplierContactTypeId;
	#endregion

	#region Order History
	[ObservableProperty]
	private int _supplierOrderHistoryOrderId;

	[ObservableProperty]
	private int _supplierOrderHistorySupplierId;

	[ObservableProperty]
	private string? _supplierOrderHistoryOrderNumber;

	[ObservableProperty]
	private string? _supplierOrderHistoryOrderDate;

	[ObservableProperty]
	private decimal _supplierOrderHistoryOrderCosts;

	[ObservableProperty]
	private decimal _supplierOrderHistoryShippingCosts;

	[ObservableProperty]
	private decimal _supplierOrderHistoryCurrencyConversionRate;

	[ObservableProperty]
	private string? _supplierOrderHistoryReceived;

	[ObservableProperty]
	private int _supplierOrderHistoryProductId;

	[ObservableProperty]
	private string? _supplierOrderHistoryProductNumber;

	[ObservableProperty]
	private string? _supplierOrderHistoryProductDescription;

	[ObservableProperty]
	private decimal _supplierOrderHistoryPrice;

	[ObservableProperty]
	private decimal _supplierOrderHistoryAmount;

	[ObservableProperty]
	private decimal _supplierOrderHistoryRowTotal;

	[ObservableProperty]
	private decimal _supplierOrderHistoryOrderTotal;
	#endregion

	#region ColumnMappings
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(SupplierId)] = [ "ID" ],

		[nameof(SupplierCode)] =
		[
			"Zoeknaam",
			"Search name",
			"Suchname" ],

		[nameof(SupplierName)] = [
			"Suppliernaam",
			"Supplier name",
			"Projektname" ],

	};
	#endregion

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.SupplierFieldNameId, "SupplierId" },
		{ DBNames.SupplierFieldNameCurrencyId, "SupplierCurrencyId" },
		{ DBNames.SupplierFieldNameCountryId, "SupplierCountryId" },
		{ DBNames.SupplierFieldNameCode, "SupplierCode" },
		{ DBNames.SupplierFieldNameName, "SupplierName" },
		{ DBNames.SupplierFieldNameAddress1, "SupplierAddress1" },
		{ DBNames.SupplierFieldNameAddress2, "SupplierAddress2" },
		{ DBNames.SupplierFieldNameZip, "SupplierZip" },
		{ DBNames.SupplierFieldNameCity, "SupplierCity" },
		{ DBNames.SupplierFieldNameUrl, "SupplierUrl" },
		{ DBNames.SupplierFieldNameMemo, "SupplierMemo" },
		{ DBNames.SupplierFieldNameShippingCosts, "SupplierShippingCosts" },
		{ DBNames.SupplierFieldNameMinShippingCosts, "SupplierMinShippingCosts" },
		{ DBNames.SupplierFieldNameOrderCosts, "SupplierOrderCosts" },
		{ DBNames.ContactTypeFieldNameId, "ContactTypeId" },
		{ DBNames.ContactTypeFieldNameName, "ContactTypeName" }
	};
}
