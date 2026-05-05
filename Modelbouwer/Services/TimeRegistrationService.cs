namespace Modelbouwer.Services;

public class TimeRegistrationService : ITimeRegistrationService
{
	private readonly GenericDataService _dataService;
	private readonly SettingsService _settingsService;

	public TimeRegistrationService( GenericDataService dataService, SettingsService settingsService )
	{
		_dataService = dataService;
		_settingsService = settingsService;
	}

	public Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId )
	{
		string query =
			$"SELECT {DBNames.TimeViewFieldNameId}, {DBNames.TimeViewFieldNameProjectId}, {DBNames.TimeViewFieldNameProjectName}, " +
			$"{DBNames.TimeViewFieldNameWorktypeId}, {DBNames.TimeViewFieldNameWorktypeName}, {DBNames.TimeViewFieldNameWorkDate}, " +
			$"{DBNames.TimeViewFieldNameStartTime}, {DBNames.TimeViewFieldNameEndTime}, {DBNames.TimeViewFieldNameComment} " +
			$"FROM {DBNames.Database}.{DBNames.TimeView} " +
			$"WHERE {DBNames.TimeViewFieldNameProjectId} = @ProjectId " +
			$"ORDER BY {DBNames.TimeViewFieldNameWorkDate} DESC, {DBNames.TimeViewFieldNameStartTime} ASC;";

		return _dataService.ExecuteQueryAsync( query, reader =>
		{
			var workDate = GetDateTime( reader[DBNames.TimeViewFieldNameWorkDate] );
			var startTime = DatabaseValueConverter.GetString( reader[DBNames.TimeViewFieldNameStartTime] );
			var endTime = DatabaseValueConverter.GetString( reader[DBNames.TimeViewFieldNameEndTime] );

			return new TimeEntryModel
			{
				TimeId = DatabaseValueConverter.GetInt( reader[DBNames.TimeViewFieldNameId] ),
				ProjectId = DatabaseValueConverter.GetInt( reader[DBNames.TimeViewFieldNameProjectId] ),
				ProjectName = DatabaseValueConverter.GetString( reader[DBNames.TimeViewFieldNameProjectName] ),
				WorktypeId = DatabaseValueConverter.GetInt( reader[DBNames.TimeViewFieldNameWorktypeId] ),
				WorktypeName = DatabaseValueConverter.GetString( reader[DBNames.TimeViewFieldNameWorktypeName] ),
				WorkDate = workDate == default ? DateTime.Today : workDate,
				StartTime = NormalizeTime( startTime ),
				EndTime = NormalizeTime( endTime ),
				Comment = DatabaseValueConverter.GetString( reader[DBNames.TimeViewFieldNameComment] ),
				State = TimeEntryModel.RecordState.Unchanged
			};
		}, new Dictionary<string, object> { { "@ProjectId", projectId } } );
	}

	public async Task<int> InsertTimeEntryAsync( TimeEntryModel entry )
	{
		string query =
			$"INSERT INTO {DBNames.Database}.{DBNames.TimeTable} " +
			$"({DBNames.TimeFieldNameProjectId}, {DBNames.TimeFieldNameWorktypeId}, {DBNames.TimeFieldNameWorkDate}, {DBNames.TimeFieldNameStartTime}, {DBNames.TimeFieldNameEndTime}, {DBNames.TimeFieldNameComment}) " +
			$"VALUES (@ProjectId, @WorktypeId, @WorkDate, @StartTime, @EndTime, @Comment); {DBNames.SqlSelectLastId}";

		uint id = await _dataService.ExecuteScalarAsync<uint>( query, CreateTimeParameters( entry ) );
		return ( int ) id;
	}

	public Task UpdateTimeEntryAsync( TimeEntryModel entry )
	{
		string query =
			$"UPDATE {DBNames.Database}.{DBNames.TimeTable} SET " +
			$"{DBNames.TimeFieldNameProjectId} = @ProjectId, " +
			$"{DBNames.TimeFieldNameWorktypeId} = @WorktypeId, " +
			$"{DBNames.TimeFieldNameWorkDate} = @WorkDate, " +
			$"{DBNames.TimeFieldNameStartTime} = @StartTime, " +
			$"{DBNames.TimeFieldNameEndTime} = @EndTime, " +
			$"{DBNames.TimeFieldNameComment} = @Comment " +
			$"WHERE {DBNames.TimeFieldNameId} = @TimeId;";

		var parameters = CreateTimeParameters( entry );
		parameters.Add( "@TimeId", entry.TimeId );
		return _dataService.ExecuteNonQueryAsync( query, parameters );
	}

	public Task DeleteTimeEntryAsync( int timeEntryId )
	{
		string query =
			$"DELETE FROM {DBNames.Database}.{DBNames.TimeTable} " +
			$"WHERE {DBNames.TimeFieldNameId} = @TimeId;";

		return _dataService.ExecuteNonQueryAsync( query, new Dictionary<string, object>
		{
			{ "@TimeId", timeEntryId }
		} );
	}

	public async Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId )
	{
		List<MaterialUsageModel> usages = [];

		await using ( MySqlConnection connection = new( DBConnect.ConnectionString ) )
		{
			await connection.OpenAsync();

			await using MySqlCommand command = new( DBNames.SPGetProductsUsageByProject, connection );
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue( DBNames.SPGetProductsUsageByProjectInputParameter, projectId );

			await using MySqlDataReader reader = ( MySqlDataReader ) await command.ExecuteReaderAsync();
			while ( await reader.ReadAsync() )
			{
				var usageDate = GetDateTime( reader[5] );
				usages.Add( new MaterialUsageModel
				{
					ProductUsageId = DatabaseValueConverter.GetInt( reader[0] ),
					ProjectId = DatabaseValueConverter.GetInt( reader[1] ),
					ProductId = DatabaseValueConverter.GetInt( reader[2] ),
					ProductName = DatabaseValueConverter.GetString( reader[3] ),
					UsageDate = usageDate == default ? DateTime.Today : usageDate,
					Amount = DatabaseValueConverter.GetDouble( reader[4] ),
					Comment = DatabaseValueConverter.GetString( reader[6] ),
					State = MaterialUsageModel.RecordState.Unchanged
				} );
			}
		}

		await EnrichMaterialUsageAsync( usages );
		foreach ( MaterialUsageModel usage in usages )
			usage.State = MaterialUsageModel.RecordState.Unchanged;

		return usages;
	}

	public async Task<int> InsertMaterialUsageAsync( MaterialUsageModel usage )
	{
		string query =
			$"INSERT INTO {DBNames.Database}.{DBNames.ProductUsageTable} " +
			$"({DBNames.ProductUsageFieldNameProjectId}, {DBNames.ProductUsageFieldNameProductId}, {DBNames.ProductUsageFieldNameAmountUsed}, {DBNames.ProductUsageFieldNameUsageDate}, {DBNames.ProductUsageFieldNameComment}) " +
			$"VALUES (@ProjectId, @ProductId, @AmountUsed, @UsageDate, @Comment); {DBNames.SqlSelectLastId}";

		uint id = await _dataService.ExecuteScalarAsync<uint>( query, new Dictionary<string, object>
		{
			{ "@ProjectId", usage.ProjectId },
			{ "@ProductId", usage.ProductId },
			{ "@AmountUsed", usage.Amount },
			{ "@UsageDate", usage.UsageDate.Date },
			{ "@Comment", usage.Comment ?? string.Empty }
		} );

		return ( int ) id;
	}

	public Task UpdateMaterialUsageAsync( MaterialUsageModel usage )
	{
		string query =
			$"UPDATE {DBNames.Database}.{DBNames.ProductUsageTable} SET " +
			$"{DBNames.ProductUsageFieldNameProjectId} = @ProjectId, " +
			$"{DBNames.ProductUsageFieldNameProductId} = @ProductId, " +
			$"{DBNames.ProductUsageFieldNameAmountUsed} = @AmountUsed, " +
			$"{DBNames.ProductUsageFieldNameUsageDate} = @UsageDate, " +
			$"{DBNames.ProductUsageFieldNameComment} = @Comment " +
			$"WHERE {DBNames.ProductUsageFieldNameId} = @ProductUsageId;";

		return _dataService.ExecuteNonQueryAsync( query, new Dictionary<string, object>
		{
			{ "@ProjectId", usage.ProjectId },
			{ "@ProductId", usage.ProductId },
			{ "@AmountUsed", usage.Amount },
			{ "@UsageDate", usage.UsageDate.Date },
			{ "@Comment", usage.Comment ?? string.Empty },
			{ "@ProductUsageId", usage.ProductUsageId }
		} );
	}

	public Task DeleteMaterialUsageAsync( int materialUsageId )
	{
		string query =
			$"DELETE FROM {DBNames.Database}.{DBNames.ProductUsageTable} " +
			$"WHERE {DBNames.ProductUsageFieldNameId} = @ProductUsageId;";

		return _dataService.ExecuteNonQueryAsync( query, new Dictionary<string, object>
		{
			{ "@ProductUsageId", materialUsageId }
		} );
	}

	public async Task<double> GetHourRateAsync()
	{
		var value = await _settingsService.GetSettingsAsync( DBNames.SettingsFieldNameHourRate );
		if ( double.TryParse( value, NumberStyles.Any, CultureInfo.CurrentCulture, out var currentRate ) )
			return currentRate;

		if ( double.TryParse( value, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantRate ) )
			return invariantRate;

		return 0;
	}

	public async Task<CultureInfo> GetCultureAsync()
	{
		var value = await _settingsService.GetSettingsAsync( DBNames.SettingsFieldNameCulture );
		try
		{
			return new CultureInfo( string.IsNullOrWhiteSpace( value ) ? "nl-NL" : value );
		}
		catch ( CultureNotFoundException )
		{
			return new CultureInfo( "nl-NL" );
		}
	}

	private static Dictionary<string, object> CreateTimeParameters( TimeEntryModel entry ) => new()
	{
		{ "@ProjectId", entry.ProjectId },
		{ "@WorktypeId", entry.WorktypeId },
		{ "@WorkDate", entry.WorkDate.Date },
		{ "@StartTime", entry.StartTime },
		{ "@EndTime", entry.EndTime },
		{ "@Comment", entry.Comment ?? string.Empty }
	};

	private static string NormalizeTime( string? value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return "00:00";

		return TimeSpan.TryParse( value, CultureInfo.CurrentCulture, out var parsed )
			? parsed.ToString( @"hh\:mm", CultureInfo.CurrentCulture )
			: value;
	}

	private static DateTime GetDateTime( object value ) =>
		value == null || value == DBNull.Value
			? DateTime.MinValue
			: Convert.ToDateTime( value, CultureInfo.CurrentCulture );

	private async Task EnrichMaterialUsageAsync( List<MaterialUsageModel> usages )
	{
		int[] productIds = usages
			.Select( usage => usage.ProductId )
			.Where( id => id > 0 )
			.Distinct()
			.ToArray();

		if ( productIds.Length == 0 )
			return;

		string[] parameterNames = productIds
			.Select( ( _, index ) => $"@ProductId{index}" )
			.ToArray();

		string query =
			$"SELECT product.{DBNames.ProductFieldNameId} AS ProductId, " +
			$"product.{DBNames.ProductFieldNamePrice} AS Price, " +
			$"product.{DBNames.ProductFieldNameCategoryId} AS CategoryId, " +
			$"category.{DBNames.CategoryFieldNameName} AS CategoryName " +
			$"FROM {DBNames.Database}.{DBNames.ProductTable} product " +
			$"LEFT JOIN {DBNames.Database}.{DBNames.CategoryTable} category ON category.{DBNames.CategoryFieldNameId} = product.{DBNames.ProductFieldNameCategoryId} " +
			$"WHERE product.{DBNames.ProductFieldNameId} IN ({string.Join( ", ", parameterNames )});";

		Dictionary<string, object> parameters = [];
		for ( int i = 0; i < productIds.Length; i++ )
			parameters.Add( parameterNames[i], productIds[i] );

		var metadata = await _dataService.ExecuteQueryAsync( query, reader => new
		{
			ProductId = DatabaseValueConverter.GetInt( reader["ProductId"] ),
			Price = DatabaseValueConverter.GetDouble( reader["Price"] ),
			CategoryId = DatabaseValueConverter.GetInt( reader["CategoryId"] ),
			CategoryName = DatabaseValueConverter.GetString( reader["CategoryName"] )
		}, parameters );

		var metadataByProductId = metadata.ToDictionary( item => item.ProductId );
		foreach ( MaterialUsageModel usage in usages )
		{
			if ( !metadataByProductId.TryGetValue( usage.ProductId, out var product ) )
				continue;

			usage.Price = product.Price;
			usage.CategoryId = product.CategoryId;
			usage.CategoryName = product.CategoryName;
			usage.Costs = usage.Amount * product.Price;
		}
	}
}
