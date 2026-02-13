namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class ProjectServiceTests
{
	private Mock<GenericDataService> _mockDataService;
	private ProjectService _projectService;

	[TestInitialize]
	public void Setup()
	{
		_mockDataService = new Mock<GenericDataService>();
		_projectService = new ProjectService(_mockDataService.Object);
	}

	[TestMethod]
	public async Task GetAllProjectsAsync_ShouldReturnListOfProjects()
	{
		// Arrange
		var expectedProjects = new List<ProjectModel>
		{
			new ProjectModel { ProjectId = 1, ProjectName = "Project 1" },
			new ProjectModel { ProjectId = 2, ProjectName = "Project 2" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProjectModel>>()))
			.ReturnsAsync(expectedProjects);

		// Act
		var result = await _projectService.GetAllProjectsAsync();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
		Assert.AreEqual("Project 1", result[0].ProjectName);
		Assert.AreEqual("Project 2", result[1].ProjectName);
	}

	[TestMethod]
	public async Task InsertNewProjectAsync_ShouldReturnNewProjectId()
	{
		// Arrange
		var parameters = new Dictionary<string, object?>
		{
			{ "@ProjectName", "New Project" }
		};

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(456);

		// Act
		var result = await _projectService.InsertNewProjectAsync(parameters);

		// Assert
		Assert.AreEqual(456, result);
	}

	[TestMethod]
	public async Task UpdateProjectAsync_ShouldCallDataService()
	{
		// Arrange
		var parameters = new Dictionary<string, object?>
		{
			{ "@ProjectId", 1 },
			{ "@ProjectName", "Updated Project" }
		};

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		await _projectService.UpdateProjectAsync(parameters);

		// Assert
		_mockDataService.Verify(x => x.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.IsAny<Dictionary<string, object>>()), Times.Once);
	}

	[TestMethod]
	public async Task DeleteProjectAsync_ShouldCallDataService()
	{
		// Arrange
		int projectId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		await _projectService.DeleteProjectAsync(projectId);

		// Assert
		_mockDataService.Verify(x => x.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>(d => d.ContainsKey("@ProjectId"))), Times.Once);
	}

	[TestMethod]
	[ExpectedException(typeof(EntityInUseException))]
	public async Task DeleteProjectAsync_WhenProjectInUse_ShouldThrowEntityInUseException()
	{
		// Arrange
		int projectId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<uint>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ThrowsAsync(new MySqlException("Foreign key constraint", 1451));

		// Act
		await _projectService.DeleteProjectAsync(projectId);

		// Assert - ExpectedException
	}

	[TestMethod]
	public async Task GetLastWorkDateOnProjectAsync_WhenDateExists_ShouldReturnDateOnly()
	{
		// Arrange
		int projectId = 1;
		var expectedDate = new DateTime(2024, 1, 15);

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<DateTime?>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(expectedDate);

		// Act
		var result = await _projectService.GetLastWorkDateOnProjectAsync(projectId);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(DateOnly.FromDateTime(expectedDate), result);
	}

	[TestMethod]
	public async Task GetLastWorkDateOnProjectAsync_WhenNoDate_ShouldReturnNull()
	{
		// Arrange
		int projectId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<DateTime?>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync((DateTime?)null);

		// Act
		var result = await _projectService.GetLastWorkDateOnProjectAsync(projectId);

		// Assert
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task IsProjectUsedAsync_WhenProjectIsUsed_ShouldReturnTrue()
	{
		// Arrange
		int projectId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<int>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(1);

		// Act
		var result = await _projectService.IsProjectUsedAsync(projectId);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsProjectUsedAsync_WhenProjectIsNotUsed_ShouldReturnFalse()
	{
		// Arrange
		int projectId = 1;

		_mockDataService
			.Setup(x => x.ExecuteScalarAsync<int>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		var result = await _projectService.IsProjectUsedAsync(projectId);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameIsNull_ShouldReturnFalse()
	{
		// Arrange
		string? projectName = null;

		// Act
		var result = await _projectService.NameExistsAsync(projectName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameIsEmpty_ShouldReturnFalse()
	{
		// Arrange
		string projectName = "";

		// Act
		var result = await _projectService.NameExistsAsync(projectName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameIsWhitespace_ShouldReturnFalse()
	{
		// Arrange
		string projectName = "   ";

		// Act
		var result = await _projectService.NameExistsAsync(projectName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameExists_ShouldReturnTrue()
	{
		// Arrange
		string projectName = "Existing Project";
		var projects = new List<ProjectModel>
		{
			new ProjectModel { ProjectId = 1, ProjectName = "Existing Project" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProjectModel>>()))
			.ReturnsAsync(projects);

		// Act
		var result = await _projectService.NameExistsAsync(projectName);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenNameDoesNotExist_ShouldReturnFalse()
	{
		// Arrange
		string projectName = "Non-Existing Project";
		var projects = new List<ProjectModel>
		{
			new ProjectModel { ProjectId = 1, ProjectName = "Existing Project" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProjectModel>>()))
			.ReturnsAsync(projects);

		// Act
		var result = await _projectService.NameExistsAsync(projectName);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NameExistsAsync_ShouldBeCaseInsensitive()
	{
		// Arrange
		string projectName = "EXISTING PROJECT";
		var projects = new List<ProjectModel>
		{
			new ProjectModel { ProjectId = 1, ProjectName = "existing project" }
		};

		_mockDataService
			.Setup(x => x.ExecuteQueryAsync(
				It.IsAny<string>(),
				It.IsAny<Func<object, ProjectModel>>()))
			.ReturnsAsync(projects);

		// Act
		var result = await _projectService.NameExistsAsync(projectName);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task GetProjectWorkStatsAsync_ShouldReturnWorkStats()
	{
		// Arrange
		int projectId = 1;
		var expectedStats = new ProjectWorkStats();

		_mockDataService
			.Setup(x => x.ExecuteSingleAsync<ProjectWorkStats>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(expectedStats);

		// Act
		var result = await _projectService.GetProjectWorkStatsAsync(projectId);

		// Assert
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task GetProjectWorkStatsAsync_WhenNoStats_ShouldReturnNull()
	{
		// Arrange
		int projectId = 1;

		_mockDataService
			.Setup(x => x.ExecuteSingleAsync<ProjectWorkStats>(
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync((ProjectWorkStats?)null);

		// Act
		var result = await _projectService.GetProjectWorkStatsAsync(projectId);

		// Assert
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ProjectUsed_Property_ShouldBeSettable()
	{
		// Arrange
		var service = new ProjectService(_mockDataService.Object);

		// Act
		service.ProjectUsed = true;

		// Assert
		Assert.IsTrue(service.ProjectUsed);
	}

	[TestMethod]
	public void QueryStrings_ShouldNotBeNullOrEmpty()
	{
		// Arrange & Act & Assert
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.CompleteProjectList));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.AddNewProjectQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.UpdateProjectQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.DeleteProjectQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.ProjectNameExistsQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.LastWorkDateOnProjectQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.FirstWorkDateAndHourTotalsOnProjectQueryWithProjectId));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.ProjectUsedQuery));
		Assert.IsFalse(string.IsNullOrEmpty(_projectService.GetProjectExpectedEndDateQuery));
	}
}