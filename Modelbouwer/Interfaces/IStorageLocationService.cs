using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Interfaces;

public interface IStorageLocationService
{
	Task<List<StorageLocationModel>> GetAllStorageLocationsAsync();
	Task<int> InsertNewStorageLocationAsync( Dictionary<string, object?> queryParameters );
	Task UpdateStorageLocationAsync( Dictionary<string, object?> queryParameters );
	Task DeleteStorageLocationAsync( int storagelocationId );
	Task<bool> IsStorageLocationUsedAsync( int storagelocationId );
	Task<bool> NameExistsAsync( string? StorageName, int? parentId );
}