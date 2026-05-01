namespace Modelbouwer.UnitTests.Views;

[TestClass]
public class ViewXamlWiringTests
{
	[TestMethod]
	public void CategoryView_WiresTreeGridLoadedHandler()
	{
		var xaml = LoadViewXaml( "CategoryView.xaml" );

		StringAssert.Contains( xaml, "Loaded=\"CategoryDataGrid_Loaded\"" );
	}

	[TestMethod]
	public void StorageLocationView_WiresTreeGridLoadedHandler()
	{
		var xaml = LoadViewXaml( "StorageLocationView.xaml" );

		StringAssert.Contains( xaml, "Loaded=\"StorageLocationDataGrid_Loaded\"" );
	}

	[TestMethod]
	public void WorktypeView_WiresTreeGridLoadedHandler()
	{
		var xaml = LoadViewXaml( "WorktypeView.xaml" );

		StringAssert.Contains( xaml, "Loaded=\"WorktypeDataGrid_Loaded\"" );
	}

	private static string LoadViewXaml( string fileName )
	{
		var root = FindSolutionRoot();
		var path = Path.Combine( root, "Modelbouwer", "Views", fileName );

		Assert.IsTrue( File.Exists( path ), $"Expected XAML file at '{path}'." );

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
