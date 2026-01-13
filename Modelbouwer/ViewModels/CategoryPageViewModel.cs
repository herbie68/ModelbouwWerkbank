using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public class CategoryPageViewModel : EntityPageViewModel<CategoryModel>
{
	private readonly ICategoryService _dataService;

	// Collections
	public ObservableCollection<CategoryModel> Categorys { get; } = [ ];

	// SelectedCategory als type-safe alias
	public CategoryModel? SelectedCategory
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddCategoryCommand => AddCommand;
	public IAsyncRelayCommand SaveCategoryCommand => SaveCommand;
	public IRelayCommand DeleteCategoryCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public CategoryPageViewModel(
		ICategoryService dataService,
		IEntityValidator<CategoryModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadCurrenciesAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultCategory te zetten
	protected override void OnSelectedItemChanged( CategoryModel? value )
	{
		if ( value == null )
			return;

		OnPropertyChanged( nameof( SelectedCategory ) );
		OnPropertyChanged( nameof( SelectedCategory.CategoryName ) );
		OnPropertyChanged( nameof( SelectedCategory.ParentId ) );
		OnPropertyChanged( nameof( SelectedCategory.CategoryId ) );
	}

	// Async currencies laden
	private async Task LoadCurrenciesAsync()
	{
		var categoryList = await _dataService.GetAllCategorysAsync();

		Categorys.Clear();
		foreach ( var c in categoryList )
			Categorys.Add( c );
	}

	// Properties voor UI binding
	public ObservableCollection<CategoryModel> Countries => Items;
	public int TotalCategoryCount => TotalItemCount;
	public int VisibleCategoryCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterCategory( object obj )
	{
		if ( obj is not CategoryModel category )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return category.CategoryName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<CategoryModel>> LoadItemsAsync() => _dataService.GetAllCategorysAsync();
	protected override Task<int> InsertAsync( CategoryModel item ) => _dataService.InsertNewCategoryAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( CategoryModel item ) => _dataService.UpdateCategoryAsync( CreateParameters( item ) );
	protected override async Task DeleteAsync( CategoryModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.CategoryName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
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

	public ObservableCollection<CategoryModel> BuildTree(
	IEnumerable<CategoryModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.CategoryId);

		foreach ( var category in lookup.Values )
		{
			if ( category.ParentId != null &&
				lookup.TryGetValue( category.ParentId.Value, out var parent ) )
			{
				parent.Children.Add( category );
			}
		}

		return new ObservableCollection<CategoryModel>(
			lookup.Values.Where( c => c.ParentId == null ) );
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
		OnPropertyChanged( nameof( TotalCategoryCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( CategoryModel c ) => new()
	{
		{ $"@{DBNames.CategoryFieldNameId}", c.CategoryId },
		{ $"@{DBNames.CategoryFieldNameId}", c.ParentId },
		{ $"@{DBNames.CategoryFieldNameName}", c.CategoryName?.Trim() }
	};
}
