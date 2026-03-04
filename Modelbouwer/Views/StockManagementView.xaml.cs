namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for StockManagement.xaml
/// </summary>
public partial class StockManagementView : UserControl
{
	public StockManagementView( StockManagementPageViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}
}
