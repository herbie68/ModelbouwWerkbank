namespace Modelbouwer.Services;

public class ProductService : IProductService
{
	private readonly GenericDataService _dataService;
	public bool ProductUsed { get; set; } = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProductService"/> class using the provided data service.
	/// </summary>
	/// <param name="dataService">Service used to execute parameterized database queries for product operations.</param>
	public ProductService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteProductList = $"" +
		$"SELECT " +
		$"{ DBNames.ProductFieldNameBrandId}, " +
		$"{ DBNames.ProductFieldNameCategoryId}, " +
		$"{ DBNames.ProductFieldNameCode}, " +
		$"{ DBNames.ProductFieldNameDimensions}, " +
		$"{ DBNames.ProductFieldNameHide}, " +
		$"{ DBNames.ProductFieldNameId}, " +
		$"{ DBNames.ProductFieldNameImage}, " +
		$"{ DBNames.ProductFieldNameImageRotationAngle}, " +
		$"{ DBNames.ProductFieldNameMemo}, " +
		$"{ DBNames.ProductFieldNameMinimalStock}, " +
		$"{ DBNames.ProductFieldNameName}, " +
		$"{ DBNames.ProductFieldNamePrice}, " +
		$"{ DBNames.ProductFieldNameProjectCosts}, " +
		$"{ DBNames.ProductFieldNameStandardOrderQuantity}, " +
		$"{ DBNames.ProductFieldNameStorageId}, " +
		$"{ DBNames.ProductFieldNameUnitId}" +
		$" FROM {DBNames.Database}.{DBNames.ProductTable};";

	public string CompleteBrandList = $"" +
		$"SELECT " +
		$"{DBNames.BrandFieldNameId} AS {DBNames.BrandFieldNameId}, " +
		$"{DBNames.BrandFieldNameName} AS {DBNames.BrandFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.BrandTable};";

	public string CompleteUnitList = $"" +
		$"SELECT " +
		$"{DBNames.UnitFieldNameUnitId} AS {DBNames.UnitFieldNameUnitId}, " +
		$"{DBNames.UnitFieldNameUnitName} AS {DBNames.UnitFieldNameUnitName}" +
		$" FROM {DBNames.Database}.{DBNames.UnitTable};";

	public string CompleteCategoryList = $"" +
		$"SELECT " +
		$"{DBNames.CategoryFieldNameId} AS {DBNames.CategoryFieldNameId}, " +
		$"{DBNames.CategoryFieldNameParentId} AS {DBNames.CategoryFieldNameParentId}, " +
		$"{DBNames.CategoryFieldNameName} AS {DBNames.CategoryFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.CategoryTable};";

