namespace Modelbouwer.UnitTests.Views;

[TestClass]
public class AsyncSafetySourceTests
{
	private static readonly string[] ExportViewFiles =
	[
		"BrandView.xaml.cs",
		"CategoryView.xaml.cs",
		"ContactTypeView.xaml.cs",
		"CountryView.xaml.cs",
		"CurrencyView.xaml.cs",
		"ProductView.xaml.cs",
		"ProjectView.xaml.cs",
		"StorageLocationView.xaml.cs",
		"SupplierView.xaml.cs",
		"UnitView.xaml.cs",
		"WorktypeView.xaml.cs"
	];

	[TestMethod]
	public void ExportButtonHandlers_AreGuardedWithTryCatch()
	{
		foreach ( var fileName in ExportViewFiles )
		{
			var source = LoadSource( "Modelbouwer", "Views", fileName );

			AssertMethodContains( source, "private async void ButtonCSVExport", "try" );
			AssertMethodContains( source, "private async void ButtonExcelExport", "try" );
		}
	}

	[TestMethod]
	public void DispatcherBeginInvokeCalls_AreExplicitlyDiscarded()
	{
		foreach ( var fileName in ExportViewFiles )
		{
			var source = LoadSource( "Modelbouwer", "Views", fileName );

			Assert.IsFalse( source.Contains( "\n\t\tgrid.Dispatcher.BeginInvoke(", StringComparison.Ordinal ) );
		}

		var stockManagementView = LoadSource( "Modelbouwer", "Views", "StockManagementView.xaml.cs" );
		Assert.IsFalse( stockManagementView.Contains( "\n\t\tgrid.Dispatcher.BeginInvoke(", StringComparison.Ordinal ) );
	}

	[TestMethod]
	public void AsyncSupportMethods_AreGuardedWithTryCatch()
	{
		var navigationSource = LoadSource( "Modelbouwer", "ViewModels", "NavigationViewModel.cs" );
		AssertMethodContains( navigationSource, "private async Task LoadNavigationItemsAsync", "try" );

		var stockManagementView = LoadSource( "Modelbouwer", "Views", "StockManagementView.xaml.cs" );
		AssertMethodContains( stockManagementView, "private async void StockManagementView_Loaded", "try" );
		AssertMethodContains( stockManagementView, "private async void SaveGridLayout", "try" );
		AssertMethodContains( stockManagementView, "private async void ResetGridLayout", "try" );

		var stockManagementViewModel = LoadSource( "Modelbouwer", "ViewModels", "StockManagementPageViewModel.cs" );
		AssertMethodContains( stockManagementViewModel, "private async void Item_PropertyChanged", "try" );
	}

	[TestMethod]
	public void AsyncViewEventHandlers_AreGuardedWithTryCatch()
	{
		var aboutView = LoadSource( "Modelbouwer", "Views", "AboutView.xaml.cs" );
		AssertMethodContains( aboutView, "private async void CommitGrid_SelectionChanged", "try" );

		var timeRegistrationView = LoadSource( "Modelbouwer", "Views", "TimeRegistrationView.xaml.cs" );
		AssertMethodContains( timeRegistrationView, "private async void SaveTimeEntriesButton_Click", "try" );
	}

	[TestMethod]
	public void ExportServices_UseAwaitedDispatcherCallsForSuccessMessages()
	{
		var csvExportService = LoadSource( "Modelbouwer", "Services", "CsvExportService.cs" );
		Assert.IsFalse( csvExportService.Contains( "Dispatcher.BeginInvoke", StringComparison.Ordinal ) );

		var excelExportService = LoadSource( "Modelbouwer", "Services", "ExcelExportService.cs" );
		Assert.IsFalse( excelExportService.Contains( "Dispatcher.BeginInvoke", StringComparison.Ordinal ) );
	}

	private static void AssertMethodContains( string source, string methodSignature, string expectedContent )
	{
		var methodStart = source.IndexOf( methodSignature, StringComparison.Ordinal );
		Assert.IsTrue( methodStart >= 0, $"Method '{methodSignature}' was not found." );

		var nextMethod = source.IndexOf( "\n\tprivate ", methodStart + methodSignature.Length, StringComparison.Ordinal );
		if ( nextMethod < 0 )
			nextMethod = source.Length;

		var methodBody = source.Substring( methodStart, nextMethod - methodStart );
		StringAssert.Contains( methodBody, expectedContent );
	}

	private static string LoadSource( params string[] relativeSegments )
	{
		var root = FindSolutionRoot();
		var path = Path.Combine( [ root, .. relativeSegments ] );

		Assert.IsTrue( File.Exists( path ), $"Expected source file at '{path}'." );
		return File.ReadAllText( path );
	}

	private static string FindSolutionRoot()
	{
		var directory = new DirectoryInfo( AppContext.BaseDirectory );

		while ( directory != null )
		{
			if ( File.Exists( Path.Combine( directory.FullName, "ModelbouwWerkbank.slnx" ) ) )
				return directory.FullName;

			directory = directory.Parent;
		}

		Assert.Fail( "Could not locate the solution root from the test output directory." );
		return string.Empty;
	}
}
