using System.Collections.Specialized;
using System.ComponentModel;

namespace Modelbouwer.ViewModels;

public partial class StockManagementPageViewModel : EntityPageViewModel<StockManagementModel>
{
	private readonly IStockService _dataService;

	private int? _lastEditedProductId;

	// Constructor
	public StockManagementPageViewModel( IStockService dataService, IEntityValidator<StockManagementModel> validator ) : base( validator )
	{
		_dataService = dataService ?? throw new ArgumentNullException( nameof( dataService ) );

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
		if ( difference == 0 )
			return;

		_lastEditedProductId = item.ProductId;

		var parameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.StocklogFieldNameProductId}", item.ProductId },
			{ $"@{DBNames.StocklogFieldNameAmountCorrection}", difference }
		};

		await _dataService.InsertCorrectionAsync( parameters );

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
		if ( e.PropertyName != nameof( StockManagementModel.ProductInventory ) )
			return;

		if ( sender is not StockManagementModel item )
			return;

		await HandleInventoryEditAsync( item );
	}

}