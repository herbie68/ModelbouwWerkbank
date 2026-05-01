namespace Modelbouwer.Views;

public partial class StockOrderView : UserControl
{
	public StockOrderView( StockOrderViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}
}
