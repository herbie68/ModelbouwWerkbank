namespace Modelbouwer.Mobile.Services;

public sealed class MobileDbConnectionSettings
{
	private const string DefaultEncodedIp = "3444, 4920, 5328, 3696";
	private string? connectionString;

	public string Database { get; init; } = "modelbuilder";
	public string Port { get; init; } = "3306";
	public string UserId { get; init; } = "root";
	public string Password { get; init; } = "OefenenKHMK24!";

	public async Task<string> GetConnectionStringAsync()
	{
		if ( connectionString is not null )
			return connectionString;

		var encodedIp = DefaultEncodedIp;
		try
		{
			await using var stream = await FileSystem.OpenAppPackageFileAsync("modelbouwer.config");
			using var reader = new StreamReader(stream);
			var fileContent = await reader.ReadToEndAsync();
			if ( !string.IsNullOrWhiteSpace( fileContent ) )
				encodedIp = fileContent.Trim();
		}
		catch
		{
			encodedIp = DefaultEncodedIp;
		}

		var server = DecodeServer(encodedIp);
		connectionString = $"SERVER={server};PORT={Port};DATABASE={Database};UID={UserId};PASSWORD={Password};SslMode=Disabled;Connection Timeout=8;Default Command Timeout=20;";
		return connectionString;
	}

	public static string DecodeServer( string encoded )
	{
		var parts = encoded
			.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
			.Select(value => ((int.Parse(value) / 2) - 1200) / 6)
			.ToArray();

		if ( parts.Length != 4 )
			throw new InvalidOperationException( "De databaseconfiguratie bevat geen geldig IP-adres." );

		return string.Join( ".", parts );
	}
}