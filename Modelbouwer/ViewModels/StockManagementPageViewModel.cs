using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class StockManagementPageViewModel : EntityPageViewModel<StockManagementModel>
{
	private readonly IStockService _dataService;
	private readonly SettingsService _settingsService;

	private int? _lastEditedProductId;

	// Constructor
	public StockManagementPageViewModel( IStockService dataService, IEntityValidator<StockManagementModel> validator, SettingsService settingsService ) : base( validator )
	{
		_dataService = dataService ?? throw new ArgumentNullException( nameof( dataService ) );
		_settingsService = settingsService;

		DataGridSaveSettingsCommand = new RelayCommand( SaveGridLayout );
		DataGridResetSettingsCommand = new RelayCommand( ResetGridLayout );

		_ = ReloadAsync();

		// Hook collection changes to monitor new items
		Items.CollectionChanged += Items_CollectionChanged;

	}

	// -----------------------------
	// Abstract overrides from EntityPageViewModel
	// -----------------------------
	protected override async Task<List<StockManagementModel>> LoadItemsAsync()
	{
		var items = await _dataService.GetCompleteInventoryAsync();
		return items;
	}

	public int TotalInventoryCount => TotalItemCount;
	public int VisibleInventoryCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterInventory( object obj )
	{
		if ( obj is not StockManagementModel inventory )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return
			( inventory.ProductCategory ?? "" ).Contains( SearchText, StringComparison.OrdinalIgnoreCase ) ||
			( inventory.ProductCode ?? "" ).Contains( SearchText, StringComparison.OrdinalIgnoreCase ) ||
			( inventory.ProductName ?? "" ).Contains( SearchText, StringComparison.OrdinalIgnoreCase ) ||
			( inventory.ProductStorageLocation ?? "" ).Contains( SearchText, StringComparison.OrdinalIgnoreCase );
		;
	}

	public Action? SaveGridLayoutAction { get; set; }
	public Action? ResetGridLayoutAction { get; set; }

	public ICommand DataGridSaveSettingsCommand { get; }
	public ICommand DataGridResetSettingsCommand { get; }

	private void SaveGridLayout()
	{
		SaveGridLayoutAction?.Invoke();
	}

	private void ResetGridLayout()
	{
		ResetGridLayoutAction?.Invoke();
	}

	public async Task SaveGridLayoutAsync( string layout ) => await _settingsService.SaveSettingAsync( "StockManagementGridLayout", layout );

	public async Task ResetGridLayoutAsync() => await _settingsService.ResetSettingsAsync( "StockManagementGridLayout" );

	protected override Task<int> InsertAsync( StockManagementModel item ) => throw new NotImplementedException( "StockManagement does not support insert via this ViewModel." );

	protected override Task UpdateAsync( StockManagementModel item ) => throw new NotImplementedException( "Use HandleInventoryEditAsync for updates." );

	protected override Task DeleteAsync( StockManagementModel item ) => throw new NotImplementedException( "StockManagement does not support delete." );

	protected override int GetId( StockManagementModel item ) => item.ProductId;

	protected override void SetId( StockManagementModel item, int id ) => item.ProductId = id;

	protected override StockManagementModel CreateNewItem() => throw new NotImplementedException( "StockManagement does not support adding new items." );

	// -----------------------------
	// Handle editing of inventory
	// -----------------------------
	public async Task HandleInventoryEditAsync( StockManagementModel item )
	{
		if ( item == null )
			return;

		double difference = item.ProductInventory - item.ProductOriginalInventory;

		_lastEditedProductId = item.ProductId;

		if ( difference != 0 )
		{
			// First save the correction to the database
			var parameters = new Dictionary<string, object?>
			{
				{ $"@{DBNames.StocklogFieldNameProductId}", item.ProductId },
				{ $"@{DBNames.StocklogFieldNameAmountCorrection}", difference }
			};

			await _dataService.InsertCorrectionAsync( parameters );
		}

		// Now save eventualy changes of product price or minimal stock as well
		var productparameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.ProductFieldNameId}", item.ProductId },
			{ $"@{DBNames.ProductFieldNameMinimalStock}", item.ProductMinimalStock },
			{ $"@{DBNames.ProductFieldNamePrice}", item.ProductPrice }
		};

		await _dataService.UpdateProductCorrectionAsync( productparameters );

		await ReloadAndReselectAsync();
	}

	private async Task ReloadAndReselectAsync()
	{
		await ReloadAsync();

		if ( _lastEditedProductId != null )
		{
			SelectedItem = Items.FirstOrDefault( x => x.ProductId == _lastEditedProductId );
		}
		else
		{
			SelectedItem = Items.FirstOrDefault();
		}
	}

	// -----------------------------
	// Collection hooks for property changes
	// -----------------------------
	private void Items_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		if ( e.NewItems == null )
			return;

		foreach ( StockManagementModel item in e.NewItems )
			item.PropertyChanged += Item_PropertyChanged;
	}

	private async void Item_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName != nameof( StockManagementModel.ProductInventory ) && e.PropertyName != nameof( StockManagementModel.ProductMinimalStock ) && e.PropertyName != nameof( StockManagementModel.ProductPrice ) )
			return;

		if ( sender is not StockManagementModel item )
			return;

		await HandleInventoryEditAsync( item );
	}

}