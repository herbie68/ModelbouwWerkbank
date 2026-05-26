using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.ViewModels;

public partial class CategoryPickerViewModel : AsyncObservableObject
{
	private readonly ICategoryService _categoryService;

	public ObservableCollection<CategoryModel> CategoryTree { get; } = [ ];

	[ObservableProperty]
	private CategoryModel? _selectedCategory;

	[ObservableProperty]
	private string? _searchText;

	public IRelayCommand OkCommand { get; }
	public IRelayCommand CancelCommand { get; }

	private IRelayCommand? _clearSearchCommand;
	public IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand ExpandAllCommand { get; }
	public IRelayCommand CollapseAllCommand { get; }

	public event Action<bool?>? CloseRequested;

	public event Action? RequestScrollToSelection;

	internal delegate void FilterChanged();
	internal FilterChanged? _filterChanged;

	public CategoryPickerViewModel(
		ICategoryService categoryService,
		CategoryModel? currentSelection = null )
	{
		_categoryService = categoryService;

		OkCommand = new RelayCommand( OnOk );
		CancelCommand = new RelayCommand( OnCancel );
		ExpandAllCommand = new RelayCommand<object>( OnExpandAll );
		CollapseAllCommand = new RelayCommand<object>( OnCollapseAll );

		ObserveBackgroundTask( LoadAsync( currentSelection ) );
	}

	private async Task LoadAsync( CategoryModel? currentSelection )
	{
		var flat = await _categoryService.GetAllCategorysAsync();

		var tree = BuildTree(flat);

		CategoryTree.Clear();
		foreach ( var root in tree )
			CategoryTree.Add( root );

		// Map the incoming selection (which may be an instance from a different view model)
		// to the instance loaded into this view model's tree so UI selection works.
		if ( currentSelection != null )
		{
			// flat contains the instances used to build the tree, so find the one with the same id
			var mapped = flat.FirstOrDefault( f => f.CategoryId == currentSelection.CategoryId );
			SelectedCategory = mapped;
		}
		else
		{
			SelectedCategory = null;
		}

		RequestScrollToSelection?.Invoke();
	}

	private void OnOk() => CloseRequested?.Invoke( true );

	private void OnCancel() => CloseRequested?.Invoke( false );

	private void OnExpandAll( object? parameter )
		=> ( parameter as SfTreeGrid )?.ExpandAllNodes();

	private void OnCollapseAll( object? parameter )
		=> ( parameter as SfTreeGrid )?.CollapseAllNodes();

	private static ObservableCollection<CategoryModel> BuildTree( IEnumerable<CategoryModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.CategoryId);

		foreach ( var c in lookup.Values )
		{
			c.Children.Clear();
			c.Parent = null;
		}

		foreach ( var category in lookup.Values )
		{
			if ( category.ParentId != null &&
				lookup.TryGetValue( category.ParentId.Value, out var parent ) )
			{
				category.Parent = parent;
				parent.Children.Add( category );
			}
		}

		return new ObservableCollection<CategoryModel>(
			lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}

	#region Filtering
	public bool FilterRecords( object o )
	{
		if ( o is not CategoryModel category )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		if ( category.CategoryName?
			.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
			return true;

		return HasMatchingChild( category );
	}

	private bool HasMatchingChild( CategoryModel parent )
	{
		if ( parent.Children == null || parent.Children.Count == 0 || SearchText == null )
			return false;

		foreach ( var child in parent.Children )
		{
			if ( child.CategoryName?
				.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
				return true;

			if ( HasMatchingChild( child ) )
				return true;
		}

		return false;
	}

	partial void OnSearchTextChanged( string? value )
	{
		_filterChanged?.Invoke();
	}
	#endregion
}

