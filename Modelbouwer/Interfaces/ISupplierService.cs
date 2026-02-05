namespace Modelbouwer.Interfaces;

public interface ISupplierService
{
	Task<List<SupplierModel>> GetAllSuppliersAsync();
	Task<int> InsertNewSupplierAsync( Dictionary<string, object?> queryParameters );
	Task UpdateSupplierAsync( Dictionary<string, object?> queryParameters );
	Task DeleteSupplierAsync( int supplierId );
	Task<bool> IsSupplierUsedAsync( int supplierId );
	Task<bool> NameExistsAsync( string? supplierName );
}
