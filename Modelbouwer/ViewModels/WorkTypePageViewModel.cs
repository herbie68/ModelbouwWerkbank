using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Windows.Shared;

namespace Modelbouwer.ViewModels;

public partial class WorktypePageViewModel : EntityPageViewModel<WorktypeModel>
{
	private readonly IWorktypeService _dataService;

	// Collections
	public ObservableCollection<WorktypeModel> WorkTypes { get; } = [ ];
	public ObservableCollection<WorktypeModel> WorkTypeTree { get; } = [ ];

	// Commands
	public IRelayCommand AddWorkTypeCommand => AddCommand;
	public IAsyncRelayCommand SaveWorkTypeCommand => SaveCommand;
	public IRelayCommand DeleteWorkTypeCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand AddSubWorkTypeCommand { get; }

	private IRelayCommand? _clearSearchCommand;

	private ICommand? _expandCommand;

	public ICommand? ExpandCommand
	{
		get { return _expandCommand; }
		set { _expandCommand = value; }
	}

	private ICommand? _collapseCommand;

	public ICommand? CollapseCommand
	{
		get { return _collapseCommand; }
		set { _collapseCommand = value; }
	}

	internal delegate void FilterChanged();
	internal FilterChanged? _filterChanged;

	// Constructor
	public WorktypePageViewModel( IWorktypeService dataService, IEntityValidator<WorktypeModel> validator ) : base( validator )
	{
		_dataService = dataService;

		_ = LoadWorkTypesAsync();
		_ = ReloadCommand.ExecuteAsync( null );

		AddSubWorkTypeCommand = new RelayCommand( AddSubWorkType, () => SelectedItem != null );

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );
	}

	protected override void OnPropertyChanged( System.ComponentModel.PropertyChangedEventArgs e )
	{
		base.OnPropertyChanged( e );

		if ( e.PropertyName == nameof( SearchText ) )
		{
			_filterChanged?.Invoke();
		}
	}

	// Override SelectedItem changed om DefaultWorkType te zetten
	protected override void OnSelectedItemChanged( WorktypeModel? oldValue, WorktypeModel? newValue )
	{
		base.OnSelectedItemChanged( oldValue, newValue );

		AddSubWorkTypeCommand.NotifyCanExecuteChanged();
	}

	private async Task LoadWorkTypesAsync()
	{
		var worktypeList = await _dataService.GetAllWorkTypesAsync();

		WorkTypes.Clear();
		foreach ( var c in worktypeList )
			WorkTypes.Add( c );
	}

	public bool FilterWorkType( object obj )
	{
		if ( obj is not WorktypeModel worktype )
			return false;

		if ( string.IsNullOrWhiteSpace( base.SearchText ) )
			return true;

		return worktype.WorktypeName?.Contains( base.SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<WorktypeModel>> LoadItemsAsync() => _dataService.GetAllWorkTypesAsync();
	protected override Task<int> InsertAsync( WorktypeModel item ) => _dataService.InsertNewWorkTypeAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( WorktypeModel item ) => _dataService.UpdateWorkTypeAsync( UpdateParameters( item ) );
	protected override async Task DeleteAsync( WorktypeModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.WorktypeName}' {Lang.toolbarButtonActionDeleteMessageQuestionWorkTypeSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteWorkTypeAsync( item.WorktypeId );
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

	private void ExpandExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid?.ExpandAllNodes();
	}

	private void CollapseExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid?.CollapseAllNodes();
	}

	public ObservableCollection<WorktypeModel> BuildTree( IEnumerable<WorktypeModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.WorktypeId);

		// Make sure Children are not doubled
		foreach ( var c in lookup.Values )
			c.Children.Clear();

		foreach ( var worktype in lookup.Values )
		{
			if ( worktype.ParentId != null &&
				lookup.TryGetValue( worktype.ParentId.Value, out var parent ) )
			{
				parent.Children.Add( worktype );
			}
		}

		return new ObservableCollection<WorktypeModel>( lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}

	protected override int GetId( WorktypeModel item ) => item.WorktypeId;
	protected override void SetId( WorktypeModel item, int id ) => item.WorktypeId = id;

	protected override WorktypeModel CreateNewItem() => new()
	{
		WorktypeId = 0,
		ParentId = 0,
		WorktypeName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		WorkTypeTree.Clear();

		var tree = BuildTree( Items );
		foreach ( var root in tree )
			WorkTypeTree.Add( root );

		OnPropertyChanged( nameof( WorkTypeTree ) );
	}

	private void AddSubWorkType()
	{
		if ( SelectedItem == null )
			return;

		var newWorkType = new WorktypeModel
		{
			WorktypeName = string.Empty,
			ParentId = SelectedItem.WorktypeId
		};

		SelectedItem.Children.Add( newWorkType );
		SelectedItem = newWorkType;
	}

	#region Filtering
	public bool FilterRecords( object o )
	{
		if ( o is not WorktypeModel worktype )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		if ( worktype.WorktypeName?
			.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
			return true;

		return HasMatchingChild( worktype );
	}

	private bool HasMatchingChild( WorktypeModel parent )
	{
		if ( parent.Children == null || parent.Children.Count == 0 )
			return false;

		foreach ( var child in parent.Children )
		{
			if ( child.WorktypeName?
				.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
				return true;

			if ( HasMatchingChild( child ) )
				return true;
		}

		return false;
	}
	#endregion

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( WorktypeModel c ) => new()
	{
		{ $"@{DBNames.WorktypeFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.WorktypeFieldNameName}", c.WorktypeName?.Trim() }
	};

	private static Dictionary<string, object?> UpdateParameters( WorktypeModel c ) => new()
	{
		{ $"@{DBNames.WorktypeFieldNameId}", c.WorktypeId == 0 ? null : c.WorktypeId },
		{ $"@{DBNames.WorktypeFieldNameId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.WorktypeFieldNameName}", c.WorktypeName?.Trim() }
	};
}