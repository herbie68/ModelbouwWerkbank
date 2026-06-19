namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class CategoryPickerViewModelTests
{
	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load categories." );
		var categoryService = new Mock<ICategoryService>();
		categoryService
			.Setup( service => service.GetAllCategorysAsync() )
			.Returns( Task.FromException<List<CategoryModel>>( expected ) );

		var viewModel = new CategoryPickerViewModel( categoryService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void CategoryPickerViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( CategoryPickerViewModel ) ) );
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