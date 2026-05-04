using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class TimeRegistrationViewModel : ObservableObject
{
	private readonly ITimeRegistrationService _timeRegistrationService;
	private readonly IProjectService _projectService;
	private readonly IProductService _productService;
	private readonly IWorktypeService _worktypeService;

	[ObservableProperty] private ProjectModel? _selectedProject;
	[ObservableProperty] private ProductModel? _selectedProduct;
	[ObservableProperty] private WorktypeModel? _selectedWorktype;
	[ObservableProperty] private DateTime _selectedWorkDate = DateTime.Today;
	[ObservableProperty] private string _startTime = "09:00";
	[ObservableProperty] private string _endTime = "10:00";
	[ObservableProperty] private string? _timeComment;
	[ObservableProperty] private DateTime _materialUsageDate = DateTime.Today;
	[ObservableProperty] private double _materialAmount;
	[ObservableProperty] private string? _materialComment;
	[ObservableProperty] private bool _includeHoursInCosts = true;
	[ObservableProperty] private double _hourRate;
	[ObservableProperty] private bool _hasUnsavedTimeChanges;

	public ObservableCollection<ProjectModel> Projects { get; } = [];
	public ObservableCollection<ProductModel> Products { get; } = [];
	public ObservableCollection<WorktypeModel> Worktypes { get; } = [];
	public ObservableCollection<TimeEntryModel> TimeEntries { get; } = [];
	public ObservableCollection<MaterialUsageModel> MaterialUsages { get; } = [];
	public ObservableCollection<ProjectCostLineModel> CostLines { get; } = [];

	public IRelayCommand AddTimeEntryCommand { get; }
	public IAsyncRelayCommand SaveTimeEntriesCommand { get; }
	public IAsyncRelayCommand AddMaterialUsageCommand { get; }
	public IAsyncRelayCommand RefreshCommand { get; }

	public TimeRegistrationViewModel(
		ITimeRegistrationService timeRegistrationService,
		IProjectService projectService,
		IProductService productService,
		IWorktypeService worktypeService )
	{
		_timeRegistrationService = timeRegistrationService;
		_projectService = projectService;
		_productService = productService;
		_worktypeService = worktypeService;

		AddTimeEntryCommand = new RelayCommand( AddTimeEntry, CanAddTimeEntry );
		SaveTimeEntriesCommand = new AsyncRelayCommand( SaveTimeEntriesAsync, () => HasUnsavedTimeChanges );
		AddMaterialUsageCommand = new AsyncRelayCommand( AddMaterialUsageAsync, CanAddMaterialUsage );
		RefreshCommand = new AsyncRelayCommand( RefreshSelectedProjectDataAsync );

		TimeEntries.CollectionChanged += TimeEntries_CollectionChanged;

		_ = InitializeAsync();
	}

	public bool IsProjectSelected => SelectedProject != null;
	public int TimeEntryCount => TimeEntries.Count;
	public int MaterialUsageCount => MaterialUsages.Count;
	public double TotalWorkedMinutes => TimeEntries.Sum( entry => entry.WorkedMinutes );
	public double TotalWorkedHours => TotalWorkedMinutes / 60;
	public double MaterialCosts => MaterialUsages.Sum( usage => usage.Costs );
	public double TimeCosts => IncludeHoursInCosts ? TotalWorkedHours * HourRate : 0;
	public double ProjectTotalCosts => MaterialCosts + TimeCosts;

	private async Task InitializeAsync()
	{
		HourRate = await _timeRegistrationService.GetHourRateAsync();

		await LoadProjectsAsync();
		await LoadProductsAsync();
		await LoadWorktypesAsync();

		SelectedProject = Projects.OrderByDescending( project => project.ProjectId ).FirstOrDefault();
		SelectedProduct = Products.FirstOrDefault();
		SelectedWorktype = Worktypes.FirstOrDefault();
	}

	private async Task LoadProjectsAsync()
	{
		Projects.Clear();
		foreach ( var project in await _projectService.GetAllProjectsAsync() )
			Projects.Add( project );
	}

	private async Task LoadProductsAsync()
	{
		Products.Clear();
		foreach ( var product in await _productService.GetAllProductsAsync() )
			Products.Add( product );
	}

	private async Task LoadWorktypesAsync()
	{
		Worktypes.Clear();
		foreach ( var worktype in await _worktypeService.GetAllWorkTypesAsync() )
			Worktypes.Add( worktype );
	}

	partial void OnSelectedProjectChanged( ProjectModel? value )
	{
		OnPropertyChanged( nameof( IsProjectSelected ) );
		_ = RefreshSelectedProjectDataAsync();
		AddTimeEntryCommand.NotifyCanExecuteChanged();
		AddMaterialUsageCommand.NotifyCanExecuteChanged();
	}

	partial void OnSelectedProductChanged( ProductModel? value ) => AddMaterialUsageCommand.NotifyCanExecuteChanged();
	partial void OnSelectedWorktypeChanged( WorktypeModel? value ) => AddTimeEntryCommand.NotifyCanExecuteChanged();
	partial void OnMaterialAmountChanged( double value ) => AddMaterialUsageCommand.NotifyCanExecuteChanged();
	partial void OnIncludeHoursInCostsChanged( bool value ) => RebuildCostLines();
	partial void OnHourRateChanged( double value ) => RebuildCostLines();

	private async Task RefreshSelectedProjectDataAsync()
	{
		if ( SelectedProject == null )
			return;

		await LoadTimeEntriesAsync( SelectedProject.ProjectId );
		await LoadMaterialUsageAsync( SelectedProject.ProjectId );

		HasUnsavedTimeChanges = false;
		RebuildCostLines();
	}

	private async Task LoadTimeEntriesAsync( int projectId )
	{
		foreach ( var entry in TimeEntries )
			entry.PropertyChanged -= TimeEntry_PropertyChanged;

		TimeEntries.Clear();
		foreach ( var entry in await _timeRegistrationService.GetTimeEntriesByProjectAsync( projectId ) )
			TimeEntries.Add( entry );

		RaiseTimeTotals();
	}

	private async Task LoadMaterialUsageAsync( int projectId )
	{
		MaterialUsages.Clear();
		foreach ( var usage in await _timeRegistrationService.GetMaterialUsageByProjectAsync( projectId ) )
			MaterialUsages.Add( usage );

		OnPropertyChanged( nameof( MaterialUsageCount ) );
	}

	private void AddTimeEntry()
	{
		if ( SelectedProject == null || SelectedWorktype == null )
			return;

		var entry = new TimeEntryModel
		{
			ProjectId = SelectedProject.ProjectId,
			ProjectName = SelectedProject.ProjectName,
			WorktypeId = SelectedWorktype.WorktypeId,
			WorktypeName = SelectedWorktype.WorktypeName,
			WorkDate = SelectedWorkDate.Date,
			StartTime = StartTime,
			EndTime = EndTime,
			Comment = TimeComment,
			State = TimeEntryModel.RecordState.Added
		};

		TimeEntries.Insert( 0, entry );
		HasUnsavedTimeChanges = true;
		SaveTimeEntriesCommand.NotifyCanExecuteChanged();
		RaiseTimeTotals();
		RebuildCostLines();
	}

	private async Task SaveTimeEntriesAsync()
	{
		foreach ( var entry in TimeEntries.Where( e => e.State != TimeEntryModel.RecordState.Unchanged ).ToList() )
		{
			if ( entry.State == TimeEntryModel.RecordState.Added )
				entry.TimeId = await _timeRegistrationService.InsertTimeEntryAsync( entry );
			else if ( entry.State == TimeEntryModel.RecordState.Modified )
				await _timeRegistrationService.UpdateTimeEntryAsync( entry );

			entry.State = TimeEntryModel.RecordState.Unchanged;
		}

		HasUnsavedTimeChanges = false;
		SaveTimeEntriesCommand.NotifyCanExecuteChanged();
		await RefreshSelectedProjectDataAsync();
	}

	private async Task AddMaterialUsageAsync()
	{
		if ( SelectedProject == null || SelectedProduct == null || MaterialAmount <= 0 )
			return;

		await _timeRegistrationService.InsertMaterialUsageAsync(
			SelectedProject.ProjectId,
			SelectedProduct,
			MaterialAmount,
			MaterialUsageDate,
			MaterialComment );

		MaterialAmount = 0;
		MaterialComment = string.Empty;

		await LoadMaterialUsageAsync( SelectedProject.ProjectId );
		RebuildCostLines();
	}

	private bool CanAddTimeEntry() => SelectedProject != null && SelectedWorktype != null;
	private bool CanAddMaterialUsage() => SelectedProject != null && SelectedProduct != null && MaterialAmount > 0;

	private void RebuildCostLines()
	{
		CostLines.Clear();

		foreach ( var line in MaterialUsages
			.GroupBy( usage => new { usage.ProductId, usage.ProductName, usage.CategoryName, usage.Price } )
			.Select( group => new ProjectCostLineModel
			{
				Description = group.Key.ProductName,
				GroupName = group.Key.CategoryName,
				Amount = group.Sum( item => item.Amount ),
				UnitPrice = group.Key.Price,
				TotalCosts = group.Sum( item => item.Costs )
			} )
			.OrderBy( line => line.GroupName )
			.ThenBy( line => line.Description ) )
		{
			CostLines.Add( line );
		}

		if ( IncludeHoursInCosts && TotalWorkedHours > 0 )
		{
			CostLines.Add( new ProjectCostLineModel
			{
				Description = "Gewerkte uren",
				GroupName = "Arbeid",
				Amount = TotalWorkedHours,
				UnitPrice = HourRate,
				TotalCosts = TimeCosts
			} );
		}

		OnPropertyChanged( nameof( MaterialCosts ) );
		OnPropertyChanged( nameof( TimeCosts ) );
		OnPropertyChanged( nameof( ProjectTotalCosts ) );
	}

	private void TimeEntries_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		if ( e.NewItems != null )
		{
			foreach ( TimeEntryModel entry in e.NewItems )
				entry.PropertyChanged += TimeEntry_PropertyChanged;
		}

		if ( e.OldItems != null )
		{
			foreach ( TimeEntryModel entry in e.OldItems )
				entry.PropertyChanged -= TimeEntry_PropertyChanged;
		}

		OnPropertyChanged( nameof( TimeEntryCount ) );
	}

	private void TimeEntry_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( sender is TimeEntryModel entry && entry.State != TimeEntryModel.RecordState.Unchanged )
			HasUnsavedTimeChanges = true;

		if ( e.PropertyName == nameof( TimeEntryModel.WorkedMinutes ) ||
			e.PropertyName == nameof( TimeEntryModel.State ) ||
			e.PropertyName == nameof( TimeEntryModel.StartTime ) ||
			e.PropertyName == nameof( TimeEntryModel.EndTime ) )
		{
			RaiseTimeTotals();
			RebuildCostLines();
		}

		SaveTimeEntriesCommand.NotifyCanExecuteChanged();
	}

	private void RaiseTimeTotals()
	{
		OnPropertyChanged( nameof( TimeEntryCount ) );
		OnPropertyChanged( nameof( TotalWorkedMinutes ) );
		OnPropertyChanged( nameof( TotalWorkedHours ) );
		OnPropertyChanged( nameof( TimeCosts ) );
		OnPropertyChanged( nameof( ProjectTotalCosts ) );
	}
}
