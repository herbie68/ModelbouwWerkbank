namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : UserControl
{
	public SettingsView( SettingsPageViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}
}
