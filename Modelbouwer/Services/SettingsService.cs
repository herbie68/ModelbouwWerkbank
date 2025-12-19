using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Services;

public class SettingsService
{
	private static readonly SettingsService _instance =new();
	public static SettingsService Instance => _instance;

	string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	public AppSettings Settings { get; private set; } = new AppSettings();
	private SettingsService() { }

	public async Task LoadSettingsAsync()
	{
		string downloadsPath = Path.Combine(userProfile, "Downloads");

		var SqlQuery = "SELECT * FROM settings";
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
				case "HourRate":
					if ( double.TryParse( value, out var rate ) )
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

	public async Task SaveSettingAsync( string key, string value )
	{
		using var connection = new MySqlConnection(DBConnect.ConnectionString);
		await connection.OpenAsync();

		var command = new MySqlCommand(
			"INSERT INTO Settings(`Key`,`Value`) VALUES(@key,@value) " +
			"ON DUPLICATE KEY UPDATE `Value` = @value", connection);
		command.Parameters.AddWithValue( "@key", key );
		command.Parameters.AddWithValue( "@value", value );
		await command.ExecuteNonQueryAsync();
	}
}
