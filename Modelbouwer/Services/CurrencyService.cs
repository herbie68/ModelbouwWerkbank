using Modelbouwer.Interfaces;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Services;

public class CurrencyService : ICurrencyService
{
	private readonly GenericDataService _dataService;
	public bool CurrencyUsed { get; set; } = false;

	public CurrencyService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteCurrencyList = $"" +
		$"SELECT " +
		$"{DBNames.CurrencyFieldNameId} AS {DBNames.CurrencyFieldNameId}, " +
		$"{DBNames.CurrencyFieldNameCode} AS {DBNames.CurrencyFieldNameCode}, " +
		$"{DBNames.CurrencyFieldNameSymbol} AS {DBNames.CurrencyFieldNameSymbol}, " +
		$"{DBNames.CurrencyFieldNameName} AS {DBNames.CurrencyFieldNameName}, " +
		$"{DBNames.CurrencyFieldNameRate} AS {DBNames.CurrencyFieldNameRate}" +
		$" FROM {DBNames.Database}.{DBNames.CurrencyTable};";

	public string AddNewCurrencyQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.CurrencyTable} " +
		$"({DBNames.CurrencyFieldNameCode}, {DBNames.CurrencyFieldNameId}, {DBNames.CurrencyFieldNameSymbol}, {DBNames.CurrencyFieldNameName}, {DBNames.CurrencyFieldNameRate}) " +
		$"VALUES " +
		$"(@{DBNames.CurrencyFieldNameCode}, @{DBNames.CurrencyFieldNameId}, @{DBNames.CurrencyFieldNameSymbol}, @{DBNames.CurrencyFieldNameName}, @{DBNames.CurrencyFieldNameRate});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateCurrencyQuery =
		$"UPDATE {DBNames.Database}.{DBNames.CurrencyTable} " +
		$"SET " +
		$"{DBNames.CurrencyFieldNameCode} = @{DBNames.CurrencyFieldNameCode}, " +
		$"{DBNames.CurrencyFieldNameId} = @{DBNames.CurrencyFieldNameId}, " +
		$"{DBNames.CurrencyFieldNameSymbol} = @{DBNames.CurrencyFieldNameSymbol}, " +
		$"{DBNames.CurrencyFieldNameName} = @{DBNames.CurrencyFieldNameName}, " +
		$"{DBNames.CurrencyFieldNameRate} = @{DBNames.CurrencyFieldNameRate}" +
		$"WHERE {DBNames.CurrencyFieldNameId} = @{DBNames.CurrencyFieldNameId};";

	public string DeleteCurrencyQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.CurrencyTable} " +
		$"WHERE {DBNames.CurrencyFieldNameId} = @{DBNames.CurrencyFieldNameId};";

	public string CurrencyCodeExistsQuery =
		$"SELECT COUNT({DBNames.CurrencyFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.CurrencyTable} " +
		$"WHERE {DBNames.CurrencyFieldNameCode} = @{DBNames.CurrencyFieldNameCode}";

	public string CurrencyNameExistsQuery =
		$"SELECT COUNT({DBNames.CurrencyFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.CurrencyTable} " +
		$"WHERE {DBNames.CurrencyFieldNameName} = @{DBNames.CurrencyFieldNameName}";

	public string CurrencyUsedQuery = $"SELECT COUNT({DBNames.CurrencyFieldNameId}) FROM {DBNames.Database}.{DBNames.CurrencyTable} WHERE {DBNames.CurrencyFieldNameId} = @CurrencyId";
	#endregion

	public Task<List<CurrencyModel>> GetAllCurrenciesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteCurrencyList, reader =>
		{
			return new CurrencyModel
			{
				CurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CurrencyFieldNameId}" ] ),
				CurrencyCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameCode}" ] ),
				CurrencyName = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameName}" ] ),
				CurrencySymbol = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameSymbol}" ] ),
				CurrencyConversionRate = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.CurrencyFieldNameRate}" ] )
			};
		} );
	}

	public async Task<int> InsertNewCurrencyAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.CurrencyFieldNameCode}", queryParameters[$"@{DBNames.CurrencyFieldNameCode}"] ?? DBNull.Value },
			{ $"@{DBNames.CurrencyFieldNameName}", queryParameters[$"@{DBNames.CurrencyFieldNameName}"] ?? DBNull.Value },
			{ $"@{DBNames.CurrencyFieldNameSymbol}", queryParameters[$"@{DBNames.CurrencyFieldNameSymbol}"] ?? DBNull.Value },
			{ $"@{DBNames.CurrencyFieldNameRate}", queryParameters[$"@{DBNames.CurrencyFieldNameRate}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewCurrencyQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateCurrencyAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.CurrencyFieldNameCode}", queryParameters[$"@{DBNames.CurrencyFieldNameCode}"] ?? DBNull.Value },
			{ $"@{DBNames.CurrencyFieldNameName}", queryParameters[$"@{DBNames.CurrencyFieldNameName}"] ?? DBNull.Value },
			{ $"@{DBNames.CurrencyFieldNameSymbol}", queryParameters[$"@{DBNames.CurrencyFieldNameSymbol}"] ?? DBNull.Value },
			{ $"@{DBNames.CurrencyFieldNameRate}", queryParameters[$"@{DBNames.CurrencyFieldNameRate}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateCurrencyQuery, parameters );
	}

	public async Task DeleteCurrencyAsync( int currencyId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.CurrencyFieldNameId}", currencyId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteCurrencyQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataCurrencyDeleteError}." );
		}
	}

	public async Task<bool> IsCurrencyUsedAsync( int currencyId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.SupplierFieldNameCurrencyId}", currencyId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			CurrencyUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> CodeExistsAsync( string? currencyCode )
	{
		if ( string.IsNullOrWhiteSpace( currencyCode ) )
			return false;

		var countries = await GetAllCurrenciesAsync();

		return countries.Any( c =>
			string.Equals( c.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase ) );
	}

	public async Task<bool> NameExistsAsync( string? currencyName )
	{
		if ( string.IsNullOrWhiteSpace( currencyName ) )
			return false;

		var countries = await GetAllCurrenciesAsync();

		return countries.Any( c =>
			string.Equals( c.CurrencyName, currencyName, StringComparison.OrdinalIgnoreCase ) );
	}
}
