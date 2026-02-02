namespace Modelbouwer.Services;

public class ProjectService : IProjectService
{
	private readonly GenericDataService _dataService;
	public bool ProjectUsed { get; set; } = false;

	public ProjectService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteProjectList = $"" +
		$"SELECT " +
		$"{ DBNames.ProjectFieldNameId} AS {DBNames.ProjectFieldNameId}, " +
		$"{ DBNames.ProjectFieldNameCode}, " +
		$"{ DBNames.ProjectFieldNameName}, " +
		$"{ DBNames.ProjectFieldNameStartDate}, " +
		$"{ DBNames.ProjectFieldNameEndDate}, " +
		$"{ DBNames.ProjectFieldNameExpectedTime}, " +
		$"{ DBNames.ProjectFieldNameClosed}, " +
		$"{ DBNames.ProjectFieldNameImage}, " +
		$"{ DBNames.ProjectFieldNameImageRotationAngle}, " +
		$"{ DBNames.ProjectFieldNameMemo}" +
		$" FROM {DBNames.Database}.{DBNames.ProjectTable};";


	public string AddNewProjectQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.ProjectTable} " +
		$"({DBNames.ProjectFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.ProjectFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateProjectQuery =
		$"UPDATE {DBNames.Database}.{DBNames.ProjectTable} " +
		$"SET " +
		$"{DBNames.ProjectFieldNameName} = @{DBNames.ProjectFieldNameName}" +
		$"WHERE {DBNames.ProjectFieldNameId} = @{DBNames.ProjectFieldNameId};";

	public string DeleteProjectQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.ProjectTable} " +
		$"WHERE {DBNames.ProjectFieldNameId} = @{DBNames.ProjectFieldNameId};";

	public string ProjectNameExistsQuery =
		$"SELECT COUNT({DBNames.ProjectFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.ProjectTable} " +
		$"WHERE {DBNames.ProjectFieldNameName} = @{DBNames.ProjectFieldNameName}";

	public string ProjectUsedQuery = $"SELECT COUNT({DBNames.TimeFieldNameProjectId}) FROM {DBNames.Database}.{DBNames.ProductTable} WHERE {DBNames.TimeFieldNameProjectId} = @ProjectId";

	public string GetProjectExpectedEndDateQuery = $"CALL {DBNames.Database}.{DBNames.SPProjectExpectedEndDate}(@ProjectId);";
	#endregion

	public Task<List<ProjectModel>> GetAllProjectsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteProjectList, reader =>
		{
			return new ProjectModel
			{
				ProjectId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProjectFieldNameId}" ] ),
				ProjectCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProjectFieldNameCode}" ] ),
				ProjectName = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProjectFieldNameName}" ] ),
				ProjectStartDate = DatabaseValueConverter.GetDateOnly( reader [ $"{DBNames.ProjectFieldNameStartDate}" ] ),
				ProjectEndDate = DatabaseValueConverter.GetDateOnly( reader [ $"{DBNames.ProjectFieldNameEndDate}" ] ),
				ProjectExpectedTime = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProjectFieldNameExpectedTime}" ] ),
				ProjectClosed = DatabaseValueConverter.GetSByte( reader [ $"{DBNames.ProjectFieldNameClosed}" ] ) == 1,
				ProjectImage = reader [ $"{DBNames.ProjectFieldNameImage}" ] as byte [ ],
				ProjectImageRotationAngle = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.ProjectFieldNameImageRotationAngle}" ] ),
				ProjectMemo = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProjectFieldNameMemo}" ] )
			};
		} );
	}

	public async Task<int> InsertNewProjectAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ProjectFieldNameCode}", queryParameters[$"@{DBNames.ProjectFieldNameCode}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameName}", queryParameters[$"@{DBNames.ProjectFieldNameName}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameStartDate}", queryParameters[$"@{DBNames.ProjectFieldNameStartDate}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameEndDate}", queryParameters[$"@{DBNames.ProjectFieldNameEndDate}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameExpectedTime}", queryParameters[$"@{DBNames.ProjectFieldNameExpectedTime}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameClosed}", queryParameters[$"@{DBNames.ProjectFieldNameClosed}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameImage}", queryParameters[$"@{DBNames.ProjectFieldNameImage}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameImageRotationAngle}", queryParameters[$"@{DBNames.ProjectFieldNameImageRotationAngle}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameMemo}", queryParameters[$"@{DBNames.ProjectFieldNameMemo}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewProjectQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateProjectAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ProjectFieldNameId}", queryParameters[$"@{DBNames.ProjectFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameCode}", queryParameters[$"@{DBNames.ProjectFieldNameCode}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameName}", queryParameters[$"@{DBNames.ProjectFieldNameName}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameStartDate}", queryParameters[$"@{DBNames.ProjectFieldNameStartDate}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameEndDate}", queryParameters[$"@{DBNames.ProjectFieldNameEndDate}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameExpectedTime}", queryParameters[$"@{DBNames.ProjectFieldNameExpectedTime}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameClosed}", queryParameters[$"@{DBNames.ProjectFieldNameClosed}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameImage}", queryParameters[$"@{DBNames.ProjectFieldNameImage}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameImageRotationAngle}", queryParameters[$"@{DBNames.ProjectFieldNameImageRotationAngle}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameMemo}", queryParameters[$"@{DBNames.ProjectFieldNameMemo}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateProjectQuery, parameters );
	}

	public async Task DeleteProjectAsync( int projectId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ProjectFieldNameId}", projectId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteProjectQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataProjectDeleteError}." );
		}
	}

	public async Task<bool> IsProjectUsedAsync( int projectId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.TimeFieldNameProjectId}", projectId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			ProjectUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? projectName )
	{
		if ( string.IsNullOrWhiteSpace( projectName ) )
			return false;

		var projects = await GetAllProjectsAsync();

		return projects.Any( c =>
			string.Equals( c.ProjectName, projectName, StringComparison.OrdinalIgnoreCase ) );
	}

	public async Task<DateOnly?> GetExpectedEndDateAsync( int projectId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ "@ProjectId", projectId }
		};

		var result = await _dataService.ExecuteScalarAsync<DateTime?>(
			GetProjectExpectedEndDateQuery,
			parameters
		);

		return result.HasValue
			? DateOnly.FromDateTime( result.Value )
			: null;
	}

}
