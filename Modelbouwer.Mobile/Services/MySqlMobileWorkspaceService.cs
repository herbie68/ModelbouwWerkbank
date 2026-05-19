using System.Collections.ObjectModel;
using System.Data.Common;
using System.Globalization;
using System.Text.Json;

using Modelbouwer.Mobile.Models;

using Microsoft.Maui.Storage;
using MySql.Data.MySqlClient;

namespace Modelbouwer.Mobile.Services;

public sealed class MySqlMobileWorkspaceService : IMobileWorkspaceService
{
	private const string ActiveTimerPreferenceKey = "time.registration.activeTimer";
	private readonly MobileDbConnectionSettings settings;

	public ObservableCollection<MobileProject> Projects { get; } = [ ];
	public ObservableCollection<MobileProduct> Products { get; } = [ ];
	public ObservableCollection<MobileWorkType> WorkTypes { get; } = [ ];
	public ObservableCollection<MobileCategory> Categories { get; } = [ ];
	public ObservableCollection<MobileUnit> Units { get; } = [ ];
	public ObservableCollection<MobileTimeEntry> TimeEntries { get; } = [ ];
	public ObservableCollection<MobileMaterialEntry> MaterialEntries { get; } = [ ];

	public MySqlMobileWorkspaceService( MobileDbConnectionSettings settings )
	{
		this.settings = settings;
	}

	public async Task LoadAsync()
	{
		var projects = await QueryAsync(
			"SELECT Id, Code, Name, StartDate, EndDate, Closed FROM modelbuilder.project ORDER BY Closed ASC, Name ASC;",
			reader => new MobileProject
			{
				Id = GetInt(reader["Id"]),
				Code = GetString(reader["Code"]),
				Name = GetString(reader["Name"]),
				StartDate = GetDateTime(reader["StartDate"], DateTime.Today),
				EndDate = GetNullableDateTime(reader["EndDate"]),
				IsClosed = GetInt(reader["Closed"]) != 0
			});

		var products = await QueryAsync(
			"""
            SELECT p.Id, p.Code, p.Name, p.Category_Id, p.Unit_Id, p.MinimalStock, p.Price,
                   IFNULL(c.Name, '') AS CategoryName,
                   IFNULL(u.Name, '') AS UnitName,
                   IFNULL(stock.InventoryAmount, 0) AS InventoryAmount
            FROM modelbuilder.product p
            LEFT JOIN modelbuilder.category c ON c.Id = p.Category_Id
            LEFT JOIN modelbuilder.unit u ON u.Id = p.Unit_Id
            LEFT JOIN (
                SELECT product_Id, SUM((AmountReceived - AmountUsed) + AmountCorrection) AS InventoryAmount
                FROM modelbuilder.stocklog
                GROUP BY product_Id
            ) stock ON stock.product_Id = p.Id
            WHERE p.Hide = 0
            ORDER BY p.Name ASC;
            """,
			reader => new MobileProduct
			{
				Id = GetInt(reader["Id"]),
				Code = GetString(reader["Code"]),
				Name = GetString(reader["Name"]),
				CategoryId = GetInt(reader["Category_Id"]),
				UnitId = GetInt(reader["Unit_Id"]),
				Category = GetString(reader["CategoryName"]),
				Unit = GetString(reader["UnitName"]),
				CurrentInventory = GetDouble(reader["InventoryAmount"]),
				MinimalStock = GetDouble(reader["MinimalStock"]),
				Price = GetDouble(reader["Price"])
			});

		var workTypes = BuildHierarchy(await QueryAsync(
			"SELECT Id, ParentId, Name FROM modelbuilder.worktype ORDER BY COALESCE(ParentId, Id), Name ASC;",
			reader => new MobileWorkType
			{
				Id = GetInt(reader["Id"]),
				ParentId = NormalizeParentId(GetNullableInt(reader["ParentId"])),
				Name = GetString(reader["Name"])
			}));

		var categories = BuildHierarchy(await QueryAsync(
			"SELECT Id, ParentId, Name FROM modelbuilder.category ORDER BY COALESCE(ParentId, Id), Name ASC;",
			reader => new MobileCategory
			{
				Id = GetInt(reader["Id"]),
				ParentId = NormalizeParentId(GetNullableInt(reader["ParentId"])),
				Name = GetString(reader["Name"])
			}));

		var units = await QueryAsync(
			"SELECT Id, Name FROM modelbuilder.unit ORDER BY Name ASC;",
			reader => new MobileUnit
			{
				Id = GetInt(reader["Id"]),
				Name = GetString(reader["Name"])
			});

		Replace( Projects, projects );
		Replace( Products, products );
		Replace( WorkTypes, workTypes );
		Replace( Categories, categories );
		Replace( Units, units );

		await ReloadRecentRegistrationsAsync();
	}

