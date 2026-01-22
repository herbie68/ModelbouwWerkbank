using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Windows.Shared;

namespace Modelbouwer.ViewModels;

public class CategoryPageViewModel : EntityPageViewModel<CategoryModel>, INotifyPropertyChanged
{
	private readonly ICategoryService _dataService;

	// Collections
	private ObservableCollection<CategoryModel> _fullTree = [];
	public ObservableCollection<CategoryModel> Categories { get; } = [ ];
	public ObservableCollection<CategoryModel> CategoryTree { get; } = [ ];

	// Commands
	public IRelayCommand AddCategoryCommand => AddCommand;
	public IAsyncRelayCommand SaveCategoryCommand => SaveCommand;
	public IRelayCommand DeleteCategoryCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand AddSubCategoryCommand { get; }

	private IRelayCommand? _clearSearchCommand;

	private ICommand expandCommand;

	public ICommand ExpandCommand
	{
		get { return expandCommand; }
		set { expandCommand = value; }
	}

	private ICommand collapseCommand;

	public ICommand CollapseCommand
	{
		get { return collapseCommand; }
		set { collapseCommand = value; }
	}


	// Constructor
	public CategoryPageViewModel(
		ICategoryService dataService,
		IEntityValidator<CategoryModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadCurrenciesAsync();
		_ = ReloadCommand.ExecuteAsync( null );

		AddSubCategoryCommand = new RelayCommand(
	AddSubCategory,
	() => SelectedItem != null  // Changed from SelectedCategory
);

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );

	}

	// Override SelectedItem changed om DefaultCategory te zetten
	protected override void OnSelectedItemChanged( CategoryModel? value )
	{
		AddSubCategoryCommand.NotifyCanExecuteChanged();
	}

	// Async categories laden
	private async Task LoadCurrenciesAsync()
	{
		var categoryList = await _dataService.GetAllCategorysAsync();

		Categories.Clear();
		foreach ( var c in categoryList )
			Categories.Add( c );
	}

	// Filtering
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

	// TreeGrid Expand and Collapse execution

	private void ExpandExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid.ExpandAllNodes();
	}

	private void CollapseExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid.CollapseAllNodes();
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
	internal delegate void FilterChanged();
	internal FilterChanged filterChanged;

	private string searchText = string.Empty;
	public string SearchText
	{
		get => searchText;
		set
		{
			if ( searchText != value )
			{
				searchText = value;
				RaisePropertyChanged();

				filterChanged?.Invoke();
			}
		}
	}

	public bool FilterRecords( object o )
	{
		if ( o is not CategoryModel category )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		// 1️⃣ Check current node
		if ( category.CategoryName?
			.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
			return true;

		// 2️⃣ Check children (important!)
		return HasMatchingChild( category );
	}

	private bool HasMatchingChild( CategoryModel parent )
	{
		if ( parent.Children == null || parent.Children.Count == 0 )
			return false;

		foreach ( var child in parent.Children )
		{
			// Child matches
			if ( child.CategoryName?
				.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
				return true;

			// Grandchildren match
			if ( HasMatchingChild( child ) )
				return true;
		}

		return false;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void RaisePropertyChanged( [CallerMemberName] string? propertyName = null )
	{
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
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
		{ $"@{DBNames.CategoryFieldNameId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.CategoryFieldNameName}", c.CategoryName?.Trim() }
	};
}
