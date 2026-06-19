namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class AboutPageViewModelTests
{
	[TestMethod]
	public void AboutPageViewModel_UsesSharedAsyncErrorObserver()
	{
		Assert.IsTrue( typeof( AsyncObservableObject ).IsAssignableFrom( typeof( AboutPageViewModel ) ) );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadCommitsSucceeds_AddsCommits()
	{
		var commits = new List<ReleaseCommitModel>
		{
			new() { Sha = "abc123", Summary = "Initial commit" }
		};
		var releaseHistoryService = new Mock<IGitHubReleaseHistoryService>();
		releaseHistoryService
			.Setup( service => service.GetCommitsAsync( 1, It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( commits );

		var viewModel = new AboutPageViewModel( releaseHistoryService.Object );

		await WaitUntilAsync( () => viewModel.Commits.Count == 1 );
		Assert.AreSame( commits [ 0 ], viewModel.Commits [ 0 ] );
		Assert.AreEqual( string.Empty, viewModel.StatusMessage );
	}

	[TestMethod]
	public async Task Constructor_WhenLoadCommitsFails_SetsStatusMessage()
	{
		var releaseHistoryService = new Mock<IGitHubReleaseHistoryService>();
		releaseHistoryService
			.Setup( service => service.GetCommitsAsync( 1, It.IsAny<CancellationToken>() ) )
			.ThrowsAsync( new InvalidOperationException( "GitHub unavailable." ) );

		var viewModel = new AboutPageViewModel( releaseHistoryService.Object );

		await WaitUntilAsync( () => viewModel.StatusMessage.Contains( "GitHub unavailable.", StringComparison.Ordinal ) );
		StringAssert.Contains( viewModel.StatusMessage, "GitHub unavailable." );
	}

	[TestMethod]
	public async Task ShowCommitDetailCommand_WhenLoadFails_SetsDetailMessage()
	{
		var commit = new ReleaseCommitModel { Sha = "abc123", Summary = "Initial commit" };
		var releaseHistoryService = new Mock<IGitHubReleaseHistoryService>();
		releaseHistoryService
			.Setup( service => service.GetCommitsAsync( 1, It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( [ ] );
		releaseHistoryService
			.Setup( service => service.GetCommitMessageAsync( commit.Sha, It.IsAny<CancellationToken>() ) )
			.ThrowsAsync( new InvalidOperationException( "Commit unavailable." ) );
		var viewModel = new AboutPageViewModel( releaseHistoryService.Object );

		await viewModel.ShowCommitDetailCommand.ExecuteAsync( commit );

		StringAssert.Contains( viewModel.DetailMessage, "Commit unavailable." );
		Assert.IsTrue( viewModel.IsDetailOpen );
		Assert.AreSame( commit, viewModel.SelectedCommit );
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
}