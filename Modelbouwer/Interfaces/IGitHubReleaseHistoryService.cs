namespace Modelbouwer.Interfaces;

public interface IGitHubReleaseHistoryService
{
	Task<IReadOnlyList<ReleaseCommitModel>> GetCommitsAsync( int page, CancellationToken cancellationToken = default );
	Task<string> GetCommitMessageAsync( string sha, CancellationToken cancellationToken = default );
}