namespace Modelbouwer.Validators;

public class StorageLocationValidator : IEntityValidator<StorageLocationModel>
{
	private readonly IStorageLocationService _dataService;

	public StorageLocationValidator( IStorageLocationService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( StorageLocationModel storagelocation )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( storagelocation.StorageLocationName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( storagelocation.StorageLocationName.Length > 400 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		if ( await _dataService.NameExistsAsync( storagelocation.StorageLocationName, storagelocation.ParentId ) )
		{
			result.Errors.Add( Lang.ExportValidationStorageLocationNameExists );
		}

		return result;
	}
}
