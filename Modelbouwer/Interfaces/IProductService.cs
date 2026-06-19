namespace Modelbouwer.Interfaces;

public interface IProductService
{
	Task<List<ProductModel>> GetAllProductsAsync();
	Task<List<BrandModel>> GetAllBrandsAsync();
	Task<List<UnitModel>> GetAllUnitsAsync();
	Task<List<CategoryModel>> GetAllCategoriesAsync();
	Task<int> InsertNewProductAsync( Dictionary<string, object?> queryParameters );
	Task UpdateProductAsync( Dictionary<string, object?> queryParameters );
	Task DeleteProductAsync( int productId );

	Task<bool> IsProductUsedAsync( int productId );
	Task<bool> NameExistsAsync( string? productName );
}