using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Windows.Shared;

namespace Modelbouwer.ViewModels;

public partial class CategoryPageViewModel : EntityPageViewModel<CategoryModel>
{
	private readonly ICategoryService _dataService;

	// Collections
	public ObservableCollection<CategoryModel> Categories { get; } = [ ];
	public ObservableCollection<CategoryModel> CategoryTree { get; } = [ ];

	// Commands
	public IRelayCommand AddCategoryCommand => AddCommand;
	public IAsyncRelayCommand SaveCategoryCommand => SaveCommand;
	public IRelayCommand DeleteCategoryCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand AddSubCategoryCommand { get; }

	private IRelayCommand? _clearSearchCommand;

	private ICommand? _expandCommand;

	public ICommand? ExpandCommand
	{
		get { return _expandCommand; }
		set { _expandCommand = value; }
	}

	private ICommand? _collapseCommand;

	public ICommand? CollapseCommand
	{
		get { return _collapseCommand; }
		set { _collapseCommand = value; }
	}

	internal delegate void FilterChanged();
	internal FilterChanged? _filterChanged;

	// Constructor
	public CategoryPageViewModel( ICategoryService dataService, IEntityValidator<CategoryModel> validator ) : base( validator )
	{
		_dataService = dataService;

		ObserveBackgroundTask( ReloadAsync() );

		AddSubCategoryCommand = new RelayCommand( AddSubCategory, () => SelectedItem != null );

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );

	}

	protected override void OnPropertyChanged( System.ComponentModel.PropertyChangedEventArgs e )
	{
		base.OnPropertyChanged( e );

		if ( e.PropertyName == nameof( SearchText ) )
		{
			_filterChanged?.Invoke();
		}
	}

	// Override SelectedItem changed om DefaultCategory te zetten
	protected override void OnSelectedItemChanged( CategoryModel? oldValue, CategoryModel? newValue )
	{
		base.OnSelectedItemChanged( oldValue, newValue );

		AddSubCategoryCommand.NotifyCanExecuteChanged();
	}

	public bool FilterCategory( object obj )
	{
		if ( obj is not CategoryModel category )
			return false;

		if ( string.IsNullOrWhiteSpace( base.SearchText ) )
			return true;

		return category.CategoryName?.Contains( base.SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<CategoryModel>> LoadItemsAsync() => _dataService.GetAllCategorysAsync();
	protected override Task<int> InsertAsync( CategoryModel item ) => _dataService.InsertNewCategoryAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( CategoryModel item ) => _dataService.UpdateCategoryAsync( UpdateParameters( item ) );
	protected override async Task DeleteAsync( CategoryModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.CategoryName}' {Lang.toolbarButtonActionDeleteMessageQuestionCategorySuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteCategoryAsync( item.CategoryId );
		}
		catch ( EntityInUseException ex )
		{
			MessageBox.Show(
				ex.Message,
				Lang.generalMessageboxWarningTitle,
				MessageBoxButton.OK,
				MessageBoxImage.Information
			);
		}
	}

	private void ExpandExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid?.ExpandAllNodes();
	}

	private void CollapseExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid?.CollapseAllNodes();
	}


	public ObservableCollection<CategoryModel> BuildTree( IEnumerable<CategoryModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.CategoryId);

		// Make sure Children are not doubled
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

		return new ObservableCollection<CategoryModel>( lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}

	protected override int GetId( CategoryModel item ) => item.CategoryId;
	protected override void SetId( CategoryModel item, int id ) => item.CategoryId = id;

	protected override CategoryModel CreateNewItem() => new()
	{
		CategoryId = 0,
		ParentId = 0,
		CategoryName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		CategoryTree.Clear();

		var tree = BuildTree( Items );
		foreach ( var root in tree )
			CategoryTree.Add( root );

		OnPropertyChanged( nameof( CategoryTree ) );
	}

	private void AddSubCategory()
	{
		if ( SelectedItem == null )  // Changed from SelectedCategory
			return;

		var newCategory = new CategoryModel
		{
			CategoryName = string.Empty,
			ParentId = SelectedItem.CategoryId  // Changed from SelectedCategory
		};

		SelectedItem.Children.Add( newCategory );  // Changed from SelectedCategory
		SelectedItem = newCategory;  // Changed from SelectedCategory
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
		if ( parent.Children == null || parent.Children.Count == 0 )
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
	#endregion

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( CategoryModel c ) => new()
	{
		{ $"@{DBNames.CategoryFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.CategoryFieldNameName}", c.CategoryName?.Trim() }
	};

	private static Dictionary<string, object?> UpdateParameters( CategoryModel c ) => new()
	{
		{ $"@{DBNames.CategoryFieldNameId}", c.CategoryId == 0 ? null : c.CategoryId },
		{ $"@{DBNames.CategoryFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.CategoryFieldNameName}", c.CategoryName?.Trim() }
	};
}