	public string AddNewProductQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.ProductTable} ( " +
		$"{ DBNames.ProductFieldNameBrandId}, " +
		$"{ DBNames.ProductFieldNameCategoryId}, " +
		$"{ DBNames.ProductFieldNameCode}, " +
		$"{ DBNames.ProductFieldNameDimensions}, " +
		$"{ DBNames.ProductFieldNameHide}, " +
		$"{ DBNames.ProductFieldNameId}, " +
		$"{ DBNames.ProductFieldNameImage}, " +
		$"{ DBNames.ProductFieldNameImageRotationAngle}, " +
		$"{ DBNames.ProductFieldNameMemo}, " +
		$"{ DBNames.ProductFieldNameMinimalStock}, " +
		$"{ DBNames.ProductFieldNameName}, " +
		$"{ DBNames.ProductFieldNamePrice}, " +
		$"{ DBNames.ProductFieldNameProjectCosts}, " +
		$"{ DBNames.ProductFieldNameStandardOrderQuantity}, " +
		$"{ DBNames.ProductFieldNameStorageId}, " +
		$"{ DBNames.ProductFieldNameUnitId}" +
		$"VALUES ( " +
		$"@{ DBNames.ProductFieldNameBrandId}, " +
		$"@{ DBNames.ProductFieldNameCategoryId}, " +
		$"@{ DBNames.ProductFieldNameCode}, " +
		$"@{ DBNames.ProductFieldNameDimensions}, " +
		$"@{ DBNames.ProductFieldNameHide}, " +
		$"@{ DBNames.ProductFieldNameId}, " +
		$"@{ DBNames.ProductFieldNameImage}, " +
		$"@{ DBNames.ProductFieldNameImageRotationAngle}, " +
		$"@{ DBNames.ProductFieldNameMemo}, " +
		$"@{ DBNames.ProductFieldNameMinimalStock}, " +
		$"@{ DBNames.ProductFieldNameName}, " +
		$"@{ DBNames.ProductFieldNamePrice}, " +
		$"@{ DBNames.ProductFieldNameProjectCosts}, " +
		$"@{ DBNames.ProductFieldNameStandardOrderQuantity}, " +
		$"@{ DBNames.ProductFieldNameStorageId}, " +
		$"@{ DBNames.ProductFieldNameUnitId}" +
		$"{ DBNames.SqlSelectLastId}";

	public string UpdateProductQuery =
		$"UPDATE {DBNames.Database}.{DBNames.ProductTable} " +
		$"SET " +
		$"{DBNames.ProductFieldNameBrandId} = @{DBNames.ProductFieldNameBrandId}, " +
		$"{DBNames.ProductFieldNameCategoryId} = @{DBNames.ProductFieldNameCategoryId}, " +
		$"{DBNames.ProductFieldNameCode} = @{DBNames.ProductFieldNameCode}, " +
		$"{DBNames.ProductFieldNameDimensions} = @{DBNames.ProductFieldNameDimensions}, " +
		$"{DBNames.ProductFieldNameHide} = @{DBNames.ProductFieldNameHide}, " +
		$"{DBNames.ProductFieldNameImage} = @{DBNames.ProductFieldNameImage}, " +
		$"{DBNames.ProductFieldNameImageRotationAngle} = @{DBNames.ProductFieldNameImageRotationAngle}, " +
		$"{DBNames.ProductFieldNameMemo} = @{DBNames.ProductFieldNameMemo}, " +
		$"{DBNames.ProductFieldNameMinimalStock} = @{DBNames.ProductFieldNameMinimalStock}, " +
		$"{DBNames.ProductFieldNameName} = @{DBNames.ProductFieldNameName}, " +
		$"{DBNames.ProductFieldNamePrice} = @{DBNames.ProductFieldNamePrice}, " +
		$"{DBNames.ProductFieldNameProjectCosts} = @{DBNames.ProductFieldNameProjectCosts}, " +
		$"{DBNames.ProductFieldNameStandardOrderQuantity} = @{DBNames.ProductFieldNameStandardOrderQuantity}, " +
		$"{DBNames.ProductFieldNameStorageId} = @{DBNames.ProductFieldNameStorageId}, " +
		$"{DBNames.ProductFieldNameUnitId} = @{DBNames.ProductFieldNameUnitId} " +
		$"WHERE {DBNames.ProductFieldNameId} = @{DBNames.ProductFieldNameId};";

	public string DeleteProductQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.ProductTable} " +
		$"WHERE {DBNames.ProductFieldNameId} = @{DBNames.ProductFieldNameId};";

	public string ProductNameExistsQuery =
		$"SELECT COUNT({DBNames.ProductFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.ProductTable} " +
		$"WHERE {DBNames.ProductFieldNameName} = @{DBNames.ProductFieldNameName}";

	public string ProductUsedQuery = $"" +
		$"SELECT" +
		$"EXISTS( " +
		$"SELECT 1 " +
		$"FROM {DBNames.Database}.{DBNames.ProductUsageTable} " +
		$"WHERE {DBNames.ProductUsageFieldNameProductId} = @ProductId " +
		$"LIMIT 1 ) " +
		$"OR EXISTS( " +
		$"SELECT 1 " +
		$"FROM {DBNames.Database}.{DBNames.OrderLineTable} " +
		$"WHERE {DBNames.OrderLineFieldNameProductId} = @ProductId " +
		$"LIMIT 1 ) " +
		$"OR EXISTS( " +
		$"SELECT 1 " +
		$"FROM {DBNames.Database}.{DBNames.ProductSupplierTable} " +
		$"WHERE {DBNames.ProductSupplierFieldNameProductId} = @ProductId " +
		$"LIMIT 1 ) " +
		$"AS ProductInUse;";
	#endregion

	/// <summary>
	/// Retrieves all products from the database and maps each result row into a ProductModel.
	/// </summary>
	/// <returns>A list of ProductModel instances representing all products from the Product table.</returns>
	public Task<List<ProductModel>> GetAllProductsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteProductList, reader =>
		{
			return new ProductModel
			{
				ProductBrandId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameBrandId}" ] ),
				ProductCategoryId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameCategoryId}" ] ),
				ProductCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductFieldNameCode}" ] ),
				ProductDimensions = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductFieldNameDimensions}" ] ),
				ProductHide = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameHide}" ] ),
				ProductId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameId}" ] ),
				ProductImage = reader [ $"{DBNames.ProductFieldNameImage}" ] as byte [ ],
				ProductImageRotationAngle = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductFieldNameImageRotationAngle}" ] ),
				ProductMemo = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductFieldNameMemo}" ] ),
				ProductMinimalStock = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.ProductFieldNameMinimalStock}" ] ),
				ProductName = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductFieldNameName}" ] ),
				ProductPrice = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.ProductFieldNamePrice}" ] ),
				ProductProjectCosts = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameProjectCosts}" ] ),
				ProductStandardQuantity = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.ProductFieldNameStandardOrderQuantity}" ] ),
				ProductStorageId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameStorageId}" ] ),
				ProductUnitId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductFieldNameUnitId}" ] )
			};
		} );
	}

	/// <summary>
	/// Retrieves all brands from the database.
	/// </summary>
	/// <returns>A list of BrandModel representing every brand record in the database.</returns>
	public Task<List<BrandModel>> GetAllBrandsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteBrandList, reader =>
		{
			return new BrandModel
			{
				BrandId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.BrandFieldNameId}" ] ),
				BrandName = DatabaseValueConverter.GetString( reader [ $"{DBNames.BrandFieldNameName}" ] )
			};
		} );
	}

	/// <summary>
	/// Retrieves all units from the database and maps each row to a UnitModel.
	/// </summary>
	/// <returns>A list of UnitModel instances containing the unit ID and unit name.</returns>
	public Task<List<UnitModel>> GetAllUnitsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteUnitList, reader =>
		{
			return new UnitModel
			{
				UnitId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.UnitFieldNameUnitId}" ] ),
				UnitName = DatabaseValueConverter.GetString( reader [ $"{DBNames.UnitFieldNameUnitName}" ] )
			};
		} );
	}

	/// <summary>
	/// Retrieves all categories from the database.
	/// </summary>
	/// <returns>A list of CategoryModel where each item contains CategoryId, ParentId, and CategoryName.</returns>
	public Task<List<CategoryModel>> GetAllCategoriesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteCategoryList, reader =>
		{
			return new CategoryModel
			{
				CategoryId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CategoryFieldNameId}" ] ),
				ParentId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CategoryFieldNameParentId}" ] ),
				CategoryName = DatabaseValueConverter.GetString( reader [ $"{DBNames.CategoryFieldNameName}" ] )
			};
		} );
	}

	/// <summary>
	/// Inserts a new product record using the provided SQL parameters and returns the created product's Id.
	/// </summary>
	/// <param name="queryParameters">A dictionary mapping SQL parameter names (including the leading '@') for product fields to their values; null values will be stored as SQL NULL.</param>
	/// <returns>The Id of the newly inserted product.</returns>
	public async Task<int> InsertNewProductAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"{DBNames.ProductFieldNameBrandId}", queryParameters[$"@{DBNames.ProductFieldNameBrandId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameCategoryId}", queryParameters[$"@{DBNames.ProductFieldNameCategoryId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameCode}", queryParameters[$"@{DBNames.ProductFieldNameCode}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameDimensions}", queryParameters[$"@{DBNames.ProductFieldNameDimensions}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameHide}", queryParameters[$"@{DBNames.ProductFieldNameHide}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameImage}", queryParameters[$"@{DBNames.ProductFieldNameImage}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameImageRotationAngle}", queryParameters[$"@{DBNames.ProductFieldNameImageRotationAngle}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameMemo}", queryParameters[$"@{DBNames.ProductFieldNameMemo}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameMinimalStock}", queryParameters[$"@{DBNames.ProductFieldNameMinimalStock}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameName}" , queryParameters[$"@{DBNames.ProductFieldNameName}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNamePrice}", queryParameters[$"@{DBNames.ProductFieldNamePrice}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameProjectCosts}", queryParameters[$"@{DBNames.ProductFieldNameProjectCosts}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameStandardOrderQuantity}", queryParameters[$"@{DBNames.ProductFieldNameStandardOrderQuantity}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameStorageId}", queryParameters[$"@{DBNames.ProductFieldNameStorageId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameUnitId}", queryParameters[$"@{DBNames.ProductFieldNameUnitId}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewProductQuery, parameters );

		return ( int ) newId;
	}

	/// <summary>
	/// Updates an existing product record in the database using the provided query parameters.
	/// </summary>
	/// <param name="queryParameters">Dictionary mapping SQL parameter names (including the '@' prefix) to their values; null or missing values are sent as <c>DBNull.Value</c> for the update.</param>
	public async Task UpdateProductAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"{DBNames.ProductFieldNameId}", queryParameters[$"@{DBNames.ProductFieldNameId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameBrandId}", queryParameters[$"@{DBNames.ProductFieldNameBrandId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameCategoryId}", queryParameters[$"@{DBNames.ProductFieldNameCategoryId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameCode}", queryParameters[$"@{DBNames.ProductFieldNameCode}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameDimensions}", queryParameters[$"@{DBNames.ProductFieldNameDimensions}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameHide}", queryParameters[$"@{DBNames.ProductFieldNameHide}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameImage}", queryParameters[$"@{DBNames.ProductFieldNameImage}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameImageRotationAngle}", queryParameters[$"@{DBNames.ProductFieldNameImageRotationAngle}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameMemo}", queryParameters[$"@{DBNames.ProductFieldNameMemo}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameMinimalStock}", queryParameters[$"@{DBNames.ProductFieldNameMinimalStock}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameName}" , queryParameters[$"@{DBNames.ProductFieldNameName}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNamePrice}", queryParameters[$"@{DBNames.ProductFieldNamePrice}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameProjectCosts}", queryParameters[$"@{DBNames.ProductFieldNameProjectCosts}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameStandardOrderQuantity}", queryParameters[$"@{DBNames.ProductFieldNameStandardOrderQuantity}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameStorageId}", queryParameters[$"@{DBNames.ProductFieldNameStorageId}"] ?? DBNull.Value },
			{ $"{DBNames.ProductFieldNameUnitId}", queryParameters[$"@{DBNames.ProductFieldNameUnitId}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateProductQuery, parameters );
	}

	/// <summary>
	/// Deletes the product with the specified id from the database.
	/// </summary>
	/// <param name="productId">Identifier of the product to delete.</param>
	/// <exception cref="EntityInUseException">Thrown when the product cannot be deleted because it is referenced by other records.</exception>
	public async Task DeleteProductAsync( int productId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ProductFieldNameId}", productId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteProductQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataProductDeleteError}." );
		}
	}

	/// <summary>
	/// Checks whether the product is referenced by other records (usage entries, order lines, or product suppliers).
	/// </summary>
	/// <param name="productId">The database identifier of the product to check.</param>
	/// <returns>`true` if the product is referenced by any related records, `false` otherwise.</returns>
	public async Task<bool> IsProductUsedAsync( int productId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@ProductId", productId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			ProductUsedQuery,
			parameters);

		return usedCount > 0;
	}

	/// <summary>
	/// Determines whether a product with the specified name exists in the catalog.
	/// </summary>
	/// <param name="productName">The product name to check; null or whitespace is treated as not found.</param>
	/// <returns>`true` if a product with the specified name exists (case-insensitive), `false` otherwise.</returns>
	public async Task<bool> NameExistsAsync( string? productName )
	{
		if ( string.IsNullOrWhiteSpace( productName ) )
			return false;

		var products = await GetAllProductsAsync();

		return products.Any( c =>
			string.Equals( c.Name, productName, StringComparison.OrdinalIgnoreCase ) );
	}
}