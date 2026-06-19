namespace Modelbouwer.Services;

public class CategoryService : ICategoryService
{
	private readonly GenericDataService _dataService;
	public bool CategoryUsed { get; set; } = false;

	public CategoryService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteCategoryList = $"" +
		$"SELECT " +
		$"{DBNames.CategoryFieldNameId} AS {DBNames.CategoryFieldNameId}, " +
		$"{DBNames.CategoryFieldNameParentId} AS {DBNames.CategoryFieldNameParentId}, " +
		$"{DBNames.CategoryFieldNameName} AS {DBNames.CategoryFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.CategoryTable};";

	public string AddNewCategoryQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.CategoryTable} " +
		$"({DBNames.CategoryFieldNameParentId}, {DBNames.CategoryFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.CategoryFieldNameParentId}, @{DBNames.CategoryFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateCategoryQuery =
		$"UPDATE {DBNames.Database}.{DBNames.CategoryTable} " +
		$"SET " +
		$"{DBNames.CategoryFieldNameParentId} = @{DBNames.CategoryFieldNameParentId}, " +
		$"{DBNames.CategoryFieldNameName} = @{DBNames.CategoryFieldNameName}" +
		$"WHERE {DBNames.CategoryFieldNameId} = @{DBNames.CategoryFieldNameId};";

	public string DeleteCategoryQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.CategoryTable} " +
		$"WHERE {DBNames.CategoryFieldNameId} = @{DBNames.CategoryFieldNameId};";

	public string CategoryNameExistsQuery =
		$"SELECT COUNT(*) " +
		$"FROM {DBNames.Database}.{DBNames.CategoryTable} " +
		$"WHERE {DBNames.CategoryFieldNameName} = @{DBNames.CategoryFieldNameName} " +
		$"AND ( " +
		$"( {DBNames.CategoryFieldNameParentId} = @{DBNames.CategoryFieldNameParentId} ) " +
		$"OR ( {DBNames.CategoryFieldNameParentId} IS NULL AND @{DBNames.CategoryFieldNameParentId} IS NULL ) );";

	public string CategoryUsedQuery =
		$"SELECT COUNT(*){DBNames.ProductFieldNameCategoryId}) FROM {DBNames.Database}.{DBNames.ProductTable} WHERE {DBNames.ProductFieldNameCategoryId} = @CategoryId";
	#endregion

	public Task<List<CategoryModel>> GetAllCategorysAsync()
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

	public async Task<int> InsertNewCategoryAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.CategoryFieldNameParentId}", queryParameters[$"@{DBNames.CategoryFieldNameParentId}"] ?? DBNull.Value },
			{ $"@{DBNames.CategoryFieldNameName}", queryParameters[$"@{DBNames.CategoryFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewCategoryQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateCategoryAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.CategoryFieldNameId}", queryParameters[$"@{DBNames.CategoryFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.CategoryFieldNameParentId}", queryParameters[$"@{DBNames.CategoryFieldNameParentId}"] ?? DBNull.Value },
			{ $"@{DBNames.CategoryFieldNameName}", queryParameters[$"@{DBNames.CategoryFieldNameName}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateCategoryQuery, parameters );
	}

	public async Task DeleteCategoryAsync( int categoryId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.CategoryFieldNameId}", categoryId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteCategoryQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataCategoryDeleteError}." );
		}
	}

	public async Task<bool> IsCategoryUsedAsync( int categoryId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ProductFieldNameCategoryId}", categoryId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			CategoryUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? categoryName, int? parentId )
	{
		if ( string.IsNullOrWhiteSpace( categoryName ) )
			return false;

		if ( parentId == 0 )
			parentId = null;

		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.CategoryFieldNameParentId}", ( object? ) parentId ?? DBNull.Value },
			{ $"@{DBNames.CategoryFieldNameName}", categoryName }
		};


		var count = await _dataService.ExecuteScalarAsync<int>( CategoryNameExistsQuery, parameters );
		return count > 0;
	}
}