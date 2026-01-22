using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Windows.Shared;

public class StorageLocationPageViewModel : EntityPageViewModel<StorageLocationModel>
{
	private readonly IStorageLocationService _dataService;

	// Collections
	public ObservableCollection<StorageLocationModel> StorageLocations { get; } = [ ];
	public ObservableCollection<StorageLocationModel> StorageLocationTree { get; } = [ ];

	// Commands
	public IRelayCommand AddStorageLocationCommand => AddCommand;
	public IAsyncRelayCommand SaveStorageLocationCommand => SaveCommand;
	public IRelayCommand DeleteStorageLocationCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand AddSubStorageLocationCommand { get; }

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
	public StorageLocationPageViewModel(
		IStorageLocationService dataService,
		IEntityValidator<StorageLocationModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadCurrenciesAsync();
		_ = ReloadCommand.ExecuteAsync( null );

		AddSubStorageLocationCommand = new RelayCommand(
			AddSubStorageLocation,
			() => SelectedItem != null  // ✅ Changed to SelectedItem
		);

		ExpandCommand = new DelegateCommand<object>( ExpandExecute );
		CollapseCommand = new DelegateCommand<object>( CollapseExecute );
	}

	// Override to handle selection changes
	protected override void OnSelectedItemChanged( StorageLocationModel? value )
	{
		AddSubStorageLocationCommand.NotifyCanExecuteChanged();
	}

	// Rest of your methods remain the same...
	private async Task LoadCurrenciesAsync()
	{
		var storagelocationList = await _dataService.GetAllStorageLocationsAsync();

		StorageLocations.Clear();
		foreach ( var c in storagelocationList )
			StorageLocations.Add( c );
	}

	public bool FilterStorageLocation( object obj )
	{
		if ( obj is not StorageLocationModel storagelocation )
			return false;

		if ( string.IsNullOrWhiteSpace( base.SearchText ) )
			return true;

		return storagelocation.StorageName?.Contains( base.SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	protected override Task<List<StorageLocationModel>> LoadItemsAsync() => _dataService.GetAllStorageLocationsAsync();
	protected override Task<int> InsertAsync( StorageLocationModel item ) => _dataService.InsertNewStorageLocationAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( StorageLocationModel item ) => _dataService.UpdateStorageLocationAsync( UpdateParameters( item ) );
	protected override async Task DeleteAsync( StorageLocationModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.StorageName}' {Lang.toolbarButtonActionDeleteMessageQuestionStorageLocationSuffix}",
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

	public ObservableCollection<StorageLocationModel> BuildTree( IEnumerable<StorageLocationModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.StorageId);

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
		StorageName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		StorageLocationTree.Clear();

		var tree = BuildTree(Items);
		foreach ( var root in tree )
			StorageLocationTree.Add( root );

		OnPropertyChanged( nameof( StorageLocationTree ) );
	}

	private void AddSubStorageLocation()
	{
		if ( SelectedItem == null )  // ✅ Changed to SelectedItem
			return;

		var newStorageLocation = new StorageLocationModel
		{
			StorageName = string.Empty,
			ParentId = SelectedItem.StorageId  // ✅ Changed to SelectedItem
		};

		SelectedItem.Children.Add( newStorageLocation );  // ✅ Changed to SelectedItem
		SelectedItem = newStorageLocation;  // ✅ Changed to SelectedItem
	}

	#region Filtering
	internal delegate void FilterChanged();
	internal FilterChanged filterChanged;

	public bool FilterRecords( object o )
	{
		if ( o is not StorageLocationModel storagelocation )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		if ( storagelocation.StorageName?
			.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
			return true;

		return HasMatchingChild( storagelocation );
	}

	private bool HasMatchingChild( StorageLocationModel parent )
	{
		if ( parent.Children == null || parent.Children.Count == 0 )
			return false;

		foreach ( var child in parent.Children )
		{
			if ( child.StorageName?
				.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) == true )
				return true;

			if ( HasMatchingChild( child ) )
				return true;
		}

		return false;
	}
	#endregion

	private static Dictionary<string, object?> CreateParameters( StorageLocationModel c ) => new()
	{
		{ $"@{DBNames.StorageFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.StorageFieldNameName}", c.StorageName?.Trim() }
	};

	private static Dictionary<string, object?> UpdateParameters( StorageLocationModel c ) => new()
	{
		{ $"@{DBNames.StorageFieldNameId}", c.StorageId },
		{ $"@{DBNames.StorageFieldNameParentId}", c.ParentId == 0 ? null : c.ParentId },
		{ $"@{DBNames.StorageFieldNameName}", c.StorageName?.Trim() }
	};
}