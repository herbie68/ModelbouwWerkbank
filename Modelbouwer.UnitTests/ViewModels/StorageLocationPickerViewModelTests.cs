namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StorageLocationPickerViewModelTests
{
	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load storage locations." );
		var storageLocationService = new Mock<IStorageLocationService>();
		storageLocationService
			.Setup( service => service.GetAllStorageLocationsAsync() )
			.Returns( Task.FromException<List<StorageLocationModel>>( expected ) );

		var viewModel = new StorageLocationPickerViewModel( storageLocationService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void StorageLocationPickerViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( StorageLocationPickerViewModel ) ) );
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
