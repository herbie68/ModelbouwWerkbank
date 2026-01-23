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
		$"{DBNames.ProjectFieldNameId} AS {DBNames.ProjectFieldNameId}, " +
		$"{DBNames.ProjectFieldNameName} AS {DBNames.ProjectFieldNameName}" +
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
	#endregion

	public Task<List<ProjectModel>> GetAllProjectsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteProjectList, reader =>
		{
			return new ProjectModel
			{
				ProjectId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProjectFieldNameId}" ] ),
				ProjectName = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProjectFieldNameName}" ] )
			};
		} );
	}

	public async Task<int> InsertNewProjectAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ProjectFieldNameName}", queryParameters[$"@{DBNames.ProjectFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewProjectQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateProjectAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ProjectFieldNameId}", queryParameters[$"@{DBNames.ProjectFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.ProjectFieldNameName}", queryParameters[$"@{DBNames.ProjectFieldNameName}"] ?? DBNull.Value }
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
}
