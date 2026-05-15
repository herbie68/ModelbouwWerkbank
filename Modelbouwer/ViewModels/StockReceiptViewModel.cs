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
	[ObservableProperty] private bool _areAllOrderLinesSelected;

	public Func<StockReceiptDialogViewModel, bool>? ShowReceiptDialog { get; set; }
	public Func<StockReceiptDateDialogViewModel, bool>? ShowReceiptDateDialog { get; set; }
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

	partial void OnAreAllOrderLinesSelectedChanged( bool value )
	{
		foreach ( var line in OpenOrderLines.Where( line => !line.Closed ) )
		{
			line.IsSelected = value;
		}
	}

	public async Task InitializeAsync()
	{
		_allOrders = await _stockOrderService.GetAllOrdersAsync();
		ApplyOrderFilters();
	}

	public void ApplySelectedOrder( StockOrderModel order, IEnumerable<StockOrderLineModel> lines )
	{
		_suppressSelectedOrderChange = true;
		if ( ReferenceEquals( SelectedOrder, order ) )
			SelectedOrder = null;

		SelectedOrder = order;
		_suppressSelectedOrderChange = false;

		OpenOrderLines.Clear();
		foreach ( var line in lines.Where( line => ShowClosedOrders || !line.Closed ) )
		{
			line.IsSelected = false;
			OpenOrderLines.Add( line );
		}

		AreAllOrderLinesSelected = false;
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
		if ( SelectedOrder == null )
			return;

		var selectedLines = OpenOrderLines.Where( line => line.IsSelected && !line.Closed ).ToList();
		if ( selectedLines.Count > 0 )
		{
			await ReceiveSelectedOrderLinesAsync( SelectedOrder, selectedLines );
			return;
		}

		if ( SelectedOrderLine == null || SelectedOrderLine.Closed )
			return;

		var order = SelectedOrder;
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

		await RefreshOrderAfterReceiptAsync( order, dialogVm.Model.DeliveryDate );
	}

	private async Task ReceiveSelectedOrderLinesAsync( StockOrderModel order, IReadOnlyCollection<StockOrderLineModel> selectedLines )
	{
		StockReceiptDateDialogViewModel dialogVm = new();
		var confirmed = ShowReceiptDateDialog?.Invoke( dialogVm ) ?? ShowReceiptDateDialogWindow( dialogVm );
		if ( !confirmed )
			return;

		foreach ( var line in selectedLines )
		{
			double receivedDelta = line.OpenAmount;
			line.Received = line.Amount;
			line.OpenAmount = 0d;
			line.Closed = true;
			line.ClosedDate = dialogVm.DeliveryDate;
			line.IsSelected = false;

			await _stockOrderService.RegisterReceiptAsync( line, receivedDelta, dialogVm.DeliveryDate );
		}

		await RefreshOrderAfterReceiptAsync( order, dialogVm.DeliveryDate );
	}

	private async Task RefreshOrderAfterReceiptAsync( StockOrderModel order, DateTime? deliveryDate )
	{
		var refreshedLines = await _stockOrderService.GetOrderLinesAsync( order.Id );
		await CloseOrderWhenAllLinesAreClosedAsync( order, refreshedLines, deliveryDate );
		ApplyOrderFilters();
		ApplySelectedOrder( order, refreshedLines );
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

	private static bool ShowReceiptDateDialogWindow( StockReceiptDateDialogViewModel viewModel )
	{
		StockReceiptDateDialog dialog = new( viewModel );
		return dialog.ShowDialog() == true;
	}
}
