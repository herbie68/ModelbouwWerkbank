namespace Modelbouwer.Interfaces;

public interface IProductService
{
	/// <summary>
/// Retrieves all products.
/// </summary>
/// <returns>A list of ProductModel containing all products; an empty list if no products are found.</returns>
Task<List<ProductModel>> GetAllProductsAsync();
	/// <summary>
/// Retrieves all brand records.
/// </summary>
/// <returns>A list of <see cref="BrandModel"/> containing every brand.</returns>
Task<List<BrandModel>> GetAllBrandsAsync();
	/// <summary>
/// Retrieves all unit records.
/// </summary>
/// <returns>A list of <see cref="UnitModel"/> objects representing all units.</returns>
Task<List<UnitModel>> GetAllUnitsAsync();
	/// <summary>
/// Retrieves all product categories.
/// </summary>
/// <returns>A list of CategoryModel containing every category.</returns>
Task<List<CategoryModel>> GetAllCategoriesAsync();
	/// <summary>
/// Inserts a new product using the provided parameter map and returns the new product's identifier.
/// </summary>
/// <param name="queryParameters">A dictionary mapping product field names to their values (e.g., name, brandId, unitId, categoryId, price). Values may be null for optional fields.</param>
/// <returns>The database identifier of the newly inserted product.</returns>
Task<int> InsertNewProductAsync( Dictionary<string, object?> queryParameters );
	/// <summary>
/// Updates an existing product using the provided parameter map.
/// </summary>
/// <param name="queryParameters">Dictionary of update fields keyed by parameter name. Must include the product identifier (for example "ProductId") and any fields to modify; values may be null to clear nullable fields.</param>
Task UpdateProductAsync( Dictionary<string, object?> queryParameters );
	/// <summary>
/// Deletes the product with the specified identifier.
/// </summary>
/// <param name="productId">The identifier of the product to delete.</param>
Task DeleteProductAsync( int productId );

	/// <summary>
/// Determines whether the product with the specified identifier is referenced by any other records (is in use).
/// </summary>
/// <param name="productId">The identifier of the product to check.</param>
/// <returns><c>true</c> if the product is in use, <c>false</c> otherwise.</returns>
Task<bool> IsProductUsedAsync( int productId );
	/// <summary>
/// Checks whether a product with the specified name exists.
/// </summary>
/// <param name="productName">The product name to check; may be null.</param>
/// <returns>`true` if a product with the specified name exists, `false` otherwise.</returns>
Task<bool> NameExistsAsync( string? productName );
}