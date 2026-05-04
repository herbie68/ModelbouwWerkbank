namespace Modelbouwer.ViewModels;

public partial class StockOrderProductDialogViewModel : ObservableObject
{
	public StockOrderProductDialogModel Model { get; }

	public StockOrderProductDialogViewModel( StockOrderProductDialogModel model )
	{
		Model = model;
	}

	public bool TryConfirm( out string? errorMessage )
	{
		if ( string.IsNullOrWhiteSpace( Model.SupplierProductName ) )
		{
			errorMessage = Lang.StockOrderProductDialogSupplierProductNameRequired;
			return false;
		}

		if ( Model.Amount <= 0 )
		{
			errorMessage = Lang.StockOrderProductDialogAmountRequired;
			return false;
		}

		if ( Model.UnitPrice <= 0 )
		{
			errorMessage = Lang.StockOrderProductDialogPriceRequired;
			return false;
		}

		errorMessage = null;
		return true;
	}
}