	public async Task AddProjectAsync( MobileProject project )
	{
		const string sql =
			"""
            INSERT INTO modelbuilder.project
            (Name, Code, StartDate, EndDate, ExpectedTime, Closed, Image, ImageRotationAngle, Memo)
            VALUES
            (@Name, @Code, @StartDate, @EndDate, NULL, @Closed, NULL, 0, @Memo);
            SELECT LAST_INSERT_ID();
            """;

		project.Id = await ExecuteScalarIntAsync( sql, new()
		{
			[ "@Name" ] = project.Name,
			[ "@Code" ] = project.Code,
			[ "@StartDate" ] = project.StartDate.Date,
			[ "@EndDate" ] = project.EndDate?.Date,
			[ "@Closed" ] = project.IsClosed ? 1 : 0
		} );

		Projects.Add( project );
	}

	public Task UpdateProjectAsync( MobileProject project )
	{
		const string sql =
			"""
            UPDATE modelbuilder.project
            SET Name = @Name,
                Code = @Code,
                StartDate = @StartDate,
                EndDate = @EndDate,
                Closed = @Closed
            WHERE Id = @Id;
            """;

		return ExecuteNonQueryAsync( sql, new()
		{
			[ "@Id" ] = project.Id,
			[ "@Name" ] = project.Name,
			[ "@Code" ] = project.Code,
			[ "@StartDate" ] = project.StartDate.Date,
			[ "@EndDate" ] = project.EndDate?.Date,
			[ "@Closed" ] = project.IsClosed ? 1 : 0
		} );
	}

	public async Task AddProductAsync( MobileProduct product )
	{
		product.CategoryId = product.CategoryId == 0 ? Categories.FirstOrDefault()?.Id ?? await GetFirstIdAsync( "category" ) : product.CategoryId;
		product.UnitId = product.UnitId == 0 ? Units.FirstOrDefault()?.Id ?? await GetFirstIdAsync( "unit" ) : product.UnitId;

		const string sql =
			"""
            INSERT INTO modelbuilder.product
            (Brand_Id, Category_Id, Code, Dimensions, Hide, Image, ImageRotationAngle, MinimalStock, Name, Price, ProjectCosts, StandardOrderQuantity, Storage_Id, Unit_Id)
            VALUES
            (0, @CategoryId, @Code, '', 0, NULL, 0, @MinimalStock, @Name, @Price, 0, 1, 0, @UnitId);
            SELECT LAST_INSERT_ID();
            """;

		product.Id = await ExecuteScalarIntAsync( sql, new()
		{
			[ "@CategoryId" ] = product.CategoryId,
			[ "@Code" ] = product.Code,
			[ "@Memo" ] = string.Empty,
			[ "@MinimalStock" ] = product.MinimalStock,
			[ "@Name" ] = product.Name,
			[ "@Price" ] = product.Price,
			[ "@UnitId" ] = product.UnitId
		} );

		Products.Add( product );
	}

	public Task UpdateProductAsync( MobileProduct product )
	{
		const string sql =
			"""
            UPDATE modelbuilder.product
            SET Code = @Code,
                Category_Id = @CategoryId,
                Unit_Id = @UnitId,
                MinimalStock = @MinimalStock,
                Name = @Name,
                Price = @Price
            WHERE Id = @Id;
            """;

		return ExecuteNonQueryAsync( sql, new()
		{
			[ "@Id" ] = product.Id,
			[ "@CategoryId" ] = product.CategoryId,
			[ "@UnitId" ] = product.UnitId,
			[ "@Code" ] = product.Code,
			[ "@MinimalStock" ] = product.MinimalStock,
			[ "@Name" ] = product.Name,
			[ "@Price" ] = product.Price
		} );
	}

	public async Task AddTimeEntryAsync( MobileTimeEntry entry )
	{
		const string sql =
			"""
            INSERT INTO modelbuilder.time
            (project_Id, Worktype_Id, WorkDate, StartTime, EndTime, Comment)
            VALUES
            (@ProjectId, @WorktypeId, @WorkDate, @StartTime, @EndTime, @Comment);
            SELECT LAST_INSERT_ID();
            """;

		entry.Id = await ExecuteScalarIntAsync( sql, new()
		{
			[ "@ProjectId" ] = entry.Project?.Id ?? 0,
			[ "@WorktypeId" ] = entry.WorkTypeItem?.Id ?? 0,
			[ "@WorkDate" ] = entry.WorkDate.Date,
			[ "@StartTime" ] = FormatTime( entry.StartTime ),
			[ "@EndTime" ] = FormatTime( entry.EndTime ),
			[ "@Comment" ] = entry.Comment
		} );

		entry.WorkType = entry.WorkTypeItem?.Name ?? entry.WorkType;
		TimeEntries.Insert( 0, entry );
		SortTimeEntries();
	}

