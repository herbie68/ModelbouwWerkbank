using System.Reflection;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class CategoryPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsCategoriesOnce()
	{
		var categoryService = new Mock<ICategoryService>();
		categoryService
			.Setup( service => service.GetAllCategorysAsync() )
			.ReturnsAsync( [] );

		_ = CreateViewModel( categoryService.Object );

		await Task.Delay( 100 );
		categoryService.Verify( service => service.GetAllCategorysAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load categories." );
		var categoryService = new Mock<ICategoryService>();
		categoryService
			.Setup( service => service.GetAllCategorysAsync() )
			.Returns( Task.FromException<List<CategoryModel>>( expected ) );

		var viewModel = CreateViewModel( categoryService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void UpdateParameters_UsesDistinctKeysForIdAndParentId()
	{
		// Arrange
		var category = new CategoryModel
		{
			CategoryId = 42,
			ParentId = 7,
			CategoryName = "Subcategory"
		};

		var method = typeof( CategoryPageViewModel ).GetMethod(
			"UpdateParameters",
			BindingFlags.NonPublic | BindingFlags.Static );

		// Act
		var parameters = method?.Invoke( null, [ category ] ) as Dictionary<string, object?>;

		// Assert
		Assert.IsNotNull( parameters );
		Assert.AreEqual( 3, parameters.Count );
		Assert.AreEqual( 42, parameters [ $"@{DBNames.CategoryFieldNameId}" ] );
		Assert.AreEqual( 7, parameters [ $"@{DBNames.CategoryFieldNameParentId}" ] );
		Assert.AreEqual( "Subcategory", parameters [ $"@{DBNames.CategoryFieldNameName}" ] );
	}

	private static CategoryPageViewModel CreateViewModel( ICategoryService categoryService )
	{
		var validator = new Mock<IEntityValidator<CategoryModel>>();

		return new CategoryPageViewModel( categoryService, validator.Object );
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
