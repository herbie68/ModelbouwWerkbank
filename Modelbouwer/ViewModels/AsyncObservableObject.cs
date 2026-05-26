namespace Modelbouwer.ViewModels;

public abstract class AsyncObservableObject : ObservableObject
{
	private Exception? _lastAsyncError;

	public Exception? LastAsyncError
	{
		get => _lastAsyncError;
		private set => SetProperty( ref _lastAsyncError, value );
	}

	protected async void ObserveBackgroundTask( Task task )
	{
		try
		{
			await task;
		}
		catch ( OperationCanceledException )
		{
		}
		catch ( Exception ex )
		{
			SetLastAsyncError( ex );
		}
	}

	protected void SetLastAsyncError( Exception exception )
	{
		LastAsyncError = exception;
	}
}
