namespace Modelbouwer.Views;

public partial class StockReceiptDateDialog : Window
{
	public StockReceiptDateDialog( StockReceiptDateDialogViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}

	private void SaveClick( object sender, RoutedEventArgs e )
	{
		DialogResult = true;
	}
}
