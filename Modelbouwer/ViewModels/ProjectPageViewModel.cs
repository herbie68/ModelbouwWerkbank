using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class ProjectPageViewModel : EntityPageViewModel<ProjectModel>
{
	private readonly IProjectService _dataService;

	// SelectedProject als type-safe alias
	public ProjectModel? SelectedProject
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddProjectCommand => AddCommand;
	public IAsyncRelayCommand SaveProjectCommand => SaveCommand;
	public IRelayCommand DeleteProjectCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public ProjectPageViewModel(
		IProjectService dataService,
		IEntityValidator<ProjectModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadProjectsAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultProject te zetten
	protected override void OnSelectedItemChanged( ProjectModel? value )
	{
		if ( value == null )
			return;

		OnPropertyChanged( nameof( SelectedProject ) );
		OnPropertyChanged( nameof( SelectedProject.ProjectName ) );
		OnPropertyChanged( nameof( SelectedProject.ProjectId ) );
	}

	// Async projects laden
	private async Task LoadProjectsAsync()
	{
		var projectList = await _dataService.GetAllProjectsAsync();

		Projects.Clear();
		foreach ( var c in projectList )
			Projects.Add( c );
	}

	// Properties voor UI binding
	public ObservableCollection<ProjectModel> Projects => Items;
	public int TotalProjectCount => TotalItemCount;
	public int VisibleProjectCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterProject( object obj )
	{
		if ( obj is not ProjectModel project )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return project.ProjectName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<ProjectModel>> LoadItemsAsync() => _dataService.GetAllProjectsAsync();
	protected override Task<int> InsertAsync( ProjectModel item ) => _dataService.InsertNewProjectAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( ProjectModel item ) => _dataService.UpdateProjectAsync( CreateParameters( item ) );
	protected override async Task DeleteAsync( ProjectModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.ProjectName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteProjectAsync( item.ProjectId );
		}
		catch ( EntityInUseException ex )
		{
			MessageBox.Show(
				ex.Message,
				Lang.generalMessageboxWarningTitle,
				MessageBoxButton.OK,
				MessageBoxImage.Information
			);
		}
	}

	protected override int GetId( ProjectModel item ) => item.ProjectId;
	protected override void SetId( ProjectModel item, int id ) => item.ProjectId = id;

	protected override ProjectModel CreateNewItem() => new()
	{
		ProjectId = 0,
		ProjectName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalProjectCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( ProjectModel c ) => new()
	{
		{ $"@{DBNames.ProjectFieldNameId}", c.ProjectId },
		{ $"@{DBNames.ProjectFieldNameName}", c.ProjectName?.Trim() }
	};
}
