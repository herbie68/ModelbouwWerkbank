using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;

using Syncfusion.Windows.Shared;

namespace Modelbouwer.ViewModels;

public class WorkTypePageViewModel : EntityPageViewModel<WorkTypeModel>, INotifyPropertyChanged
{
	private readonly IWorkTypeService _dataService;

	// Collections
	private ObservableCollection<WorkTypeModel> _fullTree = [];
	public ObservableCollection<WorkTypeModel> WorkTypes { get; } = [ ];
	public ObservableCollection<WorkTypeModel> WorkTypeTree { get; } = [ ];

	// SelectedWorkType als type-safe alias
	public WorkTypeModel? SelectedWorkType
	{
		get => SelectedItem;
		set
		{
			SelectedItem = value;
			AddSubWorkTypeCommand.NotifyCanExecuteChanged();
		}
	}

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

	private ICommand collapseCommand;

	public ICommand CollapseCommand
	{
		get { return collapseCommand; }
		set { collapseCommand = value; }
	}


	// Constructor
	public WorkTypePageViewModel(
		IWorkTypeService dataService,
		IEntityValidator<WorkTypeModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadCurrenciesAsync();
		_ = ReloadCommand.ExecuteAsync( null );

		AddSubWorkTypeCommand = new RelayCommand(
			AddSubWorkType,
			() => SelectedWorkType != null
		);

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );

	}

	// Override SelectedItem changed om DefaultWorkType te zetten
	protected override void OnSelectedItemChanged( WorkTypeModel? value )
	{
		if ( value == null )
			return;

		OnPropertyChanged( nameof( SelectedWorkType ) );
		OnPropertyChanged( nameof( SelectedWorkType.WorkTypeName ) );
		OnPropertyChanged( nameof( SelectedWorkType.ParentId ) );
		OnPropertyChanged( nameof( SelectedWorkType.WorkTypeId ) );
	}

	// Async WorkTypes laden
	private async Task LoadCurrenciesAsync()
	{
		var worktypeList = await _dataService.GetAllWorkTypesAsync();

		WorkTypes.Clear();
		foreach ( var c in worktypeList )
			WorkTypes.Add( c );
	}

	// Filtering
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

	// TreeGrid Expand and Collapse execution

	private void ExpandExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid.ExpandAllNodes();
	}

	private void CollapseExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid.CollapseAllNodes();
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
		if ( SelectedWorkType == null )
			return;

		var newWorkType = new WorkTypeModel
		{
			WorkTypeName = string.Empty,
			ParentId = SelectedWorkType.WorkTypeId
		};

		SelectedWorkType.Children.Add( newWorkType );
		SelectedWorkType = newWorkType;
	}

	#region Filtering
	internal delegate void FilterChanged();
	internal FilterChanged filterChanged;

	private string searchText = string.Empty;
	public string SearchText
	{
		get => searchText;
		set
		{
			if ( searchText != value )
			{
				searchText = value;
				RaisePropertyChanged();

				filterChanged?.Invoke();
			}
		}
	}

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
			// Child matches
			if ( child.WorkTypeName?
				.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
				return true;

			// Grandchildren match
			if ( HasMatchingChild( child ) )
				return true;
		}

		return false;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void RaisePropertyChanged( [CallerMemberName] string? propertyName = null )
	{
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
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
