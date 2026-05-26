namespace Modelbouwer.Interfaces;

public interface ISettingsService
{
	AppSettings Settings { get; }
	event EventHandler? SettingsChanged;
	Task LoadSettingsAsync();
	Task LoadSettingsAsync( CancellationToken cancellationToken );
	Task<string?> GetSettingsAsync( string key );
	Task<string?> GetSettingsAsync( string key, CancellationToken cancellationToken );
	Task ResetSettingsAsync( string key );
	Task ResetSettingsAsync( string key, CancellationToken cancellationToken );
	Task SaveSettingAsync( string key, string value );
	Task SaveSettingAsync( string key, string value, CancellationToken cancellationToken );
	void NotifySettingsChanged();
}
