namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class DependencyInjectionRegistrationTests
{
	[TestMethod]
	public void InterfaceRegistrations_ForSingletonServices_ReuseConcreteSingletons()
	{
		var source = LoadSource( "Modelbouwer", "App.xaml.cs" );

		StringAssert.Contains(
			source,
			"services.AddSingleton<ISettingsService>( provider => provider.GetRequiredService<SettingsService>() );" );

		StringAssert.Contains(
			source,
			"services.AddSingleton<IGitHubReleaseHistoryService>( provider => provider.GetRequiredService<GitHubReleaseHistoryService>() );" );
	}

	private static string LoadSource( params string[] relativeSegments )
	{
		var root = FindSolutionRoot();
		var path = Path.Combine( [ root, .. relativeSegments ] );

		Assert.IsTrue( File.Exists( path ), $"Expected source file at '{path}'." );
		return File.ReadAllText( path );
	}

	private static string FindSolutionRoot()
	{
		var directory = new DirectoryInfo( AppContext.BaseDirectory );

		while ( directory != null )
		{
			if ( File.Exists( Path.Combine( directory.FullName, "ModelbouwWerkbank.slnx" ) ) )
				return directory.FullName;

			directory = directory.Parent;
		}

		Assert.Fail( "Could not locate the solution root from the test output directory." );
		return string.Empty;
	}
}
