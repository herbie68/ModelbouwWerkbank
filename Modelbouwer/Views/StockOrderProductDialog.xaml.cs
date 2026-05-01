namespace Modelbouwer.Views;

public partial class StockOrderProductDialog : Window
{
	public StockOrderProductDialog( StockOrderProductDialogViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}

	private void ConfirmClick( object sender, RoutedEventArgs e )
	{
		if ( DataContext is not StockOrderProductDialogViewModel vm )
			return;

		if ( !vm.TryConfirm( out var errorMessage ) )
		{
			MessageBox.Show( errorMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
			return;
		}

		DialogResult = true;
	}
}
