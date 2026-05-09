using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for AboutView.xaml
/// </summary>
public partial class AboutView : UserControl
{
	public AboutView( AboutPageViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
		DateColumn.HeaderText = viewModel.DateHeader;
		AuthorColumn.HeaderText = viewModel.AuthorHeader;
		CommitTextColumn.HeaderText = viewModel.CommitTextHeader;
	}

	private async void CommitGrid_SelectionChanged( object sender, GridSelectionChangedEventArgs e )
	{
		if ( DataContext is not AboutPageViewModel viewModel )
			return;

		if ( sender is not SfDataGrid grid )
			return;

		if ( grid.SelectedItem is not ReleaseCommitModel commit )
			return;

		await viewModel.ShowCommitDetailCommand.ExecuteAsync( commit );
		grid.SelectedItem = null;
	}
}
