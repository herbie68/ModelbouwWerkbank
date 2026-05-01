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
			errorMessage = "Supplier product name is verplicht.";
			return false;
		}

		if ( Model.Amount <= 0 )
		{
			errorMessage = "Aantal moet groter zijn dan nul.";
			return false;
		}

		if ( Model.UnitPrice <= 0 )
		{
			errorMessage = "Prijs moet groter zijn dan nul.";
			return false;
		}

		errorMessage = null;
		return true;
	}
}
