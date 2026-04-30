using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class StockOrderViewModel : ObservableObject
{
	private readonly IStockOrderService _stockOrderService;
	private readonly IProductService _productService;
	private readonly ISupplierService _supplierService;
	private bool _suppressSelectedOrderChange;
	private List<StockOrderModel> _allOrders = [];

	public ObservableCollection<StockOrderModel> Orders { get; } = [];
	public ObservableCollection<StockOrderLineModel> OrderLines { get; } = [];
	public ObservableCollection<StockOrderLineModel> PendingOrderLines { get; } = [];
	public ObservableCollection<ProductModel> AvailableProducts { get; } = [];
	public ObservableCollection<SupplierModel> Suppliers { get; } = [];
	public ObservableCollection<CurrencyModel> Currencies { get; } = [];

	[ObservableProperty] private StockOrderModel _editableOrder = new();
	[ObservableProperty] private StockOrderModel? _selectedOrder;
	[ObservableProperty] private StockOrderLineModel? _selectedOrderLine;
	[ObservableProperty] private ProductModel? _selectedProduct;
	[ObservableProperty] private SupplierModel? _selectedSupplier;
	[ObservableProperty] private CurrencyModel? _selectedCurrency;
	[ObservableProperty] private bool _isNewOrder;
	[ObservableProperty] private bool _hasUnsavedChanges;
	[ObservableProperty] private bool _enableSupplierOrderFilter;

	public bool IsClosedOrder => EditableOrder.Closed;
	public bool CanEditOrder => !EditableOrder.Closed;
	public ObservableCollection<StockOrderLineModel> VisibleOrderLines => IsNewOrder ? PendingOrderLines : OrderLines;
	public Func<StockOrderProductDialogViewModel, bool>? ShowProductDialog { get; set; }

	public IRelayCommand NewOrderCommand { get; }
	public IAsyncRelayCommand SaveOrderCommand { get; }
	public IAsyncRelayCommand DeleteOrderCommand { get; }
	public IRelayCommand ResetOrderCommand { get; }
	public IAsyncRelayCommand AddProductToOrderCommand { get; }
	public IAsyncRelayCommand EditOrderLineCommand { get; }
	public IAsyncRelayCommand DeleteOrderLineCommand { get; }

	public StockOrderViewModel(
		IStockOrderService stockOrderService,
		IProductService productService,
		ISupplierService supplierService )
	{
		_stockOrderService = stockOrderService;
		_productService = productService;
		_supplierService = supplierService;

		NewOrderCommand = new RelayCommand( BeginNewOrder );
		SaveOrderCommand = new AsyncRelayCommand( SaveOrderAsync );
		DeleteOrderCommand = new AsyncRelayCommand( DeleteOrderAsync );
		ResetOrderCommand = new RelayCommand( ResetOrder );
		AddProductToOrderCommand = new AsyncRelayCommand( AddSelectedProductAsync );
		EditOrderLineCommand = new AsyncRelayCommand( EditSelectedOrderLineAsync );
		DeleteOrderLineCommand = new AsyncRelayCommand( DeleteSelectedOrderLineAsync );

		BeginNewOrder();
		_ = InitializeAsync();
	}

	partial void OnSelectedOrderChanged( StockOrderModel? value )
	{
		if ( _suppressSelectedOrderChange || value == null )
			return;

		_ = LoadSelectedOrderAsync( value );
	}

	partial void OnEnableSupplierOrderFilterChanged( bool value )
	{
		ApplySupplierFilter();
	}

	public async Task InitializeAsync()
	{
		await LoadReferenceDataAsync();
		await LoadOrdersAsync();
	}

	public void BeginNewOrder()
	{
		EditableOrder = new StockOrderModel
		{
			Id = 0,
			OrderDate = DateTime.Today
		};

		_suppressSelectedOrderChange = true;
		SelectedOrder = null;
		_suppressSelectedOrderChange = false;
		SelectedOrderLine = null;
		PendingOrderLines.Clear();
		OrderLines.Clear();
		IsNewOrder = true;
		HasUnsavedChanges = false;
		RefreshLookupsFromEditableOrder();
		RaiseOrderStateProperties();
	}

	public void ApplySelectedOrder( StockOrderModel order, IEnumerable<StockOrderLineModel> lines )
	{
		_suppressSelectedOrderChange = true;
		SelectedOrder = order;
		_suppressSelectedOrderChange = false;

		EditableOrder = new StockOrderModel
		{
			Id = order.Id,
			SupplierId = order.SupplierId,
			CurrencyId = order.CurrencyId,
			SupplierName = order.SupplierName,
			CurrencySymbol = order.CurrencySymbol,
			OrderNumber = order.OrderNumber,
			OrderDate = order.OrderDate,
			ShippingCosts = order.ShippingCosts,
			OrderCosts = order.OrderCosts,
			Memo = order.Memo,
			Closed = order.Closed,
			ClosedDate = order.ClosedDate,
			HasStockLog = order.HasStockLog
		};

		OrderLines.Clear();
		foreach ( var line in lines )
		{
			OrderLines.Add( line );
		}

		PendingOrderLines.Clear();
		SelectedOrderLine = null;
		IsNewOrder = false;
		HasUnsavedChanges = false;
		RefreshLookupsFromEditableOrder();
		RecalculateTotals();
		RaiseOrderStateProperties();
	}

	public async Task SaveOrderAsync()
	{
		var validationMessage = ValidateOrderForSave();
		if ( validationMessage != null )
		{
			MessageBox.Show( validationMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
			return;
		}

		if ( IsNewOrder )
		{
			var newOrderId = await _stockOrderService.InsertOrderAsync( EditableOrder );
			EditableOrder.Id = newOrderId;

			foreach ( var line in PendingOrderLines )
			{
				line.SupplyOrderId = newOrderId;
				if ( line.SupplierId <= 0 )
					line.SupplierId = EditableOrder.SupplierId;

				await _stockOrderService.InsertOrderLineAsync( line );
			}

			var lines = await _stockOrderService.GetOrderLinesAsync( newOrderId );
			await LoadOrdersAsync();

			var reloadedOrder = Orders.FirstOrDefault( o => o.Id == newOrderId ) ?? new StockOrderModel
			{
				Id = EditableOrder.Id,
				SupplierId = EditableOrder.SupplierId,
				CurrencyId = EditableOrder.CurrencyId,
				OrderNumber = EditableOrder.OrderNumber,
				OrderDate = EditableOrder.OrderDate
			};

			ApplySelectedOrder( reloadedOrder, lines );
			return;
		}

		await _stockOrderService.UpdateOrderAsync( EditableOrder );
		await LoadOrdersAsync();

		if ( SelectedOrder != null )
		{
			var lines = await _stockOrderService.GetOrderLinesAsync( SelectedOrder.Id );
			var reloadedOrder = Orders.FirstOrDefault( o => o.Id == SelectedOrder.Id ) ?? SelectedOrder;
			ApplySelectedOrder( reloadedOrder, lines );
		}
	}

	public string? ValidateOrderForSave()
	{
		if ( EditableOrder.Closed )
			return "Closed orders can not be saved.";

		if ( EditableOrder.SupplierId <= 0 )
			return "Supplier is verplicht.";

		if ( string.IsNullOrWhiteSpace( EditableOrder.OrderNumber ) )
			return "Ordernummer is verplicht.";

		if ( EditableOrder.OrderDate == null )
			return "Besteldatum is verplicht.";

		return null;
	}

	private async Task LoadReferenceDataAsync()
	{
		var products = await _productService.GetAllProductsAsync();
		AvailableProducts.Clear();
		foreach ( var product in products )
		{
			AvailableProducts.Add( product );
		}

		var suppliers = await _supplierService.GetAllSuppliersAsync();
		Suppliers.Clear();
		foreach ( var supplier in suppliers )
		{
			Suppliers.Add( supplier );
		}

		var currencies = await _supplierService.GetAllCurrenciesAsync();
		Currencies.Clear();
		foreach ( var currency in currencies )
		{
			Currencies.Add( currency );
		}

		RefreshLookupsFromEditableOrder();
	}

	private async Task LoadOrdersAsync()
	{
		_allOrders = await _stockOrderService.GetAllOrdersAsync();
		ApplySupplierFilter();
	}

	private async Task LoadSelectedOrderAsync( StockOrderModel order )
	{
		var lines = await _stockOrderService.GetOrderLinesAsync( order.Id );
		ApplySelectedOrder( order, lines );
	}

	private void RefreshLookupsFromEditableOrder()
	{
		SelectedSupplier = Suppliers.FirstOrDefault( s => s.Id == EditableOrder.SupplierId );
		SelectedCurrency = Currencies.FirstOrDefault( c => c.CurrencyId == EditableOrder.CurrencyId );
	}

	private void RecalculateTotals()
	{
		EditableOrder.LinesTotal = VisibleOrderLines.Sum( line => line.RealRowTotal );
	}

	private void RaiseOrderStateProperties()
	{
		OnPropertyChanged( nameof( IsClosedOrder ) );
		OnPropertyChanged( nameof( CanEditOrder ) );
		OnPropertyChanged( nameof( VisibleOrderLines ) );
	}

	public void ReplaceOrdersForTest( IEnumerable<StockOrderModel> orders )
	{
		_allOrders = orders.ToList();
		ApplySupplierFilter();
	}

	private Task DeleteOrderAsync()
	{
		return Task.CompletedTask;
	}

	private void ResetOrder()
	{
		if ( SelectedOrder != null )
		{
			ApplySelectedOrder( SelectedOrder, OrderLines.ToList() );
			return;
		}

		BeginNewOrder();
	}

	public async Task AddSelectedProductAsync()
	{
		if ( !CanEditOrder || SelectedProduct == null )
			return;

		var supplier = Suppliers.FirstOrDefault( s => s.Id == EditableOrder.SupplierId );
		if ( supplier == null )
		{
			MessageBox.Show( "Selecteer eerst een leverancier.", Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
			return;
		}

		var existingProductSupplier = await _supplierService.GetProductSupplierAsync( supplier.Id, SelectedProduct.ProductId );
		var dialogModel = StockOrderProductDialogModel.Create( SelectedProduct, supplier, existingProductSupplier );
		var dialogVm = new StockOrderProductDialogViewModel( dialogModel );
		var confirmed = ShowProductDialog?.Invoke( dialogVm ) ?? ShowProductDialogWindow( dialogVm );

		if ( !confirmed )
			return;

		await UpsertProductSupplierAsync( dialogVm.Model );

		var line = new StockOrderLineModel
		{
			SupplyOrderId = EditableOrder.Id,
			SupplierId = supplier.Id,
			ProductId = SelectedProduct.ProductId,
			ProductCode = SelectedProduct.ProductCode,
			ProductName = SelectedProduct.ProductName,
			SupplierProductName = dialogVm.Model.SupplierProductName,
			Amount = dialogVm.Model.Amount,
			OpenAmount = dialogVm.Model.Amount,
			Price = dialogVm.Model.UnitPrice,
			RealRowTotal = dialogVm.Model.RowTotal
		};

		if ( IsNewOrder )
		{
			PendingOrderLines.Add( line );
		}
		else
		{
			line.Id = await _stockOrderService.InsertOrderLineAsync( line );
			OrderLines.Add( line );
		}

		RecalculateTotals();
		HasUnsavedChanges = true;
		OnPropertyChanged( nameof( VisibleOrderLines ) );
	}

	private Task EditSelectedOrderLineAsync()
	{
		return Task.CompletedTask;
	}

	private Task DeleteSelectedOrderLineAsync()
	{
		return Task.CompletedTask;
	}

	private async Task<int> UpsertProductSupplierAsync( StockOrderProductDialogModel model )
	{
		Modelbouwer.Model.ProductSupplierModel productSupplier = new()
		{
			ProductSupplierId = model.ProductSupplierId,
			SupplierId = model.SupplierId,
			ProductId = model.ProductId,
			CurrencyId = model.CurrencyId,
			ProductNumber = model.SupplierProductNumber,
			ProductName = model.SupplierProductName,
			Price = model.UnitPrice,
			URL = model.ProductUrl,
			DefaultSupplier = false
		};

		var productSupplierId = await _supplierService.UpsertProductSupplierAsync( productSupplier );
		model.ProductSupplierId = productSupplierId;
		model.ProductSupplierExists = true;
		return productSupplierId;
	}

	private static bool ShowProductDialogWindow( StockOrderProductDialogViewModel viewModel )
	{
		StockOrderProductDialog dialog = new( viewModel );
		return dialog.ShowDialog() == true;
	}

	private void ApplySupplierFilter()
	{
		Orders.Clear();

		IEnumerable<StockOrderModel> filtered = _allOrders;

		if ( EnableSupplierOrderFilter && EditableOrder.SupplierId > 0 )
			filtered = filtered.Where( o => o.SupplierId == EditableOrder.SupplierId );

		foreach ( var order in filtered )
		{
			Orders.Add( order );
		}
	}
}
