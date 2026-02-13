namespace Modelbouwer.Validators;

public class ProductValidator : IEntityValidator<ProductModel>
{
	private readonly IProductService _dataService;

	/// <summary>
/// Initializes a new instance of <see cref="ProductValidator"/> with the specified product service.
/// </summary>
/// <param name="dataService">Service used to query product data (e.g., to check for existing product names).</param>
public ProductValidator( IProductService dataService ) => _dataService = dataService;

	/// <summary>
	/// Validates the provided product's name and returns any validation errors.
	/// </summary>
	/// <param name="product">The product model to validate.</param>
	/// <returns>A <see cref="ValidationResult"/> containing errors for a missing or whitespace name, a name longer than 100 characters, or (for new products) a name that already exists; empty <see cref="ValidationResult.Errors"/> when valid.</returns>
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