using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class StockReceiptViewModel : ObservableObject
{
	private readonly IStockOrderService _stockOrderService;
	private bool _suppressSelectedOrderChange;
	private List<StockOrderModel> _allOrders = [];

	public ObservableCollection<StockOrderModel> Orders { get; } = [];
	public ObservableCollection<StockOrderLineModel> OpenOrderLines { get; } = [];

	[ObservableProperty] private StockOrderModel? _selectedOrder;
	[ObservableProperty] private StockOrderLineModel? _selectedOrderLine;
	[ObservableProperty] private bool _showClosedOrders;

	public Func<StockReceiptDialogViewModel, bool>? ShowReceiptDialog { get; set; }
	public IAsyncRelayCommand EditReceiptCommand { get; }

	public StockReceiptViewModel( IStockOrderService stockOrderService )
	{
		_stockOrderService = stockOrderService;
		EditReceiptCommand = new AsyncRelayCommand( EditSelectedOrderLineAsync );
		_ = InitializeAsync();
	}

	partial void OnSelectedOrderChanged( StockOrderModel? value )
	{
		if ( _suppressSelectedOrderChange || value == null )
			return;

		_ = LoadSelectedOrderAsync( value );
	}

	partial void OnShowClosedOrdersChanged( bool value )
	{
		ApplyOrderFilters();
		_ = RefreshSelectedOrderLinesAsync();
	}

	public async Task InitializeAsync()
	{
		_allOrders = await _stockOrderService.GetAllOrdersAsync();
		ApplyOrderFilters();
	}

	public void ApplySelectedOrder( StockOrderModel order, IEnumerable<StockOrderLineModel> lines )
	{
		_suppressSelectedOrderChange = true;
		SelectedOrder = order;
		_suppressSelectedOrderChange = false;

		OpenOrderLines.Clear();
		foreach ( var line in lines.Where( line => ShowClosedOrders || !line.Closed ) )
		{
			OpenOrderLines.Add( line );
		}

		SelectedOrderLine = null;
	}

	private async Task LoadSelectedOrderAsync( StockOrderModel order )
	{
		var lines = await _stockOrderService.GetOrderLinesAsync( order.Id );
		ApplySelectedOrder( order, lines );
	}

	private async Task RefreshSelectedOrderLinesAsync()
	{
		if ( SelectedOrder == null )
			return;

		await LoadSelectedOrderAsync( SelectedOrder );
	}

	private async Task EditSelectedOrderLineAsync()
	{
		if ( SelectedOrder == null || SelectedOrderLine == null || SelectedOrderLine.Closed )
			return;

		var line = SelectedOrderLine;
		var dialogVm = new StockReceiptDialogViewModel( StockReceiptDialogModel.Create( line ) );
		var confirmed = ShowReceiptDialog?.Invoke( dialogVm ) ?? ShowReceiptDialogWindow( dialogVm );
		if ( !confirmed )
			return;

		var validationMessage = dialogVm.Validate();
		if ( validationMessage != null )
		{
			MessageBox.Show( validationMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
			return;
		}

		double receivedDelta = dialogVm.Model.ReceivedDelta;
		line.Amount = dialogVm.Model.OrderedAmount;
		line.Received = dialogVm.Model.ReceivedAmount;
		line.OpenAmount = dialogVm.Model.OpenAmount;
		line.Closed = dialogVm.Model.IsOrderLineClosed;
		line.ClosedDate = line.Closed ? dialogVm.Model.DeliveryDate : null;

		await _stockOrderService.RegisterReceiptAsync( line, receivedDelta, dialogVm.Model.DeliveryDate );

		var refreshedLines = await _stockOrderService.GetOrderLinesAsync( SelectedOrder.Id );
		await CloseOrderWhenAllLinesAreClosedAsync( SelectedOrder, refreshedLines, dialogVm.Model.DeliveryDate );
		ApplyOrderFilters();
		ApplySelectedOrder( SelectedOrder, refreshedLines );
	}

	private async Task CloseOrderWhenAllLinesAreClosedAsync( StockOrderModel order, IReadOnlyCollection<StockOrderLineModel> lines, DateTime? deliveryDate )
	{
		if ( order.Closed || lines.Count == 0 || lines.Any( line => !line.Closed ) )
			return;

		order.Closed = true;
		order.ClosedDate = deliveryDate ?? DateTime.Today;
		await _stockOrderService.UpdateOrderAsync( order );
	}

	private void ApplyOrderFilters()
	{
		Orders.Clear();

		IEnumerable<StockOrderModel> filtered = _allOrders;
		if ( !ShowClosedOrders )
			filtered = filtered.Where( order => !order.Closed );

		foreach ( var order in filtered )
		{
			Orders.Add( order );
		}
	}

	public void ReplaceOrdersForTest( IEnumerable<StockOrderModel> orders )
	{
		_allOrders = orders.ToList();
		ApplyOrderFilters();
	}

	private static bool ShowReceiptDialogWindow( StockReceiptDialogViewModel viewModel )
	{
		StockReceiptDialog dialog = new( viewModel );
		return dialog.ShowDialog() == true;
	}
}
