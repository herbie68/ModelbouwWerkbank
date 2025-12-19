using System.ComponentModel;

using Modelbouwer.Services;

using Mysqlx.Session;

using MySqlX.XDevAPI.Common;

namespace Modelbouwer.ViewModels;
public partial class CountryViewModel : ObservableObject
{
	private readonly CountryService _countryService;

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

	private ObservableCollection<CountryModel> _country = [ ];

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

	public CountryViewModel(CountryService countryService)
	{
		_countryService = countryService;
		_country = new ObservableCollection<CountryModel>();
	}
}
