namespace Modelbouwer.ViewModels;

public partial class StockReceiptDateDialogViewModel : ObservableObject
{
	[ObservableProperty] private DateTime? _deliveryDate = DateTime.Today;
}
