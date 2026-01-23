using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Windows.Shared;

namespace Modelbouwer.ViewModels;

public class WorkTypePageViewModel : EntityPageViewModel<WorkTypeModel>
{
	private readonly IWorkTypeService _dataService;

	// Collections
	public ObservableCollection<WorkTypeModel> WorkTypes { get; } = [ ];
	public ObservableCollection<WorkTypeModel> WorkTypeTree { get; } = [ ];

	// Commands
	public IRelayCommand AddWorkTypeCommand => AddCommand;
	public IAsyncRelayCommand SaveWorkTypeCommand => SaveCommand;
	public IRelayCommand DeleteWorkTypeCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand AddSubWorkTypeCommand { get; }

	private IRelayCommand? _clearSearchCommand;

	private ICommand expandCommand;

	public ICommand ExpandCommand
	{
		get { return expandCommand; }
		set { expandCommand = value; }
	}

	private ICommand _collapseCommand;

	public ICommand CollapseCommand
	{
		get { return _collapseCommand; }
		set { _collapseCommand = value; }
	}

	// Constructor
	public WorkTypePageViewModel(
		IWorkTypeService dataService,
		IEntityValidator<WorkTypeModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadWorkTypesAsync();
		_ = ReloadCommand.ExecuteAsync( null );

		AddSubWorkTypeCommand = new RelayCommand(
	AddSubWorkType,
	() => SelectedItem != null
);

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );

	}

	// Override SelectedItem changed om DefaultWorkType te zetten
	protected override void OnSelectedItemChanged( WorkTypeModel? value )
	{
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
		if ( obj is not WorkTypeModel worktype )
			return false;

		if ( string.IsNullOrWhiteSpace( base.SearchText ) )
			return true;

		return worktype.WorkTypeName?.Contains( base.SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<WorkTypeModel>> LoadItemsAsync() => _dataService.GetAllWorkTypesAsync();
	protected override Task<int> InsertAsync( WorkTypeModel item ) => _dataService.InsertNewWorkTypeAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( WorkTypeModel item ) => _dataService.UpdateWorkTypeAsync( UpdateParameters( item ) );
	protected override async Task DeleteAsync( WorkTypeModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.WorkTypeName}' {Lang.toolbarButtonActionDeleteMessageQuestionWorkTypeSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteWorkTypeAsync( item.WorkTypeId );
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


	public ObservableCollection<WorkTypeModel> BuildTree( IEnumerable<WorkTypeModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.WorkTypeId);

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

		return new ObservableCollection<WorkTypeModel>( lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}

	protected override int GetId( WorkTypeModel item ) => item.WorkTypeId;
	protected override void SetId( WorkTypeModel item, int id ) => item.WorkTypeId = id;

	protected override WorkTypeModel CreateNewItem() => new()
	{
		WorkTypeId = 0,
		ParentId = 0,
		WorkTypeName = string.Empty
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
		if ( SelectedItem == null )  // Changed from SelectedWorkType
			return;

		var newWorkType = new WorkTypeModel
		{
			WorkTypeName = string.Empty,
			ParentId = SelectedItem.WorkTypeId  // Changed from SelectedWorkType
		};

		SelectedItem.Children.Add( newWorkType );  // Changed from SelectedWorkType
		SelectedItem = newWorkType;  // Changed from SelectedWorkType
	}

	#region Filtering
	internal delegate void FilterChanged();
	internal FilterChanged _filterChanged;

	public bool FilterRecords( object o )
	{
		if ( o is not WorkTypeModel worktype )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		// 1️⃣ Check current node
		if ( worktype.WorkTypeName?
			.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
			return true;

		// 2️⃣ Check children (important!)
		return HasMatchingChild( worktype );
	}

	private bool HasMatchingChild( WorkTypeModel parent )
	{
		if ( parent.Children == null || parent.Children.Count == 0 )
			return false;

		foreach ( var child in parent.Children )
		{
			if ( child.WorkTypeName?
				.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
				return true;

			if ( HasMatchingChild( child ) )
				return true;
		}

		return false;
	}
	#endregion

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( WorkTypeModel c ) => new()
	{
		{ $"@{DBNames.WorktypeFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.WorktypeFieldNameName}", c.WorkTypeName?.Trim() }
	};

	private static Dictionary<string, object?> UpdateParameters( WorkTypeModel c ) => new()
	{
		{ $"@{DBNames.WorktypeFieldNameId}", c.WorkTypeId == 0 ? null : c.WorkTypeId },
		{ $"@{DBNames.WorktypeFieldNameId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.WorktypeFieldNameName}", c.WorkTypeName?.Trim() }
	};
}