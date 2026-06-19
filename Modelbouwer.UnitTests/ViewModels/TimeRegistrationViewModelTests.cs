using System.Globalization;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class TimeRegistrationViewModelTests
{
	[TestMethod]
	public async Task Constructor_WhenInitializeFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load culture." );
		var timeRegistrationService = new Mock<ITimeRegistrationService>();
		timeRegistrationService
			.Setup( service => service.GetCultureAsync() )
			.Returns( Task.FromException<CultureInfo>( expected ) );

		var viewModel = CreateViewModel( timeRegistrationService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void TimeRegistrationViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( TimeRegistrationViewModel ) ) );
	}

	[TestMethod]
	public void TimeRegistrationViewModel_CancelsPreviousProjectDataLoadAndPassesTokenToTimeRegistrationService()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "TimeRegistrationViewModel.cs" );

		StringAssert.Contains( source, "private CancellationTokenSource? _selectedProjectDataCancellationTokenSource;" );
		StringAssert.Contains( source, "_selectedProjectDataCancellationTokenSource?.Cancel();" );
		StringAssert.Contains( source, "RefreshSelectedProjectDataAsync( cancellationToken )" );
		StringAssert.Contains( source, "var selectedProject = SelectedProject;" );
		AssertMethodContains( source, "private async Task RefreshSelectedProjectDataAsync( CancellationToken cancellationToken )", "var timeEntriesTask = _timeRegistrationService.GetTimeEntriesByProjectAsync( selectedProject.ProjectId, cancellationToken );" );
		AssertMethodContains( source, "private async Task RefreshSelectedProjectDataAsync( CancellationToken cancellationToken )", "var materialUsagesTask = _timeRegistrationService.GetMaterialUsageByProjectAsync( selectedProject.ProjectId, cancellationToken );" );
		AssertMethodContains( source, "private async Task RefreshSelectedProjectDataAsync( CancellationToken cancellationToken )", "PerformanceTrace.MeasureAsync(" );
		AssertMethodContains( source, "private async Task RefreshSelectedProjectDataAsync( CancellationToken cancellationToken )", "Task.WhenAll( timeEntriesTask, materialUsagesTask )" );
		StringAssert.Contains( source, "GetTimeEntriesByProjectAsync( projectId, cancellationToken )" );
		StringAssert.Contains( source, "GetMaterialUsageByProjectAsync( projectId, cancellationToken )" );
	}

	[TestMethod]
	public void TimeRegistrationViewModel_SaveCommandsAreGuardedAgainstParallelExecution()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "TimeRegistrationViewModel.cs" );

		StringAssert.Contains( source, "[ObservableProperty] private bool _isSavingTimeEntries;" );
		StringAssert.Contains( source, "[ObservableProperty] private bool _isSavingMaterialUsages;" );
		StringAssert.Contains( source, "SaveTimeEntriesCommand = new AsyncRelayCommand( SaveTimeEntriesAsync, CanSaveTimeEntries );" );
		StringAssert.Contains( source, "SaveMaterialUsagesCommand = new AsyncRelayCommand( SaveMaterialUsagesAsync, CanSaveMaterialUsages );" );
		StringAssert.Contains( source, "private bool CanSaveTimeEntries() => HasUnsavedTimeChanges && !IsSavingTimeEntries;" );
		StringAssert.Contains( source, "private bool CanSaveMaterialUsages() => HasUnsavedMaterialChanges && !IsSavingMaterialUsages;" );
		AssertMethodContains( source, "private async Task SaveTimeEntriesAsync()", "if ( IsSavingTimeEntries )" );
		AssertMethodContains( source, "private async Task SaveTimeEntriesAsync()", "IsSavingTimeEntries = true;" );
		AssertMethodContains( source, "private async Task SaveTimeEntriesAsync()", "finally" );
		AssertMethodContains( source, "private async Task SaveMaterialUsagesAsync()", "if ( IsSavingMaterialUsages )" );
		AssertMethodContains( source, "private async Task SaveMaterialUsagesAsync()", "IsSavingMaterialUsages = true;" );
		AssertMethodContains( source, "private async Task SaveMaterialUsagesAsync()", "finally" );
	}

	[TestMethod]
	public void TimeRegistrationViewModel_InitializeStartsIndependentLoadTasksBeforeAwaiting()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "TimeRegistrationViewModel.cs" );

		AssertMethodContains( source, "private async Task InitializeAsync()", "var cultureTask = _timeRegistrationService.GetCultureAsync();" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "var hourRateTask = _timeRegistrationService.GetHourRateAsync();" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "var projectsTask = _projectService.GetAllProjectsAsync();" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "var productsTask = _productService.GetAllProductsAsync();" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "var categoriesTask = _categoryService.GetAllCategorysAsync();" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "var worktypesTask = _worktypeService.GetAllWorkTypesAsync();" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "PerformanceTrace.MeasureAsync(" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "Task.WhenAll( cultureTask, hourRateTask, projectsTask, productsTask, categoriesTask, worktypesTask )" );
		AssertMethodContains( source, "private async Task InitializeAsync()", "BuildProductTree();" );
	}

	private static TimeRegistrationViewModel CreateViewModel( ITimeRegistrationService? timeRegistrationService = null )
	{
		var projectService = new Mock<IProjectService>();
		var productService = new Mock<IProductService>();
		var worktypeService = new Mock<IWorktypeService>();
		var categoryService = new Mock<ICategoryService>();

		return new TimeRegistrationViewModel(
			timeRegistrationService ?? CreateTimeRegistrationService().Object,
			projectService.Object,
			productService.Object,
			worktypeService.Object,
			categoryService.Object );
	}

	private static Mock<ITimeRegistrationService> CreateTimeRegistrationService()
	{
		var service = new Mock<ITimeRegistrationService>();
		service.Setup( s => s.GetCultureAsync() ).ReturnsAsync( CultureInfo.InvariantCulture );
		service.Setup( s => s.GetHourRateAsync() ).ReturnsAsync( 0 );
		return service;
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