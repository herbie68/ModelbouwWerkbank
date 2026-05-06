using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class ProjectReportsViewModel : ObservableObject
{
	private readonly IProjectService _projectService;
	private readonly ITimeRegistrationService _timeRegistrationService;

	[ObservableProperty] private ProjectModel? _selectedProject;
	[ObservableProperty] private bool _isLoading;
	[ObservableProperty] private int _selectedReportTabIndex;
	[ObservableProperty] private bool _includeHoursInCosts = true;
	[ObservableProperty] private double _hourRate;

	public ObservableCollection<ProjectModel> Projects { get; } = [];
	public ObservableCollection<TimeReportItemModel> WeekdayHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> MonthHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> YearHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> MonthYearHours { get; } = [];
	public ObservableCollection<TimeReportItemModel> WorktypeHours { get; } = [];
	public ObservableCollection<CostAllocationReportItemModel> CostAllocationLines { get; } = [];
	public ObservableCollection<CostDeclarationReportItemModel> CostDeclarationLines { get; } = [];
	public ObservableCollection<CostReportItemModel> CostDeclarationSummary { get; } = [];

	public IAsyncRelayCommand RefreshCommand { get; }

	public double TotalHours => WorktypeHours.Sum( item => item.Hours );
	public double TotalMaterialCosts => CostDeclarationLines.Sum( item => item.TotalCosts );
	public double TotalReportCosts => CostDeclarationSummary.Sum( item => item.TotalCosts );
	public string HourRateDisplay => HourRate.ToString( "C2", CultureInfo.CurrentCulture );
	public bool HasReportData =>
		WeekdayHours.Count > 0 ||
		MonthHours.Count > 0 ||
		YearHours.Count > 0 ||
		MonthYearHours.Count > 0 ||
		WorktypeHours.Count > 0 ||
		CostAllocationLines.Count > 0 ||
		CostDeclarationLines.Count > 0;

	public ProjectReportsViewModel( IProjectService projectService, ITimeRegistrationService timeRegistrationService )
	{
		_projectService = projectService;
		_timeRegistrationService = timeRegistrationService;
		RefreshCommand = new AsyncRelayCommand( LoadReportsAsync, () => SelectedProject != null && !IsLoading );

		_ = LoadProjectsAsync();
	}

	public void SelectReportTab( int tabIndex ) => SelectedReportTabIndex = tabIndex;

	partial void OnSelectedProjectChanged( ProjectModel? value )
	{
		RefreshCommand.NotifyCanExecuteChanged();
		_ = LoadReportsAsync();
	}

	partial void OnIsLoadingChanged( bool value ) => RefreshCommand.NotifyCanExecuteChanged();

	partial void OnIncludeHoursInCostsChanged( bool value ) => _ = LoadReportsAsync();

	private async Task LoadProjectsAsync()
	{
		IsLoading = true;
		try
		{
			Projects.Clear();
			foreach ( var project in await _projectService.GetAllProjectsAsync() )
				Projects.Add( project );

			HourRate = await _timeRegistrationService.GetHourRateAsync();
			OnPropertyChanged( nameof( HourRateDisplay ) );
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
			await ReplaceItemsAsync( CostAllocationLines, () => _timeRegistrationService.GetCostAllocationByWorktypeAsync( SelectedProject.ProjectId, IncludeHoursInCosts, HourRate ) );
			await ReplaceItemsAsync( CostDeclarationLines, () => _timeRegistrationService.GetCostDeclarationsAsync( SelectedProject.ProjectId ) );
			await ReplaceItemsAsync( CostDeclarationSummary, () => _timeRegistrationService.GetCostDeclarationSummaryAsync( SelectedProject.ProjectId, IncludeHoursInCosts, HourRate ) );
		}
		finally
		{
			IsLoading = false;
			OnPropertyChanged( nameof( TotalHours ) );
			OnPropertyChanged( nameof( TotalMaterialCosts ) );
			OnPropertyChanged( nameof( TotalReportCosts ) );
			OnPropertyChanged( nameof( HasReportData ) );
		}
	}

	private static async Task ReplaceItemsAsync<T>( ObservableCollection<T> target, Func<Task<List<T>>> load )
	{
		target.Clear();
		foreach ( var item in await load() )
			target.Add( item );
	}
}
