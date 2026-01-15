using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Interfaces;

public interface ICategoryService
{
	Task<List<CategoryModel>> GetAllCategorysAsync();
	Task<int> InsertNewCategoryAsync( Dictionary<string, object?> queryParameters );
	Task UpdateCategoryAsync( Dictionary<string, object?> queryParameters );
	Task DeleteCategoryAsync( int categoryId );
	Task<bool> IsCategoryUsedAsync( int categoryId );
	Task<bool> NameExistsAsync( string? categoryName, int? parentId );
}