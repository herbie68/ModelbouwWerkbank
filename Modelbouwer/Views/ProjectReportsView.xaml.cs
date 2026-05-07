namespace Modelbouwer.Views;

public partial class ProjectReportsView : UserControl
{
	public ProjectReportsView( ProjectReportsViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}
}
