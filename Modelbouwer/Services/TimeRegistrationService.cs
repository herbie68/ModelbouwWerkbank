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
		if ( SettingsService.TryParseSettingsDouble( value, out var hourRate ) )
			return hourRate;

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

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByWeekdayAsync( int projectId )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId );
		var culture = await GetCultureAsync();
		return BuildReport(
			entries,
			entry => ( ( int ) entry.WorkDate.DayOfWeek + 6 ) % 7,
			entry => culture.DateTimeFormat.GetDayName( entry.WorkDate.DayOfWeek ),
			sortOrder => sortOrder );
	}

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByMonthAsync( int projectId )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId );
		var culture = await GetCultureAsync();
		return BuildReport(
			entries,
			entry => entry.WorkDate.Month,
			entry => culture.DateTimeFormat.GetMonthName( entry.WorkDate.Month ),
			sortOrder => sortOrder );
	}

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByYearAsync( int projectId )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId );
		return BuildReport(
			entries,
			entry => entry.WorkDate.Year,
			entry => entry.WorkDate.Year.ToString( CultureInfo.CurrentCulture ),
			sortOrder => sortOrder );
	}

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByMonthYearAsync( int projectId )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId );
		var culture = await GetCultureAsync();
		return BuildReport(
			entries,
			entry => ( entry.WorkDate.Year * 100 ) + entry.WorkDate.Month,
			entry => $"{culture.DateTimeFormat.GetMonthName( entry.WorkDate.Month )} {entry.WorkDate.Year}",
			sortOrder => sortOrder );
	}

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByWorktypeAsync( int projectId )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId );
		var worktypes = await GetWorktypeLookupAsync();
		var grouped = entries
			.Where( entry => entry.WorkedMinutes > 0 )
			.GroupBy( entry => entry.WorktypeId )
			.Select( group =>
			{
				var first = group.First();
				var worktypeName = GetWorktypeName( first.WorktypeId, first.WorktypeName, worktypes );
				var groupName = GetRootWorktypeName( first.WorktypeId, worktypeName, worktypes );
				var sortOrder = GetWorktypeSortOrder( first.WorktypeId, worktypes );

				return new
				{
					Name = worktypeName,
					GroupName = groupName,
					Hours = group.Sum( entry => entry.WorkedMinutes ) / 60,
					SortOrder = sortOrder
				};
			} )
			.OrderBy( item => item.SortOrder )
			.ThenBy( item => item.Name )
			.ToList();

		var totalHours = grouped.Sum( item => item.Hours );
		return grouped
			.Select( item => new TimeReportItemModel
			{
				Name = item.Name,
				WorktypeName = item.Name,
				WorktypeGroupName = item.GroupName,
				Hours = item.Hours,
				Percentage = totalHours <= 0 ? 0 : item.Hours / totalHours,
				SortOrder = item.SortOrder
			} )
			.ToList();
	}

	public async Task<List<CostAllocationReportItemModel>> GetCostAllocationByWorktypeAsync( int projectId, bool includeHoursInCosts, double hourRate )
	{
		var worktypeHours = await GetWorkedHoursByWorktypeAsync( projectId );
		var materialCosts = ( await GetMaterialUsageByProjectAsync( projectId ) )
			.Sum( usage => usage.Costs );

		if ( worktypeHours.Count == 0 )
			return [];

		return worktypeHours
			.Select( worktype =>
			{
				var allocatedMaterialCosts = materialCosts * worktype.Percentage;
				var timeCosts = includeHoursInCosts ? worktype.Hours * hourRate : 0;

				return new CostAllocationReportItemModel
				{
					Name = worktype.Name,
					WorktypeGroupName = worktype.WorktypeGroupName,
					WorktypeName = worktype.Name,
					Hours = worktype.Hours,
					Percentage = worktype.Percentage,
					WorktypePercentage = worktype.Percentage,
					MaterialCosts = allocatedMaterialCosts,
					TimeCosts = timeCosts,
					TotalCosts = allocatedMaterialCosts + timeCosts
				};
			} )
			.Where( item => item.TotalCosts > 0 )
			.OrderByDescending( item => item.TotalCosts )
			.ThenBy( item => item.WorktypeName )
			.ToList();
	}

	public async Task<List<CostDeclarationReportItemModel>> GetCostDeclarationsAsync( int projectId )
	{
		return ( await GetMaterialUsageByProjectAsync( projectId ) )
			.Where( usage => usage.Costs > 0 )
			.OrderByDescending( usage => usage.UsageDate )
			.ThenBy( usage => usage.CategoryName )
			.ThenBy( usage => usage.ProductName )
			.Select( usage => new CostDeclarationReportItemModel
			{
				UsageDate = usage.UsageDate,
				ProductName = usage.ProductName ?? string.Empty,
				CategoryName = string.IsNullOrWhiteSpace( usage.CategoryName ) ? "?" : usage.CategoryName!,
				Amount = usage.Amount,
				UnitPrice = usage.Price,
				TotalCosts = usage.Costs,
				Comment = usage.Comment
			} )
			.ToList();
	}

	public async Task<List<CostReportItemModel>> GetCostDeclarationSummaryAsync( int projectId, bool includeHoursInCosts, double hourRate )
	{
		var declarations = await GetCostDeclarationsAsync( projectId );
		var items = declarations
			.GroupBy( item => item.CategoryName )
			.Select( group => new
			{
				Name = group.Key,
				TotalCosts = group.Sum( item => item.TotalCosts )
			} )
			.ToList();

		if ( includeHoursInCosts )
		{
			var totalHours = ( await GetWorkedHoursByWorktypeAsync( projectId ) ).Sum( item => item.Hours );
			var timeCosts = totalHours * hourRate;
			if ( timeCosts > 0 )
				items.Add( new { Name = Lang.TimeRegistrationTimeCostsDescription, TotalCosts = timeCosts } );
		}

		var totalCosts = items.Sum( item => item.TotalCosts );
		return items
			.Select( item => new CostReportItemModel
			{
				Name = item.Name,
				TotalCosts = item.TotalCosts,
				Percentage = totalCosts <= 0 ? 0 : item.TotalCosts / totalCosts
			} )
			.OrderByDescending( item => item.TotalCosts )
			.ThenBy( item => item.Name )
			.ToList();
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

	private static List<TimeReportItemModel> BuildReport<TKey>(
		IEnumerable<TimeEntryModel> entries,
		Func<TimeEntryModel, TKey> keySelector,
		Func<TimeEntryModel, string> nameSelector,
		Func<TKey, int> sortOrderSelector )
		where TKey : notnull
	{
		var grouped = entries
			.Where( entry => entry.WorkedMinutes > 0 )
			.GroupBy( keySelector )
			.Select( group =>
			{
				var first = group.First();
				return new
				{
					Name = nameSelector( first ),
					Hours = group.Sum( entry => entry.WorkedMinutes ) / 60,
					SortOrder = sortOrderSelector( group.Key )
				};
			} )
			.OrderBy( item => item.SortOrder )
			.ThenBy( item => item.Name )
			.ToList();

		var totalHours = grouped.Sum( item => item.Hours );
		return grouped
			.Select( item => new TimeReportItemModel
			{
				Name = item.Name,
				Hours = item.Hours,
				Percentage = totalHours <= 0 ? 0 : item.Hours / totalHours,
				SortOrder = item.SortOrder
			} )
			.ToList();
	}

	private Task<List<WorktypeModel>> GetWorktypesAsync()
	{
		string query =
			$"SELECT {DBNames.WorktypeFieldNameId}, {DBNames.WorktypeFieldNameParentId}, {DBNames.WorktypeFieldNameName} " +
			$"FROM {DBNames.Database}.{DBNames.WorktypeTable};";

		return _dataService.ExecuteQueryAsync( query, reader => new WorktypeModel
		{
			WorktypeId = DatabaseValueConverter.GetInt( reader[DBNames.WorktypeFieldNameId] ),
			ParentId = DatabaseValueConverter.GetInt( reader[DBNames.WorktypeFieldNameParentId] ),
			WorktypeName = DatabaseValueConverter.GetString( reader[DBNames.WorktypeFieldNameName] )
		} );
	}

	private async Task<Dictionary<int, WorktypeModel>> GetWorktypeLookupAsync() =>
		( await GetWorktypesAsync() )
			.Where( worktype => worktype.WorktypeId > 0 )
			.ToDictionary( worktype => worktype.WorktypeId );

	private static string GetWorktypeName( int worktypeId, string? fallbackName, IReadOnlyDictionary<int, WorktypeModel> worktypes )
	{
		if ( worktypeId > 0 &&
			worktypes.TryGetValue( worktypeId, out var worktype ) &&
			!string.IsNullOrWhiteSpace( worktype.WorktypeName ) )
			return worktype.WorktypeName;

		return string.IsNullOrWhiteSpace( fallbackName ) ? "?" : fallbackName!;
	}

	private static string GetRootWorktypeName( int worktypeId, string fallbackName, IReadOnlyDictionary<int, WorktypeModel> worktypes )
	{
		if ( worktypeId <= 0 || !worktypes.TryGetValue( worktypeId, out var current ) )
			return fallbackName;

		var visited = new HashSet<int>();
		while ( current.ParentId is > 0 &&
			visited.Add( current.WorktypeId ) &&
			worktypes.TryGetValue( current.ParentId.Value, out var parent ) )
		{
			current = parent;
		}

		return string.IsNullOrWhiteSpace( current.WorktypeName ) ? fallbackName : current.WorktypeName;
	}

	private static int GetWorktypeSortOrder( int worktypeId, IReadOnlyDictionary<int, WorktypeModel> worktypes )
	{
		if ( worktypeId <= 0 || !worktypes.TryGetValue( worktypeId, out var current ) )
			return 0;

		var rootId = current.WorktypeId;
		var visited = new HashSet<int>();
		while ( current.ParentId is > 0 &&
			visited.Add( current.WorktypeId ) &&
			worktypes.TryGetValue( current.ParentId.Value, out var parent ) )
		{
			current = parent;
			rootId = current.WorktypeId;
		}

		return ( rootId * 1000 ) + worktypeId;
	}

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
