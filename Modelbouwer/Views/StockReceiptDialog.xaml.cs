using System.Windows.Controls;

namespace Modelbouwer.Views;

public partial class StockReceiptDialog : Window
{
	public StockReceiptDialog( StockReceiptDialogViewModel viewModel )
	{
		InitializeComponent();
		DataContext = viewModel;
	}

	private void SaveClick( object sender, RoutedEventArgs e )
	{
		if ( DataContext is not StockReceiptDialogViewModel vm )
			return;

		var validationMessage = vm.Validate();
		if ( validationMessage != null )
		{
			MessageBox.Show( validationMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
			return;
		}

		var warning = vm.GetIncompleteCloseWarning();
		if ( warning != null )
		{
			if ( ShowIncompleteCloseWarning( warning ) )
			{
				vm.CompleteWithReceivedAmount();
			}
			else
			{
				vm.LeaveOpen();
			}
		}

		DialogResult = true;
	}

	private bool ShowIncompleteCloseWarning( string warning )
	{
		bool updateOrderedAmount = false;
		Window dialog = new()
		{
			Title = Lang.generalMessageboxWarningTitle,
			Owner = this,
			Width = 520,
			Height = 180,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			ResizeMode = ResizeMode.NoResize
		};

		Grid grid = new() { Margin = new Thickness( 16 ) };
		grid.RowDefinitions.Add( new RowDefinition { Height = new GridLength( 1, GridUnitType.Star ) } );
		grid.RowDefinitions.Add( new RowDefinition { Height = GridLength.Auto } );

		TextBlock message = new()
		{
			Text = warning,
			TextWrapping = TextWrapping.Wrap,
			VerticalAlignment = VerticalAlignment.Center
		};
		grid.Children.Add( message );

		StackPanel buttons = new()
		{
			Orientation = System.Windows.Controls.Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Right,
			Margin = new Thickness( 0, 16, 0, 0 )
		};
		Grid.SetRow( buttons, 1 );

		Button leaveOpenButton = new()
		{
			Content = Lang.StockReceiptLeaveOpenButton,
			Width = 110,
			Margin = new Thickness( 0, 0, 8, 0 ),
			IsCancel = true
		};
		leaveOpenButton.Click += ( _, _ ) =>
		{
			updateOrderedAmount = false;
			dialog.DialogResult = false;
		};

		Button updateButton = new()
		{
			Content = Lang.StockReceiptUpdateButton,
			Width = 110,
			IsDefault = true
		};
		updateButton.Click += ( _, _ ) =>
		{
			updateOrderedAmount = true;
			dialog.DialogResult = true;
		};

		buttons.Children.Add( leaveOpenButton );
		buttons.Children.Add( updateButton );
		grid.Children.Add( buttons );

		dialog.Content = grid;
		dialog.ShowDialog();
		return updateOrderedAmount;
	}
}