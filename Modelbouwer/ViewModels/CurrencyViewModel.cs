using Modelbouwer.Services;

namespace Modelbouwer.ViewModels;
public partial class CurrencyViewModel : ObservableObject
{
	private readonly CurrencyService _currencyService;

	[ObservableProperty]
	public int currencyId;

	[ObservableProperty]
	public double currencyConversionRate;

	[ObservableProperty]
	public string? currencyCode;

	[ObservableProperty]
	public string? currencySymbol;

	[ObservableProperty]
	public string? currencyName;

	[ObservableProperty]
	private CurrencyModel? _selectedCurrency;

	[ObservableProperty]
	public CurrencyModel? selectedItem;

	private ObservableCollection<CurrencyModel> _currency = [];

	public ObservableCollection<CurrencyModel> Currency
	{
		get => _currency;
		set
		{
			if ( _currency != value )
			{
				_currency = value;
				OnPropertyChanged( nameof( Currency ) );
			}
		}
	}

	public CurrencyViewModel( CurrencyService = currencyService)
	{
		_CurrencyService = CurrencyService;
		_Currency = new ObservableCollection<CurrencyModel>();
	}

	public void Refresh()
	{
		_currency.Clear();
		foreach ( var currency in DBCommands.GetCurrencyList() )
		{
			_currency.Add( currency );
		}
		OnPropertyChanged( nameof( Currency ) );
	}
}