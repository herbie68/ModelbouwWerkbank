using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class SettingsPageViewModel : AsyncObservableObject
{
	private readonly ISettingsService _settingsService;
	private bool _isLoading;

	public ObservableCollection<SettingsOption> RegionOptions { get; } = [ ];
	public ObservableCollection<SettingsOption> LanguageOptions { get; } = [ ];

	[ObservableProperty] private string selectedRegion = "nl-NL";
	[ObservableProperty] private string selectedLanguage = "NL";
	[ObservableProperty] private double hourRate = 15.00;
	[ObservableProperty] private string hourRateText = "15,00";
	[ObservableProperty] private bool hasUnsavedChanges;
	[ObservableProperty] private bool isSaving;
	[ObservableProperty] private string statusMessage = string.Empty;

	public IAsyncRelayCommand SaveSettingsCommand { get; }
	public IAsyncRelayCommand ReloadSettingsCommand { get; }

	public SettingsPageViewModel( ISettingsService settingsService )
	{
		_settingsService = settingsService;

		RegionOptions.Add( new SettingsOption { DisplayName = Lang.SettingsRegionEurope, Value = "nl-NL" } );
		RegionOptions.Add( new SettingsOption { DisplayName = Lang.SettingsRegionUnitedStates, Value = "en-US" } );
		RegionOptions.Add( new SettingsOption { DisplayName = Lang.SettingsRegionEngland, Value = "en-GB" } );

		LanguageOptions.Add( new SettingsOption { DisplayName = Lang.SettingsLanguageDutch, Value = "NL" } );
		LanguageOptions.Add( new SettingsOption { DisplayName = Lang.SettingsLanguageEnglish, Value = "EN" } );
		LanguageOptions.Add( new SettingsOption { DisplayName = Lang.SettingsLanguageGerman, Value = "DE" } );

		SaveSettingsCommand = new AsyncRelayCommand( SaveSettingsAsync, CanSaveSettings );
		ReloadSettingsCommand = new AsyncRelayCommand( LoadSettingsAsync );

		ObserveBackgroundTask( LoadSettingsAsync() );
	}

	private async Task LoadSettingsAsync()
	{
		_isLoading = true;
		try
		{
			await _settingsService.LoadSettingsAsync();

			SelectedRegion = NormalizeRegion( _settingsService.Settings.Culture );
			SelectedLanguage = NormalizeLanguage( _settingsService.Settings.Language );
			HourRate = _settingsService.Settings.HourRate;
			HourRateText = SettingsService.FormatSettingsDouble( HourRate );
			StatusMessage = string.Empty;
			HasUnsavedChanges = false;
		}
		finally
		{
			_isLoading = false;
		}
	}

	private async Task SaveSettingsAsync()
	{
		if ( IsSaving )
			return;

		IsSaving = true;
		try
		{
			var normalizedRegion = NormalizeRegion( SelectedRegion );
			var normalizedLanguage = NormalizeLanguage( SelectedLanguage );
			if ( SettingsService.TryParseSettingsDouble( HourRateText, out var parsedHourRate ) )
				HourRate = parsedHourRate;

			var normalizedHourRate = SettingsService.FormatSettingsDouble( HourRate );
			HourRateText = normalizedHourRate;

			await _settingsService.SaveSettingAsync( "Culture", normalizedRegion );
			await _settingsService.SaveSettingAsync( "Language", normalizedLanguage );
			await _settingsService.SaveSettingAsync( "HourRate", normalizedHourRate );

			_settingsService.Settings.Culture = normalizedRegion;
			_settingsService.Settings.Language = normalizedLanguage;
			_settingsService.Settings.HourRate = HourRate;

			App.ApplyCulture( normalizedRegion, normalizedLanguage );
			HasUnsavedChanges = false;
			StatusMessage = Lang.SettingsSavedMessage;
			_settingsService.NotifySettingsChanged();
		}
		catch ( Exception ex )
		{
			StatusMessage = $"{Lang.SettingsSaveFailedMessage}: {ex.Message}";
		}
		finally
		{
			IsSaving = false;
		}
	}

	partial void OnSelectedRegionChanged( string value ) => MarkDirty();
	partial void OnSelectedLanguageChanged( string value ) => MarkDirty();
	partial void OnHourRateChanged( double value ) => MarkDirty();
	partial void OnHourRateTextChanged( string value ) => MarkDirty();
	partial void OnIsSavingChanged( bool value ) => SaveSettingsCommand.NotifyCanExecuteChanged();
	partial void OnHasUnsavedChangesChanged( bool value ) => SaveSettingsCommand.NotifyCanExecuteChanged();

	private bool CanSaveSettings() => HasUnsavedChanges && !IsSaving;

	private void MarkDirty()
	{
		if ( _isLoading )
			return;

		HasUnsavedChanges = true;
		StatusMessage = string.Empty;
	}

	private static string NormalizeRegion( string? culture ) =>
		culture switch
		{
			"en-US" => "en-US",
			"en-GB" => "en-GB",
			_ => "nl-NL"
		};

	private static string NormalizeLanguage( string? language ) =>
		language?.ToUpperInvariant() switch
		{
			"EN" => "EN",
			"DE" => "DE",
			_ => "NL"
		};
}