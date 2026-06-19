namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class ProjectPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsProjectsOnce()
	{
		var projectService = new Mock<IProjectService>();
		projectService
			.Setup( service => service.GetAllProjectsAsync() )
			.ReturnsAsync( [ ] );

		_ = CreateViewModel( projectService.Object );

		await Task.Delay( 100 );
		projectService.Verify( service => service.GetAllProjectsAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadProjectsFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load projects." );
		var projectService = new Mock<IProjectService>();
		projectService
			.Setup( service => service.GetAllProjectsAsync() )
			.Returns( Task.FromException<List<ProjectModel>>( expected ) );

		var viewModel = CreateViewModel( projectService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public async Task SelectedProject_WhenLoadWorkStatsFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load work stats." );
		var project = new ProjectModel { ProjectId = 7, ProjectName = "Test" };
		var projectService = new Mock<IProjectService>();
		projectService
			.Setup( service => service.GetAllProjectsAsync() )
			.ReturnsAsync( [ project ] );
		projectService
			.Setup( service => service.GetProjectWorkStatsAsync( project.ProjectId ) )
			.Returns( Task.FromException<ProjectWorkStats?>( expected ) );

		var viewModel = CreateViewModel( projectService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void ProjectPageViewModel_WorkStatsLoadIgnoresStaleSelectionResults()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "ProjectPageViewModel.cs" );

		StringAssert.Contains( source, "private int _workStatsLoadVersion;" );
		AssertMethodContains( source, "protected override void OnSelectedItemChanged", "var loadVersion = ++_workStatsLoadVersion;" );
		AssertMethodContains( source, "protected override void OnSelectedItemChanged", "ObserveBackgroundTask( LoadWorkStatsAsync( newValue, loadVersion ) );" );
		AssertMethodContains( source, "private async Task LoadWorkStatsAsync( ProjectModel project, int loadVersion )", "if ( loadVersion != _workStatsLoadVersion || !ReferenceEquals( SelectedProject, project ) )" );
	}

	private static ProjectPageViewModel CreateViewModel( IProjectService projectService )
	{
		var validator = new Mock<IEntityValidator<ProjectModel>>();

		return new ProjectPageViewModel( projectService, validator.Object );
	}

	private static string LoadSource( params string [ ] relativeSegments )
	{
		var directory = AppContext.BaseDirectory;
		while ( directory != null && !File.Exists( Path.Combine( directory, "ModelbouwWerkbank.slnx" ) ) )
		{
			directory = Directory.GetParent( directory )?.FullName;
		}

		var repositoryRoot = directory ?? throw new DirectoryNotFoundException( "Could not locate repository root." );
		var path = Path.Combine( [ repositoryRoot, .. relativeSegments ] );

		return File.ReadAllText( path );
	}

	private static void AssertMethodContains( string source, string methodSignature, string expectedContent )
	{
		var methodStart = source.IndexOf( methodSignature, StringComparison.Ordinal );
		Assert.IsTrue( methodStart >= 0, $"Method '{methodSignature}' was not found." );

		var nextMethod = source.IndexOf( "\n\tprivate ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.Length;

		var methodBody = source.Substring( methodStart, nextMethod - methodStart );
		StringAssert.Contains( methodBody, expectedContent );
	}

	private static async Task WaitUntilAsync( Func<bool> condition )
	{
		using var timeout = new CancellationTokenSource( TimeSpan.FromSeconds( 2 ) );

		while ( !condition() )
		{
			if ( timeout.IsCancellationRequested )
				Assert.Fail( "Condition was not met before timeout." );

			await Task.Delay( 10 );
		}
	}
}