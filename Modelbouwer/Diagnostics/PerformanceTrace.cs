namespace Modelbouwer.Diagnostics;

public static class PerformanceTrace
{
	public static async Task MeasureAsync( string operationName, Func<Task> operation )
	{
		ArgumentException.ThrowIfNullOrWhiteSpace( operationName );
		ArgumentNullException.ThrowIfNull( operation );

		var stopwatch = Stopwatch.StartNew();
		try
		{
			await operation();
		}
		finally
		{
			stopwatch.Stop();
			Debug.WriteLine( $"[perf] {operationName} completed in {stopwatch.ElapsedMilliseconds} ms" );
		}
	}

	public static async Task<T> MeasureAsync<T>( string operationName, Func<Task<T>> operation )
	{
		ArgumentException.ThrowIfNullOrWhiteSpace( operationName );
		ArgumentNullException.ThrowIfNull( operation );

		var stopwatch = Stopwatch.StartNew();
		try
		{
			return await operation();
		}
		finally
		{
			stopwatch.Stop();
			Debug.WriteLine( $"[perf] {operationName} completed in {stopwatch.ElapsedMilliseconds} ms" );
		}
	}
}