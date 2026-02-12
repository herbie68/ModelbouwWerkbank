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
