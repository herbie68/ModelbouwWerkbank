using Modelbouwer.Interfaces;

namespace Modelbouwer.Services;

public class CountryService( GenericDataService dataService ) : ICountryService
{
	private readonly GenericDataService _dataService = dataService;
	public bool CountryUsed { get; set; } = false;

	#region Database queries
	public string CompleteCountryList =
		$"SELECT " +
		$"{DBNames.CountryFieldNameId} AS {DBNames.CountryFieldNameId}, " +
		$"{DBNames.CountryFieldNameCode} AS {DBNames.CountryFieldNameCode}, " +
		$"{DBNames.CountryFieldNameName} AS {DBNames.CountryFieldNameName}, " +
		$"{DBNames.CountryFieldNameCurrencyId} AS {DBNames.CountryFieldNameCurrencyId}, " +
		$"{DBNames.CountryFieldNameCurrencySymbol} AS {DBNames.CountryFieldNameCurrencySymbol} " + 
        $"FROM {DBNames.Database}.{DBNames.CountryTable};";

	public string AddNewCountryQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.CountryTable} " +
		$"({DBNames.CountryFieldNameCode}, {DBNames.CountryFieldNameCurrencyId}, {DBNames.CountryFieldNameCurrencySymbol}, {DBNames.CountryFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.CountryFieldNameCode}, @{DBNames.CountryFieldNameCurrencyId}, @{DBNames.CountryFieldNameCurrencySymbol}, @{DBNames.CountryFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateCountryQuery =
		$"UPDATE {DBNames.Database}.{DBNames.CountryTable} " +
		$"SET " +
		$"{DBNames.CountryFieldNameCode} = @{DBNames.CountryFieldNameCode}, " +
		$"{DBNames.CountryFieldNameCurrencyId} = @{DBNames.CountryFieldNameCurrencyId}, " +
		$"{DBNames.CountryFieldNameCurrencySymbol} = @{DBNames.CountryFieldNameCurrencySymbol}, " +
		$"{DBNames.CountryFieldNameName} = @{DBNames.CountryFieldNameName} " + 
        $"WHERE {DBNames.CountryFieldNameId} = @{DBNames.CountryFieldNameId};";

	public string DeleteCountryQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.CountryTable} " +
		$"WHERE {DBNames.CountryFieldNameId} = @{DBNames.CountryFieldNameId};";

	public string CountryUsedQuery =
		$"SELECT COUNT({DBNames.SupplierFieldNameCountryId}) " +
		$"FROM {DBNames.Database}.{DBNames.SupplierTable} " +
		$"WHERE {DBNames.SupplierFieldNameCountryId} = @CountryId";
	#endregion

	public Task<List<CountryModel>> GetAllCountriesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteCountryList, reader =>
		{
			return new CountryModel
			{
				CountryId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CountryFieldNameId}" ] ),
				CountryCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.CountryFieldNameCode}" ] ),
				CountryName = DatabaseValueConverter.GetString( reader [ $"{DBNames.CountryFieldNameName}" ] ),
				CountryCurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CountryFieldNameCurrencyId}" ] ),
				CountryCurrencySymbol = DatabaseValueConverter.GetString( reader [ $"{DBNames.CountryFieldNameCurrencySymbol}" ] )
			};
		} );
	}

	public async Task<int> InsertNewCountryAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.CountryFieldNameCode}", queryParameters[$"@{DBNames.CountryFieldNameCode}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameCurrencyId}", queryParameters[$"@{DBNames.CountryFieldNameCurrencyId}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameCurrencySymbol}", queryParameters[$"@{DBNames.CountryFieldNameCurrencySymbol}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameName}", queryParameters[$"@{DBNames.CountryFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewCountryQuery, parameters );

		return (int)newId;
	}

	public async Task UpdateCountryAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.CountryFieldNameCode}", queryParameters[$"@{DBNames.CountryFieldNameCode}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameCurrencyId}", queryParameters[$"@{DBNames.CountryFieldNameCurrencyId}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameCurrencySymbol}", queryParameters[$"@{DBNames.CountryFieldNameCurrencySymbol}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameName}", queryParameters[$"@{DBNames.CountryFieldNameName}"] ?? DBNull.Value },
			{ $"@{DBNames.CountryFieldNameId}", queryParameters[$"@{DBNames.CountryFieldNameId}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateCountryQuery, parameters );
	}

	public async Task DeleteCountryAsync( int countryId ) 
	{ 
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.CountryFieldNameId}", countryId }
		};

		await _dataService.ExecuteScalarAsync<uint>( DeleteCountryQuery, parameters );
	}

	public async Task<bool> IsCountryUsedAsync( int countryId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.SupplierFieldNameCountryId}", countryId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			CountryUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> CodeExistsAsync( string? countryCode )
	{
		if ( string.IsNullOrWhiteSpace( countryCode ) )
			return false;

		var countries = await GetAllCountriesAsync();

		return countries.Any( c =>
			string.Equals( c.CountryCode, countryCode, StringComparison.OrdinalIgnoreCase ) );
	}

	public async Task<bool> NameExistsAsync( string? countryName )
	{
		if ( string.IsNullOrWhiteSpace( countryName ) )
			return false;

		var countries = await GetAllCountriesAsync();

		return countries.Any( c =>
			string.Equals( c.CountryName, countryName, StringComparison.OrdinalIgnoreCase ) );
	}
}