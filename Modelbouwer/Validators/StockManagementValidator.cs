namespace Modelbouwer.Validators;

public class StockManagementValidator : IEntityValidator<StockManagementModel>
{
	private readonly IStockService _dataService;

	public StockManagementValidator( IStockService dataService ) => _dataService = dataService;

	public async Task<ValidationResult> ValidateAsync( StockManagementModel stockmanagement )
	{
		var result = new ValidationResult();

		// No validation requirered for stock management as of now, but this is where you would add any future validation logic.

		return result;
	}
}