namespace Modelbouwer.Mobile;

public partial class App : Application
{
    private readonly AppShell shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
                WriteCrashLog(ex);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            WriteCrashLog(args.Exception);
            args.SetObserved();
        };

        this.shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(shell);
    }

    private static void WriteCrashLog(Exception ex)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "crash.log");
            File.WriteAllText(path, ex.ToString());
        }
        catch
        {
            // Last-chance crash logging must never throw.
        }
    }
}
