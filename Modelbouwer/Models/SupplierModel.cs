namespace Modelbouwer.Models;

public class SupplierModel : ObservableObject
{
	#region General supplier information
	public int CountryId { get; set; }
	public int CurrencyId { get; set; }
	public int Id { get; set; }
	public double CurrencyRate { get; set; }
	public double MinOrderCosts { get; set; }
	public double MinShippingCosts { get; set; }
	public double OrderCosts { get; set; }
	public double ShippingCosts { get; set; }
	public string? Address1 { get; set; }
	public string? Address2 { get; set; }
	public string? City { get; set; }
	public string? Code { get; set; }
	public string? Country { get; set; }
	public string? Currency { get; set; }
	public string? Mail { get; set; }
	public string? Memo { get; set; }
	public string? Name { get; set; }
	public string? Phone { get; set; }
	public string? Url { get; set; }
	public string? Zip { get; set; }
	#endregion

	#region Supplier Contacts
	//[ObservableProperty]
	//private int _supplierContactId;

	//[ObservableProperty]
	//private int _supplierContactSuppplierId;

	//[ObservableProperty]
	//private string? _supplierContactName;

	//[ObservableProperty]
	//private int _supplierContactContactTypeId;

	//[ObservableProperty]
	//private string? _supplierContactContactType;

	//[ObservableProperty]
	//private string? _supplierContactMail;

	//[ObservableProperty]
	//private string? _supplierContactPhone;
	#endregion

	#region Supplier Conacts functions
	//[ObservableProperty]
	//private string? _supplierContactTypeName;

	//[ObservableProperty]
	//private int _supplierContactTypeId;
	#endregion

	#region Order History
	//[ObservableProperty]
	//private int _supplierOrderHistoryOrderId;

	//[ObservableProperty]
	//private int _supplierOrderHistorySupplierId;

	//[ObservableProperty]
	//private string? _supplierOrderHistoryOrderNumber;

	//[ObservableProperty]
	//private string? _supplierOrderHistoryOrderDate;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryOrderCosts;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryShippingCosts;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryCurrencyConversionRate;

	//[ObservableProperty]
	//private string? _supplierOrderHistoryReceived;

	//[ObservableProperty]
	//private int _supplierOrderHistoryProductId;

	//[ObservableProperty]
	//private string? _supplierOrderHistoryProductNumber;

	//[ObservableProperty]
	//private string? _supplierOrderHistoryProductDescription;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryPrice;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryAmount;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryRowTotal;

	//[ObservableProperty]
	//private decimal _supplierOrderHistoryOrderTotal;
	#endregion

	#region ColumnMappings
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(Id)] = [ "ID" ],

		[nameof(Code)] =
		[
			"Zoeknaam",
			"Search name",
			"Suchname" ],

		[nameof(Name)] = [
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
