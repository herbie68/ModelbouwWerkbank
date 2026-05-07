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
	public ObservableCollection<PieChartSliceModel> WeekdayPieSlices { get; } = [];
	public ObservableCollection<PieChartSliceModel> MonthPieSlices { get; } = [];
	public ObservableCollection<PieChartSliceModel> YearPieSlices { get; } = [];
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
			ReplacePieSlices( WeekdayPieSlices, WeekdayHours );
			ReplacePieSlices( MonthPieSlices, MonthHours );
			ReplacePieSlices( YearPieSlices, YearHours );
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

	private static void ReplacePieSlices( ObservableCollection<PieChartSliceModel> target, IEnumerable<TimeReportItemModel> source )
	{
		target.Clear();

		var items = source
			.Where( item => item.Hours > 0 )
			.ToList();

		var totalHours = items.Sum( item => item.Hours );
		if ( totalHours <= 0 )
			return;

		var colors = GetPieChartBrushes();
		var startAngle = -90d;

		for ( var index = 0; index < items.Count; index++ )
		{
			var item = items[index];
			var percentage = item.Hours / totalHours;
			var sweepAngle = percentage * 360d;
			var fill = colors[index % colors.Length];

			target.Add( new PieChartSliceModel
			{
				Name = item.Name,
				Hours = item.Hours,
				Percentage = percentage,
				SliceGeometry = CreatePieSliceGeometry( startAngle, sweepAngle, 0 ),
				ShadowGeometry = CreatePieSliceGeometry( startAngle, sweepAngle, 11 ),
				Fill = fill,
				ShadowFill = DarkenBrush( fill )
			} );

			startAngle += sweepAngle;
		}
	}

	private static SolidColorBrush[] GetPieChartBrushes() =>
	[
		new( Color.FromRgb( 47, 128, 237 ) ),
		new( Color.FromRgb( 39, 174, 96 ) ),
		new( Color.FromRgb( 242, 153, 74 ) ),
		new( Color.FromRgb( 155, 81, 224 ) ),
		new( Color.FromRgb( 235, 87, 87 ) ),
		new( Color.FromRgb( 86, 204, 242 ) ),
		new( Color.FromRgb( 111, 207, 151 ) ),
		new( Color.FromRgb( 187, 107, 217 ) ),
		new( Color.FromRgb( 45, 156, 219 ) ),
		new( Color.FromRgb( 242, 201, 76 ) ),
		new( Color.FromRgb( 111, 125, 142 ) ),
		new( Color.FromRgb( 0, 150, 136 ) )
	];

	private static Brush DarkenBrush( SolidColorBrush brush )
	{
		var color = brush.Color;
		return new SolidColorBrush( Color.FromRgb(
			( byte ) ( color.R * 0.58 ),
			( byte ) ( color.G * 0.58 ),
			( byte ) ( color.B * 0.58 ) ) );
	}

	private static Geometry CreatePieSliceGeometry( double startAngle, double sweepAngle, double verticalOffset )
	{
		const double centerX = 105;
		const double centerY = 74;
		const double radiusX = 78;
		const double radiusY = 54;

		if ( sweepAngle >= 359.99 )
			return new EllipseGeometry( new Point( centerX, centerY + verticalOffset ), radiusX, radiusY );

		var start = PointOnEllipse( centerX, centerY + verticalOffset, radiusX, radiusY, startAngle );
		var end = PointOnEllipse( centerX, centerY + verticalOffset, radiusX, radiusY, startAngle + sweepAngle );
		var figure = new PathFigure
		{
			StartPoint = new Point( centerX, centerY + verticalOffset ),
			IsClosed = true,
			IsFilled = true
		};

		figure.Segments.Add( new LineSegment( start, true ) );
		figure.Segments.Add( new ArcSegment(
			end,
			new Size( radiusX, radiusY ),
			0,
			sweepAngle > 180,
			SweepDirection.Clockwise,
			true ) );
		figure.Segments.Add( new LineSegment( new Point( centerX, centerY + verticalOffset ), true ) );

		return new PathGeometry( [ figure ] );
	}

	private static Point PointOnEllipse( double centerX, double centerY, double radiusX, double radiusY, double angle )
	{
		var radians = angle * Math.PI / 180d;
		return new Point(
			centerX + radiusX * Math.Cos( radians ),
			centerY + radiusY * Math.Sin( radians ) );
	}
}
