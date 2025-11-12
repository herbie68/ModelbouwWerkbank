namespace Modelbouwer.ViewModels;
public partial class CountryViewModel : ObservableObject
{
	[ObservableProperty]
	public int countryCurrencyId;

	[ObservableProperty]
	public int countryId;

	[ObservableProperty]
	public string? countryCode;

	[ObservableProperty]
	public string? countryCurrencySymbol;

	[ObservableProperty]
	public string? countryName;

	private ObservableCollection<CountryModel>? _country;

	public ObservableCollection<CountryModel> Country
	{
		get => _country;
		set
		{
			if ( _country != value )
			{
				_country = value;
				OnPropertyChanged( nameof( Country ) );
			}
		}
	}

	public CountryViewModel()
	{
		Country = new ObservableCollection<CountryModel>( DBCommands.GetCountryList() );
	}
}
