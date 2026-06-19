namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class ImportCleanupTests
{
	[TestMethod]
	public void OneOffCornwallImportCodeHasBeenRemoved()
	{
		var repositoryRoot = FindRepositoryRoot();
		var sourceFiles = Directory
			.EnumerateFiles( repositoryRoot, "*.cs", SearchOption.AllDirectories )
			.Where( file => !file.Contains( $"{Path.DirectorySeparatorChar}Builds{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase ) )
			.Where( file => !file.EndsWith( nameof( ImportCleanupTests ) + ".cs", StringComparison.OrdinalIgnoreCase ) );

		foreach ( var file in sourceFiles )
		{
			var source = File.ReadAllText( file );
			Assert.IsFalse( source.Contains( "CornwallDowel", StringComparison.Ordinal ), $"One-off Cornwall import reference remains in {file}." );
		}
	}

	private static string FindRepositoryRoot()
	{
		var directory = AppContext.BaseDirectory;
		while ( directory != null && !File.Exists( Path.Combine( directory, "ModelbouwWerkbank.slnx" ) ) )
		{
			directory = Directory.GetParent( directory )?.FullName;
		}

		return directory ?? throw new DirectoryNotFoundException( "Could not locate repository root." );
	}
}