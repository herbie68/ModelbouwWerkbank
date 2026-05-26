using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class AboutPageViewModel : AsyncObservableObject
{
	private readonly IGitHubReleaseHistoryService _releaseHistoryService;
	private int _currentPage;

	public ObservableCollection<ReleaseCommitModel> Commits { get; } = [];

	[ObservableProperty] private ReleaseCommitModel? selectedCommit;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isDetailOpen;
	[ObservableProperty] private string statusMessage = string.Empty;
	[ObservableProperty] private string detailMessage = string.Empty;

	public string AppVersion => NavigationViewModel.AppVersion;
	public string HeaderTitle => GetText( "AboutHeaderTitle", "Over Modelbouwer" );
	public string HeaderSubtitle => GetText( "AboutHeaderSubtitle", "Applicatie-informatie en release historie" );
	public string Description => GetText( "AboutDescription", "Modelbouwer Werkbank ondersteunt het beheren van modelbouwprojecten, voorraad, leveranciers, tijdregistratie en rapportages in een centrale werkruimte." );
	public string ReleaseHistoryTitle => GetText( "AboutReleaseHistoryTitle", "Release historie" );
	public string LoadMoreText => GetText( "AboutLoadMoreButton", "Meer laden" );
	public string CloseText => GetText( "generalButtonClose", "Sluiten" );
	public string DateHeader => GetText( "AboutGridDateHeader", "Datum" );
	public string AuthorHeader => GetText( "AboutGridAuthorHeader", "Auteur" );
	public string CommitTextHeader => GetText( "AboutGridCommitTextHeader", "Commit tekst" );

	public IAsyncRelayCommand LoadMoreCommand { get; }
	public IAsyncRelayCommand<ReleaseCommitModel> ShowCommitDetailCommand { get; }
	public IRelayCommand CloseDetailCommand { get; }

	public AboutPageViewModel( IGitHubReleaseHistoryService releaseHistoryService )
	{
		_releaseHistoryService = releaseHistoryService;
		LoadMoreCommand = new AsyncRelayCommand( LoadMoreAsync, () => !IsLoading );
		ShowCommitDetailCommand = new AsyncRelayCommand<ReleaseCommitModel>( ShowCommitDetailAsync );
		CloseDetailCommand = new RelayCommand( () => IsDetailOpen = false );

		ObserveBackgroundTask( LoadMoreAsync() );
	}

	private async Task LoadMoreAsync()
	{
		if ( IsLoading )
			return;

		IsLoading = true;
		StatusMessage = GetText( "AboutLoadingCommits", "Release historie wordt geladen..." );
		LoadMoreCommand.NotifyCanExecuteChanged();

		try
		{
			var nextPage = _currentPage + 1;
			var commits = await _releaseHistoryService.GetCommitsAsync( nextPage );

			foreach ( var commit in commits )
				Commits.Add( commit );

			_currentPage = nextPage;
			StatusMessage = commits.Count == 0
				? GetText( "AboutNoMoreCommits", "Er zijn geen extra commits gevonden." )
				: string.Empty;
		}
		catch ( Exception ex )
		{
			StatusMessage = $"{GetText( "AboutLoadFailed", "Release historie kon niet worden geladen" )}: {ex.Message}";
		}
		finally
		{
			IsLoading = false;
			LoadMoreCommand.NotifyCanExecuteChanged();
		}
	}

	private async Task ShowCommitDetailAsync( ReleaseCommitModel? commit )
	{
		if ( commit == null )
			return;

		SelectedCommit = commit;
		DetailMessage = GetText( "AboutLoadingCommitDetail", "Commitdetails worden geladen..." );
		IsDetailOpen = true;

		try
		{
			commit.FullMessage ??= await _releaseHistoryService.GetCommitMessageAsync( commit.Sha );
			DetailMessage = commit.FullMessage;
		}
		catch ( Exception ex )
		{
			DetailMessage = $"{GetText( "AboutCommitDetailFailed", "Commitdetails konden niet worden geladen" )}: {ex.Message}";
		}
	}

	private static string GetText( string key, string fallback ) =>
		Lang.ResourceManager.GetString( key, Lang.Culture ) ?? fallback;
}
