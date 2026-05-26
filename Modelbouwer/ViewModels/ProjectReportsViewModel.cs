using CommunityToolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.Charts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Modelbouwer.ViewModels;

public partial class ProjectReportsViewModel : AsyncObservableObject
{
	private readonly IProjectService _projectService;
	private readonly ITimeRegistrationService _timeRegistrationService;
	private CancellationTokenSource? _loadReportsCancellationTokenSource;

	[ObservableProperty] private ProjectModel? _selectedProject;
	[ObservableProperty] private bool _isLoading;
	[ObservableProperty] private int _selectedReportTabIndex;
	[ObservableProperty] private bool _includeHoursInCosts = true;
	[ObservableProperty] private double _hourRate;
	[ObservableProperty] private bool _weekdayShowsTable;
	[ObservableProperty] private bool _monthShowsTable;
	[ObservableProperty] private bool _yearShowsTable;
	[ObservableProperty] private bool _monthYearShowsTable;
	[ObservableProperty] private bool _worktypeShowsTable;
	[ObservableProperty] private bool _costAllocationShowsTable;
	[ObservableProperty] private bool _costDeclarationsShowsTable;
	[ObservableProperty] private ChartSeriesCollection _monthYearStackedSeries = [];
	[ObservableProperty] private ChartSeriesCollection _worktypeStackedSeries = [];
	[ObservableProperty] private ChartSeriesCollection _costAllocationStackedSeries = [];
	[ObservableProperty] private ChartSeriesCollection _costDeclarationStackedSeries = [];

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
		RefreshCommand = new AsyncRelayCommand( StartLoadReportsAsync, () => SelectedProject != null && !IsLoading );

