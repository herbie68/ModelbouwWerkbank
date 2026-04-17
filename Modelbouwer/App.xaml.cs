using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Modelbouwer.Validators;

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
		services.AddSingleton<BrandService>();
		services.AddSingleton<CategoryService>();
		services.AddSingleton<ContactService>();
		services.AddSingleton<ContactTypeService>();
		services.AddSingleton<CountryService>();
		services.AddSingleton<CurrencyService>();
		services.AddSingleton<GenericDataService>();
		services.AddSingleton<ProductService>();
		services.AddSingleton<ProjectService>();
		services.AddSingleton<SettingsService>();
		services.AddSingleton<StockService>();
		services.AddSingleton<StorageLocationService>();
		services.AddSingleton<SupplierService>();
		services.AddSingleton<UnitService>();
		services.AddSingleton<WorktypeService>();

		// Register ViewModels
		services.AddTransient<BrandPageViewModel>();
		services.AddTransient<CategoryPageViewModel>();
		services.AddTransient<ContactTypePageViewModel>();
		services.AddTransient<CountryPageViewModel>();
		services.AddTransient<CurrencyPageViewModel>();
		services.AddTransient<ProductPageViewModel>();
		services.AddTransient<ProjectPageViewModel>();
		services.AddTransient<StockManagementPageViewModel>();
		services.AddTransient<StockOrderViewModel>();
		services.AddTransient<StorageLocationPageViewModel>();
		services.AddTransient<SupplierPageViewModel>();
		services.AddTransient<UnitPageViewModel>();
		services.AddTransient<WorktypePageViewModel>();

		// Register NavigationViewModel
		services.AddSingleton<NavigationViewModel>();

		// Register Views
		services.AddTransient<BrandView>();
		services.AddTransient<CategoryView>();
		services.AddTransient<ContactTypeView>();
		services.AddTransient<CountryView>();
		services.AddTransient<CurrencyView>();
		services.AddTransient<ProductView>();
		services.AddTransient<ProjectView>();
		services.AddTransient<StockManagementView>();
		services.AddTransient<StockOrderView>();
		services.AddTransient<StorageLocationView>();
		services.AddTransient<SupplierView>();
		services.AddTransient<UnitView>();
		services.AddTransient<WorktypeView>();

		// Register MainWindow
		services.AddSingleton<MainWindow>();

		// Export services
		services.AddSingleton<ILanguageProvider, ResourceLanguageProvider>();
		services.AddSingleton<IExportService, CsvExportService>();
		services.AddSingleton<IExportService, ExcelExportService>();
		services.AddSingleton<CsvExportService>();
		services.AddSingleton<ExcelExportService>();
		services.AddSingleton<IExportService>( provider => provider.GetRequiredService<CsvExportService>() );

		services.AddScoped<IBrandService, BrandService>();
		services.AddScoped<ICategoryService, CategoryService>();
		services.AddScoped<IContactService, ContactService>();
		services.AddScoped<IContactTypeService, ContactTypeService>();
		services.AddScoped<ICountryService, CountryService>();
		services.AddScoped<ICurrencyService, CurrencyService>();
		services.AddScoped<IProductService, ProductService>();
		services.AddScoped<IProjectService, ProjectService>();
		services.AddScoped<IStockService, StockService>();
		services.AddScoped<IStorageLocationService, StorageLocationService>();
		services.AddScoped<ISupplierService, SupplierService>();
		services.AddScoped<IUnitService, UnitService>();
		services.AddScoped<IWorktypeService, WorktypeService>();

		services.AddScoped<IEntityValidator<BrandModel>, BrandValidator>();
		services.AddScoped<IEntityValidator<CategoryModel>, CategoryValidator>();
		services.AddScoped<IEntityValidator<ContactTypeModel>, ContactTypeValidator>();
		services.AddScoped<IEntityValidator<CountryModel>, CountryValidator>();
		services.AddScoped<IEntityValidator<CurrencyModel>, CurrencyValidator>();
		services.AddScoped<IEntityValidator<ProductModel>, ProductValidator>();
		services.AddScoped<IEntityValidator<ProjectModel>, ProjectValidator>();
		services.AddScoped<IEntityValidator<StockManagementModel>, StockManagementValidator>();
		services.AddScoped<IEntityValidator<StorageLocationModel>, StorageLocationValidator>();
		services.AddScoped<IEntityValidator<SupplierModel>, SupplierValidator>();
		services.AddScoped<IEntityValidator<SupplierContactModel>, ContactValidator>();
		services.AddScoped<IEntityValidator<UnitModel>, UnitValidator>();
		services.AddScoped<IEntityValidator<WorktypeModel>, WorktypeValidator>();
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