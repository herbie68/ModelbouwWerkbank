using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Modelbouwer.Interfaces;
using Modelbouwer.Models;
using Modelbouwer.Services;

namespace Modelbouwer.ViewModels;

public partial class CountryPageViewModel : EntityPageViewModel<CountryModel>
{
	private readonly ICountryService _countryService;
	private readonly CurrencyService _currencyService;

	public IRelayCommand AddCountryCommand => AddCommand;
	public IRelayCommand DeleteCountryCommand => DeleteCommand;
	public IAsyncRelayCommand SaveCountryCommand => SaveCommand;

	public ObservableCollection<CurrencyModel> Currencies { get; } = [ ];

	public CountryModel? SelectedCountry { get => SelectedItem; set => SelectedItem = value; }

	protected override void OnSelectedItemChanged( CountryModel? value )
	{
		OnPropertyChanged( nameof( SelectedCountry ) );


		if ( value == null )
			return;

		if ( Currencies.Any() )
		{
			value.DefaultCurrency = Currencies.FirstOrDefault( c => c.CurrencyId == value.CountryCurrencyId ) ?? Currencies.First();
		}
	}

	public CountryPageViewModel( ICountryService countryService, CurrencyService currencyService, IEntityValidator<CountryModel> validator ) : base( validator )
	{
		_countryService = countryService;
		_currencyService = currencyService;

		LoadCurrencies();

		_ = ReloadCommand.ExecuteAsync( null );
	}

	private async void LoadCurrencies()
	{
		var currencyList = await _currencyService.GetAllCurrenciesAsync();

		Currencies.Clear();

		foreach ( var c in currencyList ) 
			Currencies.Add( c );

		if ( SelectedCountry != null )
			SelectedCountry.DefaultCurrency = Currencies
				.FirstOrDefault( c => c.CurrencyId == SelectedCountry.CountryCurrencyId );
	}

	public int TotalCountryCount => TotalItemCount;

	public ObservableCollection<CountryModel> Countries => Items;

	public int VisibleCountryCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	protected override Task<List<CountryModel>> LoadItemsAsync()
		=> _countryService.GetAllCountriesAsync();

	protected override Task<int> InsertAsync( CountryModel item )
		=> _countryService.InsertNewCountryAsync( CreateParameters( item ) );

	protected override Task UpdateAsync( CountryModel item )
		=> _countryService.UpdateCountryAsync( CreateParameters( item ) );

	//protected override Task DeleteAsync( CountryModel item )
	//	=> _countryService.DeleteCountryAsync( item.CountryId );

	protected override int GetId( CountryModel item )
		=> item.CountryId;

	protected override void SetId( CountryModel item, int id )
		=> item.CountryId = id;

	protected override CountryModel CreateNewItem()
		=> new()
	{
		CountryId = 0,
		CountryCode = string.Empty,
		CountryName = string.Empty
	};

	protected override async Task DeleteAsync( CountryModel item )
	{
		if ( item == null )
			return;

		// Optioneel: bevestiging vragen
		var result = MessageBox.Show(
		$"Weet je zeker dat je '{item.CountryName}' wilt verwijderen?",
		"Bevestiging verwijderen",
		MessageBoxButton.YesNo,
		MessageBoxImage.Warning
	);

		if ( result != MessageBoxResult.Yes )
			return;

		// Verwijder het item via service
		await _countryService.DeleteCountryAsync( item.CountryId );

		// Haal de lijst opnieuw op
		var countries = await LoadItemsAsync();
		Items.Clear();
		foreach ( var c in countries )
			Items.Add( c );

		// Selecteer een nieuw item (of null)
		SelectedItem = Items.FirstOrDefault();
	}

	public bool FilterCountry( object obj )
	{
		if ( obj is not CountryModel country )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return
			country.CountryCode?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true
			|| country.CountryName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true
			|| country.CountryCurrencySymbol?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalCountryCount ) );
	}

	private static Dictionary<string, object?> CreateParameters( CountryModel c )
	=> new()
	{
		{ $"@{DBNames.CountryFieldNameId}", c.CountryId },
		{ $"@{DBNames.CountryFieldNameCode}", c.CountryCode?.Trim().ToUpper() },
		{ $"@{DBNames.CountryFieldNameName}", c.CountryName?.Trim() },
		{ $"@{DBNames.CountryFieldNameCurrencyId}", c.CountryCurrencyId },
		{ $"@{DBNames.CountryFieldNameCurrencySymbol}", c.CountryCurrencySymbol }
	};
}