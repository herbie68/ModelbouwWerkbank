using Modelbouwer.Helpers;
using Modelbouwer.Models;
using Modelbouwer.Services;

SetCurrentDirectoryForModelbouwerConfig();

var importer = new CornwallDowelImportService();
var dataService = new GenericDataService();
var productService = new ProductService( dataService );
var supplierService = new SupplierService( dataService );
var brandService = new BrandService( dataService );
var categoryService = new CategoryService( dataService );
var unitService = new UnitService( dataService );
var storageLocationService = new StorageLocationService( dataService );

var defaults = new CornwallDowelImportDefaults
{
	BrandId = ( await brandService.GetAllBrandsAsync() ).OrderBy( b => b.BrandId ).First().BrandId,
	CategoryId = ( await categoryService.GetAllCategorysAsync() ).OrderBy( c => c.CategoryId ).First().CategoryId,
	UnitId = ( await unitService.GetAllUnitsAsync() ).OrderBy( u => u.UnitId ).First().UnitId,
	StorageId = ( await storageLocationService.GetAllStorageLocationsAsync() ).OrderBy( s => s.StorageId ).First().StorageId
};

Console.WriteLine( $"Using defaults: brand={defaults.BrandId}, category={defaults.CategoryId}, unit={defaults.UnitId}, storage={defaults.StorageId}" );

var result = await importer.RunImportAsync( productService, supplierService, defaults, supplierId: 1 );

Console.WriteLine( $"Processed source rows: {result.Items.Count}" );
Console.WriteLine( $"Created products: {result.CreatedProducts}" );
Console.WriteLine( $"Reused products: {result.ReusedProducts}" );
Console.WriteLine( $"Created supplier rows: {result.CreatedSupplierRows}" );
Console.WriteLine( $"Updated supplier rows: {result.UpdatedSupplierRows}" );

var importedNames = new HashSet<string>(
	result.Items.Select( i => i.Name ),
	StringComparer.OrdinalIgnoreCase );

var products = await productService.GetAllProductsAsync();
var linkedSupplierRows = ( await supplierService.GetAllProductSuppliersAsync() )
	.Where( ps => ps.SupplierId == 1 && importedNames.Contains( ps.ProductName ?? string.Empty ) )
	.ToList();

Console.WriteLine();
Console.WriteLine( "Verification sample:" );

foreach ( var product in products
	.Where( p => importedNames.Contains( p.ProductName ?? string.Empty ) )
	.OrderBy( p => p.ProductName )
	.Take( 5 ) )
{
	var imageLength = product.ProductImage?.Length ?? 0;
	Console.WriteLine( $"PRODUCT\t{product.ProductId}\t{product.ProductCode}\t{product.ProductName}\timageBytes={imageLength}" );
}

foreach ( var supplierRow in linkedSupplierRows
	.OrderBy( ps => ps.ProductName )
	.Take( 5 ) )
{
	Console.WriteLine( $"SUPPLIER\t{supplierRow.ProductSupplierId}\t{supplierRow.ProductNumber}\t{supplierRow.ProductName}\t{supplierRow.Price.ToString( "0.00" )}\t{supplierRow.URL}" );
}

static void SetCurrentDirectoryForModelbouwerConfig()
{
	var current = new DirectoryInfo( AppContext.BaseDirectory );

	while ( current is not null )
	{
		var candidate = Path.Combine( current.FullName, "Modelbouwer", "Resources", "Config", "modelbouwer.config" );
		if ( File.Exists( candidate ) )
		{
			Directory.SetCurrentDirectory( Path.Combine( current.FullName, "Modelbouwer" ) );
			return;
		}

		current = current.Parent;
	}

	throw new InvalidOperationException( "Could not locate Modelbouwer/Resources/Config/modelbouwer.config from the current runtime path." );
}
