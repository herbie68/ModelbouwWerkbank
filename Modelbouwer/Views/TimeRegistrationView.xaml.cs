namespace Modelbouwer.Views;

public partial class TimeRegistrationView : UserControl
{
	public TimeRegistrationView( TimeRegistrationViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}

	private void WorktypeTree_SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
	{
		if ( DataContext is TimeRegistrationViewModel viewModel && e.NewValue is WorktypeModel worktype )
			viewModel.SelectWorktype( worktype );
	}

	private void ProductTree_SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
	{
		if ( DataContext is TimeRegistrationViewModel viewModel && e.NewValue is ProductSelectionNodeModel node )
			viewModel.SelectProductNode( node );
	}

	private async void SaveTimeEntriesButton_Click( object sender, RoutedEventArgs e )
	{
		try
		{
			Keyboard.ClearFocus();
			await Dispatcher.InvokeAsync( () => { }, System.Windows.Threading.DispatcherPriority.Background );

			if ( DataContext is TimeRegistrationViewModel viewModel )
				await viewModel.SaveTimeEntriesFromViewAsync();
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.Message, Lang.ExportGeneralFailedMessageboxTitle, MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}
}
