namespace Modelbouwer.Views;

public partial class StockReceiptView : UserControl
{
	public StockReceiptView( StockReceiptViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}
}