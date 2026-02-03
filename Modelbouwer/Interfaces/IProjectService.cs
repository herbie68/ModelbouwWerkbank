namespace Modelbouwer.Interfaces;

public interface IProjectService
{
	Task<List<ProjectModel>> GetAllProjectsAsync();
	Task<int> InsertNewProjectAsync( Dictionary<string, object?> queryParameters );
	Task UpdateProjectAsync( Dictionary<string, object?> queryParameters );
	Task DeleteProjectAsync( int projectId );
	Task<bool> IsProjectUsedAsync( int projectId );
	Task<bool> NameExistsAsync( string? projectName );
	//Task<DateOnly?> GetExpectedEndDateAsync( int projectId );
	Task<DateOnly?> GetLastWorkDateOnProjectAsync( int projectId );
	Task<ProjectWorkStats?> GetProjectWorkStatsAsync( int projectId );
}