	public async Task AddMaterialEntryAsync( MobileMaterialEntry entry )
	{
		const string sql =
			"""
            INSERT INTO modelbuilder.productusage
            (project_Id, product_Id, AmountUsed, UsageDate, Comment)
            VALUES
            (@ProjectId, @ProductId, @AmountUsed, @UsageDate, @Comment);
            SELECT LAST_INSERT_ID();
            """;

		entry.Id = await ExecuteScalarIntAsync( sql, new()
		{
			[ "@ProjectId" ] = entry.Project?.Id ?? 0,
			[ "@ProductId" ] = entry.Product?.Id ?? 0,
			[ "@AmountUsed" ] = entry.Amount,
			[ "@UsageDate" ] = entry.UsageDate.Date,
			[ "@Comment" ] = entry.Comment
		} );

		MaterialEntries.Insert( 0, entry );
		SortMaterialEntries();
	}

	public Task<MobileTimerSession?> GetActiveTimerAsync()
	{
		var json = Preferences.Default.Get( ActiveTimerPreferenceKey, string.Empty );
		if ( string.IsNullOrWhiteSpace( json ) )
			return Task.FromResult<MobileTimerSession?>( null );

		try
		{
			var stored = JsonSerializer.Deserialize<StoredTimerSession>( json );
			if ( stored is null )
				return Task.FromResult<MobileTimerSession?>( null );

			var project = FindProject( stored.ProjectId, stored.ProjectName );
			var workType = FindWorkType( stored.WorkTypeId, stored.WorkTypeName );
			return Task.FromResult<MobileTimerSession?>( new MobileTimerSession
			{
				Project = project,
				WorkTypeItem = workType,
				WorkDate = stored.WorkDate.Date,
				StartTime = stored.StartTime,
				Comment = stored.Comment
			} );
		}
		catch ( JsonException )
		{
			Preferences.Default.Remove( ActiveTimerPreferenceKey );
			return Task.FromResult<MobileTimerSession?>( null );
		}
	}

	public async Task StartTimerAsync( MobileTimerSession session )
	{
		if ( await GetActiveTimerAsync() is not null )
			throw new InvalidOperationException( "Er loopt al een timer." );

		var stored = new StoredTimerSession
		{
			ProjectId = session.Project?.Id ?? 0,
			ProjectName = session.Project?.Name ?? string.Empty,
			WorkTypeId = session.WorkTypeItem?.Id ?? 0,
			WorkTypeName = session.WorkTypeItem?.Name ?? string.Empty,
			WorkDate = session.WorkDate.Date,
			StartTime = session.StartTime,
			Comment = session.Comment
		};

		Preferences.Default.Set( ActiveTimerPreferenceKey, JsonSerializer.Serialize( stored ) );
	}

	public Task ClearActiveTimerAsync()
	{
		Preferences.Default.Remove( ActiveTimerPreferenceKey );
		return Task.CompletedTask;
	}

