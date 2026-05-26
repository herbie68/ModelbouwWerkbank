using System.Reflection;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class WorktypePageViewModelTests
{
	[TestMethod]
	public async Task Constructor_LoadsWorktypesOnce()
	{
		var worktypeService = new Mock<IWorktypeService>();
		worktypeService
			.Setup( service => service.GetAllWorkTypesAsync() )
			.ReturnsAsync( [] );

		_ = CreateViewModel( worktypeService.Object );

		await Task.Delay( 100 );
		worktypeService.Verify( service => service.GetAllWorkTypesAsync(), Times.Once );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load worktypes." );
		var worktypeService = new Mock<IWorktypeService>();
		worktypeService
			.Setup( service => service.GetAllWorkTypesAsync() )
			.Returns( Task.FromException<List<WorktypeModel>>( expected ) );

		var viewModel = CreateViewModel( worktypeService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public void UpdateParameters_UsesDistinctKeysForIdAndParentId()
	{
		var worktype = new WorktypeModel
		{
			WorktypeId = 42,
			ParentId = 7,
			WorktypeName = "Subworktype"
		};

		var method = typeof( WorktypePageViewModel ).GetMethod(
			"UpdateParameters",
			BindingFlags.NonPublic | BindingFlags.Static );

		var parameters = method?.Invoke( null, [ worktype ] ) as Dictionary<string, object?>;

		Assert.IsNotNull( parameters );
		Assert.AreEqual( 3, parameters.Count );
		Assert.AreEqual( 42, parameters [ $"@{DBNames.WorktypeFieldNameId}" ] );
		Assert.AreEqual( 7, parameters [ $"@{DBNames.WorktypeFieldNameParentId}" ] );
		Assert.AreEqual( "Subworktype", parameters [ $"@{DBNames.WorktypeFieldNameName}" ] );
	}

	private static WorktypePageViewModel CreateViewModel( IWorktypeService worktypeService )
	{
		var validator = new Mock<IEntityValidator<WorktypeModel>>();

		return new WorktypePageViewModel( worktypeService, validator.Object );
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
