using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class SupplierPageViewModel : EntityPageViewModel<SupplierModel>
{
	private readonly ISupplierService _dataService;

	private int? _lastSelectedSupplierId;

	public SupplierModel? SelectedSupplier
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddSupplierCommand => AddCommand;
	public IRelayCommand AddContactCommand => AddContactCommand;
	public IAsyncRelayCommand SaveSupplierCommand => SaveCommand;
	public IAsyncRelayCommand SaveContactCommand => SaveContactCommand;
	public IRelayCommand DeleteSupplierCommand => DeleteCommand;
	public IRelayCommand DeleteContactCommand => DeleteContactCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private SupplierModel? _previousSupplier;

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public SupplierPageViewModel( ISupplierService dataService, IEntityValidator<SupplierModel> validator ) : base( validator )
	{
		_dataService = dataService;

		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultSupplier te zetten
	protected override void OnSelectedItemChanged( SupplierModel? value )
	{
		base.OnSelectedItemChanged( value );

		_previousSupplier = value;
	}


	// Properties voor UI binding
	public ObservableCollection<SupplierModel> Suppliers => Items;
	public int TotalSupplierCount => TotalItemCount;
	public int VisibleSupplierCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterSupplier( object obj )
	{
		if ( obj is not SupplierModel Supplier )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return Supplier.SupplierName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<SupplierModel>> LoadItemsAsync() => _dataService.GetAllSuppliersAsync();
	protected override Task<int> InsertAsync( SupplierModel item ) => _dataService.InsertNewSupplierAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( SupplierModel item )
	{
		if ( SelectedItem == null )
			return Task.CompletedTask;

		_lastSelectedSupplierId = SelectedItem.SupplierId;

		return _dataService.UpdateSupplierAsync( CreateParameters( SelectedItem ) );
	}
	protected override async Task DeleteAsync( SupplierModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.SupplierName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteSupplierAsync( item.SupplierId );
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

	protected override int GetId( SupplierModel item ) => item.SupplierId;
	protected override void SetId( SupplierModel item, int id ) => item.SupplierId = id;

	protected override SupplierModel CreateNewItem() => new()
	{
		SupplierId = 0,
		SupplierName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		OnPropertyChanged( nameof( TotalSupplierCount ) );

		if ( _lastSelectedSupplierId.HasValue )
		{
			var match = Suppliers.FirstOrDefault( p => p.SupplierId == _lastSelectedSupplierId.Value );

			if ( match != null )
			{
				SelectedItem = match;
				return;
			}

			_lastSelectedSupplierId = null;
		}

		// Default Supplier selection (Highest Id)
		SelectSupplierWithHighestId();
	}

	private void SelectSupplierWithHighestId()
	{
		if ( Suppliers.Count == 0 )
		{
			SelectedItem = null;
			return;
		}

		SelectedItem = Suppliers
			.OrderByDescending( p => p.SupplierId )
			.First();
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( SupplierModel c ) => new()
	{
		{ $"@{DBNames.SupplierFieldNameId}", c.SupplierId },
		{ $"@{DBNames.SupplierFieldNameCode}", c.SupplierCode },
		{ $"@{DBNames.SupplierFieldNameName}", c.SupplierName?.Trim() },
		{ $"@{DBNames.SupplierFieldNameMemo}", c.SupplierMemo }
	};

}
