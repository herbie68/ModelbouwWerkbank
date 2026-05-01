namespace Modelbouwer.Services;

public class StorageLocationService : IStorageLocationService
{
	private readonly GenericDataService _dataService;
	public bool StorageLocationUsed { get; set; } = false;

	public StorageLocationService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteStorageLocationList = $"" +
		$"SELECT " +
		$"{DBNames.StorageFieldNameId} AS {DBNames.StorageFieldNameId}, " +
		$"{DBNames.StorageFieldNameParentId} AS {DBNames.StorageFieldNameParentId}, " +
		$"{DBNames.StorageFieldNameName} AS {DBNames.StorageFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.StorageTable};";

	public string AddNewStorageLocationQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.StorageTable} " +
		$"({DBNames.StorageFieldNameParentId}, {DBNames.StorageFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.StorageFieldNameParentId}, @{DBNames.StorageFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateStorageLocationQuery =
		$"UPDATE {DBNames.Database}.{DBNames.StorageTable} " +
		$"SET " +
		$"{DBNames.StorageFieldNameParentId} = @{DBNames.StorageFieldNameParentId}, " +
		$"{DBNames.StorageFieldNameName} = @{DBNames.StorageFieldNameName}" +
		$"WHERE {DBNames.StorageFieldNameId} = @{DBNames.StorageFieldNameId};";

	public string DeleteStorageLocationQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.StorageTable} " +
		$"WHERE {DBNames.StorageFieldNameId} = @{DBNames.StorageFieldNameId};";

	public string StorageNameExistsQuery =
		$"SELECT COUNT(*) " +
		$"FROM {DBNames.Database}.{DBNames.StorageTable} " +
		$"WHERE {DBNames.StorageFieldNameName} = @{DBNames.StorageFieldNameName} " +
		$"AND ( " +
		$"( {DBNames.StorageFieldNameParentId} = @{DBNames.StorageFieldNameParentId} ) " +
		$"OR ( {DBNames.StorageFieldNameParentId} IS NULL AND @{DBNames.StorageFieldNameParentId} IS NULL ) );";

	public string StorageLocationUsedQuery =
		$"SELECT COUNT(*){DBNames.ProductFieldNameStorageId}) FROM {DBNames.Database}.{DBNames.ProductTable} WHERE {DBNames.ProductFieldNameStorageId} = @StorageLocationId";
	#endregion

	public Task<List<StorageLocationModel>> GetAllStorageLocationsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteStorageLocationList, reader =>
		{
			return new StorageLocationModel
			{
				StorageId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.StorageFieldNameId}" ] ),
				ParentId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.StorageFieldNameParentId}" ] ),
				StorageName = DatabaseValueConverter.GetString( reader [ $"{DBNames.StorageFieldNameName}" ] )
			};
		} );
	}

	public async Task<int> InsertNewStorageLocationAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.StorageFieldNameParentId}", queryParameters[$"@{DBNames.StorageFieldNameParentId}"] ?? DBNull.Value },
			{ $"@{DBNames.StorageFieldNameName}", queryParameters[$"@{DBNames.StorageFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewStorageLocationQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateStorageLocationAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.StorageFieldNameId}", queryParameters[$"@{DBNames.StorageFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.StorageFieldNameParentId}", queryParameters[$"@{DBNames.StorageFieldNameParentId}"] ?? DBNull.Value },
			{ $"@{DBNames.StorageFieldNameName}", queryParameters[$"@{DBNames.StorageFieldNameName}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateStorageLocationQuery, parameters );
	}

	public async Task DeleteStorageLocationAsync( int storagelocationId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.StorageFieldNameId}", storagelocationId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteStorageLocationQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataStorageLocationDeleteError}." );
		}
	}

	public async Task<bool> IsStorageLocationUsedAsync( int storagelocationId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ProductFieldNameStorageId}", storagelocationId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			StorageLocationUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? StorageName, int? parentId )
	{
		if ( string.IsNullOrWhiteSpace( StorageName ) )
			return false;

		if ( parentId == 0 )
			parentId = null;

		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.StorageFieldNameParentId}", ( object? ) parentId ?? DBNull.Value },
			{ $"@{DBNames.StorageFieldNameName}", StorageName }
		};


		var count = await _dataService.ExecuteScalarAsync<int>( StorageNameExistsQuery, parameters );
		return count > 0;
	}
}
