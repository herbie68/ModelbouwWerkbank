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

	public Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId ) =>
		GetTimeEntriesByProjectAsync( projectId, CancellationToken.None );

	public Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId, CancellationToken cancellationToken )
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
		}, new Dictionary<string, object> { { "@ProjectId", projectId } }, cancellationToken );
	}

	public Task<int> InsertTimeEntryAsync( TimeEntryModel entry ) =>
		InsertTimeEntryAsync( entry, CancellationToken.None );

	public async Task<int> InsertTimeEntryAsync( TimeEntryModel entry, CancellationToken cancellationToken )
	{
		string query =
			$"INSERT INTO {DBNames.Database}.{DBNames.TimeTable} " +
			$"({DBNames.TimeFieldNameProjectId}, {DBNames.TimeFieldNameWorktypeId}, {DBNames.TimeFieldNameWorkDate}, {DBNames.TimeFieldNameStartTime}, {DBNames.TimeFieldNameEndTime}, {DBNames.TimeFieldNameComment}) " +
			$"VALUES (@ProjectId, @WorktypeId, @WorkDate, @StartTime, @EndTime, @Comment); {DBNames.SqlSelectLastId}";

		uint id = await _dataService.ExecuteScalarAsync<uint>( query, CreateTimeParameters( entry ), cancellationToken );
		return ( int ) id;
	}

	public Task UpdateTimeEntryAsync( TimeEntryModel entry ) =>
		UpdateTimeEntryAsync( entry, CancellationToken.None );

	public Task UpdateTimeEntryAsync( TimeEntryModel entry, CancellationToken cancellationToken )
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
		return _dataService.ExecuteNonQueryAsync( query, parameters, cancellationToken );
	}

	public Task DeleteTimeEntryAsync( int timeEntryId ) =>
		DeleteTimeEntryAsync( timeEntryId, CancellationToken.None );

	public Task DeleteTimeEntryAsync( int timeEntryId, CancellationToken cancellationToken )
	{
		string query =
			$"DELETE FROM {DBNames.Database}.{DBNames.TimeTable} " +
			$"WHERE {DBNames.TimeFieldNameId} = @TimeId;";

		return _dataService.ExecuteNonQueryAsync( query, new Dictionary<string, object>
		{
			{ "@TimeId", timeEntryId }
		}, cancellationToken );
	}

	public Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId ) =>
		GetMaterialUsageByProjectAsync( projectId, CancellationToken.None );

	public async Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId, CancellationToken cancellationToken )
	{
		List<MaterialUsageModel> usages = [];

		await using ( MySqlConnection connection = new( DBConnect.ConnectionString ) )
		{
			await connection.OpenAsync( cancellationToken );

			await using MySqlCommand command = new( DBNames.SPGetProductsUsageByProject, connection );
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue( DBNames.SPGetProductsUsageByProjectInputParameter, projectId );

			await using MySqlDataReader reader = ( MySqlDataReader ) await command.ExecuteReaderAsync( cancellationToken );
			while ( await reader.ReadAsync( cancellationToken ) )
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

		await EnrichMaterialUsageAsync( usages, cancellationToken );
		foreach ( MaterialUsageModel usage in usages )
			usage.State = MaterialUsageModel.RecordState.Unchanged;

		return usages;
	}

	public Task<int> InsertMaterialUsageAsync( MaterialUsageModel usage ) =>
		InsertMaterialUsageAsync( usage, CancellationToken.None );

	public async Task<int> InsertMaterialUsageAsync( MaterialUsageModel usage, CancellationToken cancellationToken )
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
		}, cancellationToken );

		return ( int ) id;
	}

	public Task UpdateMaterialUsageAsync( MaterialUsageModel usage ) =>
		UpdateMaterialUsageAsync( usage, CancellationToken.None );

	public Task UpdateMaterialUsageAsync( MaterialUsageModel usage, CancellationToken cancellationToken )
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
		}, cancellationToken );
	}

	public Task DeleteMaterialUsageAsync( int materialUsageId ) =>
		DeleteMaterialUsageAsync( materialUsageId, CancellationToken.None );

	public Task DeleteMaterialUsageAsync( int materialUsageId, CancellationToken cancellationToken )
	{
		string query =
			$"DELETE FROM {DBNames.Database}.{DBNames.ProductUsageTable} " +
			$"WHERE {DBNames.ProductUsageFieldNameId} = @ProductUsageId;";

		return _dataService.ExecuteNonQueryAsync( query, new Dictionary<string, object>
		{
			{ "@ProductUsageId", materialUsageId }
		}, cancellationToken );
	}

	public Task<double> GetHourRateAsync() =>
		GetHourRateAsync( CancellationToken.None );

	public async Task<double> GetHourRateAsync( CancellationToken cancellationToken )
	{
		var value = await _settingsService.GetSettingsAsync( DBNames.SettingsFieldNameHourRate, cancellationToken );
		if ( SettingsService.TryParseSettingsDouble( value, out var hourRate ) )
			return hourRate;

		return 0;
	}

	public Task<CultureInfo> GetCultureAsync() =>
		GetCultureAsync( CancellationToken.None );

	public async Task<CultureInfo> GetCultureAsync( CancellationToken cancellationToken )
	{
		var value = await _settingsService.GetSettingsAsync( DBNames.SettingsFieldNameCulture, cancellationToken );
		try
		{
			return new CultureInfo( string.IsNullOrWhiteSpace( value ) ? "nl-NL" : value );
		}
		catch ( CultureNotFoundException )
		{
			return new CultureInfo( "nl-NL" );
		}
	}

	public Task<List<TimeReportItemModel>> GetWorkedHoursByWeekdayAsync( int projectId ) =>
		GetWorkedHoursByWeekdayAsync( projectId, CancellationToken.None );

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByWeekdayAsync( int projectId, CancellationToken cancellationToken )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		var culture = await GetCultureAsync( cancellationToken );
		return BuildReport(
			entries,
			entry => ( ( int ) entry.WorkDate.DayOfWeek + 6 ) % 7,
			entry => culture.DateTimeFormat.GetDayName( entry.WorkDate.DayOfWeek ),
			sortOrder => sortOrder );
	}

	public Task<List<TimeReportItemModel>> GetWorkedHoursByMonthAsync( int projectId ) =>
		GetWorkedHoursByMonthAsync( projectId, CancellationToken.None );

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByMonthAsync( int projectId, CancellationToken cancellationToken )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		var culture = await GetCultureAsync( cancellationToken );
		return BuildReport(
			entries,
			entry => entry.WorkDate.Month,
			entry => culture.DateTimeFormat.GetMonthName( entry.WorkDate.Month ),
			sortOrder => sortOrder );
	}

	public Task<List<TimeReportItemModel>> GetWorkedHoursByYearAsync( int projectId ) =>
		GetWorkedHoursByYearAsync( projectId, CancellationToken.None );

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByYearAsync( int projectId, CancellationToken cancellationToken )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		return BuildReport(
			entries,
			entry => entry.WorkDate.Year,
			entry => entry.WorkDate.Year.ToString( CultureInfo.CurrentCulture ),
			sortOrder => sortOrder );
	}

	public Task<List<TimeReportItemModel>> GetWorkedHoursByMonthYearAsync( int projectId ) =>
		GetWorkedHoursByMonthYearAsync( projectId, CancellationToken.None );

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByMonthYearAsync( int projectId, CancellationToken cancellationToken )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		var culture = await GetCultureAsync( cancellationToken );
		return BuildReport(
			entries,
			entry => ( entry.WorkDate.Year * 100 ) + entry.WorkDate.Month,
			entry => $"{culture.DateTimeFormat.GetMonthName( entry.WorkDate.Month )} {entry.WorkDate.Year}",
			sortOrder => sortOrder );
	}

	public Task<List<TimeReportItemModel>> GetWorkedHoursByWorktypeAsync( int projectId ) =>
		GetWorkedHoursByWorktypeAsync( projectId, CancellationToken.None );

	public async Task<List<TimeReportItemModel>> GetWorkedHoursByWorktypeAsync( int projectId, CancellationToken cancellationToken )
	{
		var entries = await GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		var worktypes = await GetWorktypeLookupAsync( cancellationToken );
		return BuildWorkedHoursByWorktype( entries, worktypes );
	}

	public Task<ProjectReportsDataModel> GetProjectReportsAsync( int projectId, bool includeHoursInCosts, double hourRate ) =>
		GetProjectReportsAsync( projectId, includeHoursInCosts, hourRate, CancellationToken.None );

	public async Task<ProjectReportsDataModel> GetProjectReportsAsync( int projectId, bool includeHoursInCosts, double hourRate, CancellationToken cancellationToken )
	{
		var entriesTask = GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		var usagesTask = GetMaterialUsageByProjectAsync( projectId, cancellationToken );
		var cultureTask = GetCultureAsync( cancellationToken );
		var worktypesTask = GetWorktypeLookupAsync( cancellationToken );

		await Task.WhenAll( entriesTask, usagesTask, cultureTask, worktypesTask );

		var entries = await entriesTask;
		var usages = await usagesTask;
		var culture = await cultureTask;
		var worktypes = await worktypesTask;
		var worktypeHours = BuildWorkedHoursByWorktype( entries, worktypes );
		var declarations = BuildCostDeclarations( usages );

		return new ProjectReportsDataModel
		{
			WeekdayHours = BuildReport(
				entries,
				entry => ( ( int ) entry.WorkDate.DayOfWeek + 6 ) % 7,
				entry => culture.DateTimeFormat.GetDayName( entry.WorkDate.DayOfWeek ),
				sortOrder => sortOrder ),
			MonthHours = BuildReport(
				entries,
				entry => entry.WorkDate.Month,
				entry => culture.DateTimeFormat.GetMonthName( entry.WorkDate.Month ),
				sortOrder => sortOrder ),
			YearHours = BuildReport(
				entries,
				entry => entry.WorkDate.Year,
				entry => entry.WorkDate.Year.ToString( CultureInfo.CurrentCulture ),
				sortOrder => sortOrder ),
			MonthYearHours = BuildReport(
				entries,
				entry => ( entry.WorkDate.Year * 100 ) + entry.WorkDate.Month,
				entry => $"{culture.DateTimeFormat.GetMonthName( entry.WorkDate.Month )} {entry.WorkDate.Year}",
				sortOrder => sortOrder ),
			WorktypeHours = worktypeHours,
			CostAllocationLines = BuildCostAllocationByWorktype( worktypeHours, usages, includeHoursInCosts, hourRate ),
			CostDeclarationLines = declarations,
			CostDeclarationSummary = BuildCostDeclarationSummary( declarations, worktypeHours, includeHoursInCosts, hourRate )
		};
	}

	private static List<TimeReportItemModel> BuildWorkedHoursByWorktype( IEnumerable<TimeEntryModel> entries, IReadOnlyDictionary<int, WorktypeModel> worktypes )
	{
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

	public Task<List<CostAllocationReportItemModel>> GetCostAllocationByWorktypeAsync( int projectId, bool includeHoursInCosts, double hourRate ) =>
		GetCostAllocationByWorktypeAsync( projectId, includeHoursInCosts, hourRate, CancellationToken.None );

	public async Task<List<CostAllocationReportItemModel>> GetCostAllocationByWorktypeAsync( int projectId, bool includeHoursInCosts, double hourRate, CancellationToken cancellationToken )
	{
		var worktypeHours = await GetWorkedHoursByWorktypeAsync( projectId, cancellationToken );
		var usages = await GetMaterialUsageByProjectAsync( projectId, cancellationToken );

		return BuildCostAllocationByWorktype( worktypeHours, usages, includeHoursInCosts, hourRate );
	}

	private static List<CostAllocationReportItemModel> BuildCostAllocationByWorktype(
		IReadOnlyCollection<TimeReportItemModel> worktypeHours,
		IEnumerable<MaterialUsageModel> usages,
		bool includeHoursInCosts,
		double hourRate )
	{
		var materialCosts = usages.Sum( usage => usage.Costs );

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

	public Task<List<CostDeclarationReportItemModel>> GetCostDeclarationsAsync( int projectId ) =>
		GetCostDeclarationsAsync( projectId, CancellationToken.None );

	public async Task<List<CostDeclarationReportItemModel>> GetCostDeclarationsAsync( int projectId, CancellationToken cancellationToken )
	{
		var usages = await GetMaterialUsageByProjectAsync( projectId, cancellationToken );
		return BuildCostDeclarations( usages );
	}

	private static List<CostDeclarationReportItemModel> BuildCostDeclarations( IEnumerable<MaterialUsageModel> usages )
	{
		return usages
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

	public Task<List<CostReportItemModel>> GetCostDeclarationSummaryAsync( int projectId, bool includeHoursInCosts, double hourRate ) =>
		GetCostDeclarationSummaryAsync( projectId, includeHoursInCosts, hourRate, CancellationToken.None );

	public async Task<List<CostReportItemModel>> GetCostDeclarationSummaryAsync( int projectId, bool includeHoursInCosts, double hourRate, CancellationToken cancellationToken )
	{
		var declarations = await GetCostDeclarationsAsync( projectId, cancellationToken );
		var worktypeHours = includeHoursInCosts
			? await GetWorkedHoursByWorktypeAsync( projectId, cancellationToken )
			: [];

		return BuildCostDeclarationSummary( declarations, worktypeHours, includeHoursInCosts, hourRate );
	}

	private static List<CostReportItemModel> BuildCostDeclarationSummary(
		IEnumerable<CostDeclarationReportItemModel> declarations,
		IEnumerable<TimeReportItemModel> worktypeHours,
		bool includeHoursInCosts,
		double hourRate )
	{
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
			var totalHours = worktypeHours.Sum( item => item.Hours );
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

	private Task<List<WorktypeModel>> GetWorktypesAsync( CancellationToken cancellationToken )
	{
		string query =
			$"SELECT {DBNames.WorktypeFieldNameId}, {DBNames.WorktypeFieldNameParentId}, {DBNames.WorktypeFieldNameName} " +
			$"FROM {DBNames.Database}.{DBNames.WorktypeTable};";

		return _dataService.ExecuteQueryAsync( query, reader => new WorktypeModel
		{
			WorktypeId = DatabaseValueConverter.GetInt( reader[DBNames.WorktypeFieldNameId] ),
			ParentId = DatabaseValueConverter.GetInt( reader[DBNames.WorktypeFieldNameParentId] ),
			WorktypeName = DatabaseValueConverter.GetString( reader[DBNames.WorktypeFieldNameName] )
		}, null, cancellationToken );
	}

	private async Task<Dictionary<int, WorktypeModel>> GetWorktypeLookupAsync( CancellationToken cancellationToken ) =>
		( await GetWorktypesAsync( cancellationToken ) )
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

	private async Task EnrichMaterialUsageAsync( List<MaterialUsageModel> usages, CancellationToken cancellationToken )
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
		}, parameters, cancellationToken );

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
