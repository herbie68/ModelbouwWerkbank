namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class CountryPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsCountriesAndCurrenciesOnce()
	{
		var countryService = new Mock<ICountryService>();
		var currencyService = new Mock<ICurrencyService>();
		countryService.Setup( service => service.GetAllCountriesAsync() ).ReturnsAsync( [ ] );
		currencyService.Setup( service => service.GetAllCurrenciesAsync() ).ReturnsAsync( [ ] );

		_ = CreateViewModel( countryService.Object, currencyService.Object );

		await Task.Delay( 100 );
		countryService.Verify( service => service.GetAllCountriesAsync(), Times.Once );
		currencyService.Verify( service => service.GetAllCurrenciesAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenCurrencyLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load currencies." );
		var currencyService = new Mock<ICurrencyService>();
		currencyService
			.Setup( service => service.GetAllCurrenciesAsync() )
			.Returns( Task.FromException<List<CurrencyModel>>( expected ) );

		var viewModel = CreateViewModel( currencyService: currencyService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public async Task Constructor_WhenCountryLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load countries." );
		var countryService = new Mock<ICountryService>();
		countryService
			.Setup( service => service.GetAllCountriesAsync() )
			.Returns( Task.FromException<List<CountryModel>>( expected ) );

		var viewModel = CreateViewModel( countryService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	private static CountryPageViewModel CreateViewModel( ICountryService? countryService = null, ICurrencyService? currencyService = null )
	{
		var defaultCountryService = new Mock<ICountryService>();
		var defaultCurrencyService = new Mock<ICurrencyService>();
		var validator = new Mock<IEntityValidator<CountryModel>>();

		defaultCountryService.Setup( service => service.GetAllCountriesAsync() ).ReturnsAsync( [ ] );
		defaultCurrencyService.Setup( service => service.GetAllCurrenciesAsync() ).ReturnsAsync( [ ] );

		return new CountryPageViewModel(
			countryService ?? defaultCountryService.Object,
			currencyService ?? defaultCurrencyService.Object,
			validator.Object );
	}

	private static async Task WaitUntilAsync( Func<bool> condition )
	{
		using var timeout = new CancellationTokenSource( TimeSpan.FromSeconds( 2 ) );

		while ( !condition() )
		{
			if ( timeout.IsCancellationRequested )
				Assert.Fail( "Condition was not met before timeout." );

			await Task.Delay( 10 );
		}
	}
}