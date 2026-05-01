using System.Windows.Controls;
using System.Windows.Threading;

using Syncfusion.UI.Xaml.Grid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for StockManagement.xaml
/// </summary>
public partial class StockManagementView : UserControl
{
	private readonly SettingsService _settingsService;
	public StockManagementView( StockManagementPageViewModel viewModel, SettingsService settingsService )
	{
		InitializeComponent();

		_settingsService = settingsService ??
		throw new ArgumentNullException( nameof( settingsService ) );

		DataContext = viewModel;
		Loaded += StockManagementView_Loaded;
	}

	private async void StockManagementView_Loaded( object sender, RoutedEventArgs e )
	{
		try
		{
			if ( DataContext is not StockManagementPageViewModel vm )
				return;

			vm.SaveGridLayoutAction = SaveGridLayout;
			vm.ResetGridLayoutAction = ResetGridLayout;
			vm.RefreshGridFilter = () =>
			{
				dataGrid.View?.RefreshFilter();
				vm.VisibleInventoryCount = dataGrid.View?.Records.Count ?? 0;
			};

			await LoadGridLayoutAsync( dataGrid );
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.Message, Lang.ExportGeneralFailedMessageboxTitle, MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}

	private async void SaveGridLayout()
	{
		try
		{
			var options = new SerializationOptions
			{
				SerializeColumns = true,
				SerializeGrouping = true,
				SerializeFiltering = true,
				SerializeSorting = true
			};

			using var stream = new MemoryStream();

			dataGrid.Serialize( stream, options );

			stream.Position = 0;

			using var reader = new StreamReader(stream);

			string layout = reader.ReadToEnd();

			if ( DataContext is StockManagementPageViewModel vm )
				await vm.SaveGridLayoutAsync( layout );
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.Message, Lang.ExportGeneralFailedMessageboxTitle, MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}

	private async void ResetGridLayout()
	{
		try
		{
			if ( DataContext is not StockManagementPageViewModel vm )
				return;

			await vm.ResetGridLayoutAsync();

			var parent = this.Parent as ContentControl;
			if ( parent != null )
			{
				parent.Content = null;
				parent.Content = new StockManagementView( vm, _settingsService );
			}

			dataGrid.View?.RefreshFilter();

			vm.VisibleInventoryCount = dataGrid.View?.Records.Count ?? 0;
		}
		catch ( Exception ex )
		{
			MessageBox.Show( ex.Message, Lang.ExportGeneralFailedMessageboxTitle, MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}

	private void StockManagementDataGrid_Loaded( object sender, RoutedEventArgs e )
	{
		if ( sender is not SfDataGrid grid )
			return;

		if ( DataContext is not StockManagementPageViewModel vm )
			return;

		_ = grid.Dispatcher.BeginInvoke(
			new Action( () =>
			{
				if ( grid.View == null )
					return;

				grid.View.Filter = vm.FilterInventory;
				grid.View.RefreshFilter();
				vm.VisibleInventoryCount = grid.View.Records.Count;
			} ),
			DispatcherPriority.Loaded
		);
	}

	public async Task LoadGridLayoutAsync( SfDataGrid grid )
	{
		var layout = await _settingsService.GetSettingsAsync("StockManagementGridLayout");

		if ( string.IsNullOrWhiteSpace( layout ) )
			return;

		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(layout));

		grid.Deserialize( stream, new DeserializationOptions
		{
			DeserializeColumns = true,
			DeserializeGrouping = true,
			DeserializeFiltering = true,
			DeserializeSorting = true
		} );
	}
}