	private async Task ReloadRecentRegistrationsAsync()
	{
		var timeEntries = await QueryAsync(
			"""
            SELECT t.Id, t.ProjectId, t.ProjectName, t.WorktypeId, t.WorktypeName, t.WorkDate, t.StartTime, t.EndTime, t.Comment
            FROM modelbuilder.view_time t
            ORDER BY t.WorkDate DESC, t.StartTime DESC;
            """,
			reader =>
			{
				var project = FindProject(GetInt(reader["ProjectId"]), GetString(reader["ProjectName"]));
				var workType = FindWorkType(GetInt(reader["WorktypeId"]), GetString(reader["WorktypeName"]));
				return new MobileTimeEntry
				{
					Id = GetInt(reader["Id"]),
					Project = project,
					WorkTypeItem = workType,
					WorkType = workType.Name,
					WorkDate = GetDateTime(reader["WorkDate"], DateTime.Today),
					StartTime = ParseTime(reader["StartTime"]),
					EndTime = ParseTime(reader["EndTime"]),
					Comment = GetString(reader["Comment"])
				};
			});

		var materialEntries = await QueryAsync(
			"""
            SELECT pu.Id,
                   pu.project_Id AS ProjectId,
                   pr.Name AS ProjectName,
                   pu.product_Id AS ProductId,
                   p.Name AS ProductName,
                   pu.AmountUsed,
                   pu.UsageDate,
                   p.Price,
                   pu.Comment
            FROM modelbuilder.productusage pu
            LEFT JOIN modelbuilder.project pr ON pr.Id = pu.project_Id
            LEFT JOIN modelbuilder.product p ON p.Id = pu.product_Id
            ORDER BY pu.UsageDate DESC, pu.Id DESC;
            """,
			reader =>
			{
				var product = FindProduct(GetInt(reader["ProductId"]), GetString(reader["ProductName"]), GetDouble(reader["Price"]));
				return new MobileMaterialEntry
				{
					Id = GetInt(reader["Id"]),
					Project = FindProject(GetInt(reader["ProjectId"]), GetString(reader["ProjectName"])),
					Product = product,
					Amount = GetDouble(reader["AmountUsed"]),
					UsageDate = GetDateTime(reader["UsageDate"], DateTime.Today),
					Price = product.Price,
					Comment = GetString(reader["Comment"])
				};
			});

		Replace( TimeEntries, SortTimeEntries( timeEntries ) );
		Replace( MaterialEntries, SortMaterialEntries( materialEntries ) );
	}

	private void SortTimeEntries()
	{
		Replace( TimeEntries, SortTimeEntries( TimeEntries ) );
	}

	private void SortMaterialEntries()
	{
		Replace( MaterialEntries, SortMaterialEntries( MaterialEntries ) );
	}

	private static List<MobileTimeEntry> SortTimeEntries( IEnumerable<MobileTimeEntry> entries )
	{
		return entries
			.OrderByDescending( entry => entry.WorkDate.Date )
			.ThenByDescending( entry => entry.StartTime )
			.ThenByDescending( entry => entry.Id )
			.ToList();
	}

	private static List<MobileMaterialEntry> SortMaterialEntries( IEnumerable<MobileMaterialEntry> entries )
	{
		return entries
			.OrderByDescending( entry => entry.UsageDate.Date )
			.ThenByDescending( entry => entry.Id )
			.ToList();
	}

	private async Task<List<T>> QueryAsync<T>( string sql, Func<DbDataReader, T> map, Dictionary<string, object?>? parameters = null )
	{
		var result = new List<T>();
		await using var connection = await OpenConnectionAsync();
		await using var command = new MySqlCommand(sql, connection);
		AddParameters( command, parameters );
		await using var reader = await command.ExecuteReaderAsync();
		while ( await reader.ReadAsync() )
			result.Add( map( reader ) );
		return result;
	}

	private async Task ExecuteNonQueryAsync( string sql, Dictionary<string, object?> parameters )
	{
		await using var connection = await OpenConnectionAsync();
		await using var command = new MySqlCommand(sql, connection);
		AddParameters( command, parameters );
		await command.ExecuteNonQueryAsync();
	}

	private async Task<int> ExecuteScalarIntAsync( string sql, Dictionary<string, object?> parameters )
	{
		await using var connection = await OpenConnectionAsync();
		await using var command = new MySqlCommand(sql, connection);
		AddParameters( command, parameters );
		var value = await command.ExecuteScalarAsync();
		return Convert.ToInt32( value );
	}

	private async Task<MySqlConnection> OpenConnectionAsync()
	{
		var connection = new MySqlConnection(await settings.GetConnectionStringAsync());
		await connection.OpenAsync();
		return connection;
	}

	private Task<int> GetFirstIdAsync( string table )
	{
		return ExecuteScalarIntAsync( $"SELECT Id FROM modelbuilder.{table} ORDER BY Id LIMIT 1;", [ ] );
	}

	private static void AddParameters( MySqlCommand command, Dictionary<string, object?>? parameters )
	{
		if ( parameters is null )
			return;

		foreach ( var parameter in parameters )
			command.Parameters.AddWithValue( parameter.Key, parameter.Value ?? DBNull.Value );
	}

	private MobileProject FindProject( int id, string name )
	{
		return Projects.FirstOrDefault( project => project.Id == id )
			?? new MobileProject { Id = id, Name = name };
	}

	private MobileProduct FindProduct( int id, string name, double price )
	{
		return Products.FirstOrDefault( product => product.Id == id )
			?? new MobileProduct { Id = id, Name = name, Price = price };
	}

