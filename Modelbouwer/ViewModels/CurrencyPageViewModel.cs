using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class CurrencyPageViewModel : EntityPageViewModel<CurrencyModel>
{
	private readonly ICurrencyService _dataService;

	// Collections
	public ObservableCollection<CurrencyModel> Currencies { get; } = [ ];

	// SelectedCurrency als type-safe alias
	public CurrencyModel? SelectedCurrency
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddCurrencyCommand => AddCommand;
	public IAsyncRelayCommand SaveCurrencyCommand => SaveCommand;
	public IRelayCommand DeleteCurrencyCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public CurrencyPageViewModel(
		ICurrencyService dataService,
		IEntityValidator<CurrencyModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadCurrenciesAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultCurrency te zetten
	protected override void OnSelectedItemChanged( CurrencyModel? oldValue, CurrencyModel? newValue )
	{
		if ( newValue == null )
			return;

		base.OnSelectedItemChanged( oldValue, newValue );

		OnPropertyChanged( nameof( SelectedCurrency ) );
		OnPropertyChanged( nameof( SelectedCurrency.CurrencyCode ) );
		OnPropertyChanged( nameof( SelectedCurrency.CurrencyName ) );
		OnPropertyChanged( nameof( SelectedCurrency.CurrencyId ) );
		OnPropertyChanged( nameof( SelectedCurrency.CurrencySymbol ) );
		OnPropertyChanged( nameof( SelectedCurrency.CurrencyConversionRate ) );
	}

	// Async currencies laden
	private async Task LoadCurrenciesAsync()
	{
		var currencyList = await _dataService.GetAllCurrenciesAsync();

		Currencies.Clear();
		foreach ( var c in currencyList )
			Currencies.Add( c );
	}

	// Properties voor UI binding
	public ObservableCollection<CurrencyModel> Countries => Items;
	public int TotalCurrencyCount => TotalItemCount;
	public int VisibleCurrencyCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterCurrency( object obj )
	{
		if ( obj is not CurrencyModel currency )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return currency.CurrencyCode?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true
			|| currency.CurrencyName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true
			|| currency.CurrencySymbol?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<CurrencyModel>> LoadItemsAsync() => _dataService.GetAllCurrenciesAsync();
	protected override Task<int> InsertAsync( CurrencyModel item ) => _dataService.InsertNewCurrencyAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( CurrencyModel item ) => _dataService.UpdateCurrencyAsync( CreateParameters( item ) );
	protected override async Task DeleteAsync( CurrencyModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.CurrencyName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteCurrencyAsync( item.CurrencyId );
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

	protected override int GetId( CurrencyModel item ) => item.CurrencyId;
	protected override void SetId( CurrencyModel item, int id ) => item.CurrencyId = id;

	protected override CurrencyModel CreateNewItem() => new()
	{
		CurrencyId = 0,
		CurrencyCode = string.Empty,
		CurrencyName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalCurrencyCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( CurrencyModel c ) => new()
	{
		{ $"@{DBNames.CurrencyFieldNameId}", c.CurrencyId },
		{ $"@{DBNames.CurrencyFieldNameCode}", c.CurrencyCode?.Trim().ToUpper() },
		{ $"@{DBNames.CurrencyFieldNameName}", c.CurrencyName?.Trim() },
		{ $"@{DBNames.CurrencyFieldNameRate}", c.CurrencyConversionRate },
		{ $"@{DBNames.CurrencyFieldNameSymbol}", c.CurrencySymbol }
	};
}