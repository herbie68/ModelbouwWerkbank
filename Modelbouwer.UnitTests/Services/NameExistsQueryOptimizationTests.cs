namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class NameExistsQueryOptimizationTests
{
	[TestMethod]
	public void MetadataServices_ExistenceChecksUseDirectCountQueries()
	{
		AssertServiceUsesDirectExistsQuery( "BrandService.cs", "public async Task<bool> NameExistsAsync", "BrandNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "ContactService.cs", "public async Task<bool> NameExistsAsync", "ContactNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "ContactTypeService.cs", "public async Task<bool> NameExistsAsync", "ContactTypeNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "CountryService.cs", "public async Task<bool> CodeExistsAsync", "CountryCodeExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "CountryService.cs", "public async Task<bool> NameExistsAsync", "CountryNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "CurrencyService.cs", "public async Task<bool> CodeExistsAsync", "CurrencyCodeExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "CurrencyService.cs", "public async Task<bool> NameExistsAsync", "CurrencyNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "ProductService.cs", "public async Task<bool> NameExistsAsync", "ProductNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "ProjectService.cs", "public async Task<bool> NameExistsAsync", "ProjectNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "SupplierService.cs", "public async Task<bool> NameExistsAsync", "SupplierNameExistsQuery" );
		AssertServiceUsesDirectExistsQuery( "UnitService.cs", "public async Task<bool> NameExistsAsync", "UnitNameExistsQuery" );
	}

	private static void AssertServiceUsesDirectExistsQuery( string serviceFileName, string methodSignature, string queryName )
	{
		var source = LoadSource( "Modelbouwer", "Services", serviceFileName );
		var method = GetMethodBody( source, methodSignature );

		StringAssert.Contains( source, $"public string {queryName}" );
		StringAssert.Contains( source, "LOWER(" );
		StringAssert.Contains( method, $"ExecuteScalarAsync<int>( {queryName}, parameters )" );
		Assert.IsFalse( method.Contains( "GetAll", StringComparison.Ordinal ), $"{serviceFileName} {methodSignature} should not load full lists for existence checks." );
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

	private static string GetMethodBody( string source, string methodSignature )
	{
		var methodStart = source.IndexOf( methodSignature, StringComparison.Ordinal );
		Assert.IsTrue( methodStart >= 0, $"Method '{methodSignature}' was not found." );

		var nextMethod = source.IndexOf( "\n\tpublic ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.IndexOf( "\n\tprivate ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.Length;

		return source.Substring( methodStart, nextMethod - methodStart );
	}
}
