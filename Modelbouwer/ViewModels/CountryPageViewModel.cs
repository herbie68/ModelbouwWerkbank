using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class CountryPageViewModel : ObservableObject
{
	// Collection of countries
	[ObservableProperty]
	private ObservableCollection<CountryModel> countries = new(DBCommands.GetCountryList());

	// Collection of currencies
	[ObservableProperty]
	private ObservableCollection<CurrencyModel> currencies = new(DBCommands.GetCurrencyList());

	// Currently selected country
	[ObservableProperty]
	private CountryModel? selectedCountry;

	[RelayCommand]
	private void RefreshData()
	{
		Countries = new( DBCommands.GetCountryList() );
		Currencies = new( DBCommands.GetCurrencyList() );
	}
}
