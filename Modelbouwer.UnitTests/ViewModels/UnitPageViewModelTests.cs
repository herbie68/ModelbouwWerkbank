namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class UnitPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsUnitsOnce()
	{
		var unitService = new Mock<IUnitService>();
		unitService
			.Setup( service => service.GetAllUnitsAsync() )
			.ReturnsAsync( [ ] );

		_ = CreateViewModel( unitService.Object );

		await Task.Delay( 100 );
		unitService.Verify( service => service.GetAllUnitsAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load units." );
		var unitService = new Mock<IUnitService>();
		unitService
			.Setup( service => service.GetAllUnitsAsync() )
			.Returns( Task.FromException<List<UnitModel>>( expected ) );

		var viewModel = CreateViewModel( unitService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void UnitPageViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( UnitPageViewModel ) ) );
	}

	private static UnitPageViewModel CreateViewModel( IUnitService unitService )
	{
		var validator = new Mock<IEntityValidator<UnitModel>>();

		return new UnitPageViewModel( unitService, validator.Object );
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