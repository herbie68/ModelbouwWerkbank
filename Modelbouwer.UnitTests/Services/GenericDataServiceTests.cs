using System.Reflection;

using MySql.Data.MySqlClient;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class GenericDataServiceTests
{
	[TestMethod]
	public void GenericDataService_DoesNotKeepMySqlConnectionAsInstanceField()
	{
		// Arrange & Act
		var connectionFields = typeof( GenericDataService )
			.GetFields( System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance )
			.Where( field => field.FieldType == typeof( MySqlConnection ) )
			.ToArray();

		// Assert
		Assert.AreEqual( 0, connectionFields.Length );
	}

	[TestMethod]
	public void GenericDataService_ExposesCancellationTokenOverloads()
	{
		// Arrange
		var methods = typeof( GenericDataService )
			.GetMethods( BindingFlags.Public | BindingFlags.Instance )
			.ToArray();

		// Act & Assert
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.GetLastInsertIdAsync ), 1 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteQueryAsync ), 4 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteNonQueryAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteScalarAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteReaderAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteReaderAsync ), 4 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteSingleAsync ), 3 );
		AssertHasCancellationTokenOverload( methods, nameof( GenericDataService.ExecuteInTransactionAsync ), 2 );
	}

	[TestMethod]
	public void GenericDataService_CancellationTokenOverloadsPassTokenToDatabaseCalls()
	{
		// Arrange
		var source = File.ReadAllText( Path.Combine(
			GetRepositoryRoot(),
			"Modelbouwer",
			"Services",
			"GenericDataService.cs" ) );

		// Assert
		StringAssert.Contains( source, "OpenAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteReaderAsync( cancellationToken )" );
		StringAssert.Contains( source, "ReadAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteNonQueryAsync( cancellationToken )" );
		StringAssert.Contains( source, "ExecuteScalarAsync( cancellationToken )" );
		StringAssert.Contains( source, "BeginTransactionAsync( cancellationToken )" );
		StringAssert.Contains( source, "CommitAsync( cancellationToken )" );
		StringAssert.Contains( source, "RollbackAsync( cancellationToken )" );
	}

	private static void AssertHasCancellationTokenOverload( MethodInfo [ ] methods, string methodName, int parameterCount )
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

	private static string GetRepositoryRoot()
	{
		var directory = AppContext.BaseDirectory;
		while ( directory != null && !File.Exists( Path.Combine( directory, "ModelbouwWerkbank.slnx" ) ) )
		{
			directory = Directory.GetParent( directory )?.FullName;
		}

		return directory ?? throw new DirectoryNotFoundException( "Could not locate repository root." );
	}
}