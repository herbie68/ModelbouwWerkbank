using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.TreeGrid;

namespace Modelbouwer.ViewModels;

public partial class StorageLocationPickerViewModel : AsyncObservableObject
{
	private readonly IStorageLocationService _storagelocationService;

	public ObservableCollection<StorageLocationModel> StorageLocationTree { get; } = [ ];

	[ObservableProperty]
	private StorageLocationModel? _selectedStorageLocation;

	[ObservableProperty]
	private string? _searchText;

	public IRelayCommand OkCommand { get; }
	public IRelayCommand CancelCommand { get; }

	private IRelayCommand? _clearSearchCommand;
	public IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );
	public IRelayCommand ExpandAllCommand { get; }
	public IRelayCommand CollapseAllCommand { get; }

	public event Action<bool?>? CloseRequested;

	public event Action? RequestScrollToSelection;

	internal delegate void FilterChanged();
	internal FilterChanged? _filterChanged;

	public StorageLocationPickerViewModel(
		IStorageLocationService storagelocationService,
		StorageLocationModel? currentSelection = null )
	{
		_storagelocationService = storagelocationService;

		OkCommand = new RelayCommand( OnOk );
		CancelCommand = new RelayCommand( OnCancel );
		ExpandAllCommand = new RelayCommand<object>( OnExpandAll );
		CollapseAllCommand = new RelayCommand<object>( OnCollapseAll );

		ObserveBackgroundTask( LoadAsync( currentSelection ) );
	}

	private async Task LoadAsync( StorageLocationModel? currentSelection )
	{
		var flat = await _storagelocationService.GetAllStorageLocationsAsync();

		var tree = BuildTree(flat);

		StorageLocationTree.Clear();
		foreach ( var root in tree )
			StorageLocationTree.Add( root );

		// Map the incoming selection (which may be an instance from a different view model)
		// to the instance loaded into this view model's tree so UI selection works.
		if ( currentSelection != null )
		{
			// flat contains the instances used to build the tree, so find the one with the same id
			var mapped = flat.FirstOrDefault( f => f.StorageId == currentSelection.StorageId );
			SelectedStorageLocation = mapped;
		}
		else
		{
			SelectedStorageLocation = null;
		}

		RequestScrollToSelection?.Invoke();
	}

	private void OnOk() => CloseRequested?.Invoke( true );

	private void OnCancel() => CloseRequested?.Invoke( false );

	private void OnExpandAll( object? parameter )
		=> ( parameter as SfTreeGrid )?.ExpandAllNodes();

	private void OnCollapseAll( object? parameter )
		=> ( parameter as SfTreeGrid )?.CollapseAllNodes();

	private static ObservableCollection<StorageLocationModel> BuildTree( IEnumerable<StorageLocationModel> flatList )
	{
		var lookup = flatList.ToDictionary(c => c.StorageId);

		foreach ( var c in lookup.Values )
		{
			c.Children.Clear();
			c.Parent = null;
		}

		foreach ( var storagelocation in lookup.Values )
		{
			if ( storagelocation.ParentId != null &&
				lookup.TryGetValue( storagelocation.ParentId.Value, out var parent ) )
			{
				storagelocation.Parent = parent;
				parent.Children.Add( storagelocation );
			}
		}

		return new ObservableCollection<StorageLocationModel>(
			lookup.Values.Where( c => c.ParentId == null || c.ParentId == 0 ) );
	}

	#region Filtering
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
		if ( parent.Children == null || parent.Children.Count == 0 || SearchText == null )
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

	partial void OnSearchTextChanged( string? value )
	{
		_filterChanged?.Invoke();
	}
	#endregion
}
