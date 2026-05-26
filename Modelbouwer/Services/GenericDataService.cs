using System.Data.Common;
using System.Reflection;

using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySqlTransaction = MySql.Data.MySqlClient.MySqlTransaction;

namespace Modelbouwer.Services;

public class GenericDataService
{
	private readonly string _connectionString;

	#region Database query's
	public string GetLastInsertIdQuery = "SELECT LAST_INSERT_ID();";
	#endregion

	public GenericDataService()
	{
		_connectionString = DBConnect.ConnectionString;
	}

	#region General Get statenents
	public virtual async Task<uint> GetLastInsertIdAsync()
	{
		return await GetLastInsertIdAsync( CancellationToken.None );
	}

	public virtual async Task<uint> GetLastInsertIdAsync( CancellationToken cancellationToken )
	{
		return await ExecuteScalarAsync<uint>( GetLastInsertIdQuery, cancellationToken );
	}
	#endregion

	public virtual async Task<List<T>> ExecuteQueryAsync<T>(
		string? query,
	Func<DbDataReader, T> mapFunc,
	Dictionary<string, object>? parameters = null )
	{
		return await ExecuteQueryAsync( query, mapFunc, parameters, CancellationToken.None );
	}

	public virtual async Task<List<T>> ExecuteQueryAsync<T>(
		string? query,
	Func<DbDataReader, T> mapFunc,
	Dictionary<string, object>? parameters,
	CancellationToken cancellationToken )
	{
		List<T> results = [];

		await using MySqlConnection connection = new(_connectionString);
		await connection.OpenAsync( cancellationToken );

		using MySqlCommand cmd = new(query, connection);

		if ( parameters is not null )
		{
			foreach ( KeyValuePair<string, object> param in parameters )
			{
				cmd.Parameters.AddWithValue( param.Key, param.Value );
			}
		}

		using DbDataReader reader = await cmd.ExecuteReaderAsync( cancellationToken );

		while ( await reader.ReadAsync( cancellationToken ) )
		{
			results.Add( mapFunc( reader ) );
		}

		return results;
	}

	public virtual async Task<int> ExecuteNonQueryAsync(
	string? query,
	Dictionary<string, object>? parameters = null )
	{
		return await ExecuteNonQueryAsync( query, parameters, CancellationToken.None );
	}

	public virtual async Task<int> ExecuteNonQueryAsync(
	string? query,
	Dictionary<string, object>? parameters,
	CancellationToken cancellationToken )
	{
		await using MySqlConnection connection = new(_connectionString);
		await connection.OpenAsync( cancellationToken );

		using MySqlCommand cmd = new(query, connection);

		if ( parameters is not null )
		{
			foreach ( KeyValuePair<string, object> param in parameters )
			{
				cmd.Parameters.AddWithValue( param.Key, param.Value );
			}
		}

		return await cmd.ExecuteNonQueryAsync( cancellationToken );
	}

	public virtual async Task<T?> ExecuteScalarAsync<T>(
	string? query,
	Dictionary<string, object>? parameters = null )
	{
		return await ExecuteScalarAsync<T>( query, parameters, CancellationToken.None );
	}

	public virtual Task<T?> ExecuteScalarAsync<T>(
	string? query,
	CancellationToken cancellationToken )
	{
		return ExecuteScalarAsync<T>( query, null, cancellationToken );
	}

	public virtual async Task<T?> ExecuteScalarAsync<T>(
	string? query,
	Dictionary<string, object>? parameters,
	CancellationToken cancellationToken )
	{
		await using MySqlConnection connection = new(_connectionString);
		await connection.OpenAsync( cancellationToken );

		await using MySqlCommand command = new(query, connection);

		if ( parameters != null )
		{
			foreach ( var param in parameters )
			{
				command.Parameters.AddWithValue( param.Key, param.Value ?? DBNull.Value );
			}
		}

		object? result = await command.ExecuteScalarAsync( cancellationToken );

		if ( result == null || result == DBNull.Value )
			return default;

		Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

		object converted = Convert.ChangeType(result, targetType);

		return ( T ) converted;
	}

	public virtual T? ExecuteScalarQuery<T>( string? sql, Dictionary<string, object> parameters )
	{
		using var connection = new MySqlConnection(_connectionString);
		using var command = new MySqlCommand(sql, connection);

		foreach ( var param in parameters )
		{
			command.Parameters.AddWithValue( param.Key, param.Value );
		}

		connection.Open();
		object result = command.ExecuteScalar();
		connection.Close();

		if ( result == null || result == DBNull.Value )
			return default;

		return ( T ) Convert.ChangeType( result, typeof( T ) );
	}

