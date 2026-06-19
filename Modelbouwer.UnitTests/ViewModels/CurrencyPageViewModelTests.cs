namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class CurrencyPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsCurrenciesOnce()
	{
		var currencyService = new Mock<ICurrencyService>();
		currencyService
			.Setup( service => service.GetAllCurrenciesAsync() )
			.ReturnsAsync( [ ] );

		_ = CreateViewModel( currencyService.Object );

		await Task.Delay( 100 );
		currencyService.Verify( service => service.GetAllCurrenciesAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load currencies." );
		var currencyService = new Mock<ICurrencyService>();
		currencyService
			.Setup( service => service.GetAllCurrenciesAsync() )
			.Returns( Task.FromException<List<CurrencyModel>>( expected ) );

		var viewModel = CreateViewModel( currencyService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	private static CurrencyPageViewModel CreateViewModel( ICurrencyService currencyService )
	{
		var validator = new Mock<IEntityValidator<CurrencyModel>>();

		return new CurrencyPageViewModel( currencyService, validator.Object );
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