	private MobileWorkType FindWorkType( int id, string name )
	{
		return WorkTypes.FirstOrDefault( workType => workType.Id == id )
			?? new MobileWorkType { Id = id, Name = name };
	}

	private static void Replace<T>( ObservableCollection<T> target, IEnumerable<T> items )
	{
		target.Clear();
		foreach ( var item in items )
			target.Add( item );
	}

	private static List<MobileWorkType> BuildHierarchy( List<MobileWorkType> items )
	{
		var result = new List<MobileWorkType>();
		var childrenByParent = items
			.GroupBy(item => item.ParentId ?? 0)
			.ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name).ToList());

		void AddChildren( int parentId, int depth )
		{
			if ( !childrenByParent.TryGetValue( parentId, out var children ) )
				return;

			foreach ( var child in children )
			{
				child.DisplayName = $"{new string( ' ', depth * 2 )}{child.Name}";
				result.Add( child );
				AddChildren( child.Id, depth + 1 );
			}
		}

		AddChildren( 0, 0 );
		foreach ( var orphan in items.Where( item => !result.Contains( item ) ).OrderBy( item => item.Name ) )
		{
			orphan.DisplayName = orphan.Name;
			result.Add( orphan );
		}

		return result;
	}

	private static List<MobileCategory> BuildHierarchy( List<MobileCategory> items )
	{
		var result = new List<MobileCategory>();
		var childrenByParent = items
			.GroupBy(item => item.ParentId ?? 0)
			.ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name).ToList());

		void AddChildren( int parentId, int depth )
		{
			if ( !childrenByParent.TryGetValue( parentId, out var children ) )
				return;

			foreach ( var child in children )
			{
				child.DisplayName = $"{new string( ' ', depth * 2 )}{child.Name}";
				result.Add( child );
				AddChildren( child.Id, depth + 1 );
			}
		}

		AddChildren( 0, 0 );
		foreach ( var orphan in items.Where( item => !result.Contains( item ) ).OrderBy( item => item.Name ) )
		{
			orphan.DisplayName = orphan.Name;
			result.Add( orphan );
		}

		return result;
	}

	private static string GetString( object value ) => value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;
	private static int GetInt( object value ) => value == DBNull.Value ? 0 : Convert.ToInt32( value );
	private static int? GetNullableInt( object value ) => value == DBNull.Value ? null : Convert.ToInt32( value );
	private static int? NormalizeParentId( int? value ) => value is null or 0 ? null : value;
	private static double GetDouble( object value ) => value == DBNull.Value ? 0 : Convert.ToDouble( value );
	private static DateTime GetDateTime( object value, DateTime fallback )
	{
		return TryGetDateTime( value, out var dateTime ) ? dateTime : fallback;
	}

	private static DateTime? GetNullableDateTime( object value )
	{
		return TryGetDateTime( value, out var dateTime ) ? dateTime : null;
	}

	private static bool TryGetDateTime( object value, out DateTime dateTime )
	{
		dateTime = default;
		if ( value == DBNull.Value || value is null )
			return false;

		if ( value is DateTime typedDateTime )
		{
			dateTime = typedDateTime;
			return true;
		}

		var text = value.ToString();
		if ( string.IsNullOrWhiteSpace( text ) )
			return false;

		string[] formats =
		[
			"dd-MM-yyyy",
			"d-M-yyyy",
			"yyyy-MM-dd",
			"yyyy-MM-dd HH:mm:ss",
			"dd-MM-yyyy HH:mm:ss",
			"d-M-yyyy H:mm:ss"
		];

		return DateTime.TryParseExact( text, formats, CultureInfo.GetCultureInfo( "nl-NL" ), DateTimeStyles.None, out dateTime ) ||
			DateTime.TryParse( text, CultureInfo.GetCultureInfo( "nl-NL" ), DateTimeStyles.None, out dateTime ) ||
			DateTime.TryParse( text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime );
	}
	private static string FormatTime( TimeSpan time ) => time.ToString( @"hh\:mm" );

	private static TimeSpan ParseTime( object value )
	{
		var text = GetString(value);
		return TimeSpan.TryParse( text, out var parsed ) ? parsed : TimeSpan.Zero;
	}

	private sealed class StoredTimerSession
	{
		public int ProjectId { get; set; }
		public string ProjectName { get; set; } = string.Empty;
		public int WorkTypeId { get; set; }
		public string WorkTypeName { get; set; } = string.Empty;
		public DateTime WorkDate { get; set; }
		public TimeSpan StartTime { get; set; }
		public string Comment { get; set; } = string.Empty;
	}
}
