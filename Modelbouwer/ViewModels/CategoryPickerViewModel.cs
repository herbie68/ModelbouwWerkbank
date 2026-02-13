using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.ViewModels;

public partial class CategoryPickerViewModel : ObservableObject
{
	private readonly ICategoryService _categoryService;

	public ObservableCollection<CategoryModel> CategoryTree { get; } = [ ];

	[ObservableProperty]
	private CategoryModel? selectedCategory;

	[ObservableProperty]
	private string? searchText;

	public IRelayCommand OkCommand { get; }
	public IRelayCommand CancelCommand { get; }
	public IRelayCommand ExpandAllCommand { get; }
	public IRelayCommand CollapseAllCommand { get; }

	public event Action<bool?>? CloseRequested;

	public event Action? RequestScrollToSelection;

	public CategoryPickerViewModel(
		ICategoryService categoryService,
		CategoryModel? currentSelection = null )
	{
		_categoryService = categoryService;

		OkCommand = new RelayCommand( OnOk );
		CancelCommand = new RelayCommand( OnCancel );
		ExpandAllCommand = new RelayCommand<object>( OnExpandAll );
		CollapseAllCommand = new RelayCommand<object>( OnCollapseAll );

		_ = LoadAsync( currentSelection );
	}

	private async Task LoadAsync( CategoryModel? currentSelection )
	{
		var flat = await _categoryService.GetAllCategorysAsync();

		var tree = BuildTree(flat);

		CategoryTree.Clear();
		foreach ( var root in tree )
			CategoryTree.Add( root );

		SelectedCategory = currentSelection;

		RequestScrollToSelection?.Invoke();
	}

	private void OnOk() => CloseRequested?.Invoke( true );

	private void OnCancel() => CloseRequested?.Invoke( false );

	private void OnExpandAll( object? parameter )
		=> ( parameter as SfTreeGrid )?.ExpandAllNodes();

	private void OnCollapseAll( object? parameter )
		=> ( parameter as SfTreeGrid )?.CollapseAllNodes();

	// Same tree builder as jouw CategoryPageVM
	private ObservableCollection<CategoryModel> BuildTree( IEnumerable<CategoryModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.CategoryId);

		foreach ( var c in lookup.Values )
			c.Children.Clear();

		foreach ( var category in lookup.Values )
		{
			if ( category.ParentId != null &&
				lookup.TryGetValue( category.ParentId.Value, out var parent ) )
			{
				parent.Children.Add( category );
			}
		}

		return new ObservableCollection<CategoryModel>(
			lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}
}

