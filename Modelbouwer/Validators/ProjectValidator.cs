namespace Modelbouwer.Validators;

public class ProjectValidator : IEntityValidator<ProjectModel>
{
	private readonly IProjectService _dataService;

	public ProjectValidator( IProjectService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( ProjectModel project )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( project.ProjectName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( project.ProjectName.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		// ❗ Duplicate checks
		if ( project.ProjectId == 0 )
		{
			if ( await _dataService.NameExistsAsync( project.ProjectName ) )
				result.Errors.Add( Lang.ExportValidationProjectNameExists );
		}

		return result;
	}
}