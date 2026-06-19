using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class TimeRegistrationViewModel : AsyncObservableObject
{
	private readonly ITimeRegistrationService _timeRegistrationService;
	private readonly IProjectService _projectService;
	private readonly IProductService _productService;
	private readonly IWorktypeService _worktypeService;
	private readonly ICategoryService _categoryService;
	private bool _isSyncingSelectedMaterialUsage;
	private CancellationTokenSource? _selectedProjectDataCancellationTokenSource;
	private CultureInfo _displayCulture = CultureInfo.CurrentCulture;

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
	[ObservableProperty] private bool _hasUnsavedMaterialChanges;
	[ObservableProperty] private bool _isSavingTimeEntries;
	[ObservableProperty] private bool _isSavingMaterialUsages;
	[ObservableProperty] private TimeEntryModel? _selectedTimeEntry;
	[ObservableProperty] private MaterialUsageModel? _selectedMaterialUsage;
	[ObservableProperty] private bool _isProductPopupOpen;

	public ObservableCollection<ProjectModel> Projects { get; } = [ ];
	public ObservableCollection<ProductModel> Products { get; } = [ ];
	public ObservableCollection<CategoryModel> Categories { get; } = [ ];
	public ObservableCollection<ProductSelectionNodeModel> ProductTree { get; } = [ ];
	public ObservableCollection<WorktypeModel> Worktypes { get; } = [ ];
	public ObservableCollection<WorktypeModel> WorktypeTree { get; } = [ ];
	public ObservableCollection<TimeEntryModel> TimeEntries { get; } = [ ];
	public ObservableCollection<MaterialUsageModel> MaterialUsages { get; } = [ ];
	public ObservableCollection<ProjectCostLineModel> CostLines { get; } = [ ];

	public IRelayCommand AddTimeEntryCommand { get; }
	public IAsyncRelayCommand SaveTimeEntriesCommand { get; }
	public IAsyncRelayCommand AddMaterialUsageCommand { get; }
	public IAsyncRelayCommand SaveMaterialUsagesCommand { get; }
	public IAsyncRelayCommand RefreshCommand { get; }
	public IRelayCommand ToggleWorktypePopupCommand { get; }
	public IRelayCommand ToggleProductPopupCommand { get; }
	public IAsyncRelayCommand DeleteTimeEntryCommand { get; }
	public IAsyncRelayCommand DeleteMaterialUsageCommand { get; }

	private bool _isWorktypePopupOpen;
	public bool IsWorktypePopupOpen
	{
		get => _isWorktypePopupOpen;
		set => SetProperty( ref _isWorktypePopupOpen, value );
	}

	public string SelectedWorktypeName => SelectedWorktype?.WorktypeName ?? Lang.TimeRegistrationDefaultWorktypeText;
	public string SelectedProductName => SelectedProduct?.ProductName ?? Lang.TimeRegistrationDefaultProductText;

	public TimeRegistrationViewModel(
		ITimeRegistrationService timeRegistrationService,
		IProjectService projectService,
		IProductService productService,
		IWorktypeService worktypeService,
		ICategoryService categoryService )
	{
		_timeRegistrationService = timeRegistrationService;
		_projectService = projectService;
		_productService = productService;
		_worktypeService = worktypeService;
		_categoryService = categoryService;

		AddTimeEntryCommand = new RelayCommand( AddTimeEntry, CanAddTimeEntry );
		SaveTimeEntriesCommand = new AsyncRelayCommand( SaveTimeEntriesAsync, CanSaveTimeEntries );
		AddMaterialUsageCommand = new AsyncRelayCommand( AddMaterialUsageAsync, CanAddMaterialUsage );
		SaveMaterialUsagesCommand = new AsyncRelayCommand( SaveMaterialUsagesAsync, CanSaveMaterialUsages );
		RefreshCommand = new AsyncRelayCommand( StartRefreshSelectedProjectDataAsync );
		ToggleWorktypePopupCommand = new RelayCommand( () => IsWorktypePopupOpen = !IsWorktypePopupOpen );
		ToggleProductPopupCommand = new RelayCommand( () => IsProductPopupOpen = !IsProductPopupOpen );
		DeleteTimeEntryCommand = new AsyncRelayCommand( DeleteSelectedTimeEntryAsync, () => SelectedTimeEntry != null );
		DeleteMaterialUsageCommand = new AsyncRelayCommand( DeleteSelectedMaterialUsageAsync, () => SelectedMaterialUsage != null );

		TimeEntries.CollectionChanged += TimeEntries_CollectionChanged;
		MaterialUsages.CollectionChanged += MaterialUsages_CollectionChanged;

		ObserveBackgroundTask( InitializeAsync() );
	}

	public bool IsProjectSelected => SelectedProject != null;
	public bool IsTimeEntrySelected => SelectedTimeEntry != null;
	public bool IsMaterialUsageSelected => SelectedMaterialUsage != null;
	public bool HasUnsavedChanges => HasUnsavedTimeChanges || HasUnsavedMaterialChanges;
	public int TimeEntryCount => TimeEntries.Count;
	public int MaterialUsageCount => MaterialUsages.Count;
	public double TotalWorkedMinutes => TimeEntries.Sum( entry => entry.WorkedMinutes );
	public double TotalWorkedHours => TotalWorkedMinutes / 60;
	public double MaterialCosts => MaterialUsages.Sum( usage => usage.Costs );
	public double TimeCosts => IncludeHoursInCosts ? TotalWorkedHours * HourRate : 0;
	public double ProjectTotalCosts => MaterialCosts + TimeCosts;
	public string MaterialCostsDisplay => FormatCurrency( MaterialCosts );
	public string HourRateDisplay => FormatCurrency( HourRate );
	public string ProjectTotalCostsDisplay => FormatCurrency( ProjectTotalCosts );

	private async Task InitializeAsync()
	{
		var cultureTask = _timeRegistrationService.GetCultureAsync();
		var hourRateTask = _timeRegistrationService.GetHourRateAsync();
		var projectsTask = _projectService.GetAllProjectsAsync();
		var productsTask = _productService.GetAllProductsAsync();
		var categoriesTask = _categoryService.GetAllCategorysAsync();
		var worktypesTask = _worktypeService.GetAllWorkTypesAsync();

		await PerformanceTrace.MeasureAsync(
			$"{nameof( TimeRegistrationViewModel )}.{nameof( InitializeAsync )}",
			() => Task.WhenAll( cultureTask, hourRateTask, projectsTask, productsTask, categoriesTask, worktypesTask ) );

		_displayCulture = await cultureTask;
		HourRate = await hourRateTask;

		Projects.Clear();
		foreach ( var project in await projectsTask )
			Projects.Add( project );

		Products.Clear();
		foreach ( var product in await productsTask )
			Products.Add( product );

		Categories.Clear();
		foreach ( var category in await categoriesTask )
			Categories.Add( category );

		Worktypes.Clear();
		foreach ( var worktype in await worktypesTask )
			Worktypes.Add( worktype );

		WorktypeTree.Clear();
		foreach ( var root in BuildWorktypeTree( Worktypes ) )
			WorktypeTree.Add( root );

		BuildProductTree();

		SelectedProject = Projects.OrderByDescending( project => project.ProjectId ).FirstOrDefault();
		SelectedProduct = Products.FirstOrDefault();
		SelectedWorktype = Worktypes.FirstOrDefault();
	}

	partial void OnSelectedProjectChanged( ProjectModel? value )
	{
		OnPropertyChanged( nameof( IsProjectSelected ) );
		ObserveBackgroundTask( StartRefreshSelectedProjectDataAsync() );
		AddTimeEntryCommand.NotifyCanExecuteChanged();
		AddMaterialUsageCommand.NotifyCanExecuteChanged();
	}

	partial void OnSelectedProductChanged( ProductModel? value )
	{
		OnPropertyChanged( nameof( SelectedProductName ) );
		AddMaterialUsageCommand.NotifyCanExecuteChanged();
		ApplySelectedProductToMaterialUsage();
	}
	partial void OnSelectedWorktypeChanged( WorktypeModel? value )
	{
		OnPropertyChanged( nameof( SelectedWorktypeName ) );
		AddTimeEntryCommand.NotifyCanExecuteChanged();
	}
	partial void OnMaterialAmountChanged( double value )
	{
		AddMaterialUsageCommand.NotifyCanExecuteChanged();

		if ( !_isSyncingSelectedMaterialUsage && SelectedMaterialUsage != null )
			SelectedMaterialUsage.Amount = value;
	}
	partial void OnMaterialUsageDateChanged( DateTime value )
	{
		if ( !_isSyncingSelectedMaterialUsage && SelectedMaterialUsage != null )
			SelectedMaterialUsage.UsageDate = value.Date;
	}
	partial void OnMaterialCommentChanged( string? value )
	{
		if ( !_isSyncingSelectedMaterialUsage && SelectedMaterialUsage != null )
			SelectedMaterialUsage.Comment = value;
	}
	partial void OnIncludeHoursInCostsChanged( bool value ) => RebuildCostLines();
	partial void OnHourRateChanged( double value )
	{
		OnPropertyChanged( nameof( HourRateDisplay ) );
		RebuildCostLines();
	}
	partial void OnHasUnsavedTimeChangesChanged( bool value ) => OnPropertyChanged( nameof( HasUnsavedChanges ) );
	partial void OnHasUnsavedMaterialChangesChanged( bool value ) => OnPropertyChanged( nameof( HasUnsavedChanges ) );
	partial void OnIsSavingTimeEntriesChanged( bool value ) => SaveTimeEntriesCommand.NotifyCanExecuteChanged();
	partial void OnIsSavingMaterialUsagesChanged( bool value ) => SaveMaterialUsagesCommand.NotifyCanExecuteChanged();
	partial void OnSelectedTimeEntryChanged( TimeEntryModel? value )
	{
		DeleteTimeEntryCommand.NotifyCanExecuteChanged();
		OnPropertyChanged( nameof( IsTimeEntrySelected ) );

		if ( value != null )
			SelectedWorktype = Worktypes.FirstOrDefault( worktype => worktype.WorktypeId == value.WorktypeId );
	}
	partial void OnSelectedMaterialUsageChanged( MaterialUsageModel? value )
	{
		DeleteMaterialUsageCommand.NotifyCanExecuteChanged();
		OnPropertyChanged( nameof( IsMaterialUsageSelected ) );
		SyncMaterialDetailFields( value );
	}

	private Task StartRefreshSelectedProjectDataAsync()
	{
		_selectedProjectDataCancellationTokenSource?.Cancel();
		_selectedProjectDataCancellationTokenSource?.Dispose();
		_selectedProjectDataCancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _selectedProjectDataCancellationTokenSource.Token;

		return RefreshSelectedProjectDataAsync( cancellationToken );
	}

	private async Task RefreshSelectedProjectDataAsync( CancellationToken cancellationToken )
	{
		var selectedProject = SelectedProject;
		if ( selectedProject == null )
			return;

		var timeEntriesTask = _timeRegistrationService.GetTimeEntriesByProjectAsync( selectedProject.ProjectId, cancellationToken );
		var materialUsagesTask = _timeRegistrationService.GetMaterialUsageByProjectAsync( selectedProject.ProjectId, cancellationToken );

		await PerformanceTrace.MeasureAsync(
			$"{nameof( TimeRegistrationViewModel )}.{nameof( RefreshSelectedProjectDataAsync )}",
			() => Task.WhenAll( timeEntriesTask, materialUsagesTask ) );
		cancellationToken.ThrowIfCancellationRequested();

		ApplyTimeEntries( await timeEntriesTask );
		ApplyMaterialUsages( await materialUsagesTask );

		HasUnsavedTimeChanges = false;
		HasUnsavedMaterialChanges = false;
		RebuildCostLines();
	}

	private async Task LoadTimeEntriesAsync( int projectId, CancellationToken cancellationToken )
	{
		var entries = await _timeRegistrationService.GetTimeEntriesByProjectAsync( projectId, cancellationToken );
		cancellationToken.ThrowIfCancellationRequested();

		ApplyTimeEntries( entries );
	}

	private void ApplyTimeEntries( IEnumerable<TimeEntryModel> entries )
	{
		foreach ( var entry in TimeEntries )
			entry.PropertyChanged -= TimeEntry_PropertyChanged;

		TimeEntries.Clear();
		foreach ( var entry in entries )
			TimeEntries.Add( entry );

		SortTimeEntries();
		SelectedTimeEntry = TimeEntries.FirstOrDefault();
		RaiseTimeTotals();
	}

	private async Task LoadMaterialUsageAsync( int projectId, CancellationToken cancellationToken )
	{
		var usages = await _timeRegistrationService.GetMaterialUsageByProjectAsync( projectId, cancellationToken );
		cancellationToken.ThrowIfCancellationRequested();

		ApplyMaterialUsages( usages );
	}

	private void ApplyMaterialUsages( IEnumerable<MaterialUsageModel> usages )
	{
		foreach ( var usage in MaterialUsages )
			usage.PropertyChanged -= MaterialUsage_PropertyChanged;

		MaterialUsages.Clear();
		foreach ( var usage in usages )
			MaterialUsages.Add( usage );

		SortMaterialUsages();
		SelectedMaterialUsage = MaterialUsages.FirstOrDefault();
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
			WorkDate = DateTime.Today,
			StartTime = "09:00",
			EndTime = "10:00",
			Comment = string.Empty,
			State = TimeEntryModel.RecordState.Added
		};

		TimeEntries.Add( entry );
		SortTimeEntries();
		SelectedTimeEntry = entry;
		HasUnsavedTimeChanges = true;
		SaveTimeEntriesCommand.NotifyCanExecuteChanged();
		RaiseTimeTotals();
		RebuildCostLines();
	}

	private async Task SaveTimeEntriesAsync()
	{
		if ( IsSavingTimeEntries )
			return;

		IsSavingTimeEntries = true;
		try
		{
			var changedEntries = TimeEntries
				.Where( e => e.State != TimeEntryModel.RecordState.Unchanged )
				.ToList();

			if ( changedEntries.Count == 0 )
				return;

			foreach ( var entry in changedEntries )
			{
				if ( !TryValidateTimeInput( entry.WorkDate.Date, entry.StartTime, entry.EndTime, entry, out string validationMessage ) )
				{
					MessageBox.Show( validationMessage, Lang.TimeRegistrationTimeRegistrationTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
					return;
				}
			}

			foreach ( var entry in changedEntries )
			{
				if ( entry.State == TimeEntryModel.RecordState.Added )
					entry.TimeId = await _timeRegistrationService.InsertTimeEntryAsync( entry );
				else if ( entry.State == TimeEntryModel.RecordState.Modified )
					await _timeRegistrationService.UpdateTimeEntryAsync( entry );

				entry.State = TimeEntryModel.RecordState.Unchanged;
			}

			HasUnsavedTimeChanges = false;
			SaveTimeEntriesCommand.NotifyCanExecuteChanged();
			await RefreshSelectedProjectDataAsync( CancellationToken.None );
		}
		finally
		{
			IsSavingTimeEntries = false;
		}
	}

	public Task SaveTimeEntriesFromViewAsync() => SaveTimeEntriesAsync();

	private async Task AddMaterialUsageAsync()
	{
		if ( SelectedProject == null || SelectedProduct == null || MaterialAmount <= 0 )
			return;

		var usage = new MaterialUsageModel
		{
			ProjectId = SelectedProject.ProjectId,
			ProjectName = SelectedProject.ProjectName,
			ProductId = SelectedProduct.ProductId,
			ProductName = SelectedProduct.ProductName,
			CategoryId = SelectedProduct.ProductCategoryId,
			UsageDate = MaterialUsageDate.Date,
			Amount = MaterialAmount,
			Price = SelectedProduct.ProductPrice,
			Comment = MaterialComment,
			State = MaterialUsageModel.RecordState.Added
		};

		usage.Costs = usage.Amount * usage.Price;
		MaterialUsages.Add( usage );
		SortMaterialUsages();
		SelectedMaterialUsage = usage;
		HasUnsavedMaterialChanges = true;
		SaveMaterialUsagesCommand.NotifyCanExecuteChanged();
		RebuildCostLines();
	}

	private bool CanAddTimeEntry() => SelectedProject != null && SelectedWorktype != null;
	private bool CanAddMaterialUsage() => SelectedProject != null && SelectedProduct != null && MaterialAmount > 0;
	private bool CanSaveTimeEntries() => HasUnsavedTimeChanges && !IsSavingTimeEntries;
	private bool CanSaveMaterialUsages() => HasUnsavedMaterialChanges && !IsSavingMaterialUsages;

	private async Task SaveMaterialUsagesAsync()
	{
		if ( IsSavingMaterialUsages )
			return;

		IsSavingMaterialUsages = true;
		try
		{
			var changedUsages = MaterialUsages
				.Where( usage => usage.State != MaterialUsageModel.RecordState.Unchanged )
				.ToList();

			if ( changedUsages.Count == 0 )
				return;

			foreach ( var usage in changedUsages )
			{
				if ( usage.ProductId <= 0 )
				{
					MessageBox.Show( Lang.TimeRegistrationSelectProductWarning, Lang.TimeRegistrationMaterialRegistrationTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
					return;
				}

				if ( usage.Amount <= 0 )
				{
					MessageBox.Show( Lang.TimeRegistrationPositiveAmountWarning, Lang.TimeRegistrationMaterialRegistrationTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
					return;
				}
			}

			foreach ( var usage in changedUsages )
			{
				if ( usage.State == MaterialUsageModel.RecordState.Added )
					usage.ProductUsageId = await _timeRegistrationService.InsertMaterialUsageAsync( usage );
				else if ( usage.State == MaterialUsageModel.RecordState.Modified )
					await _timeRegistrationService.UpdateMaterialUsageAsync( usage );

				usage.State = MaterialUsageModel.RecordState.Unchanged;
			}

			HasUnsavedMaterialChanges = false;
			SaveMaterialUsagesCommand.NotifyCanExecuteChanged();

			if ( SelectedProject != null )
				await LoadMaterialUsageAsync( SelectedProject.ProjectId, CancellationToken.None );

			RebuildCostLines();
		}
		finally
		{
			IsSavingMaterialUsages = false;
		}
	}

	public void SelectWorktype( WorktypeModel worktype )
	{
		SelectedWorktype = worktype;

		if ( SelectedTimeEntry != null )
		{
			SelectedTimeEntry.WorktypeId = worktype.WorktypeId;
			SelectedTimeEntry.WorktypeName = worktype.WorktypeName;
		}

		IsWorktypePopupOpen = false;
	}

	public void SelectProductNode( ProductSelectionNodeModel node )
	{
		if ( node.Product == null )
			return;

		SelectedProduct = node.Product;
		IsProductPopupOpen = false;
	}

	private async Task DeleteSelectedTimeEntryAsync()
	{
		if ( SelectedTimeEntry == null )
			return;

		var entry = SelectedTimeEntry;
		var result = MessageBox.Show(
			Lang.TimeRegistrationDeleteTimeQuestion,
			Lang.TimeRegistrationTimeRegistrationTitle,
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning );

		if ( result != MessageBoxResult.Yes )
			return;

		if ( entry.TimeId > 0 )
			await _timeRegistrationService.DeleteTimeEntryAsync( entry.TimeId );

		TimeEntries.Remove( entry );
		SelectedTimeEntry = TimeEntries.FirstOrDefault();
		HasUnsavedTimeChanges = TimeEntries.Any( item => item.State != TimeEntryModel.RecordState.Unchanged );
		SaveTimeEntriesCommand.NotifyCanExecuteChanged();
		RaiseTimeTotals();
		RebuildCostLines();
	}

	private async Task DeleteSelectedMaterialUsageAsync()
	{
		if ( SelectedMaterialUsage == null )
			return;

		var usage = SelectedMaterialUsage;
		var result = MessageBox.Show(
			Lang.TimeRegistrationDeleteMaterialQuestion,
			Lang.TimeRegistrationMaterialRegistrationTitle,
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning );

		if ( result != MessageBoxResult.Yes )
			return;

		if ( usage.ProductUsageId > 0 )
			await _timeRegistrationService.DeleteMaterialUsageAsync( usage.ProductUsageId );

		MaterialUsages.Remove( usage );
		SelectedMaterialUsage = MaterialUsages.FirstOrDefault();
		HasUnsavedMaterialChanges = MaterialUsages.Any( item => item.State != MaterialUsageModel.RecordState.Unchanged );
		SaveMaterialUsagesCommand.NotifyCanExecuteChanged();
		OnPropertyChanged( nameof( MaterialUsageCount ) );
		RebuildCostLines();
	}

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
				Description = Lang.TimeRegistrationTimeCostsDescription,
				GroupName = Lang.TimeRegistrationLabourGroupName,
				Amount = TotalWorkedHours,
				UnitPrice = HourRate,
				TotalCosts = TimeCosts
			} );
		}

		OnPropertyChanged( nameof( MaterialCosts ) );
		OnPropertyChanged( nameof( TimeCosts ) );
		OnPropertyChanged( nameof( ProjectTotalCosts ) );
		OnPropertyChanged( nameof( MaterialCostsDisplay ) );
		OnPropertyChanged( nameof( ProjectTotalCostsDisplay ) );
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

	private void MaterialUsages_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		if ( e.NewItems != null )
		{
			foreach ( MaterialUsageModel usage in e.NewItems )
				usage.PropertyChanged += MaterialUsage_PropertyChanged;
		}

		if ( e.OldItems != null )
		{
			foreach ( MaterialUsageModel usage in e.OldItems )
				usage.PropertyChanged -= MaterialUsage_PropertyChanged;
		}

		OnPropertyChanged( nameof( MaterialUsageCount ) );
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

	private void MaterialUsage_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( sender is MaterialUsageModel usage && usage.State != MaterialUsageModel.RecordState.Unchanged )
			HasUnsavedMaterialChanges = true;

		if ( e.PropertyName == nameof( MaterialUsageModel.Amount ) ||
			e.PropertyName == nameof( MaterialUsageModel.Price ) ||
			e.PropertyName == nameof( MaterialUsageModel.Costs ) ||
			e.PropertyName == nameof( MaterialUsageModel.ProductId ) ||
			e.PropertyName == nameof( MaterialUsageModel.ProductName ) ||
			e.PropertyName == nameof( MaterialUsageModel.CategoryName ) )
		{
			RebuildCostLines();
		}

		SaveMaterialUsagesCommand.NotifyCanExecuteChanged();
	}

	private void SyncMaterialDetailFields( MaterialUsageModel? usage )
	{
		_isSyncingSelectedMaterialUsage = true;
		try
		{
			if ( usage == null )
				return;

			SelectedProduct = Products.FirstOrDefault( product => product.ProductId == usage.ProductId );
			MaterialAmount = usage.Amount;
			MaterialUsageDate = usage.UsageDate.Date;
			MaterialComment = usage.Comment;
		}
		finally
		{
			_isSyncingSelectedMaterialUsage = false;
		}
	}

	private void ApplySelectedProductToMaterialUsage()
	{
		if ( _isSyncingSelectedMaterialUsage || SelectedMaterialUsage == null || SelectedProduct == null )
			return;

		SelectedMaterialUsage.ProductId = SelectedProduct.ProductId;
		SelectedMaterialUsage.ProductName = SelectedProduct.ProductName;
		SelectedMaterialUsage.CategoryId = SelectedProduct.ProductCategoryId;
		SelectedMaterialUsage.CategoryName = Categories.FirstOrDefault( category => category.CategoryId == SelectedProduct.ProductCategoryId )?.CategoryName;
		SelectedMaterialUsage.Price = SelectedProduct.ProductPrice;
	}

	private void RaiseTimeTotals()
	{
		OnPropertyChanged( nameof( TimeEntryCount ) );
		OnPropertyChanged( nameof( TotalWorkedMinutes ) );
		OnPropertyChanged( nameof( TotalWorkedHours ) );
		OnPropertyChanged( nameof( TimeCosts ) );
		OnPropertyChanged( nameof( ProjectTotalCosts ) );
		OnPropertyChanged( nameof( ProjectTotalCostsDisplay ) );
	}

	private string FormatCurrency( double value ) => value.ToString( "C2", _displayCulture );

	private bool TryValidateTimeInput( DateTime workDate, string startTime, string endTime, TimeEntryModel? currentEntry, out string validationMessage )
	{
		validationMessage = string.Empty;

		if ( !TryParseTime( startTime, out TimeSpan start ) )
		{
			validationMessage = Lang.TimeRegistrationInvalidStartTimeWarning;
			return false;
		}

		if ( !TryParseTime( endTime, out TimeSpan end ) )
		{
			validationMessage = Lang.TimeRegistrationInvalidEndTimeWarning;
			return false;
		}

		if ( end <= start )
		{
			validationMessage = Lang.TimeRegistrationEndAfterStartWarning;
			return false;
		}

		foreach ( TimeEntryModel existingEntry in TimeEntries.Where( entry => !IsSameTimeEntry( entry, currentEntry ) && entry.WorkDate.Date == workDate.Date ) )
		{
			if ( !TryParseTime( existingEntry.StartTime, out TimeSpan existingStart ) ||
				!TryParseTime( existingEntry.EndTime, out TimeSpan existingEnd ) )
				continue;

			bool overlaps = start < existingEnd && end > existingStart;
			if ( !overlaps )
				continue;

			if ( start == existingStart && end == existingEnd )
			{
				validationMessage = Lang.TimeRegistrationDuplicateTimeWarning;
				return false;
			}

			if ( start > existingStart && start < existingEnd )
			{
				validationMessage = Lang.TimeRegistrationStartOverlapWarning;
				return false;
			}

			if ( end > existingStart && end < existingEnd )
			{
				validationMessage = Lang.TimeRegistrationEndOverlapWarning;
				return false;
			}

			if ( start <= existingStart && end >= existingEnd )
			{
				validationMessage = Lang.TimeRegistrationTimeOverlapWarning;
				return false;
			}

			validationMessage = Lang.TimeRegistrationTimeOverlapWarning;
			return false;
		}

		return true;
	}

	private static bool IsSameTimeEntry( TimeEntryModel entry, TimeEntryModel? currentEntry )
	{
		if ( currentEntry == null )
			return false;

		if ( ReferenceEquals( entry, currentEntry ) )
			return true;

		return entry.TimeId > 0 && entry.TimeId == currentEntry.TimeId;
	}

	private void SortTimeEntries()
	{
		var sortedEntries = TimeEntries
			.OrderByDescending( entry => entry.WorkDate.Date )
			.ThenBy( entry => TryParseTime( entry.StartTime, out TimeSpan start ) ? start : TimeSpan.MaxValue )
			.ToList();

		if ( sortedEntries.SequenceEqual( TimeEntries ) )
			return;

		TimeEntries.Clear();
		foreach ( var entry in sortedEntries )
			TimeEntries.Add( entry );
	}

	private void SortMaterialUsages()
	{
		var sortedUsages = MaterialUsages
			.OrderByDescending( usage => usage.UsageDate.Date )
			.ThenBy( usage => usage.ProductName )
			.ToList();

		if ( sortedUsages.SequenceEqual( MaterialUsages ) )
			return;

		MaterialUsages.Clear();
		foreach ( var usage in sortedUsages )
			MaterialUsages.Add( usage );
	}

	private static bool TryParseTime( string? value, out TimeSpan time )
	{
		if ( TimeSpan.TryParseExact( value, @"h\:mm", CultureInfo.CurrentCulture, out time ) ||
			TimeSpan.TryParseExact( value, @"hh\:mm", CultureInfo.CurrentCulture, out time ) ||
			TimeSpan.TryParse( value, CultureInfo.CurrentCulture, out time ) )
			return true;

		time = TimeSpan.Zero;
		return false;
	}

	private static ObservableCollection<WorktypeModel> BuildWorktypeTree( IEnumerable<WorktypeModel> flatList )
	{
		var lookup = flatList.ToDictionary( worktype => worktype.WorktypeId );

		foreach ( var worktype in lookup.Values )
			worktype.Children.Clear();

		foreach ( var worktype in lookup.Values )
		{
			if ( worktype.ParentId is > 0 && lookup.TryGetValue( worktype.ParentId.Value, out var parent ) )
				parent.Children.Add( worktype );
		}

		return new ObservableCollection<WorktypeModel>(
			lookup.Values
				.Where( worktype => worktype.ParentId is null or 0 )
				.OrderBy( worktype => worktype.WorktypeName ) );
	}

	private void BuildProductTree()
	{
		ProductTree.Clear();

		var categoryNodes = Categories.ToDictionary(
			category => category.CategoryId,
			category => new ProductSelectionNodeModel
			{
				DisplayName = category.CategoryName,
				CategoryId = category.CategoryId
			} );

		foreach ( var category in Categories )
		{
			if ( category.ParentId is > 0 &&
				categoryNodes.TryGetValue( category.ParentId.Value, out var parentNode ) &&
				categoryNodes.TryGetValue( category.CategoryId, out var categoryNode ) )
			{
				parentNode.Children.Add( categoryNode );
			}
		}

		foreach ( var product in Products.OrderBy( product => product.ProductName ) )
		{
			var productNode = new ProductSelectionNodeModel
			{
				DisplayName = product.ProductName ?? string.Empty,
				CategoryId = product.ProductCategoryId,
				Product = product
			};

			if ( categoryNodes.TryGetValue( product.ProductCategoryId, out var categoryNode ) )
				categoryNode.Children.Add( productNode );
			else
				ProductTree.Add( productNode );
		}

		foreach ( var category in Categories )
		{
			if ( !categoryNodes.TryGetValue( category.CategoryId, out var categoryNode ) ||
				category.ParentId is > 0 ||
				!HasProducts( categoryNode ) )
				continue;

			RemoveEmptyCategories( categoryNode );
			ProductTree.Add( categoryNode );
		}

		SortProductTree( ProductTree );
	}

	private static bool HasProducts( ProductSelectionNodeModel node ) =>
		node.IsProduct || node.Children.Any( HasProducts );

	private static void RemoveEmptyCategories( ProductSelectionNodeModel node )
	{
		for ( int index = node.Children.Count - 1; index >= 0; index-- )
		{
			var child = node.Children[index];
			if ( child.IsProduct )
				continue;

			RemoveEmptyCategories( child );
			if ( !HasProducts( child ) )
				node.Children.RemoveAt( index );
		}
	}

	private static void SortProductTree( ObservableCollection<ProductSelectionNodeModel> nodes )
	{
		var sortedNodes = nodes
			.OrderBy( node => node.IsProduct )
			.ThenBy( node => node.DisplayName )
			.ToList();

		nodes.Clear();
		foreach ( var node in sortedNodes )
		{
			SortProductTree( node.Children );
			nodes.Add( node );
		}
	}
}