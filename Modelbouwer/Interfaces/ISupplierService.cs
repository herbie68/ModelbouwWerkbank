using Modelbouwer.Model;

namespace Modelbouwer.Interfaces;

public interface ISupplierService
{
	Task<List<SupplierModel>> GetAllSuppliersAsync();
	Task<List<ProductSupplierModel>> GetAllProductSuppliersAsync();
	Task<ProductSupplierModel?> GetProductSupplierAsync( int supplierId, int productId );
	Task<int> UpsertProductSupplierAsync( ProductSupplierModel productSupplier );
	Task<List<CountryModel>> GetAllCountriesAsync();
	Task<List<CurrencyModel>> GetAllCurrenciesAsync();
	Task<int> InsertNewSupplierAsync( Dictionary<string, object?> queryParameters );
	Task UpdateSupplierAsync( Dictionary<string, object?> queryParameters );
	Task DeleteSupplierAsync( int supplierId );

	Task<bool> IsSupplierUsedAsync( int supplierId );
	Task<bool> NameExistsAsync( string? supplierName );
}
