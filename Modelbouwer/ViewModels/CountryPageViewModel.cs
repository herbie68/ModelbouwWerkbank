namespace Modelbouwer.ViewModels;

public partial class CountryPageViewModel : ObservableObject
{
	private readonly CountryService _countryService;

	// Collection of countries
	public ObservableCollection<CountryModel> Countries { get; }

	// Collection of currencies
	[ObservableProperty]
	private ObservableCollection<CurrencyModel> _currencies = new(DBCommands.GetCurrencyList());

	private CountryModel? _selectedCountry;
	public CountryModel? SelectedCountry
	{
		get => _selectedCountry;
		set
		{
			if ( SetProperty( ref _selectedCountry, value ) && value != null )
			{
				_ = CheckIfCountryIsUsedAsync( value.CountryId );
			}
		}
	}

	// Search text for filtering
	[ObservableProperty]
	private string searchText = string.Empty;

	[ObservableProperty]
	private int _visibleCountryCount;

	public int TotalCountryCount => Countries.Count;

	#region for Country deletion check
	private bool _countryUsed;
	public bool CountryUsed
	{
		get => _countryUsed;
		set
		{
			SetProperty( ref _countryUsed, value );
			OnPropertyChanged( nameof( CanDeleteCountry ) );
			OnPropertyChanged( nameof( DeleteToolTipKey ) );
		}
	}

	public string DeleteToolTipKey =>
	CountryUsed
		? nameof( Language.toolbarButtonActionCanNotDelete )
		: nameof( Language.toolbarButtonActionDelete );

	public bool CanDeleteCountry => !CountryUsed;
	#endregion

	[RelayCommand]
	private void AddCountry()
	{
		var newCountry = new CountryModel
		{
			CountryId = 0, // 0 = new country, not saved yet
			CountryCode = string.Empty,
			CountryName = string.Empty,
			CountryCurrencySymbol = null
		};

		Countries.Add( newCountry );
		SelectedCountry = newCountry;
	}

	public CountryPageViewModel()
	{
		if ( Countries.Any() )
			SelectedCountry = Countries.First();
	}

	/// <summary>
	/// Refresh data from database
	/// </summary>
	[RelayCommand]
	private async Task RefreshDataAsync()
	{
		if ( _countryService == null )
			return;

		var list = await _countryService.GetAllCountriesAsync();

		Countries.Clear();
		foreach ( var c in list )
			Countries.Add( c );

		Currencies.Clear();
		foreach ( var c in DBCommands.GetCurrencyList() )
			Currencies.Add( c );

		if ( Countries.Any() )
			SelectedCountry = Countries.First();
	}

	partial void OnSearchTextChanged( string value )
	{
		RefreshGridFilter?.Invoke();
	}

	public Action? RefreshGridFilter { get; set; }

	/// <summary>
	/// Clear search box
	/// </summary>
	[RelayCommand]
	private void ClearSearch()
	{
		SearchText = string.Empty;
	}

	/// <summary>
	/// SfDataGrid filter callback
	/// Must be hooked to DataGrid.View.Filter in XAML or codebehind
	/// </summary>
	public bool FilterCountry( object obj )
	{
		if ( obj is not CountryModel country )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		var text = SearchText.ToLower();
		return ( country.CountryCode?.ToLower().Contains( text, StringComparison.CurrentCultureIgnoreCase ) == true )
			|| ( country.CountryName?.ToLower().Contains( text, StringComparison.CurrentCultureIgnoreCase ) == true )
			|| ( country.CountryCurrencySymbol?.ToLower().Contains( text, StringComparison.CurrentCultureIgnoreCase ) == true );
	}

	public CountryPageViewModel( CountryService countryService )
	{
		_countryService = countryService;
		Countries = [];
	}

	public async Task LoadCountriesAsync()
	{
		var countries = await _countryService.GetAllCountriesAsync();

		Countries.Clear();
		foreach ( var country in countries )
		{
			Countries.Add( country );
		}
	}

	#region check if country is used to prevent deletion
	public async Task CheckIfCountryIsUsedAsync( int countryId )
	{
		if ( _countryService == null )
		{
			return;
		}

		CountryUsed = await _countryService.IsCountryUsedAsync( countryId );
	}
	#endregion
}
