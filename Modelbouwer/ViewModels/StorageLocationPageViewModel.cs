using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;

using Syncfusion.Windows.Shared;

namespace Modelbouwer.ViewModels;

public class StorageLocationPageViewModel : EntityPageViewModel<StorageLocationModel>, INotifyPropertyChanged
{
	private readonly IStorageLocationService _dataService;

	// Collections
	private ObservableCollection<StorageLocationModel> _fullTree = [];
	public ObservableCollection<StorageLocationModel> StorageLocations { get; } = [ ];
	public ObservableCollection<StorageLocationModel> StorageLocationTree { get; } = [ ];

	// SelectedStorageLocation als type-safe alias
	public StorageLocationModel? SelectedStorageLocation
	{
		get => SelectedItem;
		set
		{
			SelectedItem = value;
			OnPropertyChanged();
			AddSubStorageLocationCommand.NotifyCanExecuteChanged();
		}
	}

	// Commands
	public IRelayCommand AddStorageLocationCommand => AddCommand;
	public IAsyncRelayCommand SaveStorageLocationCommand => SaveCommand;
	public IRelayCommand DeleteStorageLocationCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand AddSubStorageLocationCommand { get; }

	private IRelayCommand? _clearSearchCommand;

	private ICommand _expandCommand;

	public ICommand ExpandCommand
	{
		get { return _expandCommand; }
		set { _expandCommand = value; }
	}

	private ICommand _collapseCommand;

	public ICommand CollapseCommand
	{
		get { return _collapseCommand; }
		set { _collapseCommand = value; }
	}


	// Constructor
	public StorageLocationPageViewModel(
		IStorageLocationService dataService,
		IEntityValidator<StorageLocationModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadStorageLocationsAsync();
		_ = ReloadCommand.ExecuteAsync( null );

		AddSubStorageLocationCommand = new RelayCommand(
			AddSubStorageLocation,
			() => SelectedStorageLocation != null
		);

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );

	}

	// Override SelectedItem changed om DefaultStorageLocation te zetten
	protected override void OnSelectedItemChanged( StorageLocationModel? value )
	{
		if ( value == null )
			return;

		OnPropertyChanged( nameof( SelectedStorageLocation ) );
	}

	// Async categories laden
	private async Task LoadStorageLocationsAsync()
	{
		var storagelocationList = await _dataService.GetAllStorageLocationsAsync();

		StorageLocations.Clear();
		foreach ( var c in storagelocationList )
			StorageLocations.Add( c );
	}

	// Filtering
	public bool FilterStorageLocation( object obj )
	{
		if ( obj is not StorageLocationModel storagelocation )
			return false;

		if ( string.IsNullOrWhiteSpace( base.SearchText ) )
			return true;

		return storagelocation.StorageLocationName?.Contains( base.SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<StorageLocationModel>> LoadItemsAsync() => _dataService.GetAllStorageLocationsAsync();
	protected override Task<int> InsertAsync( StorageLocationModel item ) => _dataService.InsertNewStorageLocationAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( StorageLocationModel item ) => _dataService.UpdateStorageLocationAsync( UpdateParameters( item ) );
	protected override async Task DeleteAsync( StorageLocationModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.StorageLocationName}' {Lang.toolbarButtonActionDeleteMessageQuestionStorageLocationSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteStorageLocationAsync( item.StorageId );
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
		treeGrid?.ExpandAllNodes();
	}

	private void CollapseExecute( object obj )
	{
		var treeGrid = obj as SfTreeGrid;
		treeGrid?.CollapseAllNodes();
	}


	public static ObservableCollection<StorageLocationModel> BuildTree( IEnumerable<StorageLocationModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.StorageId);

		// Make sure Children are not doubled
		foreach ( var c in lookup.Values )
			c.Children.Clear();

		foreach ( var storagelocation in lookup.Values )
		{
			if ( storagelocation.ParentId != null &&
				lookup.TryGetValue( storagelocation.ParentId.Value, out var parent ) )
			{
				parent.Children.Add( storagelocation );
			}
		}

		return new ObservableCollection<StorageLocationModel>( lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}

	protected override int GetId( StorageLocationModel item ) => item.StorageId;
	protected override void SetId( StorageLocationModel item, int id ) => item.StorageId = id;

	protected override StorageLocationModel CreateNewItem() => new()
	{
		StorageId = 0,
		ParentId = 0,
		StorageLocationName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		StorageLocationTree.Clear();

		var tree = BuildTree( Items );
		foreach ( var root in tree )
			StorageLocationTree.Add( root );

		OnPropertyChanged( nameof( StorageLocationTree ) );
	}

	private void AddSubStorageLocation()
	{
		if ( SelectedStorageLocation == null )
			return;

		var newStorageLocation = new StorageLocationModel
		{
			StorageLocationName = string.Empty,
			ParentId = SelectedStorageLocation.StorageId
		};

		SelectedStorageLocation.Children.Add( newStorageLocation );
		SelectedStorageLocation = newStorageLocation;
	}

	#region Filtering
	internal delegate void FilterChanged();
	internal FilterChanged _filterChanged;

	private string _searchText = string.Empty;
	public string SearchText
	{
		get => _searchText;
		set
		{
			if ( _searchText != value )
			{
				_searchText = value;
				RaisePropertyChanged();

				_filterChanged?.Invoke();
			}
		}
	}

	public bool FilterRecords( object o )
	{
		if ( o is not StorageLocationModel storagelocation )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		// 1️⃣ Check current node
		if ( storagelocation.StorageLocationName?
			.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
			return true;

		// 2️⃣ Check children (important!)
		return HasMatchingChild( storagelocation );
	}

	private bool HasMatchingChild( StorageLocationModel parent )
	{
		if ( parent.Children == null || parent.Children.Count == 0 )
			return false;

		foreach ( var child in parent.Children )
		{
			// Child matches
			if ( child.StorageLocationName?
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
	private static Dictionary<string, object?> CreateParameters( StorageLocationModel c ) => new()
	{
		{ $"@{DBNames.StorageFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.StorageFieldNameName}", c.StorageLocationName?.Trim() }
	};

	private static Dictionary<string, object?> UpdateParameters( StorageLocationModel c ) => new()
	{
		{ $"@{DBNames.StorageFieldNameId}", c.StorageId == 0 ? null : c.StorageId },
		{ $"@{DBNames.StorageFieldNameId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.StorageFieldNameName}", c.StorageLocationName?.Trim() }
	};
}
