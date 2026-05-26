using System.Reflection;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class SettingsServiceTests
{
	[TestMethod]
	public void ISettingsService_ExposesCancellationTokenOverloads()
	{
		var methods = typeof( ISettingsService )
			.GetMethods()
			.ToArray();

		AssertHasCancellationTokenOverload( methods, nameof( ISettingsService.LoadSettingsAsync ), 1 );
		AssertHasCancellationTokenOverload( methods, nameof( ISettingsService.GetSettingsAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ISettingsService.ResetSettingsAsync ), 2 );
		AssertHasCancellationTokenOverload( methods, nameof( ISettingsService.SaveSettingAsync ), 3 );
	}

	[TestMethod]
	public void SettingsService_TokenlessMethodsDelegateToCancellationTokenOverloads()
	{
		var source = LoadSource( "Modelbouwer", "Services", "SettingsService.cs" );

		StringAssert.Contains( source, "LoadSettingsAsync( CancellationToken.None )" );
		StringAssert.Contains( source, "GetSettingsAsync( key, CancellationToken.None )" );
		StringAssert.Contains( source, "ResetSettingsAsync( key, CancellationToken.None )" );
		StringAssert.Contains( source, "SaveSettingAsync( key, value, CancellationToken.None )" );
	}

	[TestMethod]
	public void SettingsService_CancellationTokenOverloadsPassTokenToDatabaseCalls()
	{
		var source = LoadSource( "Modelbouwer", "Services", "SettingsService.cs" );

		StringAssert.Contains( source, "OpenAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteReaderAsync( cancellationToken )" );
		StringAssert.Contains( source, "ReadAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteScalarAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteNonQueryAsync( cancellationToken )" );
	}

	private static void AssertHasCancellationTokenOverload( MethodInfo[] methods, string methodName, int parameterCount )
	{
		var hasOverload = methods.Any( method =>
		{
			if ( method.Name != methodName )
				return false;

			var parameters = method.GetParameters();
			return parameters.Length == parameterCount
				&& parameters[^1].ParameterType == typeof( CancellationToken );
		} );

		Assert.IsTrue( hasOverload, $"{methodName} should expose a {parameterCount}-parameter overload ending in CancellationToken." );
	}

	private static string LoadSource( params string[] relativeSegments )
	{
		var directory = AppContext.BaseDirectory;
		while ( directory != null && !File.Exists( Path.Combine( directory, "ModelbouwWerkbank.slnx" ) ) )
		{
			directory = Directory.GetParent( directory )?.FullName;
		}

		var repositoryRoot = directory ?? throw new DirectoryNotFoundException( "Could not locate repository root." );
		var path = Path.Combine( [ repositoryRoot, .. relativeSegments ] );

		return File.ReadAllText( path );
	}
}
