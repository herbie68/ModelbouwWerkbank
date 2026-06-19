using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Validators;

public class WorktypeValidator : IEntityValidator<WorktypeModel>
{
	private readonly IWorktypeService _dataService;

	public WorktypeValidator( IWorktypeService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( WorktypeModel worktype )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( worktype.WorktypeName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( worktype.WorktypeName.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		if ( await _dataService.NameExistsAsync( worktype.WorktypeName, worktype.ParentId ) )
		{
			result.Errors.Add( Lang.ExportValidationWorkTypeNameExists );
		}

		return result;
	}
}