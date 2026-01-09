using CommunityToolkit.Mvvm.Input;

using Modelbouwer.Interfaces;
using Modelbouwer.Services;

namespace Modelbouwer.ViewModels;

public partial class CountryPageViewModel : EntityPageViewModel<CountryModel>
{
	private readonly ICountryService _countryService;
	private readonly CurrencyService _currencyService;

	// Collections
	public ObservableCollection<CurrencyModel> Currencies { get; } = new();

	// SelectedCountry als type-safe alias
	public CountryModel? SelectedCountry
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddCountryCommand => AddCommand;
	public IAsyncRelayCommand SaveCountryCommand => SaveCommand;
	public IRelayCommand DeleteCountryCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public CountryPageViewModel(
		ICountryService countryService,
		CurrencyService currencyService,
		IEntityValidator<CountryModel> validator
	) : base( validator )
	{
		_countryService = countryService;
		_currencyService = currencyService;

		_ = LoadCurrenciesAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultCurrency te zetten
	protected override void OnSelectedItemChanged( CountryModel? value )
	{
		if ( value == null ) return;

		if ( Currencies.Any() )
		{
			value.DefaultCurrency = Currencies
				.FirstOrDefault( c => c.CurrencyId == value.CountryCurrencyId )
				?? Currencies.First();
		}

		OnPropertyChanged( nameof( SelectedCountry ) );
		OnPropertyChanged( nameof( SelectedCountry.CountryCode ) );
		OnPropertyChanged( nameof( SelectedCountry.CountryName ) );
		OnPropertyChanged( nameof( SelectedCountry.CountryCurrencyId ) );
		OnPropertyChanged( nameof( SelectedCountry.CountryCurrencySymbol ) );
		OnPropertyChanged( nameof( SelectedCountry.DefaultCurrency ) );
	}

	// Async currencies laden
	private async Task LoadCurrenciesAsync()
	{
		var currencyList = await _currencyService.GetAllCurrenciesAsync();

		Currencies.Clear();
		foreach ( var c in currencyList )
			Currencies.Add( c );

		// DefaultCurrency voor geselecteerd item instellen
		if ( SelectedCountry != null )
		{
			SelectedCountry.DefaultCurrency = Currencies
				.FirstOrDefault( c => c.CurrencyId == SelectedCountry.CountryCurrencyId )
				?? Currencies.FirstOrDefault();
		}
	}

	// Properties voor UI binding
	public ObservableCollection<CountryModel> Countries => Items;
	public int TotalCountryCount => TotalItemCount;
	public int VisibleCountryCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterCountry( object obj )
	{
		if ( obj is not CountryModel country )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return country.CountryCode?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true
			|| country.CountryName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true
			|| country.CountryCurrencySymbol?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<CountryModel>> LoadItemsAsync() => _countryService.GetAllCountriesAsync();
	protected override Task<int> InsertAsync( CountryModel item ) => _countryService.InsertNewCountryAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( CountryModel item ) => _countryService.UpdateCountryAsync( CreateParameters( item ) );
	protected override Task DeleteAsync( CountryModel item )
	{
		if ( item == null )
			return Task.CompletedTask;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.CountryName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return Task.CompletedTask;

		return _countryService.DeleteCountryAsync( item.CountryId );
	}

	protected override int GetId( CountryModel item ) => item.CountryId;
	protected override void SetId( CountryModel item, int id ) => item.CountryId = id;

	protected override CountryModel CreateNewItem() => new()
	{
		CountryId = 0,
		CountryCode = string.Empty,
		CountryName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalCountryCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( CountryModel c ) => new()
	{
		{ $"@{DBNames.CountryFieldNameId}", c.CountryId },
		{ $"@{DBNames.CountryFieldNameCode}", c.CountryCode?.Trim().ToUpper() },
		{ $"@{DBNames.CountryFieldNameName}", c.CountryName?.Trim() },
		{ $"@{DBNames.CountryFieldNameCurrencyId}", c.CountryCurrencyId },
		{ $"@{DBNames.CountryFieldNameCurrencySymbol}", c.CountryCurrencySymbol }
	};
}
