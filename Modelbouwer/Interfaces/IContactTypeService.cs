using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Interfaces;

	public interface IContactTypeService
	{
	Task<List<ContactTypeModel>> GetAllContactTypesAsync();
	Task<int> InsertNewContactTypeAsync( Dictionary<string, object?> queryParameters );
	Task UpdateContactTypeAsync( Dictionary<string, object?> queryParameters );
	Task DeleteContactTypeAsync( int contacttypeId );
	Task<bool> IsContactTypeUsedAsync( int contacttypeId );
	Task<bool> NameExistsAsync( string? contacttypeName );
}
