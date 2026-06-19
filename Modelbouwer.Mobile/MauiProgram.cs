using Microsoft.Extensions.Logging;

using Modelbouwer.Mobile.Services;
using Modelbouwer.Mobile.ViewModels;
using Modelbouwer.Mobile.Views;

namespace Modelbouwer.Mobile;

public static class MauiProgram
{
	public static IServiceProvider? Services { get; private set; }

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts( fonts =>
			{
				fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" );
				fonts.AddFont( "OpenSans-Semibold.ttf", "OpenSansSemibold" );
			} );

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<MobileDbConnectionSettings>();
		builder.Services.AddSingleton<IMobileWorkspaceService, LazyMobileWorkspaceService>();
		builder.Services.AddTransient<AppShell>();
		builder.Services.AddTransient<RegistrationViewModel>();
		builder.Services.AddTransient<ProductsViewModel>();
		builder.Services.AddTransient<ProjectsViewModel>();
		builder.Services.AddTransient<TimeRegistrationPage>();
		builder.Services.AddTransient<MaterialUsagePage>();
		builder.Services.AddTransient<ProductsPage>();
		builder.Services.AddTransient<ProjectsPage>();

		var app = builder.Build();
		Services = app.Services;
		return app;
	}
}