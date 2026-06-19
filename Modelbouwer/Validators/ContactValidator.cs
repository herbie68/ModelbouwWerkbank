namespace Modelbouwer.Validators;

public class ContactValidator : IEntityValidator<SupplierContactModel>
{
	private readonly IContactService _dataService;

	public ContactValidator( IContactService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( SupplierContactModel contact )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( contact.Name ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( contact.Name.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		// ❗ Duplicate checks
		if ( contact.ContactId == 0 )
		{
			if ( await _dataService.NameExistsAsync( contact.Name ) )
				result.Errors.Add( Lang.ExportValidationContactNameExists );
		}

		return result;
	}
}