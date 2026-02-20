using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public class UnitPageViewModel : EntityPageViewModel<UnitModel>
{
	private readonly IUnitService _dataService;

	// SelectedUnit als type-safe alias
	public UnitModel? SelectedUnit
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddUnitCommand => AddCommand;
	public IAsyncRelayCommand SaveUnitCommand => SaveCommand;
	public IRelayCommand DeleteUnitCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public UnitPageViewModel(
		IUnitService dataService,
		IEntityValidator<UnitModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadUnitsAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultUnit te zetten
	protected override void OnSelectedItemChanged( UnitModel? oldValue, UnitModel? newValue )
	{
		base.OnSelectedItemChanged( oldValue, newValue );

		if ( newValue == null )
			return;

		OnPropertyChanged( nameof( SelectedUnit ) );
		OnPropertyChanged( nameof( SelectedUnit.UnitName ) );
		OnPropertyChanged( nameof( SelectedUnit.UnitId ) );
	}

	// Async units laden
	private async Task LoadUnitsAsync()
	{
		var unitList = await _dataService.GetAllUnitsAsync();

		Units.Clear();
		foreach ( var c in unitList )
			Units.Add( c );
	}

	// Properties voor UI binding
	public ObservableCollection<UnitModel> Units => Items;
	public int TotalUnitCount => TotalItemCount;
	public int VisibleUnitCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterUnit( object obj )
	{
		if ( obj is not UnitModel unit )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return unit.UnitName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<UnitModel>> LoadItemsAsync() => _dataService.GetAllUnitsAsync();
	protected override Task<int> InsertAsync( UnitModel item ) => _dataService.InsertNewUnitAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( UnitModel item ) => _dataService.UpdateUnitAsync( CreateParameters( item ) );
	protected override async Task DeleteAsync( UnitModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.UnitName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteUnitAsync( item.UnitId );
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

	protected override int GetId( UnitModel item ) => item.UnitId;
	protected override void SetId( UnitModel item, int id ) => item.UnitId = id;

	protected override UnitModel CreateNewItem() => new()
	{
		UnitId = 0,
		UnitName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalUnitCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( UnitModel c ) => new()
	{
		{ $"@{DBNames.UnitFieldNameUnitId}", c.UnitId },
		{ $"@{DBNames.UnitFieldNameUnitName}", c.UnitName?.Trim() }
	};
}
