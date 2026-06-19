namespace Modelbouwer.Models;

public partial class SupplierModel : ObservableObject
{
	#region General supplier information
	[ObservableProperty] public int _countryId;
	[ObservableProperty] public int _currencyId;
	[ObservableProperty] public int _id;
	[ObservableProperty] public double _currencyRate;
	[ObservableProperty] public double _minOrderCosts;
	[ObservableProperty] public double _minShippingCosts;
	[ObservableProperty] public double _orderCosts;
	[ObservableProperty] public double _shippingCosts;
	[ObservableProperty] public string? _address1;
	[ObservableProperty] public string? _address2;
	[ObservableProperty] public string? _city;
	[ObservableProperty] public string? _code;
	[ObservableProperty] public string? _country;
	[ObservableProperty] public string? _currency;
	[ObservableProperty] public string? _mail;
	[ObservableProperty] public string? _memo;
	[ObservableProperty] public string? _name;
	[ObservableProperty] public string? _phone;
	[ObservableProperty] public string? _url;
	[ObservableProperty] public string? _zip;
	#endregion

	// Injected lookup
	public IReadOnlyList<CountryModel>? CountryList { get; set; }

	public string? CountryName =>
		CountryList?.FirstOrDefault( c => c.CountryId == CountryId )?.CountryName;

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
			"Lieferantenname" ],

	};
	#endregion

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.SupplierFieldNameId, "Id" },
		{ DBNames.SupplierFieldNameCurrencyId, "CurrencyId" },
		{ DBNames.SupplierFieldNameCountryId, "CountryId" },
		{ DBNames.SupplierFieldNameCode, "Code" },
		{ DBNames.SupplierFieldNameName, "Name" },
		{ DBNames.SupplierFieldNameAddress1, "Address1" },
		{ DBNames.SupplierFieldNameAddress2, "Address2" },
		{ DBNames.SupplierFieldNameZip, "Zip" },
		{ DBNames.SupplierFieldNameCity, "City" },
		{ DBNames.SupplierFieldNameUrl, "Url" },
		{ DBNames.SupplierFieldNameMemo, "Memo" },
		{ DBNames.SupplierFieldNameShippingCosts, "ShippingCosts" },
		{ DBNames.SupplierFieldNameMinShippingCosts, "MinShippingCosts" },
		{ DBNames.SupplierFieldNameOrderCosts, "OrderCosts" },
	};
}