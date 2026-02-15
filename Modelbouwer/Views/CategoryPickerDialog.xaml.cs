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
			ExpandToSelection( CategoryTreeGrid );
		};
	}

	private void ExpandToSelection( SfTreeGrid treeGrid )
	{
		if ( DataContext is not CategoryPickerViewModel vm || vm.SelectedCategory == null )
			return;

		var selected = vm.SelectedCategory;

		int rowIndex = treeGrid.ResolveToRowIndex(selected);

		treeGrid.ExpandAllNodes();

		treeGrid.SelectedItem = selected;

		if ( rowIndex > 0 )
		{
			treeGrid.ScrollInView( new RowColumnIndex( rowIndex, 0 ) );
		}
	}
}
