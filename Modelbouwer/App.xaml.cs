using Modelbouwer.Services;

namespace Modelbouwer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1fdnVURmVZUUN+X0FWYEs=" );

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );
        CultureInfo culture = new("nl-NL");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

    }
	protected override async void OnStartup( StartupEventArgs e )
	{
		base.OnStartup( e );

		try
		{
			await SettingsService.Instance.LoadSettingsAsync();
		}
		catch ( Exception ex )
		{
			MessageBox.Show( $"Settings could not be loaded: {ex.Message}" );
		}

		//var mainWindow = new MainWindow();
		//mainWindow.Show();
	}
}
