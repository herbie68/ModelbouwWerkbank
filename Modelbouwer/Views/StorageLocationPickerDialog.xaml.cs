using System.Windows.Threading;

using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for StorageLocationPickerDialog.xaml
/// </summary>
public partial class StorageLocationPickerDialog : Window
{
	public StorageLocationPickerDialog( StorageLocationPickerViewModel vm )
	{
		InitializeComponent();
		DataContext = vm;

		vm.CloseRequested += result =>
		{
			DialogResult = result;
			Close();
		};

		vm.RequestScrollToSelection += () =>
		{
			ExpandToSelection();
		};

		vm._filterChanged += () =>
		{
			if ( StorageLocationTreeGrid.View == null )
				return;

			if ( string.IsNullOrWhiteSpace( vm.SearchText ) )
				StorageLocationTreeGrid.View.Filter = null;
			else
				StorageLocationTreeGrid.View.Filter = vm.FilterRecords;

			StorageLocationTreeGrid.View.RefreshFilter();
		};

		// If the ViewModel already had a selected storagelocation (LoadAsync may have fired before this dialog
		// subscribed to RequestScrollToSelection), ensure we expand to it once the dialog is loaded.
		Loaded += ( s, e ) =>
		{
			_ = Dispatcher.BeginInvoke( () =>
			{
				if ( vm.SelectedStorageLocation != null )
					ExpandToSelection();
			}, DispatcherPriority.Background );
		};
	}

	private void ExpandToSelection()
	{
		if ( DataContext is not StorageLocationPickerViewModel vm || vm.SelectedStorageLocation == null )
			return;

		var selected = vm.SelectedStorageLocation;

		_ = Dispatcher.BeginInvoke( () =>
		{
			// Collapse alles eerst
			StorageLocationTreeGrid.CollapseAllNodes();

			// Expand alleen de parent chain
			ExpandParentChain( selected );

			// Zet geselecteerde item
			StorageLocationTreeGrid.SelectedItem = selected;

			// Scroll naar geselecteerde item using row/column overloads
			int rowIndex = StorageLocationTreeGrid.ResolveToRowIndex( selected );
			if ( rowIndex >= 0 )
			{
				StorageLocationTreeGrid.ScrollInView( new RowColumnIndex( rowIndex, 0 ) );
			}
			// <- let op: StorageLocationTreeGrid moet van type SfTreeGrid zijn
		}, DispatcherPriority.Background );
	}

	private void ExpandParentChain( StorageLocationModel node )
	{
		var ancestors = new List<StorageLocationModel>();
		var current = node.Parent;
		while ( current != null )
		{
			ancestors.Add( current );
			current = current.Parent;
		}

		// Expand from root down to immediate parent
		ancestors.Reverse();

		foreach ( var ancestor in ancestors )
		{
			int idx = StorageLocationTreeGrid.ResolveToRowIndex(ancestor);
			if ( idx >= 0 )
				StorageLocationTreeGrid.ExpandNode( idx );
		}
	}
}
