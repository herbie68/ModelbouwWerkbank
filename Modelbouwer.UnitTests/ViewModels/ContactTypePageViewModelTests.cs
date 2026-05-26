namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class ContactTypePageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsContactTypesOnce()
	{
		var contactTypeService = new Mock<IContactTypeService>();
		contactTypeService
			.Setup( service => service.GetAllContactTypesAsync() )
			.ReturnsAsync( [] );

		_ = CreateViewModel( contactTypeService.Object );

		await Task.Delay( 100 );
		contactTypeService.Verify( service => service.GetAllContactTypesAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load contact types." );
		var contactTypeService = new Mock<IContactTypeService>();
		contactTypeService
			.Setup( service => service.GetAllContactTypesAsync() )
			.Returns( Task.FromException<List<ContactTypeModel>>( expected ) );

		var viewModel = CreateViewModel( contactTypeService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	private static ContactTypePageViewModel CreateViewModel( IContactTypeService contactTypeService )
	{
		var validator = new Mock<IEntityValidator<ContactTypeModel>>();

		return new ContactTypePageViewModel( contactTypeService, validator.Object );
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
