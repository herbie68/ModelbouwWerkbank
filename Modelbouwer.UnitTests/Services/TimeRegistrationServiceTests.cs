using System.Reflection;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class TimeRegistrationServiceTests
{
	[TestMethod]
	public void ITimeRegistrationService_ExposesCancellationTokenOverloads()
	{
		var methods = typeof( ITimeRegistrationService )
			.GetMethods()
			.ToArray();

		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetTimeEntriesByProjectAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.InsertTimeEntryAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.UpdateTimeEntryAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.DeleteTimeEntryAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetMaterialUsageByProjectAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.InsertMaterialUsageAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.UpdateMaterialUsageAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.DeleteMaterialUsageAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetHourRateAsync ), 1 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetCultureAsync ), 1 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetWorkedHoursByWeekdayAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetWorkedHoursByMonthAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetWorkedHoursByYearAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetWorkedHoursByMonthYearAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetWorkedHoursByWorktypeAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetCostAllocationByWorktypeAsync ), 4 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetCostDeclarationsAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetCostDeclarationSummaryAsync ), 4 );
		AssertHasCancellationTokenOverload( methods, nameof( ITimeRegistrationService.GetProjectReportsAsync ), 4 );
	}

	[TestMethod]
	public void TimeRegistrationService_TokenlessMethodsDelegateToCancellationTokenOverloads()
	{
		var source = LoadSource( "Modelbouwer", "Services", "TimeRegistrationService.cs" );

		StringAssert.Contains( source, "GetTimeEntriesByProjectAsync( projectId, CancellationToken.None )" );
		StringAssert.Contains( source, "InsertTimeEntryAsync( entry, CancellationToken.None )" );
		StringAssert.Contains( source, "UpdateTimeEntryAsync( entry, CancellationToken.None )" );
		StringAssert.Contains( source, "DeleteTimeEntryAsync( timeEntryId, CancellationToken.None )" );
		StringAssert.Contains( source, "GetMaterialUsageByProjectAsync( projectId, CancellationToken.None )" );
		StringAssert.Contains( source, "InsertMaterialUsageAsync( usage, CancellationToken.None )" );
		StringAssert.Contains( source, "UpdateMaterialUsageAsync( usage, CancellationToken.None )" );
		StringAssert.Contains( source, "DeleteMaterialUsageAsync( materialUsageId, CancellationToken.None )" );
		StringAssert.Contains( source, "GetHourRateAsync( CancellationToken.None )" );
		StringAssert.Contains( source, "GetCultureAsync( CancellationToken.None )" );
		StringAssert.Contains( source, "GetProjectReportsAsync( projectId, includeHoursInCosts, hourRate, CancellationToken.None )" );
	}

	[TestMethod]
	public void TimeRegistrationService_CancellationTokenOverloadsPassTokenToDependencies()
	{
		var source = LoadSource( "Modelbouwer", "Services", "TimeRegistrationService.cs" );

		StringAssert.Contains( source, "_dataService.ExecuteQueryAsync( query, reader =>" );
		StringAssert.Contains( source, "}, new Dictionary<string, object> { { \"@ProjectId\", projectId } }, cancellationToken )" );
		StringAssert.Contains( source, "_dataService.ExecuteScalarAsync<uint>( query, CreateTimeParameters( entry ), cancellationToken )" );
		StringAssert.Contains( source, "_dataService.ExecuteNonQueryAsync( query, parameters, cancellationToken )" );
		StringAssert.Contains( source, "_settingsService.GetSettingsAsync( DBNames.SettingsFieldNameHourRate, cancellationToken )" );
		StringAssert.Contains( source, "_settingsService.GetSettingsAsync( DBNames.SettingsFieldNameCulture, cancellationToken )" );
		StringAssert.Contains( source, "OpenAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteReaderAsync( cancellationToken )" );
		StringAssert.Contains( source, "ReadAsync( cancellationToken )" );
		StringAssert.Contains( source, "EnrichMaterialUsageAsync( usages, cancellationToken )" );
		StringAssert.Contains( source, "ExecuteQueryAsync( query, reader => new" );
		StringAssert.Contains( source, "}, parameters, cancellationToken )" );
	}

	[TestMethod]
	public void TimeRegistrationService_ProjectReportsReuseSharedBaseData()
	{
		var source = LoadSource( "Modelbouwer", "Services", "TimeRegistrationService.cs" );

		AssertMethodContains( source, "public async Task<ProjectReportsDataModel> GetProjectReportsAsync", "var entriesTask = GetTimeEntriesByProjectAsync( projectId, cancellationToken );" );
		AssertMethodContains( source, "public async Task<ProjectReportsDataModel> GetProjectReportsAsync", "var usagesTask = GetMaterialUsageByProjectAsync( projectId, cancellationToken );" );
		AssertMethodContains( source, "public async Task<ProjectReportsDataModel> GetProjectReportsAsync", "var cultureTask = GetCultureAsync( cancellationToken );" );
		AssertMethodContains( source, "public async Task<ProjectReportsDataModel> GetProjectReportsAsync", "var worktypesTask = GetWorktypeLookupAsync( cancellationToken );" );
		AssertMethodContains( source, "public async Task<ProjectReportsDataModel> GetProjectReportsAsync", "await Task.WhenAll( entriesTask, usagesTask, cultureTask, worktypesTask );" );
	}

	private static void AssertHasCancellationTokenOverload( MethodInfo[] methods, string methodName, int parameterCount )
	{
		var hasOverload = methods.Any( method =>
		{
			if ( method.Name != methodName )
				return false;

			var parameters = method.GetParameters();
			return parameters.Length == parameterCount
				&& parameters[^1].ParameterType == typeof( CancellationToken );
		} );

		Assert.IsTrue( hasOverload, $"{methodName} should expose a {parameterCount}-parameter overload ending in CancellationToken." );
	}

	private static void AssertMethodContains( string source, string methodSignature, string expectedContent )
	{
		var methodStart = source.IndexOf( methodSignature, StringComparison.Ordinal );
		Assert.IsTrue( methodStart >= 0, $"Method '{methodSignature}' was not found." );

		var nextMethod = source.IndexOf( "\n\tpublic ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.IndexOf( "\n\tprivate ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.Length;

		var methodBody = source.Substring( methodStart, nextMethod - methodStart );
		StringAssert.Contains( methodBody, expectedContent );
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
}
