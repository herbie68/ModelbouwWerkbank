using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Modelbouwer.Models;
using Modelbouwer.Services;

using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Modelbouwer.ViewModels;

public partial class CurrencyViewModel : ObservableObject
{
	private readonly CurrencyService _currencyService;

	[ObservableProperty]
	private int _currencyId;

	[ObservableProperty]
	private double _currencyConversionRate;

	[ObservableProperty]
	private string? _currencyCode;

	[ObservableProperty]
	private string? _currencySymbol;

	[ObservableProperty]
	private string? _currencyName;

	[ObservableProperty]
	private CurrencyModel? _selectedCurrency;

	[ObservableProperty]
	private CurrencyModel? _selectedItem;

	[ObservableProperty]
	private bool _isLoading;

	private ObservableCollection<CurrencyModel> _currencies = [];

	public ObservableCollection<CurrencyModel> Currencies
	{
		get => _currencies;
		set => SetProperty( ref _currencies, value );
	}

	public CurrencyViewModel( CurrencyService currencyService )
	{
		_currencyService = currencyService ?? throw new ArgumentNullException( nameof( currencyService ) );
		_currencies = new ObservableCollection<CurrencyModel>();

		// Load data asynchronously
		LoadDataAsync();
	}

	private async void LoadDataAsync()
	{
		await LoadCurrenciesAsync();
	}

	[RelayCommand]
	private async Task LoadCurrenciesAsync()
	{
		IsLoading = true;
		try
		{
			var currencies = await _currencyService.GetAllCurrenciesAsync();
			Currencies.Clear();
			foreach ( var currency in currencies )
			{
				Currencies.Add( currency );
			}
		}
		catch ( Exception ex )
		{
			// Handle exception
			System.Diagnostics.Debug.WriteLine( $"Error loading currencies: {ex.Message}" );
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		await LoadCurrenciesAsync();
	}
}