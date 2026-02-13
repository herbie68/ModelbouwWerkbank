namespace Modelbouwer.Services;

public class ProductService : IProductService
{
	private readonly GenericDataService _dataService;
	public bool ProductUsed { get; set; } = false;

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
		catch ( Exception ex )
		{
			// Try to get a SQL error number (MySqlException has a Number property).
			// We use reflection so unit tests can throw a lightweight exception exposing a Number property.
			int? number = null;
			try
			{
				var prop = ex.GetType().GetProperty( "Number" );
				if ( prop != null && prop.PropertyType == typeof( int ) )
					number = ( int? ) prop.GetValue( ex );
			}
			catch { /* ignore reflection issues */ }

			if ( number == 1451 )
			{
				throw new EntityInUseException(
					$"{Lang.metadataProductDeleteError}." );
			}

			// Not a MySQL foreign key constraint — rethrow original.
			throw;
		}
	}

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

	public async Task<bool> NameExistsAsync( string? productName )
	{
		if ( string.IsNullOrWhiteSpace( productName ) )
			return false;

		var products = await GetAllProductsAsync();

		return products.Any( c =>
			string.Equals( c.Name, productName, StringComparison.OrdinalIgnoreCase ) );
	}
}
