namespace Modelbouwer.UnitTests.Diagnostics;

[TestClass]
public class PerformanceTraceTests
{
	[TestMethod]
	public void PerformanceTrace_MeasuresAsyncOperationsAndAlwaysWritesElapsedTime()
	{
		var source = LoadSource( "Modelbouwer", "Diagnostics", "PerformanceTrace.cs" );

		StringAssert.Contains( source, "public static async Task MeasureAsync" );
		StringAssert.Contains( source, "public static async Task<T> MeasureAsync<T>" );
		StringAssert.Contains( source, "Stopwatch.StartNew()" );
		StringAssert.Contains( source, "finally" );
		StringAssert.Contains( source, "Debug.WriteLine" );
		StringAssert.Contains( source, "[perf]" );
	}

	[TestMethod]
	public void HighTrafficViewModels_ExposePerformanceMeasurementsForLoadPaths()
	{
		AssertSourceContainsMeasurement( "EntityPageViewModel.cs", "EntityPageViewModel" );
		AssertSourceContainsMeasurement( "ProjectReportsViewModel.cs", "ProjectReportsViewModel" );
		AssertSourceContainsMeasurement( "TimeRegistrationViewModel.cs", "TimeRegistrationViewModel" );
		AssertSourceContainsMeasurement( "StockOrderViewModel.cs", "StockOrderViewModel" );
	}

	private static void AssertSourceContainsMeasurement( string fileName, string operationName )
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", fileName );

		StringAssert.Contains( source, "PerformanceTrace.MeasureAsync" );
		StringAssert.Contains( source, operationName );
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
