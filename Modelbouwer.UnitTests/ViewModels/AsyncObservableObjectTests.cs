namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class AsyncObservableObjectTests
{
	[TestMethod]
	public async Task ObserveBackgroundTask_WhenTaskFails_StoresLastAsyncError()
	{
		var viewModel = new TestAsyncObservableObject();
		var expected = new InvalidOperationException( "Background load failed." );

		viewModel.Observe( Task.FromException( expected ) );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public async Task ObserveBackgroundTask_WhenTaskIsCanceled_DoesNotStoreLastAsyncError()
	{
		var viewModel = new TestAsyncObservableObject();

		viewModel.Observe( Task.FromCanceled( new CancellationToken( true ) ) );

		await Task.Delay( 50 );
		Assert.IsNull( viewModel.LastAsyncError );
	}

	[TestMethod]
	public void StockViewModels_UseSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( StockOrderViewModel ) ) );
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( StockReceiptViewModel ) ) );
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

	private sealed class TestAsyncObservableObject : AsyncObservableObject
	{
		public void Observe( Task task )
		{
			ObserveBackgroundTask( task );
		}
	}
}