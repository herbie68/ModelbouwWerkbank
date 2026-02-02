using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

namespace Modelbouwer.ViewModels;

public partial class ProjectPageViewModel : EntityPageViewModel<ProjectModel>
{
	private readonly IProjectService _dataService;
	public DateOnly? ProjectExpectedEndDate;

	public bool IsProjectClosed => SelectedProject?.ProjectClosed == true;

	public Visibility EndDatePickerVisibility =>
		IsProjectClosed ? Visibility.Visible : Visibility.Collapsed;

	public Visibility ExpectedEndDateVisibility =>
		IsProjectClosed ? Visibility.Collapsed : Visibility.Visible;

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
	public IRelayCommand RotateCommand => _rotateCommand ??= new RelayCommand( RotateImage );
	public IRelayCommand AddImageCommand => _addImageCommand ??= new RelayCommand( AddImage );

	private IRelayCommand? _rotateCommand;
	private IRelayCommand? _addImageCommand;

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public ProjectPageViewModel( IProjectService dataService, IEntityValidator<ProjectModel> validator ) : base( validator )
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
		OnPropertyChanged( nameof( IsProjectClosed ) );
		OnPropertyChanged( nameof( EndDatePickerVisibility ) );
		OnPropertyChanged( nameof( ExpectedEndDateVisibility ) );

		_ = LoadExpectedEndDateAsync( value );
	}

	private async Task LoadExpectedEndDateAsync( ProjectModel project )
	{
		var projectId = project.ProjectId;

		if ( !project.ProjectClosed )
		{
			var result = await _dataService.GetExpectedEndDateAsync(projectId);

			if ( SelectedProject?.ProjectId == projectId )
				ProjectExpectedEndDate = result;
		}
		else
		{
			if ( SelectedProject?.ProjectId == projectId )
				ProjectExpectedEndDate = null;
		}
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

	private void RotateImage()
	{
		if ( SelectedProject == null )
			return;

		SelectedProject.ProjectImageRotationAngle = ( SelectedProject.ProjectImageRotationAngle + 90 ) % 360;
		Debug.WriteLine( SelectedProject.ProjectImageRotationAngle );
	}

	private void AddImage()
	{

		if ( SelectedProject == null )
			return;

		var dialog = new OpenFileDialog
		{
			Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp",
			Title = "Select image"
		};

		if ( dialog.ShowDialog() != true )
			return;

		SelectedProject.ProjectImage = File.ReadAllBytes( dialog.FileName );
		SelectedProject.ProjectImageRotationAngle = 0;
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
		{ $"@{DBNames.ProjectFieldNameCode}", c.ProjectCode },
		{ $"@{DBNames.ProjectFieldNameName}", c.ProjectName?.Trim() },
		{ $"@{DBNames.ProjectFieldNameStartDate}", c.ProjectStartDate },
		{ $"@{DBNames.ProjectFieldNameEndDate}", c.ProjectEndDate },
		{ $"@{DBNames.ProjectFieldNameExpectedTime}", c.ProjectExpectedTime },
		{ $"@{DBNames.ProjectFieldNameImage}", c.ProjectImage },
		{ $"@{DBNames.ProjectFieldNameImageRotationAngle}", c.ProjectImageRotationAngle },
		{ $"@{DBNames.ProjectFieldNameClosed}", c.ProjectClosed },
		{ $"@{DBNames.ProjectFieldNameMemo}", c.ProjectMemo }
	};
}