	/// <summary>
	/// Execute a query and return an open DbDataReader.
	/// Caller is responsible for disposing the reader (which will also close the connection).
	/// Accepts parameters as Dictionary<string, object> for convenience.
	/// </summary>
	public virtual async Task ExecuteReaderAsync(
		string? sql,
		Func<DbDataReader, Task> map )
	{
		await ExecuteReaderAsync( sql, map, null, CancellationToken.None );
	}

	public virtual async Task ExecuteReaderAsync(
		string? sql,
		Func<DbDataReader, Task> map,
		CancellationToken cancellationToken )
	{
		await ExecuteReaderAsync( sql, map, null, cancellationToken );
	}

	public virtual async Task ExecuteReaderAsync(
		string? sql,
		Func<DbDataReader, Task> map,
		Dictionary<string, object>? parameters )
	{
		await ExecuteReaderAsync( sql, map, parameters, CancellationToken.None );
	}

	public virtual async Task ExecuteReaderAsync(
		string? sql,
		Func<DbDataReader, Task> map,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken )
	{
		using var connection = new MySqlConnection(_connectionString);
		await connection.OpenAsync( cancellationToken );

		using var cmd = new MySqlCommand(sql, connection);

		// Add parameters when provided
		if ( parameters != null )
		{
			foreach ( var p in parameters )
			{
				cmd.Parameters.AddWithValue( p.Key, p.Value ?? DBNull.Value );
			}
		}

		using var reader = await cmd.ExecuteReaderAsync( cancellationToken );

		// Execute the reader callback
		await map( reader );
	}

	public virtual async Task<T?> ExecuteSingleAsync<T>(
	string query,
	Dictionary<string, object>? parameters = null )
	where T : class, new()
	{
		return await ExecuteSingleAsync<T>( query, parameters, CancellationToken.None );
	}

	public virtual async Task<T?> ExecuteSingleAsync<T>(
	string query,
	Dictionary<string, object>? parameters,
	CancellationToken cancellationToken )
	where T : class, new()
	{
		using MySqlConnection connection = new(_connectionString);
		await connection.OpenAsync( cancellationToken );

		using MySqlCommand cmd = new(query, connection);

		if ( parameters != null )
		{
			foreach ( KeyValuePair<string, object> param in parameters )
			{
				cmd.Parameters.AddWithValue( $"@{param.Key}", param.Value );
			}
		}

		using var reader = await cmd.ExecuteReaderAsync( cancellationToken );

		if ( !await reader.ReadAsync( cancellationToken ) )
			return null;

		T result = new();

		for ( int i = 0; i < reader.FieldCount; i++ )
		{
			string columnName = reader.GetName(i);

			PropertyInfo? property =
			typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance);

			if ( property == null || reader.IsDBNull( i ) )
				continue;

			object value = reader.GetValue(i);

			Type targetType = Nullable.GetUnderlyingType(property.PropertyType)
						  ?? property.PropertyType;

			property.SetValue( result, Convert.ChangeType( value, targetType ) );
		}

		return result;
	}

	public virtual async Task ExecuteInTransactionAsync( Func<MySqlConnection, MySqlTransaction, Task> operation )
	{
		await ExecuteInTransactionAsync( operation, CancellationToken.None );
	}

	public virtual async Task ExecuteInTransactionAsync( Func<MySqlConnection, MySqlTransaction, Task> operation, CancellationToken cancellationToken )
	{
		await using MySqlConnection connection = new(_connectionString);
		await connection.OpenAsync( cancellationToken );

		DbTransaction dbTransaction = await connection.BeginTransactionAsync( cancellationToken );
		MySqlTransaction transaction = ( MySqlTransaction ) dbTransaction;

		try
		{
			await operation( connection, transaction );
			await transaction.CommitAsync( cancellationToken );
		}
		catch
		{
			try
			{
				await transaction.RollbackAsync( cancellationToken );
			}
			catch
			{
				// Preserve the original operation/commit exception when rollback also fails.
			}

			throw;
		}
	}

	public virtual async Task<T> ExecuteInTransactionAsync<T>( Func<MySqlConnection, MySqlTransaction, Task<T>> operation )
	{
		return await ExecuteInTransactionAsync( operation, CancellationToken.None );
	}

	public virtual async Task<T> ExecuteInTransactionAsync<T>( Func<MySqlConnection, MySqlTransaction, Task<T>> operation, CancellationToken cancellationToken )
	{
		await using MySqlConnection connection = new(_connectionString);
		await connection.OpenAsync( cancellationToken );

		DbTransaction dbTransaction = await connection.BeginTransactionAsync( cancellationToken );
		MySqlTransaction transaction = ( MySqlTransaction ) dbTransaction;

		try
		{
			T result = await operation( connection, transaction );
			await transaction.CommitAsync( cancellationToken );
			return result;
		}
		catch
		{
			try
			{
				await transaction.RollbackAsync( cancellationToken );
			}
			catch
			{
				// Preserve the original operation/commit exception when rollback also fails.
			}

			throw;
		}
	}

}
