using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class ProjectReportsViewModel : ObservableObject
{
	private readonly IProjectService _projectService;
	private readonly ITimeRegistrationService _timeRegistrationService;

	[ObservableProperty] private ProjectModel? _selectedProject;
	[ObservableProperty] private bool _isLoading;

	public ObservableCollection<ProjectModel> Projects { get; } = [];
	public ObservableCollection<TimeReportItemModel> WeekdayHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> MonthHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> YearHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> MonthYearHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> WorktypeHours { get; } = [];

	public IAsyncRelayCommand RefreshCommand { get; }

	public double TotalHours => WorktypeHours.Sum( item => item.Hours );
	public bool HasReportData =>
		WeekdayHours.Count > 0 ||
		MonthHours.Count > 0 ||
		YearHours.Count > 0 ||
		MonthYearHours.Count > 0 ||
		WorktypeHours.Count > 0;

	public ProjectReportsViewModel( IProjectService projectService, ITimeRegistrationService timeRegistrationService )
	{
		_projectService = projectService;
		_timeRegistrationService = timeRegistrationService;
		RefreshCommand = new AsyncRelayCommand( LoadReportsAsync, () => SelectedProject != null && !IsLoading );

		_ = LoadProjectsAsync();
	}

	partial void OnSelectedProjectChanged( ProjectModel? value )
	{
		RefreshCommand.NotifyCanExecuteChanged();
		_ = LoadReportsAsync();
	}

	partial void OnIsLoadingChanged( bool value ) => RefreshCommand.NotifyCanExecuteChanged();

	private async Task LoadProjectsAsync()
	{
		IsLoading = true;
		try
		{
			Projects.Clear();
			foreach ( var project in await _projectService.GetAllProjectsAsync() )
				Projects.Add( project );

			SelectedProject = Projects.FirstOrDefault();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async Task LoadReportsAsync()
	{
		if ( SelectedProject == null )
			return;

		IsLoading = true;
		try
		{
			await ReplaceItemsAsync( WeekdayHours, () => _timeRegistrationService.GetWorkedHoursByWeekdayAsync( SelectedProject.ProjectId ) );
			await ReplaceItemsAsync( MonthHours, () => _timeRegistrationService.GetWorkedHoursByMonthAsync( SelectedProject.ProjectId ) );
			await ReplaceItemsAsync( YearHours, () => _timeRegistrationService.GetWorkedHoursByYearAsync( SelectedProject.ProjectId ) );
			await ReplaceItemsAsync( MonthYearHours, () => _timeRegistrationService.GetWorkedHoursByMonthYearAsync( SelectedProject.ProjectId ) );
			await ReplaceItemsAsync( WorktypeHours, () => _timeRegistrationService.GetWorkedHoursByWorktypeAsync( SelectedProject.ProjectId ) );
		}
		finally
		{
			IsLoading = false;
			OnPropertyChanged( nameof( TotalHours ) );
			OnPropertyChanged( nameof( HasReportData ) );
		}
	}

	private static async Task ReplaceItemsAsync( ObservableCollection<TimeReportItemModel> target, Func<Task<List<TimeReportItemModel>>> load )
	{
		target.Clear();
		foreach ( var item in await load() )
			target.Add( item );
	}
}
