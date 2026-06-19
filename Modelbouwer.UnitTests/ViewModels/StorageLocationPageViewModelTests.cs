using System.Reflection;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StorageLocationPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsStorageLocationsOnce()
	{
		var storageLocationService = new Mock<IStorageLocationService>();
		storageLocationService
			.Setup( service => service.GetAllStorageLocationsAsync() )
			.ReturnsAsync( [ ] );

		_ = CreateViewModel( storageLocationService.Object );

		await Task.Delay( 100 );
		storageLocationService.Verify( service => service.GetAllStorageLocationsAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load storage locations." );
		var storageLocationService = new Mock<IStorageLocationService>();
		storageLocationService
			.Setup( service => service.GetAllStorageLocationsAsync() )
			.Returns( Task.FromException<List<StorageLocationModel>>( expected ) );

		var viewModel = CreateViewModel( storageLocationService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void UpdateParameters_UsesDistinctKeysForIdAndParentId()
	{
		var storageLocation = new StorageLocationModel
		{
			StorageId = 42,
			ParentId = 7,
			StorageName = "Drawer"
		};

		var method = typeof( StorageLocationPageViewModel ).GetMethod(
			"UpdateParameters",
			BindingFlags.NonPublic | BindingFlags.Static );

		var parameters = method?.Invoke( null, [ storageLocation ] ) as Dictionary<string, object?>;

		Assert.IsNotNull( parameters );
		Assert.AreEqual( 3, parameters.Count );
		Assert.AreEqual( 42, parameters [ $"@{DBNames.StorageFieldNameId}" ] );
		Assert.AreEqual( 7, parameters [ $"@{DBNames.StorageFieldNameParentId}" ] );
		Assert.AreEqual( "Drawer", parameters [ $"@{DBNames.StorageFieldNameName}" ] );
	}

	private static StorageLocationPageViewModel CreateViewModel( IStorageLocationService storageLocationService )
	{
		var validator = new Mock<IEntityValidator<StorageLocationModel>>();

		return new StorageLocationPageViewModel( storageLocationService, validator.Object );
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