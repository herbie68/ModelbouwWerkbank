using System;
using System.Collections.Generic;
using System.Text;

using Modelbouwer.Interfaces;
using Modelbouwer.Services;

namespace Modelbouwer.Validators;

public class CountryValidator : IEntityValidator<CountryModel>
{
	private readonly ICountryService _countryService;

	public CountryValidator( ICountryService countryService ) => _countryService = countryService;

	public async Task<ValidationResult> ValidateAsync( CountryModel country )
	{
		var result = new ValidationResult();

		// Code
		if ( string.IsNullOrWhiteSpace( country.CountryCode ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageCodeRequirered );
		}
		else if ( country.CountryCode.Length > 10 )
		{
			result.Errors.Add( Lang.ExportValidationMessageCodeLength );
		}

		// Name
		if ( string.IsNullOrWhiteSpace( country.CountryName ) )
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		else if ( country.CountryName.Length > 100 )
			result.Errors.Add( Lang.ExportValidationMessageNameLength );

		// ❗ Duplicate checks (alleen bij nieuw)
		if ( country.CountryId == 0 )
		{
			if ( await _countryService.CodeExistsAsync( country.CountryCode ) )
				result.Errors.Add( Lang.ExportValidationCountryCodeExists );

			if ( await _countryService.NameExistsAsync( country.CountryName ) )
				result.Errors.Add( Lang.ExportValidationCountryNameExists );
		}

		return result;
	}
}

