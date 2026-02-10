namespace Modelbouwer.Interfaces;

public interface IContactService
{
	Task<List<SupplierContactModel>> GetAllContactsAsync();
	Task<int> InsertNewContactAsync( Dictionary<string, object?> queryParameters );
	Task UpdateContactAsync( Dictionary<string, object?> queryParameters );
	Task DeleteContactAsync( int supplierId );
	Task<bool> NameExistsAsync( string? contactName );


}
