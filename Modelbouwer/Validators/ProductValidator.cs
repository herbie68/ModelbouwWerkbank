namespace Modelbouwer.Validators;

public class ProductValidator : IEntityValidator<ProductModel>
{
	private readonly IProductService _dataService;

	public ProductValidator( IProductService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( ProductModel product )
	{
		var result = new ValidationResult();

		// Name
		if ( string.IsNullOrWhiteSpace( product.Name ) )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameRequirered );
		}
		else if ( product.Name.Length > 100 )
		{
			result.Errors.Add( Lang.ExportValidationMessageNameLength );
		}

		// ❗ Duplicate checks
		if ( product.ProductId == 0 )
		{
			if ( await _dataService.NameExistsAsync( product.Name ) )
				result.Errors.Add( Lang.ExportValidationProductNameExists );
		}

		return result;
	}
}
