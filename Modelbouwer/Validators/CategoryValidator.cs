using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Validators;

public class CategoryValidator : IEntityValidator<CategoryModel>
{
	private readonly ICategoryService _dataService;

	public CategoryValidator( ICategoryService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( CategoryModel category )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( category.CategoryName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( category.CategoryName.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		if ( await _dataService.NameExistsAsync( category.CategoryName, category.ParentId ) )
		{
			result.Errors.Add( Lang.ExportValidationCategoryNameExists );
		}

		return result;
	}
}