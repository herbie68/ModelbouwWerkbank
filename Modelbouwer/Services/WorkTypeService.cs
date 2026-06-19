namespace Modelbouwer.Services;

public class WorktypeService : IWorktypeService
{
	private readonly GenericDataService _dataService;
	public bool WorkTypeUsed { get; set; } = false;

	public WorktypeService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteWorkTypeList = $"" +
		$"SELECT " +
		$"{DBNames.WorktypeFieldNameId} AS {DBNames.WorktypeFieldNameId}, " +
		$"{DBNames.WorktypeFieldNameParentId} AS {DBNames.WorktypeFieldNameParentId}, " +
		$"{DBNames.WorktypeFieldNameName} AS {DBNames.WorktypeFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.WorktypeTable};";

	public string AddNewWorkTypeQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.WorktypeTable} " +
		$"({DBNames.WorktypeFieldNameParentId}, {DBNames.WorktypeFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.WorktypeFieldNameParentId}, @{DBNames.WorktypeFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateWorkTypeQuery =
		$"UPDATE {DBNames.Database}.{DBNames.WorktypeTable} " +
		$"SET " +
		$"{DBNames.WorktypeFieldNameParentId} = @{DBNames.WorktypeFieldNameParentId}, " +
		$"{DBNames.WorktypeFieldNameName} = @{DBNames.WorktypeFieldNameName}" +
		$"WHERE {DBNames.WorktypeFieldNameId} = @{DBNames.WorktypeFieldNameId};";

	public string DeleteWorkTypeQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.WorktypeTable} " +
		$"WHERE {DBNames.WorktypeFieldNameId} = @{DBNames.WorktypeFieldNameId};";

	public string WorkTypeNameExistsQuery =
		$"SELECT COUNT(*) " +
		$"FROM {DBNames.Database}.{DBNames.WorktypeTable} " +
		$"WHERE {DBNames.WorktypeFieldNameName} = @{DBNames.WorktypeFieldNameName} " +
		$"AND ( " +
		$"( {DBNames.WorktypeFieldNameParentId} = @{DBNames.WorktypeFieldNameParentId} ) " +
		$"OR ( {DBNames.WorktypeFieldNameParentId} IS NULL AND @{DBNames.WorktypeFieldNameParentId} IS NULL ) );";

	public string WorkTypeUsedQuery =
		$"SELECT COUNT(*){DBNames.TimeFieldNameWorktypeId}) FROM {DBNames.Database}.{DBNames.TimeTable} WHERE {DBNames.TimeFieldNameWorktypeId} = @WorktypeId";
	#endregion

	public Task<List<WorktypeModel>> GetAllWorkTypesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteWorkTypeList, reader =>
		{
			return new WorktypeModel
			{
				WorktypeId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.WorktypeFieldNameId}" ] ),
				ParentId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.WorktypeFieldNameParentId}" ] ),
				WorktypeName = DatabaseValueConverter.GetString( reader [ $"{DBNames.WorktypeFieldNameName}" ] )
			};
		} );
	}

	public async Task<int> InsertNewWorkTypeAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.WorktypeFieldNameParentId}", queryParameters[$"@{DBNames.WorktypeFieldNameParentId}"] ?? DBNull.Value },
			{ $"@{DBNames.WorktypeFieldNameName}", queryParameters[$"@{DBNames.WorktypeFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewWorkTypeQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateWorkTypeAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.WorktypeFieldNameId}", queryParameters[$"@{DBNames.WorktypeFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.WorktypeFieldNameParentId}", queryParameters[$"@{DBNames.WorktypeFieldNameParentId}"] ?? DBNull.Value },
			{ $"@{DBNames.WorktypeFieldNameName}", queryParameters[$"@{DBNames.WorktypeFieldNameName}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateWorkTypeQuery, parameters );
	}

	public async Task DeleteWorkTypeAsync( int worktypeId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.WorktypeFieldNameId}", worktypeId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteWorkTypeQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataWorkTypeDeleteError}." );
		}
	}

	public async Task<bool> IsWorkTypeUsedAsync( int worktypeId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.TimeFieldNameWorktypeId}", worktypeId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			WorkTypeUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? worktypeName, int? parentId )
	{
		if ( string.IsNullOrWhiteSpace( worktypeName ) )
			return false;

		if ( parentId == 0 )
			parentId = null;

		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.WorktypeFieldNameParentId}", ( object? ) parentId ?? DBNull.Value },
			{ $"@{DBNames.WorktypeFieldNameName}", worktypeName }
		};


		var count = await _dataService.ExecuteScalarAsync<int>( WorkTypeNameExistsQuery, parameters );
		return count > 0;
	}
}