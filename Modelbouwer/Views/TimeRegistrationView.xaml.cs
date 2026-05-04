namespace Modelbouwer.Views;

public partial class TimeRegistrationView : UserControl
{
	public TimeRegistrationView( TimeRegistrationViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}
}
