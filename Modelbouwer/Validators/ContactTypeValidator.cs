using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Validators
{
    public class ContactTypeValidator : IEntityValidator<ContactTypeModel>
	{
		private readonly IContactTypeService _dataService;

		public ContactTypeValidator( IContactTypeService dataService ) => _dataService = dataService;

		public async Task<ValidationResult> ValidateAsync( ContactTypeModel contacttype )
		{
			var result = new ValidationResult();

			// Name
			if ( string.IsNullOrWhiteSpace( contacttype.ContactTypeName ) )
			{
				result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
			}
			else if ( contacttype.ContactTypeName.Length > 100 )
			{
				result.Errors.Add( Lang.ExportValidationMessageNameLength );
			}

			// ❗ Duplicate checks
			if ( contacttype.ContactTypeId == 0 )
			{
				if ( await _dataService.NameExistsAsync( contacttype.ContactTypeName ) )
					result.Errors.Add( Lang.ExportValidationContactTypeNameExists );
			}

			return result;
		}
	}
}
