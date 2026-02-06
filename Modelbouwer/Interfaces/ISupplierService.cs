namespace Modelbouwer.Interfaces;

public interface ISupplierService
{
	Task<List<SupplierModel>> GetAllSuppliersAsync();
	Task<List<SupplierContactModel>> GetAllContactsAsync();
	Task<List<ContactTypeModel>> GetAllContactFunctionsAsync();
	Task<List<CountryModel>> GetAllCountriesAsync();
	Task<List<CurrencyModel>> GetAllCurrenciesAsync();
	Task<int> InsertNewSupplierAsync( Dictionary<string, object?> queryParameters );
	Task<int> InsertNewContactAsync( Dictionary<string, object?> queryParameters );
	Task UpdateSupplierAsync( Dictionary<string, object?> queryParameters );
	Task UpdateContactAsync( Dictionary<string, object?> queryParameters );
	Task DeleteSupplierAsync( int supplierId );
	Task DeleteContactAsync( int supplierId );

	Task<bool> IsSupplierUsedAsync( int supplierId );
	Task<bool> NameExistsAsync( string? supplierName );
	Task<bool> ContactNameExistsAsync( string? contactName );
}
