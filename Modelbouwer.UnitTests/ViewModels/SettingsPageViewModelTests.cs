namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class SettingsPageViewModelTests
{
	[TestMethod]
	public async Task Constructor_WhenLoadSettingsFails_StoresAsyncError()
	{
		var expected = new InvalidOperationException( "Unable to load settings." );
		var settingsService = new Mock<ISettingsService>();
		settingsService
			.Setup( service => service.LoadSettingsAsync() )
			.Returns( Task.FromException( expected ) );

		var viewModel = new SettingsPageViewModel( settingsService.Object );

		await WaitUntilAsync( () => ReferenceEquals( expected, viewModel.LastAsyncError ) );
		Assert.AreSame( expected, viewModel.LastAsyncError );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadSettingsSucceeds_AppliesSettings()
	{
		var settings = new AppSettings
		{
			Culture = "en-US",
			Language = "EN",
			HourRate = 17.5
		};
		var settingsService = new Mock<ISettingsService>();
		settingsService.Setup( service => service.Settings ).Returns( settings );
		settingsService.Setup( service => service.LoadSettingsAsync() ).Returns( Task.CompletedTask );

		var viewModel = new SettingsPageViewModel( settingsService.Object );

		await WaitUntilAsync( () => viewModel.SelectedRegion == "en-US" );
		Assert.AreEqual( "en-US", viewModel.SelectedRegion );
		Assert.AreEqual( "EN", viewModel.SelectedLanguage );
		Assert.AreEqual( 17.5, viewModel.HourRate );
		Assert.AreEqual( "17,50", viewModel.HourRateText );
		Assert.IsFalse( viewModel.HasUnsavedChanges );
	}

	[TestMethod]
	public void SettingsPageViewModel_SaveCommandUsesIsSavingAndUnsavedChangesForCanExecute()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "SettingsPageViewModel.cs" );

		StringAssert.Contains( source, "SaveSettingsCommand = new AsyncRelayCommand( SaveSettingsAsync, CanSaveSettings );" );
		StringAssert.Contains( source, "private bool CanSaveSettings() => HasUnsavedChanges && !IsSaving;" );
		StringAssert.Contains( source, "partial void OnIsSavingChanged( bool value ) => SaveSettingsCommand.NotifyCanExecuteChanged();" );
		StringAssert.Contains( source, "partial void OnHasUnsavedChangesChanged( bool value ) => SaveSettingsCommand.NotifyCanExecuteChanged();" );
		AssertMethodContains( source, "private async Task SaveSettingsAsync()", "if ( IsSaving )" );
	}

	private static async Task WaitUntilAsync( Func<bool> condition )
	{
		using var timeout = new CancellationTokenSource( TimeSpan.FromSeconds( 2 ) );

		while ( !condition() )
		{
			if ( timeout.IsCancellationRequested )
				Assert.Fail( "Condition was not met before timeout." );

			await Task.Delay( 10 );
		}
	}

	private static string LoadSource( params string[] relativeSegments )
	{
		var directory = AppContext.BaseDirectory;
		while ( directory != null && !File.Exists( Path.Combine( directory, "ModelbouwWerkbank.slnx" ) ) )
		{
			directory = Directory.GetParent( directory )?.FullName;
		}

		var repositoryRoot = directory ?? throw new DirectoryNotFoundException( "Could not locate repository root." );
		var path = Path.Combine( [ repositoryRoot, .. relativeSegments ] );

		return File.ReadAllText( path );
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
}
