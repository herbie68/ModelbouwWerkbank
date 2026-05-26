namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class EntityPageViewModelTests
{
	[TestMethod]
	public void EntityPageViewModel_SaveCommandsAreGuardedAgainstParallelExecution()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "EntityPageViewModel.cs" );

		StringAssert.Contains( source, "new AsyncRelayCommand( SaveAsync, CanSave )" );
		StringAssert.Contains( source, "partial void OnIsSavingChanged( bool value ) => NotifySaveCommandsCanExecuteChanged();" );
		StringAssert.Contains( source, "partial void OnHasUnsavedChangesChanged( bool value ) => NotifySaveCommandsCanExecuteChanged();" );
		StringAssert.Contains( source, "private bool CanSave() => HasUnsavedChanges && !IsSaving;" );
		AssertMethodContains( source, "private async Task SaveAsync()", "if ( IsSaving )" );
	}

	[TestMethod]
	public void EntityPageViewModel_DeleteCommandsAreGuardedAgainstParallelExecution()
	{
		var source = LoadSource( "Modelbouwer", "ViewModels", "EntityPageViewModel.cs" );

		StringAssert.Contains( source, "[ObservableProperty] protected bool _isDeleting;" );
		StringAssert.Contains( source, "new AsyncRelayCommand( DeleteCommandAsync, CanDelete )" );
		StringAssert.Contains( source, "partial void OnIsDeletingChanged( bool value ) => NotifyDeleteCommandsCanExecuteChanged();" );
		StringAssert.Contains( source, "private bool CanDelete() => SelectedItem != null && !IsDeleting;" );
		AssertMethodContains( source, "private async Task DeleteCommandAsync()", "if ( IsDeleting )" );
		AssertMethodContains( source, "private async Task DeleteCommandAsync()", "IsDeleting = true;" );
		AssertMethodContains( source, "private async Task DeleteCommandAsync()", "finally" );
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
