namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class ProjectReportsViewModelTests
{
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
	public void ProjectReportsViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( ProjectReportsViewModel ) ) );
	}

	[TestMethod]
	public void ProjectReportsViewModel_CancelsPreviousReportLoadAndPassesTokenToReportServices()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "ProjectReportsViewModel.cs" );

		StringAssert.Contains( source, "private CancellationTokenSource? _loadReportsCancellationTokenSource;" );
		StringAssert.Contains( source, "_loadReportsCancellationTokenSource?.Cancel();" );
		StringAssert.Contains( source, "LoadReportsAsync( cancellationToken )" );
		StringAssert.Contains( source, "var selectedProject = SelectedProject;" );
		AssertMethodContains( source, "private async Task LoadReportsAsync( CancellationToken cancellationToken )", "var reports = await PerformanceTrace.MeasureAsync(" );
		AssertMethodContains( source, "private async Task LoadReportsAsync( CancellationToken cancellationToken )", "_timeRegistrationService.GetProjectReportsAsync( selectedProject.ProjectId, IncludeHoursInCosts, HourRate, cancellationToken )" );
		AssertMethodContains( source, "private async Task LoadReportsAsync( CancellationToken cancellationToken )", "ReplaceItems( WeekdayHours, reports.WeekdayHours );" );
		AssertMethodContains( source, "private async Task LoadReportsAsync( CancellationToken cancellationToken )", "ReplaceItems( CostDeclarationSummary, reports.CostDeclarationSummary );" );
	}

	[TestMethod]
	public void ProjectReportsViewModel_LoadProjectsStartsIndependentServiceCallsBeforeAwaiting()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "ProjectReportsViewModel.cs" );

		AssertMethodContains( source, "private async Task LoadProjectsAsync()", "var projectsTask = _projectService.GetAllProjectsAsync();" );
		AssertMethodContains( source, "private async Task LoadProjectsAsync()", "var hourRateTask = _timeRegistrationService.GetHourRateAsync();" );
		AssertMethodContains( source, "private async Task LoadProjectsAsync()", "await Task.WhenAll( projectsTask, hourRateTask );" );
	}

	private static ProjectReportsViewModel CreateViewModel( IProjectService? projectService = null, ITimeRegistrationService? timeRegistrationService = null )
	{
		var defaultProjectService = new Mock<IProjectService>();
		defaultProjectService.Setup( service => service.GetAllProjectsAsync() ).ReturnsAsync( [] );

		var defaultTimeRegistrationService = new Mock<ITimeRegistrationService>();
		defaultTimeRegistrationService.Setup( service => service.GetHourRateAsync() ).ReturnsAsync( 0 );

		return new ProjectReportsViewModel(
			projectService ?? defaultProjectService.Object,
			timeRegistrationService ?? defaultTimeRegistrationService.Object );
	}

	private static string LoadSource( params string[] relativeSegments )
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
