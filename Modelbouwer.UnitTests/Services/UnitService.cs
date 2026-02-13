namespace Modelbouwer.Services;

public class UnitService : IUnitService
{
	private readonly GenericDataService _dataService;
	public bool UnitUsed { get; set; } = false;

	public UnitService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteUnitList = $"" +
		$"SELECT " +
		$"{DBNames.UnitFieldNameUnitId} AS {DBNames.UnitFieldNameUnitId}, " +
		$"{DBNames.UnitFieldNameUnitName} AS {DBNames.UnitFieldNameUnitName}" +
		$" FROM {DBNames.Database}.{DBNames.UnitTable};";

	public string AddNewUnitQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.UnitTable} " +
		$"({DBNames.UnitFieldNameUnitName}) " +
		$"VALUES " +
		$"(@{DBNames.UnitFieldNameUnitName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateUnitQuery =
		$"UPDATE {DBNames.Database}.{DBNames.UnitTable} " +
		$"SET " +
		$"{DBNames.UnitFieldNameUnitName} = @{DBNames.UnitFieldNameUnitName}" +
		$"WHERE {DBNames.UnitFieldNameUnitId} = @{DBNames.UnitFieldNameUnitId};";

	public string DeleteUnitQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.UnitTable} " +
		$"WHERE {DBNames.UnitFieldNameUnitId} = @{DBNames.UnitFieldNameUnitId};";

	public string UnitNameExistsQuery =
		$"SELECT COUNT({DBNames.UnitFieldNameUnitId}) " +
		$"FROM {DBNames.Database}.{DBNames.UnitTable} " +
		$"WHERE {DBNames.UnitFieldNameUnitName} = @{DBNames.UnitFieldNameUnitName}";

	public string UnitUsedQuery = $"SELECT COUNT({DBNames.ProductFieldNameUnitId}) FROM {DBNames.Database}.{DBNames.ProductTable} WHERE {DBNames.ProductFieldNameUnitId} = @UnitId";
	#endregion

	public Task<List<UnitModel>> GetAllUnitsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteUnitList, reader =>
		{
			return new UnitModel
			{
				UnitId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.UnitFieldNameUnitId}" ] ),
				UnitName = DatabaseValueConverter.GetString( reader [ $"{DBNames.UnitFieldNameUnitName}" ] )
			};
		} );
	}

	public async Task<int> InsertNewUnitAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.UnitFieldNameUnitName}", queryParameters[$"@{DBNames.UnitFieldNameUnitName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewUnitQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateUnitAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.UnitFieldNameUnitId}", queryParameters[$"@{DBNames.UnitFieldNameUnitId}"] ?? DBNull.Value },
			{ $"@{DBNames.UnitFieldNameUnitName}", queryParameters[$"@{DBNames.UnitFieldNameUnitName}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateUnitQuery, parameters );
	}

	public async Task DeleteUnitAsync( int unitId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.UnitFieldNameUnitId}", unitId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteUnitQuery, parameters );
		}
		catch ( Exception ex )
		{
			int? number = null;
			try
			{
				var prop = ex.GetType().GetProperty( "Number" );
				if ( prop != null && prop.PropertyType == typeof( int ) )
					number = ( int? ) prop.GetValue( ex );
			}
			catch { }

			if ( number == 1451 )
			{
				throw new EntityInUseException(
					$"{Lang.metadataUnitDeleteError}." );
			}

			throw;
		}
	}

	public async Task<bool> IsUnitUsedAsync( int unitId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ProductFieldNameUnitId}", unitId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			UnitUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? unitName )
	{
		if ( string.IsNullOrWhiteSpace( unitName ) )
			return false;

		var units = await GetAllUnitsAsync();

		return units.Any( c =>
			string.Equals( c.UnitName, unitName, StringComparison.OrdinalIgnoreCase ) );
	}
}