namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockManagementPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_WhenInventoryLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load inventory." );
		var stockService = new Mock<IStockService>();
		stockService
			.Setup( service => service.GetCompleteInventoryAsync() )
			.Returns( Task.FromException<List<StockManagementModel>>( expected ) );

		var viewModel = CreateViewModel( stockService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public async Task ItemPropertyChanged_WhenInventoryUpdateFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to save inventory." );
		var stockService = new Mock<IStockService>();
		stockService.Setup( service => service.GetCompleteInventoryAsync() ).ReturnsAsync( [] );
		stockService
			.Setup( service => service.InsertCorrectionAsync( It.IsAny<Dictionary<string, object?>>() ) )
			.Returns( Task.FromException<int>( expected ) );
		var viewModel = CreateViewModel( stockService.Object );
		var item = new StockManagementModel
		{
			ProductId = 7,
			ProductOriginalInventory = 0,
			ProductInventory = 0
		};
		viewModel.Items.Add( item );

		item.ProductInventory = 1;

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	private static StockManagementPageViewModel CreateViewModel( IStockService stockService )
	{
		var validator = new Mock<IEntityValidator<StockManagementModel>>();

		return new StockManagementPageViewModel( stockService, validator.Object, new SettingsService() );
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
