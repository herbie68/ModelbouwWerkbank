using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Validators;

public class WorkTypeValidator : IEntityValidator<WorkTypeModel>
{
	private readonly IWorkTypeService _dataService;

	public WorkTypeValidator( IWorkTypeService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( WorkTypeModel worktype )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( worktype.WorkTypeName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( worktype.WorkTypeName.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		if ( await _dataService.NameExistsAsync( worktype.WorkTypeName, worktype.ParentId ) )
		{
			result.Errors.Add( Lang.ExportValidationWorkTypeNameExists );
		}

		return result;
	}
}
