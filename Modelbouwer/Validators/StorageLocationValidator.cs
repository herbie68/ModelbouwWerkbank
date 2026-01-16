using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Validators;

public class StorageLocationValidator : IEntityValidator<StorageLocationModel>
{
	private readonly IStorageLocationService _dataService;

	public StorageLocationValidator( IStorageLocationService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( StorageLocationModel storagelocation )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( storagelocation.StorageName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( storagelocation.StorageName.Length > 400 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		if ( await _dataService.NameExistsAsync( storagelocation.StorageName, storagelocation.StorageParentId ) )
		{
			result.Errors.Add( Lang.ExportValidationStorageLocationNameExists );
		}

		return result;
	}
}
