namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class BrandPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsBrandsOnce()
	{
		var brandService = new Mock<IBrandService>();
		brandService
			.Setup( service => service.GetAllBrandsAsync() )
			.ReturnsAsync( [] );

		_ = CreateViewModel( brandService.Object );

		await Task.Delay( 100 );
		brandService.Verify( service => service.GetAllBrandsAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load brands." );
		var brandService = new Mock<IBrandService>();
		brandService
			.Setup( service => service.GetAllBrandsAsync() )
			.Returns( Task.FromException<List<BrandModel>>( expected ) );

		var viewModel = CreateViewModel( brandService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void BrandPageViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( BrandPageViewModel ) ) );
	}

	[TestMethod]
	public void DeleteBrandCommand_UsesAsyncRelayCommand()
	{
		var brandService = new Mock<IBrandService>();
		brandService
			.Setup( service => service.GetAllBrandsAsync() )
			.ReturnsAsync( [] );
		var viewModel = CreateViewModel( brandService.Object );

		Assert.IsInstanceOfType( viewModel.DeleteBrandCommand, typeof( CommunityToolkit.Mvvm.Input.IAsyncRelayCommand ) );
	}

	private static BrandPageViewModel CreateViewModel( IBrandService brandService )
	{
		var validator = new Mock<IEntityValidator<BrandModel>>();

		return new BrandPageViewModel( brandService, validator.Object );
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
