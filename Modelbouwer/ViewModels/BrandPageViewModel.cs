using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public class BrandPageViewModel : EntityPageViewModel<BrandModel>
{
	private readonly IBrandService _dataService;

	// SelectedBrand als type-safe alias
	public BrandModel? SelectedBrand
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddBrandCommand => AddCommand;
	public IAsyncRelayCommand SaveBrandCommand => SaveCommand;
	public IRelayCommand DeleteBrandCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public BrandPageViewModel(
		IBrandService dataService,
		IEntityValidator<BrandModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadBrandsAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultBrand te zetten
	protected override void OnSelectedItemChanged( BrandModel? oldValue, BrandModel? newValue )
	{
		if ( newValue == null )
			return;
		base.OnSelectedItemChanged( oldValue, newValue );

		OnPropertyChanged( nameof( SelectedBrand ) );
		OnPropertyChanged( nameof( SelectedBrand.BrandName ) );
		OnPropertyChanged( nameof( SelectedBrand.BrandId ) );
	}

	// Async brands laden
	private async Task LoadBrandsAsync()
	{
		var brandList = await _dataService.GetAllBrandsAsync();

		Brands.Clear();
		foreach ( var c in brandList )
			Brands.Add( c );
	}

	// Properties voor UI binding
	public ObservableCollection<BrandModel> Brands => Items;
	public int TotalBrandCount => TotalItemCount;
	public int VisibleBrandCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterBrand( object obj )
	{
		if ( obj is not BrandModel brand )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return brand.BrandName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<BrandModel>> LoadItemsAsync() => _dataService.GetAllBrandsAsync();
	protected override Task<int> InsertAsync( BrandModel item ) => _dataService.InsertNewBrandAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( BrandModel item ) => _dataService.UpdateBrandAsync( CreateParameters( item ) );
	protected override async Task DeleteAsync( BrandModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.BrandName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteBrandAsync( item.BrandId );
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

	protected override int GetId( BrandModel item ) => item.BrandId;
	protected override void SetId( BrandModel item, int id ) => item.BrandId = id;

	protected override BrandModel CreateNewItem() => new()
	{
		BrandId = 0,
		BrandName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalBrandCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( BrandModel c ) => new()
	{
		{ $"@{DBNames.BrandFieldNameId}", c.BrandId },
		{ $"@{DBNames.BrandFieldNameName}", c.BrandName?.Trim() }
	};
}
