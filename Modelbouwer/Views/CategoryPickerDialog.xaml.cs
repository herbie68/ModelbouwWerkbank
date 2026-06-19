using System.Windows.Threading;

using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.Views;

/// <summary>
/// Interaction logic for CategoryPickerDialog.xaml
/// </summary>
public partial class CategoryPickerDialog : Window
{
	public CategoryPickerDialog( CategoryPickerViewModel vm )
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
			if ( CategoryTreeGrid.View == null )
				return;

			if ( string.IsNullOrWhiteSpace( vm.SearchText ) )
				CategoryTreeGrid.View.Filter = null;
			else
				CategoryTreeGrid.View.Filter = vm.FilterRecords;

			CategoryTreeGrid.View.RefreshFilter();
		};

		// If the ViewModel already had a selected category (LoadAsync may have fired before this dialog
		// subscribed to RequestScrollToSelection), ensure we expand to it once the dialog is loaded.
		Loaded += ( s, e ) =>
		{
			_ = Dispatcher.BeginInvoke( () =>
			{
				if ( vm.SelectedCategory != null )
					ExpandToSelection();
			}, DispatcherPriority.Background );
		};
	}

	private void ExpandToSelection()
	{
		if ( DataContext is not CategoryPickerViewModel vm || vm.SelectedCategory == null )
			return;

		var selected = vm.SelectedCategory;

		_ = Dispatcher.BeginInvoke( () =>
		{
			// Collapse alles eerst
			CategoryTreeGrid.CollapseAllNodes();

			// Expand alleen de parent chain
			ExpandParentChain( selected );

			// Zet geselecteerde item
			CategoryTreeGrid.SelectedItem = selected;

			// Scroll naar geselecteerde item using row/column overloads
			int rowIndex = CategoryTreeGrid.ResolveToRowIndex( selected );
			if ( rowIndex >= 0 )
			{
				CategoryTreeGrid.ScrollInView( new RowColumnIndex( rowIndex, 0 ) );
			}
			// <- let op: CategoryTreeGrid moet van type SfTreeGrid zijn
		}, DispatcherPriority.Background );
	}

	private void ExpandParentChain( CategoryModel node )
	{
		var ancestors = new List<CategoryModel>();
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
			int idx = CategoryTreeGrid.ResolveToRowIndex(ancestor);
			if ( idx >= 0 )
				CategoryTreeGrid.ExpandNode( idx );
		}
	}
}