		ObserveBackgroundTask( LoadProjectsAsync() );
	}

	public void SelectReportTab( int tabIndex ) => SelectedReportTabIndex = tabIndex;

	partial void OnSelectedProjectChanged( ProjectModel? value )
	{
		RefreshCommand.NotifyCanExecuteChanged();
		ObserveBackgroundTask( StartLoadReportsAsync() );
	}

	partial void OnIsLoadingChanged( bool value ) => RefreshCommand.NotifyCanExecuteChanged();

	partial void OnIncludeHoursInCostsChanged( bool value ) => ObserveBackgroundTask( StartLoadReportsAsync() );

	private async Task LoadProjectsAsync()
	{
		IsLoading = true;
		try
		{
			var projectsTask = _projectService.GetAllProjectsAsync();
			var hourRateTask = _timeRegistrationService.GetHourRateAsync();

			await Task.WhenAll( projectsTask, hourRateTask );

			Projects.Clear();
			foreach ( var project in await projectsTask )
				Projects.Add( project );

			HourRate = await hourRateTask;
			OnPropertyChanged( nameof( HourRateDisplay ) );
			SelectedProject = Projects.FirstOrDefault();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private Task StartLoadReportsAsync()
	{
		_loadReportsCancellationTokenSource?.Cancel();
		_loadReportsCancellationTokenSource?.Dispose();
		_loadReportsCancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _loadReportsCancellationTokenSource.Token;

		return LoadReportsAsync( cancellationToken );
	}

	private async Task LoadReportsAsync( CancellationToken cancellationToken )
	{
		var selectedProject = SelectedProject;
		if ( selectedProject == null )
			return;

		IsLoading = true;
		try
		{
			var reports = await PerformanceTrace.MeasureAsync(
				$"{nameof( ProjectReportsViewModel )}.{nameof( LoadReportsAsync )}",
				() => _timeRegistrationService.GetProjectReportsAsync( selectedProject.ProjectId, IncludeHoursInCosts, HourRate, cancellationToken ) );
			cancellationToken.ThrowIfCancellationRequested();

			ReplaceItems( WeekdayHours, reports.WeekdayHours );
			ReplaceItems( MonthHours, reports.MonthHours );
			ReplaceItems( YearHours, reports.YearHours );
			ReplaceItems( MonthYearHours, reports.MonthYearHours );
			ReplaceItems( WorktypeHours, reports.WorktypeHours );
			ReplaceItems( CostAllocationLines, reports.CostAllocationLines );
			ReplaceItems( CostDeclarationLines, reports.CostDeclarationLines );
			ReplaceItems( CostDeclarationSummary, reports.CostDeclarationSummary );
			BuildCharts();
		}
		finally
		{
			if ( !cancellationToken.IsCancellationRequested )
				IsLoading = false;

			OnPropertyChanged( nameof( TotalHours ) );
			OnPropertyChanged( nameof( TotalMaterialCosts ) );
			OnPropertyChanged( nameof( TotalReportCosts ) );
			OnPropertyChanged( nameof( HasReportData ) );
		}
	}

	private static void ReplaceItems<T>( ObservableCollection<T> target, IEnumerable<T> items )
	{
		target.Clear();
		foreach ( var item in items )
			target.Add( item );
	}

	private void BuildCharts()
	{
		MonthYearStackedSeries = BuildMonthYearStackedSeries();
		WorktypeStackedSeries = BuildWorktypeStackedSeries();
		CostAllocationStackedSeries = BuildCostAllocationStackedSeries();
		CostDeclarationStackedSeries = BuildCostDeclarationStackedSeries();
	}

	private ChartSeriesCollection BuildMonthYearStackedSeries()
	{
		var rows = MonthYearHours
			.Where( item => item.Hours > 0 && item.SortOrder > 0 )
			.Select( item => new
			{
				Year = item.SortOrder / 100,
				Month = item.SortOrder % 100,
				item.Name,
				item.Hours
			} )
			.OrderBy( item => item.Year )
			.ThenBy( item => item.Month )
			.ToList();

		var years = rows.Select( item => item.Year ).Distinct().OrderBy( year => year ).ToArray();
		var monthNames = rows
			.GroupBy( item => item.Month )
			.ToDictionary( group => group.Key, group => group.First().Name.Split( ' ' )[0] );

		var colors = GetChartColors();
		var series = new ChartSeriesCollection();
		foreach ( var ( month, index ) in Enumerable.Range( 1, 12 ).Where( month => rows.Any( item => item.Month == month ) ).Select( ( month, index ) => ( month, index ) ) )
		{
			var label = monthNames.TryGetValue( month, out var monthName ) ? monthName : month.ToString( CultureInfo.CurrentCulture );
			var points = years
				.Select( year => CreateHoursChartPoint( year.ToString( CultureInfo.CurrentCulture ), rows.Where( item => item.Year == year && item.Month == month ).Sum( item => item.Hours ) ) )
				.ToList();
			series.Add( CreateColumnSeries( label, points, colors[index % colors.Length] ) );
		}

		return series;
	}

	private ChartSeriesCollection BuildWorktypeStackedSeries()
	{
		var rows = WorktypeHours
			.Where( item => item.Hours > 0 )
			.ToList();

		var groups = rows
			.Select( item => string.IsNullOrWhiteSpace( item.WorktypeGroupName ) ? item.Name : item.WorktypeGroupName )
			.Distinct()
			.OrderBy( group => group )
			.ToArray();

		var worktypes = rows
			.Select( item => item.Name )
			.Distinct()
			.OrderBy( name => name )
			.ToArray();

		var colors = GetChartColors();
		var series = new ChartSeriesCollection();
		foreach ( var ( worktype, index ) in worktypes.Select( ( worktype, index ) => ( worktype, index ) ) )
		{
			var points = groups
				.Select( group => CreateHoursChartPoint( group, rows
					.Where( item => item.Name == worktype && ( string.IsNullOrWhiteSpace( item.WorktypeGroupName ) ? item.Name : item.WorktypeGroupName ) == group )
					.Sum( item => item.Hours ) ) )
				.ToList();
			series.Add( CreateColumnSeries( worktype, points, colors[index % colors.Length] ) );
		}

		return series;
	}

	private ChartSeriesCollection BuildCostAllocationStackedSeries()
	{
		var groups = CostAllocationLines
			.Select( item => string.IsNullOrWhiteSpace( item.WorktypeGroupName ) ? item.WorktypeName : item.WorktypeGroupName )
			.Distinct()
			.OrderBy( group => group )
			.ToArray();

		var materialValues = groups
			.Select( group => CostAllocationLines
				.Where( item => ( string.IsNullOrWhiteSpace( item.WorktypeGroupName ) ? item.WorktypeName : item.WorktypeGroupName ) == group )
				.Sum( item => item.MaterialCosts ) )
			.ToArray();

		var timeValues = groups
			.Select( group => CostAllocationLines
				.Where( item => ( string.IsNullOrWhiteSpace( item.WorktypeGroupName ) ? item.WorktypeName : item.WorktypeGroupName ) == group )
				.Sum( item => item.TimeCosts ) )
			.ToArray();

		var series = new ChartSeriesCollection
		{
			CreateColumnSeries( Lang.ProjectReportsMaterialCostsHeader, groups.Select( ( group, index ) => CreateCurrencyChartPoint( group, materialValues[index] ) ).ToList(), Color.FromRgb( 47, 128, 237 ) ),
			CreateColumnSeries( Lang.ProjectReportsTimeCostsHeader, groups.Select( ( group, index ) => CreateCurrencyChartPoint( group, timeValues[index] ) ).ToList(), Color.FromRgb( 39, 174, 96 ) )
		};

		return series;
	}

	private ChartSeriesCollection BuildCostDeclarationStackedSeries()
	{
		var rows = CostDeclarationLines
			.Where( item => item.TotalCosts > 0 )
			.ToList();

		var categories = rows
			.Select( item => item.CategoryName )
			.Distinct()
			.OrderBy( category => category )
			.ToArray();

		var topProducts = rows
			.GroupBy( item => item.ProductName )
			.Select( group => new { ProductName = group.Key, TotalCosts = group.Sum( item => item.TotalCosts ) } )
			.OrderByDescending( item => item.TotalCosts )
			.Take( 8 )
			.Select( item => item.ProductName )
			.ToArray();

		var colors = GetChartColors();
		var series = new ChartSeriesCollection();
		foreach ( var ( product, index ) in topProducts.Select( ( product, index ) => ( product, index ) ) )
		{
			var points = categories
				.Select( category => CreateCurrencyChartPoint( category, rows.Where( item => item.CategoryName == category && item.ProductName == product ).Sum( item => item.TotalCosts ) ) )
				.ToList();
			series.Add( CreateColumnSeries( product, points, colors[index % colors.Length] ) );
		}

		var otherValues = categories
			.Select( category => rows.Where( item => item.CategoryName == category && !topProducts.Contains( item.ProductName ) ).Sum( item => item.TotalCosts ) )
			.ToArray();

		if ( otherValues.Any( value => value > 0 ) )
		{
			series.Add( CreateColumnSeries( "Overig", categories.Select( ( category, index ) => CreateCurrencyChartPoint( category, otherValues[index] ) ).ToList(), Color.FromRgb( 111, 125, 142 ) ) );
		}

		return series;
	}

	private static ColumnSeries CreateColumnSeries( string label, IList<ChartPoint> points, Color color ) =>
		new()
		{
			Label = label,
			ItemsSource = points,
			XBindingPath = nameof( ChartPoint.Category ),
			YBindingPath = nameof( ChartPoint.Value ),
			Interior = new SolidColorBrush( color ),
			SegmentSpacing = 0.08,
			EnableAnimation = true,
			AnimationDuration = TimeSpan.FromMilliseconds( 700 ),
			ShowTooltip = true,
			TooltipTemplate = CreateBarTooltipTemplate()
		};

	private static DataTemplate CreateBarTooltipTemplate()
	{
		var border = new FrameworkElementFactory( typeof( Border ) );
		border.SetValue( Border.PaddingProperty, new Thickness( 8, 6, 8, 6 ) );
		border.SetValue( Border.BackgroundProperty, new SolidColorBrush( Color.FromRgb( 245, 245, 245 ) ) );
		border.SetValue( Border.BorderBrushProperty, new SolidColorBrush( Color.FromRgb( 24, 58, 90 ) ) );
		border.SetValue( Border.BorderThicknessProperty, new Thickness( 1 ) );
		border.SetValue( Border.CornerRadiusProperty, new CornerRadius( 3 ) );

		var stack = new FrameworkElementFactory( typeof( StackPanel ) );
		stack.SetValue( FrameworkElement.MinWidthProperty, 130d );

		var seriesLabel = new FrameworkElementFactory( typeof( TextBlock ) );
		seriesLabel.SetValue( TextBlock.FontWeightProperty, FontWeights.SemiBold );
		seriesLabel.SetValue( TextBlock.ForegroundProperty, new SolidColorBrush( Color.FromRgb( 31, 53, 80 ) ) );
		seriesLabel.SetBinding( TextBlock.TextProperty, new Binding( "Series.Label" ) );
		stack.AppendChild( seriesLabel );

		var category = new FrameworkElementFactory( typeof( TextBlock ) );
		category.SetValue( FrameworkElement.MarginProperty, new Thickness( 0, 5, 0, 0 ) );
		category.SetValue( TextBlock.ForegroundProperty, new SolidColorBrush( Color.FromRgb( 67, 86, 106 ) ) );
		category.SetBinding( TextBlock.TextProperty, new Binding( "Item.Category" ) );
		stack.AppendChild( category );

		var value = new FrameworkElementFactory( typeof( TextBlock ) );
		value.SetValue( TextBlock.FontWeightProperty, FontWeights.SemiBold );
		value.SetValue( TextBlock.ForegroundProperty, new SolidColorBrush( Color.FromRgb( 31, 53, 80 ) ) );
		value.SetBinding( TextBlock.TextProperty, new Binding( "Item.DisplayValue" ) );
		stack.AppendChild( value );

		border.AppendChild( stack );
		return new DataTemplate { VisualTree = border };
	}

	private static Color[] GetChartColors() =>
	[
		Color.FromRgb( 47, 128, 237 ),
		Color.FromRgb( 39, 174, 96 ),
		Color.FromRgb( 242, 153, 74 ),
		Color.FromRgb( 155, 81, 224 ),
		Color.FromRgb( 235, 87, 87 ),
		Color.FromRgb( 86, 204, 242 ),
		Color.FromRgb( 111, 207, 151 ),
		Color.FromRgb( 187, 107, 217 ),
		Color.FromRgb( 45, 156, 219 ),
		Color.FromRgb( 242, 201, 76 ),
		Color.FromRgb( 111, 125, 142 ),
		Color.FromRgb( 0, 150, 136 )
	];

	private static ChartPoint CreateHoursChartPoint( string category, double value ) =>
		new( category, value, value.ToString( "N2", CultureInfo.CurrentCulture ) );

	private static ChartPoint CreateCurrencyChartPoint( string category, double value ) =>
		new( category, value, value.ToString( "C2", CultureInfo.CurrentCulture ) );

	private sealed record ChartPoint( string Category, double Value, string DisplayValue );
}
