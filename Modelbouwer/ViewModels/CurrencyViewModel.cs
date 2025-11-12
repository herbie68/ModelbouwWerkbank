namespace Modelbouwer.ViewModels;
public partial class CurrencyViewModel : ObservableObject
{
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
	private ObservableCollection<CurrencyModel>? _currency;

	[ObservableProperty]
	public CurrencyModel? selectedItem;

	//public CurrencyViewModel()
	//{
	//	DBCommands dbCommands = new();
	//	Currency = [ .. DBCommands.GetCurrencyList() ];
	//}

	//public void Refresh()
	//{
	//	DBCommands dbCommands = new();
	//	Currency = [ .. DBCommands.GetCurrencyList() ];
	//	OnPropertyChanged( nameof( Currency ) );
	//}
}