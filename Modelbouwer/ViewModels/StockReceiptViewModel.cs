using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class StockReceiptViewModel : AsyncObservableObject
{
	private readonly IStockOrderService _stockOrderService;
	private bool _suppressSelectedOrderChange;
	private List<StockOrderModel> _allOrders = [];

	public ObservableCollection<StockOrderModel> Orders { get; } = [ ];
	public ObservableCollection<StockOrderLineModel> OpenOrderLines { get; } = [ ];

	[ObservableProperty] private StockOrderModel? _selectedOrder;
	[ObservableProperty] private StockOrderLineModel? _selectedOrderLine;
	[ObservableProperty] private bool _showClosedOrders;
	[ObservableProperty] private bool _areAllOrderLinesSelected;
	[ObservableProperty] private bool _isEditingReceipt;

	public Func<StockReceiptDialogViewModel, bool>? ShowReceiptDialog { get; set; }
	public Func<StockReceiptDateDialogViewModel, bool>? ShowReceiptDateDialog { get; set; }
	public IAsyncRelayCommand EditReceiptCommand { get; }

	public StockReceiptViewModel( IStockOrderService stockOrderService )
	{
		_stockOrderService = stockOrderService;
		EditReceiptCommand = new AsyncRelayCommand( () => EditSelectedOrderLineAsync(), () => !IsEditingReceipt );
		ObserveBackgroundTask( InitializeAsync() );
	}

	partial void OnIsEditingReceiptChanged( bool value ) => EditReceiptCommand.NotifyCanExecuteChanged();

	partial void OnSelectedOrderChanged( StockOrderModel? value )
	{
		if ( _suppressSelectedOrderChange || value == null )
			return;

		ObserveBackgroundTask( LoadSelectedOrderAsync( value ) );
	}

	partial void OnShowClosedOrdersChanged( bool value )
	{
		ApplyOrderFilters();
		ObserveBackgroundTask( RefreshSelectedOrderLinesAsync() );
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

	public async Task InitializeAsync( CancellationToken cancellationToken )
	{
		_allOrders = await _stockOrderService.GetAllOrdersAsync( cancellationToken );
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

	private async Task LoadSelectedOrderAsync( StockOrderModel order, CancellationToken cancellationToken )
	{
		var lines = await _stockOrderService.GetOrderLinesAsync( order.Id, cancellationToken );
		ApplySelectedOrder( order, lines );
	}

	private async Task RefreshSelectedOrderLinesAsync()
	{
		if ( SelectedOrder == null )
			return;

		await LoadSelectedOrderAsync( SelectedOrder );
	}

	public async Task EditSelectedOrderLineAsync()
	{
		await EditSelectedOrderLineCoreAsync( null );
	}

	public async Task EditSelectedOrderLineAsync( CancellationToken cancellationToken )
	{
		await EditSelectedOrderLineCoreAsync( cancellationToken == CancellationToken.None ? null : cancellationToken );
	}

	private async Task EditSelectedOrderLineCoreAsync( CancellationToken? cancellationToken )
	{
		if ( IsEditingReceipt )
			return;

		if ( SelectedOrder == null )
			return;

		IsEditingReceipt = true;
		try
		{
			var selectedLines = OpenOrderLines.Where( line => line.IsSelected && !line.Closed ).ToList();
			if ( selectedLines.Count > 0 )
			{
				await ReceiveSelectedOrderLinesAsync( SelectedOrder, selectedLines, cancellationToken );
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

			await RegisterReceiptAsync( line, receivedDelta, dialogVm.Model.DeliveryDate, cancellationToken );

			await RefreshOrderAfterReceiptAsync( order, dialogVm.Model.DeliveryDate, cancellationToken );
		}
		finally
		{
			IsEditingReceipt = false;
		}
	}

	private async Task ReceiveSelectedOrderLinesAsync( StockOrderModel order, IReadOnlyCollection<StockOrderLineModel> selectedLines, CancellationToken? cancellationToken )
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

			await RegisterReceiptAsync( line, receivedDelta, dialogVm.DeliveryDate, cancellationToken );
		}

		await RefreshOrderAfterReceiptAsync( order, dialogVm.DeliveryDate, cancellationToken );
	}

	private async Task RefreshOrderAfterReceiptAsync( StockOrderModel order, DateTime? deliveryDate, CancellationToken? cancellationToken )
	{
		var refreshedLines = await GetOrderLinesAsync( order.Id, cancellationToken );
		await CloseOrderWhenAllLinesAreClosedAsync( order, refreshedLines, deliveryDate, cancellationToken );
		ApplyOrderFilters();
		ApplySelectedOrder( order, refreshedLines );
	}

	private async Task CloseOrderWhenAllLinesAreClosedAsync( StockOrderModel order, IReadOnlyCollection<StockOrderLineModel> lines, DateTime? deliveryDate, CancellationToken? cancellationToken )
	{
		if ( order.Closed || lines.Count == 0 || lines.Any( line => !line.Closed ) )
			return;

		order.Closed = true;
		order.ClosedDate = deliveryDate ?? DateTime.Today;
		await UpdateOrderAsync( order, cancellationToken );
	}

	private Task RegisterReceiptAsync( StockOrderLineModel line, double receivedAmount, DateTime? deliveryDate, CancellationToken? cancellationToken )
	{
		return cancellationToken.HasValue
			? _stockOrderService.RegisterReceiptAsync( line, receivedAmount, deliveryDate, cancellationToken.Value )
			: _stockOrderService.RegisterReceiptAsync( line, receivedAmount, deliveryDate );
	}

	private Task<List<StockOrderLineModel>> GetOrderLinesAsync( int orderId, CancellationToken? cancellationToken )
	{
		return cancellationToken.HasValue
			? _stockOrderService.GetOrderLinesAsync( orderId, cancellationToken.Value )
			: _stockOrderService.GetOrderLinesAsync( orderId );
	}

	private Task UpdateOrderAsync( StockOrderModel order, CancellationToken? cancellationToken )
	{
		return cancellationToken.HasValue
			? _stockOrderService.UpdateOrderAsync( order, cancellationToken.Value )
			: _stockOrderService.UpdateOrderAsync( order );
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