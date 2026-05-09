namespace Modelbouwer.Services;

public class SettingsService
{
	private static readonly CultureInfo SettingsNumberCulture = new("nl-NL");
	string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	public AppSettings Settings { get; private set; } = new AppSettings();
	public event EventHandler? SettingsChanged;
	public SettingsService() { }

	public async Task LoadSettingsAsync()
	{
		string downloadsPath = Path.Combine(userProfile, "Downloads");

		var SqlQuery = $"SELECT * FROM {DBNames.Database}.{DBNames.SettingsTable}";
		using var connection = new MySqlConnection(DBConnect.ConnectionString);
		await connection.OpenAsync();

		var command = new MySqlCommand(SqlQuery, connection);
		using var reader = await command.ExecuteReaderAsync();

		while ( await reader.ReadAsync() )
		{
			var key = reader.GetString("Key");
			var value = reader.GetString("Value");

			switch ( key )
			{
				case "Culture":
					Settings.Culture = value;
					break;
				case "Language":
					Settings.Language = value;
					break;
				case "StockManagementGridLayout":
					Settings.StockManagementGridLayout = value;
					break;
				case "HourRate":
					if ( TryParseSettingsDouble( value, out var rate ) )
						Settings.HourRate = rate;
					break;
				case "ExportFolder":
					if ( string.IsNullOrWhiteSpace( value ) )
						Settings.ExportFolder = downloadsPath;
					else
						Settings.ExportFolder = value;
					break;
			}
		}
	}

	public async Task<string?> GetSettingsAsync( string key )
	{
		if ( key == null )
			return null;

		using var connection = new MySqlConnection(DBConnect.ConnectionString);
		await connection.OpenAsync();

		var command = new MySqlCommand(
		$"SELECT `Value` FROM {DBNames.Database}.{DBNames.SettingsTable} WHERE `Key` = @key",
		connection);

		command.Parameters.AddWithValue( "@key", key );

		var result = await command.ExecuteScalarAsync();

		return result?.ToString();
	}

	public async Task ResetSettingsAsync( string key )
	{
		if ( key == null )
			return;

		using var connection = new MySqlConnection(DBConnect.ConnectionString);
		await connection.OpenAsync();

		var command = new MySqlCommand(
		$"DELETE FROM {DBNames.Database}.{DBNames.SettingsTable} WHERE `Key` = @key",
		connection);

		command.Parameters.AddWithValue( "@key", key );

		await command.ExecuteNonQueryAsync();
	}

	public async Task SaveSettingAsync( string key, string value )
	{
		using var connection = new MySqlConnection(DBConnect.ConnectionString);
		await connection.OpenAsync();

		var command = new MySqlCommand(
			$"INSERT INTO {DBNames.Database}.{DBNames.SettingsTable}(`Key`,`Value`) VALUES(@key,@value) " +
			$"ON DUPLICATE KEY UPDATE `Value` = @value", connection);
		command.Parameters.AddWithValue( "@key", key );
		command.Parameters.AddWithValue( "@value", value );
		await command.ExecuteNonQueryAsync();
	}

	public static string FormatSettingsDouble( double value ) =>
		value.ToString( "0.00", SettingsNumberCulture );

	public static bool TryParseSettingsDouble( string? value, out double result )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
		{
			result = 0;
			return false;
		}

		var culture = value.Contains( ',' ) && !value.Contains( '.' )
			? SettingsNumberCulture
			: CultureInfo.InvariantCulture;

		if ( double.TryParse( value, NumberStyles.Any, culture, out result ) )
			return true;

		return double.TryParse( value, NumberStyles.Any, CultureInfo.CurrentCulture, out result );
	}

	public void NotifySettingsChanged() =>
		SettingsChanged?.Invoke( this, EventArgs.Empty );
}
