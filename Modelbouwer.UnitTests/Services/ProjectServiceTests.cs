using Moq;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class ProjectServiceTests
{
	private Mock<GenericDataService> _mockDataService = null!;
	private ProjectService _project_service = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockDataService = new Mock<GenericDataService>();
		_project_service = new ProjectService( _mockDataService.Object );
	}

	[TestMethod]
	public async Task GetAllProjectsAsync_ReturnsProjectList()
	{
		// Arrange
		var expectedProjects = new List<ProjectModel>
		{
			new ProjectModel { ProjectId = 1, ProjectName = "Project 1" },
			new ProjectModel { ProjectId = 2, ProjectName = "Project 2" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProjectModel>>() ) )
			.ReturnsAsync( expectedProjects );

		// Act
		var result = await _project_service.GetAllProjectsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.HasCount( 2, result );
		Assert.AreEqual( "Project 1", result [ 0 ].ProjectName );
		Assert.AreEqual( "Project 2", result [ 1 ].ProjectName );
	}

	[TestMethod]
	public async Task GetAllProjectsAsync_WithEmptyDatabase_ReturnsEmptyList()
	{
		// Arrange
		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, ProjectModel>>() ) )
			.ReturnsAsync( new List<ProjectModel>() );

		// Act
		var result = await _project_service.GetAllProjectsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.IsEmpty( result );
	}

	[TestMethod]
	public async Task InsertNewProjectAsync_ReturnsNewProjectId()
	{
		// Arrange
		var parameters = CreateValidProjectParameters();

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 42u );

		// Act
		var result = await _project_service.InsertNewProjectAsync(parameters);

		// Assert
		Assert.AreEqual( 42, result );
	}

	[TestMethod]
	public async Task InsertNewProjectAsync_PassesCorrectParameters()
	{
		// Arrange
		var parameters = CreateValidProjectParameters();

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1u );

		// Act
		await _project_service.InsertNewProjectAsync( parameters );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.ProjectFieldNameStartDate}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameEndDate}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameExpectedTime}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameClosed}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameImage}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameImageRotationAngle}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameMemo}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameName}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameCode}" )
			) ), Times.Once );
	}

	[TestMethod]
	public async Task UpdateProjectAsync_CallsDataService()
	{
		// Arrange
		var parameters = CreateValidProjectParameters( 1, "Updated Project", "UP001" );

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		// Act
		await _project_service.UpdateProjectAsync( parameters );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.ProjectFieldNameId}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameCode}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameStartDate}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameEndDate}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameExpectedTime}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameClosed}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameImage}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameImageRotationAngle}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameMemo}" ) &&
				d.ContainsKey( $"@{DBNames.ProjectFieldNameName}" )
			) ), Times.Once );
	}

	[TestMethod]
	public async Task DeleteProjectAsync_CallsDataServiceWithCorrectId()
	{
		// Arrange
		var projectId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		// Act
		await _project_service.DeleteProjectAsync( projectId );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.ProjectFieldNameId}" ) &&
				( int ) d [ $"@{DBNames.ProjectFieldNameId}" ] == projectId
			) ), Times.Once );
	}

	[TestMethod]
	public async Task DeleteProjectAsync_WithConstraintViolation_ThrowsEntityInUseException()
	{
		// Arrange
		var projectId = 123;

		// Moq will throw a lightweight test exception that exposes a Number property.
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ThrowsAsync( new TestMySqlException( 1451, "Foreign key constraint" ) );

		// Act / Assert: use try/catch because Assert.ThrowsExceptionAsync isn't available in current test framework
		try
		{
			await _project_service.DeleteProjectAsync( projectId );
			Assert.Fail( "Expected EntityInUseException was not thrown." );
		}
		catch ( EntityInUseException )
		{
			// expected
		}
	}

	[TestMethod]
	public async Task GetLastWorkDateOnProjectAsync_WithWorkDate_ReturnsDateOnly()
	{
		// Arrange
		var projectId = 123;
		var expectedDate = new DateTime(2024, 1, 15);

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<DateTime?>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( expectedDate );

		// Act
		var result = await _project_service.GetLastWorkDateOnProjectAsync(projectId);

		// Assert
		Assert.IsNotNull( result );
		Assert.AreEqual( DateOnly.FromDateTime( expectedDate ), result );
	}

	[TestMethod]
	public async Task GetLastWorkDateOnProjectAsync_WithNoWorkDate_ReturnsNull()
	{
		// Arrange
		var projectId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<DateTime?>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( ( DateTime? ) null );

		// Act
		var result = await _project_service.GetLastWorkDateOnProjectAsync(projectId);

		// Assert
		Assert.IsNull( result );
	}

	[TestMethod]
	public async Task IsProjectUsedAsync_WhenProjectIsUsed_ReturnsTrue()
	{
		// Arrange
		var projectId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1 );

		// Act
		var result = await _project_service.IsProjectUsedAsync(projectId);

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public async Task IsProjectUsedAsync_WhenProjectIsNotUsed_ReturnsFalse()
	{
		// Arrange
		var projectId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0 );

		// Act
		var result = await _project_service.IsProjectUsedAsync(projectId);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithNullName_ReturnsFalse()
	{
		// Act
		var result = await _project_service.NameExistsAsync(null);

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithWhitespaceName_ReturnsFalse()
	{
		// Act
		var result = await _project_service.NameExistsAsync("   ");

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithExistingName_ReturnsTrue()
	{
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1 );

		var result = await _project_service.NameExistsAsync("Existing Project");

		Assert.IsTrue( result );
		_mockDataService.Verify( s => s.ExecuteQueryAsync(
			It.IsAny<string>(),
			It.IsAny<Func<System.Data.Common.DbDataReader, ProjectModel>>() ), Times.Never );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithNonExistingName_ReturnsFalse()
	{
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0 );

		var result = await _project_service.NameExistsAsync("Project B");

		Assert.IsFalse( result );
		_mockDataService.Verify( s => s.ExecuteQueryAsync(
			It.IsAny<string>(),
			It.IsAny<Func<System.Data.Common.DbDataReader, ProjectModel>>() ), Times.Never );
	}

	[TestMethod]
	public async Task NameExistsAsync_IsCaseInsensitive()
	{
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.Is<string>( query => query.Contains( "LOWER(" ) ), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1 );

		var result = await _project_service.NameExistsAsync("TEST PROJECT");

		Assert.IsTrue( result );
	}

	[TestMethod]
	public async Task GetProjectWorkStatsAsync_ReturnsWorkStats()
	{
		// Arrange
		var projectId = 123;
		var expectedStats = new ProjectWorkStats
		{
			// ProjectWorkStats.StartDate is a DateTime, assign a DateTime here
			StartDate = DateTime.Today,
			TotalHours = 40.5
		};

		_mockDataService
			.Setup( s => s.ExecuteSingleAsync<ProjectWorkStats>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( expectedStats );

		// Act
		var result = await _project_service.GetProjectWorkStatsAsync(projectId);

		// Assert
		Assert.IsNotNull( result );
		Assert.AreEqual( 40.5, result.TotalHours );
	}

	[TestMethod]
	public async Task GetProjectWorkStatsAsync_WithNoData_ReturnsNull()
	{
		// Arrange
		var projectId = 123;

		_mockDataService
			.Setup( s => s.ExecuteSingleAsync<ProjectWorkStats>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( ( ProjectWorkStats? ) null );

		// Act
		var result = await _project_service.GetProjectWorkStatsAsync(projectId);

		// Assert
		Assert.IsNull( result );
	}

	[TestMethod]
	public void ProjectUsed_Property_CanBeSetAndRetrieved()
	{
		// Arrange
		var service = new ProjectService(_mockDataService.Object);

		// Act
		service.ProjectUsed = true;

		// Assert
		Assert.IsTrue( service.ProjectUsed );
	}

	[TestMethod]
	public void ProjectUsed_DefaultValue_IsFalse()
	{
		// Arrange
		var service = new ProjectService(_mockDataService.Object);

		// Act & Assert
		Assert.IsFalse( service.ProjectUsed );
	}

	private static Dictionary<string, object?> CreateValidProjectParameters(
		int? projectId = null,
		string projectName = "New Project",
		string projectCode = "NP001" )
	{
		var parameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.ProjectFieldNameCode}", projectCode },
			{ $"@{DBNames.ProjectFieldNameName}", projectName },
			{ $"@{DBNames.ProjectFieldNameStartDate}", new DateOnly( 2026, 4, 30 ) },
			{ $"@{DBNames.ProjectFieldNameEndDate}", new DateOnly( 2026, 5, 31 ) },
			{ $"@{DBNames.ProjectFieldNameExpectedTime}", 16 },
			{ $"@{DBNames.ProjectFieldNameClosed}", false },
			{ $"@{DBNames.ProjectFieldNameImage}", null },
			{ $"@{DBNames.ProjectFieldNameImageRotationAngle}", 0d },
			{ $"@{DBNames.ProjectFieldNameMemo}", "Test memo" }
		};

		if ( projectId.HasValue )
		{
			parameters.Add( $"@{DBNames.ProjectFieldNameId}", projectId.Value );
		}

		return parameters;
	}

	// Small test exception that mimics MySqlException's Number property
	private class TestMySqlException : Exception
	{
		public int Number { get; }
		public TestMySqlException( int number, string? message = null ) : base( message )
		{
			Number = number;
		}
	}
}
