namespace Modelbouwer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH5eeHRQQ2hZVEN+V0BWYEg=" );

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );
        CultureInfo culture = new("nl-NL");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

    }
}
