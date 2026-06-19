using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modelbouwer.Services;

public class GitHubReleaseHistoryService : IGitHubReleaseHistoryService
{
	private const string Owner = "hnsoftwaredevelopment";
	private const string Repository = "ModelbouwWerkbank";
	private const int PageSize = 25;

	private static readonly HttpClient HttpClient = CreateHttpClient();
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public async Task<IReadOnlyList<ReleaseCommitModel>> GetCommitsAsync( int page, CancellationToken cancellationToken = default )
	{
		var url = $"repos/{Owner}/{Repository}/commits?per_page={PageSize}&page={page}";
		var commits = await GetFromGitHubAsync<List<GitHubCommitListItem>>( url, cancellationToken );

		return commits
			.Where( item => !IsMergeCommit( item.Commit.Message ) )
			.Select( item => new ReleaseCommitModel
			{
				Sha = item.Sha,
				Date = item.Commit.Author.Date,
				DateText = item.Commit.Author.Date.ToLocalTime().ToString( "dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture ),
				Author = item.Commit.Author.Name,
				Summary = GetSummary( item.Commit.Message )
			} )
			.OrderByDescending( item => item.Date )
			.ToList();
	}

	public async Task<string> GetCommitMessageAsync( string sha, CancellationToken cancellationToken = default )
	{
		var commit = await GetFromGitHubAsync<GitHubCommitDetail>( $"repos/{Owner}/{Repository}/commits/{sha}", cancellationToken );
		return commit.Commit.Message;
	}

	private static async Task<T> GetFromGitHubAsync<T>( string relativeUrl, CancellationToken cancellationToken )
	{
		using var response = await HttpClient.GetAsync( relativeUrl, cancellationToken );
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync( cancellationToken );
		return await JsonSerializer.DeserializeAsync<T>( stream, JsonOptions, cancellationToken )
			?? throw new InvalidOperationException( "GitHub gaf geen bruikbare release-informatie terug." );
	}

	private static HttpClient CreateHttpClient()
	{
		var client = new HttpClient
		{
			BaseAddress = new Uri( "https://api.github.com/" )
		};

		client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue( "Modelbouwer", "1.0" ) );
		client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/vnd.github+json" ) );
		return client;
	}

	private static bool IsMergeCommit( string message ) =>
		message.StartsWith( "Merge ", StringComparison.OrdinalIgnoreCase );

	private static string GetSummary( string message )
	{
		var firstLine = message
			.Split( [ "\r\n", "\n" ], StringSplitOptions.None )
			.FirstOrDefault();

		return string.IsNullOrWhiteSpace( firstLine ) ? Lang.generalUnknownVersion : firstLine.Trim();
	}

	private sealed class GitHubCommitListItem
	{
		[JsonPropertyName( "sha" )]
		public string Sha { get; set; } = string.Empty;

		[JsonPropertyName( "commit" )]
		public GitHubCommit Commit { get; set; } = new();
	}

	private sealed class GitHubCommitDetail
	{
		[JsonPropertyName( "commit" )]
		public GitHubCommit Commit { get; set; } = new();
	}

	private sealed class GitHubCommit
	{
		[JsonPropertyName( "message" )]
		public string Message { get; set; } = string.Empty;

		[JsonPropertyName( "author" )]
		public GitHubCommitAuthor Author { get; set; } = new();
	}

	private sealed class GitHubCommitAuthor
	{
		[JsonPropertyName( "name" )]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName( "date" )]
		public DateTimeOffset Date { get; set; }
	}
}