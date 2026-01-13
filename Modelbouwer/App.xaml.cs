using System;
using System.Globalization;
using System.Threading;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Modelbouwer.Interfaces;
using Modelbouwer.Services;
using Modelbouwer.Validators;
using Modelbouwer.ViewModels;
using Modelbouwer.Views;

namespace Modelbouwer;

public partial class App : Application
{
	private readonly IHost _host;

	public App()
	{
		// Set culture before anything else
		CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
		CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );
		CultureInfo culture = new("nl-NL");
		Thread.CurrentThread.CurrentCulture = culture;
		Thread.CurrentThread.CurrentUICulture = culture;

		_host = Host.CreateDefaultBuilder()
		   .ConfigureServices( ConfigureServices )
		   .Build();
	}

	private void ConfigureServices( IServiceCollection services )
	{
		// Register Syncfusion license
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1dcHVXQmVcU0dxX0BWYEs=" );

		// Register services
		services.AddSingleton<GenericDataService>();
		services.AddSingleton<BrandService>();
		services.AddSingleton<CategoryService>();
		services.AddSingleton<CurrencyService>();
		services.AddSingleton<CountryService>();

		// Register ViewModels
		services.AddTransient<BrandPageViewModel>();
		services.AddTransient<CategoryPageViewModel>();
		services.AddTransient<CurrencyPageViewModel>();
		services.AddTransient<CountryPageViewModel>();

		// Register NavigationViewModel
		services.AddSingleton<NavigationViewModel>();

		// Register Views
		services.AddTransient<BrandView>();
		services.AddTransient<CategoryView>();
		services.AddTransient<CountryView>();
		services.AddTransient<CurrencyView>();

		// Register MainWindow
		services.AddSingleton<MainWindow>();

		// Export services
		services.AddSingleton<ILanguageProvider, ResourceLanguageProvider>();
		services.AddSingleton<IExportService, CsvExportService>();
		services.AddSingleton<IExportService, ExcelExportService>();
		services.AddSingleton<CsvExportService>();
		services.AddSingleton<ExcelExportService>();
		services.AddSingleton<IExportService>(provider => provider.GetRequiredService<CsvExportService>() );
		services.AddScoped<IBrandService, BrandService>();
		services.AddScoped<ICategoryService, CategoryService>();
		services.AddScoped<ICountryService, CountryService>();
		services.AddScoped<ICurrencyService, CurrencyService>();
		services.AddScoped<IEntityValidator<BrandModel>, BrandValidator>();
		services.AddScoped<IEntityValidator<CategoryModel>, CategoryValidator>();
		services.AddScoped<IEntityValidator<CountryModel>, CountryValidator>();
		services.AddScoped<IEntityValidator<CurrencyModel>, CurrencyValidator>();
	}

	protected override async void OnStartup( StartupEventArgs e )
	{
		await _host.StartAsync();

		var navigationViewModel = _host.Services.GetRequiredService<NavigationViewModel>();
		var mainWindow = _host.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();

		base.OnStartup( e );
	}

	protected override async void OnExit( ExitEventArgs e )
	{
		await _host.StopAsync();
		_host.Dispose();

		base.OnExit( e );
	}

	public static T GetService<T>() where T : class
	{
		if ( ( Current as App )!._host.Services.GetService( typeof( T ) ) is not T service )
		{
			throw new ArgumentException( $"{typeof( T )} needs to be registered in ConfigureServices within App.xaml.cs." );
		}

		return service;
	}
}