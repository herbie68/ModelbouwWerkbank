using System;
using System.Collections.Generic;
using System.Text;

using Modelbouwer.Interfaces;

namespace Modelbouwer.Validators;

public class CurrencyValidator : IEntityValidator<CurrencyModel>
{
	private readonly ICurrencyService _currencyService;

	public CurrencyValidator( ICurrencyService currencyService ) => _currencyService = currencyService;

	public async Task<ValidationResult> ValidateAsync( CurrencyModel currency )
	{
		var result = new ValidationResult();

		// Code
		if ( string.IsNullOrWhiteSpace( currency.CurrencyCode ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageCodeRequirered );
		}
		else if ( currency.CurrencyCode.Length > 10 )
		{
			result.Errors.Add( Lang.ExportValidationMessageCodeLength );
		}

		// Name
		if ( string.IsNullOrWhiteSpace( currency.CurrencyName ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( currency.CurrencyName.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		// Currency
		if ( currency.CurrencyId <= 0 )
			result.Errors.Add( Lang.ExportValidationMessageCurrencyRequired );

		// ❗ Duplicate checks (alleen bij nieuw)
		if ( currency.CurrencyId == 0 )
		{
			if ( await _currencyService.CodeExistsAsync( currency.CurrencyCode ) )
				result.Errors.Add( Lang.ExportValidationCurrencyCodeExists );

			if ( await _currencyService.NameExistsAsync( currency.CurrencyName ) )
				result.Errors.Add( Lang.ExportValidationCurrencyNameExists );
		}

		return result;
	}
}
