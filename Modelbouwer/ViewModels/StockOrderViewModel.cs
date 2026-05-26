using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class StockOrderViewModel : AsyncObservableObject
{
	private readonly IStockOrderService _stockOrderService;
	private readonly IProductService _productService;
	private readonly IStockService _stockService;
	private readonly ISupplierService _supplierService;
	private bool _suppressSelectedOrderChange;
	private bool _suppressEditableOrderTracking;
	private bool _shippingCostsOverridden;
	private bool _orderCostsOverridden;
	private StockOrderModel? _trackedEditableOrder;
	private List<StockOrderModel> _allOrders = [];
	private List<ProductModel> _allAvailableProducts = [];
	private Dictionary<int, StockManagementModel> _inventoryByProductId = [];
	private int _selectionLoadVersion;
	private IRelayCommand? _clearSearchCommand;

	public ObservableCollection<StockOrderModel> Orders { get; } = [ ];
	public ObservableCollection<StockOrderLineModel> OrderLines { get; } = [ ];
	public ObservableCollection<StockOrderLineModel> PendingOrderLines { get; } = [ ];
	public ObservableCollection<ProductModel> AvailableProducts { get; } = [ ];
	public ObservableCollection<SupplierModel> Suppliers { get; } = [ ];
	public ObservableCollection<CurrencyModel> Currencies { get; } = [ ];

	[ObservableProperty] private StockOrderModel _editableOrder = new();
	[ObservableProperty] private StockOrderModel? _selectedOrder;
	[ObservableProperty] private StockOrderLineModel? _selectedOrderLine;
	[ObservableProperty] private ProductModel? _selectedProduct;
	[ObservableProperty] private SupplierModel? _selectedSupplier;
	[ObservableProperty] private CurrencyModel? _selectedCurrency;
	[ObservableProperty] private bool _isNewOrder;
	[ObservableProperty] private bool _hasUnsavedChanges;
	[ObservableProperty] private bool _isSavingOrder;
	[ObservableProperty] private bool _isEditingOrderLine;
	[ObservableProperty] private bool _enableSupplierOrderFilter;
	[ObservableProperty] private bool _showClosedOrders;
	[ObservableProperty] private string? _searchText;

	public bool IsClosedOrder => EditableOrder.Closed;
	public bool CanEditOrder => !EditableOrder.Closed;
	public ObservableCollection<StockOrderLineModel> VisibleOrderLines => IsNewOrder ? PendingOrderLines : OrderLines;
	public Func<StockOrderProductDialogViewModel, bool>? ShowProductDialog { get; set; }

	public IRelayCommand NewOrderCommand { get; }
	public IAsyncRelayCommand SaveOrderCommand { get; }
	public IAsyncRelayCommand DeleteOrderCommand { get; }
	public IAsyncRelayCommand AddProductToOrderCommand { get; }
	public IAsyncRelayCommand EditOrderLineCommand { get; }
	public IAsyncRelayCommand DeleteOrderLineCommand { get; }
	public IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	public StockOrderViewModel(
		IStockOrderService stockOrderService,
		IProductService productService,
		IStockService stockService,
		ISupplierService supplierService )
	{
		_stockOrderService = stockOrderService;
		_productService = productService;
		_stockService = stockService;
		_supplierService = supplierService;

		NewOrderCommand = new RelayCommand( BeginNewOrder );
		SaveOrderCommand = new AsyncRelayCommand( cancellationToken => SaveOrderAsync( cancellationToken ), CanSaveOrder );
		DeleteOrderCommand = new AsyncRelayCommand( DeleteOrderAsync );
		AddProductToOrderCommand = new AsyncRelayCommand( AddSelectedProductAsync, CanEditOrderLineCommand );
		EditOrderLineCommand = new AsyncRelayCommand( EditSelectedOrderLineAsync, CanEditOrderLineCommand );
		DeleteOrderLineCommand = new AsyncRelayCommand( DeleteSelectedOrderLineAsync, CanEditOrderLineCommand );

		BeginNewOrder();
		ObserveBackgroundTask( InitializeAsync() );
	}

	partial void OnSelectedOrderChanged( StockOrderModel? value )
	{
		if ( _suppressSelectedOrderChange || value == null )
			return;

		int loadVersion = ++_selectionLoadVersion;
		ObserveBackgroundTask( LoadSelectedOrderAsync( value, loadVersion ) );
	}

	partial void OnEditableOrderChanged( StockOrderModel value )
	{
		if ( _trackedEditableOrder != null )
		{
			_trackedEditableOrder.PropertyChanged -= EditableOrder_PropertyChanged;
		}

		_trackedEditableOrder = value;
		_trackedEditableOrder.PropertyChanged += EditableOrder_PropertyChanged;
		RefreshLookupsFromEditableOrder();
	}

	partial void OnEnableSupplierOrderFilterChanged( bool value )
	{
		ApplyOrderFilters();
	}

	partial void OnShowClosedOrdersChanged( bool value )
	{
		ApplyOrderFilters();
	}

	partial void OnSearchTextChanged( string? value )
	{
		ApplyProductSearchFilter();
	}

	partial void OnIsSavingOrderChanged( bool value ) => SaveOrderCommand.NotifyCanExecuteChanged();
	partial void OnIsEditingOrderLineChanged( bool value ) => NotifyOrderLineCommandsCanExecuteChanged();

	public async Task InitializeAsync()
	{
		await LoadReferenceDataAsync();
		await LoadOrdersAsync();
	}

	public async Task InitializeAsync( CancellationToken cancellationToken )
	{
		await LoadReferenceDataAsync();
		await LoadOrdersAsync( cancellationToken );
	}

	public void BeginNewOrder()
	{
		_shippingCostsOverridden = false;
		_orderCostsOverridden = false;

		EditableOrder = new StockOrderModel
		{
			Id = 0,
			OrderDate = DateTime.Today
		};

		_suppressSelectedOrderChange = true;
		SelectedOrder = null;
		_suppressSelectedOrderChange = false;
		_selectionLoadVersion++;
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
		_shippingCostsOverridden = true;
		_orderCostsOverridden = true;

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
		await SaveOrderCoreAsync( null );
	}

	public async Task SaveOrderAsync( CancellationToken cancellationToken )
	{
		await SaveOrderCoreAsync( cancellationToken );
	}

	private async Task SaveOrderCoreAsync( CancellationToken? cancellationToken )
	{
		if ( IsSavingOrder )
			return;

		var validationMessage = ValidateOrderForSave();
		if ( validationMessage != null )
		{
			MessageBox.Show( validationMessage, Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
			return;
		}

		IsSavingOrder = true;
		try
		{
			if ( IsNewOrder )
			{
				List<StockOrderLineModel> pendingLines = PendingOrderLines.ToList();
				var newOrderId = await InsertOrderWithLinesAsync( EditableOrder, pendingLines, cancellationToken );
				EditableOrder.Id = newOrderId;
				foreach ( var line in pendingLines )
				{
					ApplyLocalInventoryCorrection( line.ProductId, line.Amount, line.OpenAmount );
				}

				var lines = await GetOrderLinesAsync( newOrderId, cancellationToken );
				await LoadOrdersAsync( cancellationToken );

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

			await UpdateOrderAsync( EditableOrder, cancellationToken );
			await LoadOrdersAsync( cancellationToken );

			if ( SelectedOrder != null )
			{
				var lines = await GetOrderLinesAsync( SelectedOrder.Id, cancellationToken );
				var reloadedOrder = Orders.FirstOrDefault( o => o.Id == SelectedOrder.Id ) ?? SelectedOrder;
				ApplySelectedOrder( reloadedOrder, lines );
			}
		}
		finally
		{
			IsSavingOrder = false;
		}
	}

	private bool CanSaveOrder() => !IsSavingOrder;
	private bool CanEditOrderLineCommand() => CanEditOrder && !IsEditingOrderLine;

	private void NotifyOrderLineCommandsCanExecuteChanged()
	{
		AddProductToOrderCommand.NotifyCanExecuteChanged();
		EditOrderLineCommand.NotifyCanExecuteChanged();
		DeleteOrderLineCommand.NotifyCanExecuteChanged();
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

	private void ApplyProductSearchFilter()
	{
		AvailableProducts.Clear();
		IEnumerable<ProductModel> filtered = _allAvailableProducts;

		if ( !string.IsNullOrWhiteSpace( SearchText ) )
		{
			filtered = filtered.Where( ProductMatchesSearch );
		}

		foreach ( var product in filtered )
		{
			AvailableProducts.Add( product );
		}
	}

	private bool ProductMatchesSearch( ProductModel product )
	{
		return product.ProductCode?.Contains( SearchText!, StringComparison.CurrentCultureIgnoreCase ) == true
			|| product.ProductName?.Contains( SearchText!, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	private async Task LoadReferenceDataAsync()
	{
		var inventoryTask = _stockService.GetCompleteInventoryAsync();
		var productsTask = _productService.GetAllProductsAsync();
		var suppliersTask = _supplierService.GetAllSuppliersAsync();
		var currenciesTask = _supplierService.GetAllCurrenciesAsync();

		await PerformanceTrace.MeasureAsync(
			$"{nameof( StockOrderViewModel )}.{nameof( LoadReferenceDataAsync )}",
			() => Task.WhenAll( inventoryTask, productsTask, suppliersTask, currenciesTask ) );

		var inventory = await inventoryTask;
		_inventoryByProductId = inventory.ToDictionary( item => item.ProductId );

		var products = await productsTask;
		_allAvailableProducts = products;
		foreach ( var product in _allAvailableProducts )
		{
			ApplyInventorySnapshotToProduct( product );
		}
		ApplyProductSearchFilter();

		var suppliers = await suppliersTask;
		Suppliers.Clear();
		foreach ( var supplier in suppliers )
		{
			Suppliers.Add( supplier );
		}

		var currencies = await currenciesTask;
		Currencies.Clear();
		foreach ( var currency in currencies )
		{
			Currencies.Add( currency );
		}

		RefreshLookupsFromEditableOrder();
	}

	private async Task LoadOrdersAsync()
	{
		_allOrders = await PerformanceTrace.MeasureAsync(
			$"{nameof( StockOrderViewModel )}.{nameof( LoadOrdersAsync )}",
			() => _stockOrderService.GetAllOrdersAsync() );
		ApplyOrderFilters();
	}

	private async Task LoadOrdersAsync( CancellationToken? cancellationToken )
	{
		_allOrders = await PerformanceTrace.MeasureAsync(
			$"{nameof( StockOrderViewModel )}.{nameof( LoadOrdersAsync )}",
			() => cancellationToken.HasValue
				? _stockOrderService.GetAllOrdersAsync( cancellationToken.Value )
				: _stockOrderService.GetAllOrdersAsync() );
		ApplyOrderFilters();
	}

	private async Task LoadSelectedOrderAsync( StockOrderModel order, int loadVersion )
	{
		var lines = await _stockOrderService.GetOrderLinesAsync( order.Id );

		if ( loadVersion != _selectionLoadVersion || SelectedOrder?.Id != order.Id )
			return;

		ApplySelectedOrder( order, lines );
	}

	private Task<int> InsertOrderWithLinesAsync( StockOrderModel order, IEnumerable<StockOrderLineModel> lines, CancellationToken? cancellationToken )
	{
		return cancellationToken.HasValue
			? _stockOrderService.InsertOrderWithLinesAsync( order, lines, cancellationToken.Value )
			: _stockOrderService.InsertOrderWithLinesAsync( order, lines );
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

	private SupplierModel? GetEditableOrderSupplier()
	{
		return Suppliers.FirstOrDefault( s => s.Id == EditableOrder.SupplierId );
	}

	private void EditableOrder_PropertyChanged( object? sender, System.ComponentModel.PropertyChangedEventArgs e )
	{
		if ( _suppressEditableOrderTracking )
			return;

		switch ( e.PropertyName )
		{
			case nameof( StockOrderModel.SupplierId ):
				ApplySupplierDefaults();
				if ( EnableSupplierOrderFilter )
				{
					ApplyOrderFilters();
				}
				break;
			case nameof( StockOrderModel.CurrencyId ):
				RefreshLookupsFromEditableOrder();
				break;
			case nameof( StockOrderModel.ShippingCosts ):
				_shippingCostsOverridden = true;
				break;
			case nameof( StockOrderModel.OrderCosts ):
				_orderCostsOverridden = true;
				break;
		}
	}

	private ProductModel BuildProductForLine( StockOrderLineModel line )
	{
		ProductModel? existingProduct = _allAvailableProducts.FirstOrDefault( p => p.ProductId == line.ProductId );
		if ( existingProduct != null )
			return existingProduct;

		return new ProductModel
		{
			ProductId = line.ProductId,
			ProductCode = line.ProductCode,
			ProductName = line.ProductName,
			ProductPrice = line.Price
		};
	}

	private void RefreshLookupsFromEditableOrder()
	{
		SelectedSupplier = Suppliers.FirstOrDefault( s => s.Id == EditableOrder.SupplierId );
		SelectedCurrency = Currencies.FirstOrDefault( c => c.CurrencyId == EditableOrder.CurrencyId );
	}

	private void ApplySupplierDefaults()
	{
		RefreshLookupsFromEditableOrder();

		var supplier = GetEditableOrderSupplier();
		if ( supplier == null )
			return;

		_suppressEditableOrderTracking = true;
		try
		{
			EditableOrder.CurrencyId = supplier.CurrencyId;

			if ( !_shippingCostsOverridden )
			{
				EditableOrder.ShippingCosts = supplier.ShippingCosts;
			}

			if ( !_orderCostsOverridden )
			{
				EditableOrder.OrderCosts = supplier.OrderCosts;
			}
		}
		finally
		{
			_suppressEditableOrderTracking = false;
		}

		RefreshLookupsFromEditableOrder();
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
		NotifyOrderLineCommandsCanExecuteChanged();
	}

	public void ReplaceOrdersForTest( IEnumerable<StockOrderModel> orders )
	{
		_allOrders = orders.ToList();
		ApplyOrderFilters();
	}

	private async Task DeleteOrderAsync()
	{
		if ( !CanEditOrder )
			return;

		if ( IsNewOrder || EditableOrder.Id <= 0 )
		{
			BeginNewOrder();
			return;
		}

		List<StockOrderLineModel> linesToDelete = OrderLines.ToList();
		if ( linesToDelete.Count == 0 )
		{
			linesToDelete = await _stockOrderService.GetOrderLinesAsync( EditableOrder.Id );
		}

		foreach ( var line in linesToDelete )
		{
			ApplyLocalInventoryCorrection( line.ProductId, -line.Amount, -line.OpenAmount );
		}

		await _stockOrderService.DeleteOrderWithLinesAsync( EditableOrder.Id, linesToDelete );
		await LoadOrdersAsync();
		BeginNewOrder();
	}

	public async Task AddSelectedProductAsync()
	{
		if ( IsEditingOrderLine )
			return;

		if ( !CanEditOrder || SelectedProduct == null )
			return;

		IsEditingOrderLine = true;
		try
		{
			var supplier = GetEditableOrderSupplier();
			if ( supplier == null )
			{
				MessageBox.Show( "Selecteer eerst een leverancier.", Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
				return;
			}

			var existingProductSupplier = await _supplierService.GetProductSupplierAsync( supplier.Id, SelectedProduct.ProductId );
			var dialogModel = StockOrderProductDialogModel.Create( SelectedProduct, supplier, existingProductSupplier );
			dialogModel.Amount = GetDefaultOrderAmount( SelectedProduct.ProductId );
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
				line.Id = await _stockOrderService.InsertOrderLineWithStockCorrectionAsync( line, line.Amount );
				OrderLines.Add( line );
				ApplyLocalInventoryCorrection( line.ProductId, line.Amount, line.OpenAmount );
			}

			RecalculateTotals();
			HasUnsavedChanges = true;
			OnPropertyChanged( nameof( VisibleOrderLines ) );
		}
		finally
		{
			IsEditingOrderLine = false;
		}
	}

	private async Task EditSelectedOrderLineAsync()
	{
		if ( IsEditingOrderLine )
			return;

		if ( !CanEditOrder || SelectedOrderLine == null )
			return;

		IsEditingOrderLine = true;
		try
		{
			var supplier = GetEditableOrderSupplier();
			if ( supplier == null )
			{
				MessageBox.Show( "Selecteer eerst een leverancier.", Lang.generalMessageboxWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning );
				return;
			}

			var line = SelectedOrderLine;
			double originalAmount = line.Amount;
			double originalOpenAmount = line.OpenAmount;
			ProductModel product = BuildProductForLine( line );
			var existingProductSupplier = await _supplierService.GetProductSupplierAsync( supplier.Id, line.ProductId );
			var dialogModel = StockOrderProductDialogModel.Create( product, supplier, existingProductSupplier );

			dialogModel.SupplierProductName = line.SupplierProductName;
			dialogModel.Amount = line.Amount;
			dialogModel.UnitPrice = line.Price;

			var dialogVm = new StockOrderProductDialogViewModel( dialogModel );
			var confirmed = ShowProductDialog?.Invoke( dialogVm ) ?? ShowProductDialogWindow( dialogVm );
			if ( !confirmed )
				return;

			await UpsertProductSupplierAsync( dialogVm.Model );

			line.SupplierProductName = dialogVm.Model.SupplierProductName;
			line.Amount = dialogVm.Model.Amount;
			line.Price = dialogVm.Model.UnitPrice;
			line.RealRowTotal = dialogVm.Model.RowTotal;

			if ( IsNewOrder || line.Id <= 0 )
			{
				line.OpenAmount = dialogVm.Model.Amount;
			}
			else
			{
				line.OpenAmount = Math.Max( dialogVm.Model.Amount - line.Received, 0d );
				double stockCorrection = dialogVm.Model.Amount - originalAmount;
				await _stockOrderService.UpdateOrderLineWithStockCorrectionAsync( line, stockCorrection );
				ApplyLocalInventoryCorrection( line.ProductId, stockCorrection, line.OpenAmount - originalOpenAmount );
			}

			RecalculateTotals();
			HasUnsavedChanges = true;
			OnPropertyChanged( nameof( VisibleOrderLines ) );
		}
		finally
		{
			IsEditingOrderLine = false;
		}
	}

	private async Task DeleteSelectedOrderLineAsync()
	{
		if ( IsEditingOrderLine )
			return;

		if ( !CanEditOrder || SelectedOrderLine == null )
			return;

		IsEditingOrderLine = true;
		try
		{
			var line = SelectedOrderLine;

			if ( line.Id > 0 )
			{
				await _stockOrderService.DeleteOrderLineWithStockCorrectionAsync( line, -line.Amount );
				ApplyLocalInventoryCorrection( line.ProductId, -line.Amount, -line.OpenAmount );
			}

			if ( PendingOrderLines.Contains( line ) )
			{
				PendingOrderLines.Remove( line );
			}
			else
			{
				OrderLines.Remove( line );
			}

			SelectedOrderLine = null;
			RecalculateTotals();
			HasUnsavedChanges = true;
			OnPropertyChanged( nameof( VisibleOrderLines ) );
		}
		finally
		{
			IsEditingOrderLine = false;
		}
	}

	private void ApplyInventorySnapshotToProduct( ProductModel product )
	{
		if ( _inventoryByProductId.TryGetValue( product.ProductId, out StockManagementModel? stockInfo ) )
		{
			product.CurrentInventory = stockInfo.ProductInventory;
			product.InOrder = stockInfo.ProductInOrder;
			if ( product.ProductMinimalStock <= 0 )
			{
				product.ProductMinimalStock = stockInfo.ProductMinimalStock;
			}
		}
	}

	private void ApplyLocalInventoryCorrection( int productId, double inventoryCorrection, double inOrderCorrection )
	{
		if ( productId <= 0 || ( inventoryCorrection == 0d && inOrderCorrection == 0d ) )
			return;

		if ( _inventoryByProductId.TryGetValue( productId, out StockManagementModel? inventory ) )
		{
			inventory.ProductInventory += inventoryCorrection;
			inventory.ProductInOrder += inOrderCorrection;
		}

		ProductModel? product = _allAvailableProducts.FirstOrDefault( p => p.ProductId == productId );
		if ( product != null )
		{
			product.CurrentInventory += inventoryCorrection;
			product.InOrder += inOrderCorrection;
		}
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

	private double GetDefaultOrderAmount( int productId )
	{
		if ( _inventoryByProductId.TryGetValue( productId, out StockManagementModel? inventory ) )
		{
			double shortageToMinimum = Math.Max( inventory.ProductMinimalStock - inventory.ProductInventory, 0d );
			return shortageToMinimum > 0 ? shortageToMinimum : 1d;
		}

		return 1d;
	}

	private static bool ShowProductDialogWindow( StockOrderProductDialogViewModel viewModel )
	{
		StockOrderProductDialog dialog = new( viewModel );
		return dialog.ShowDialog() == true;
	}

	private void ApplyOrderFilters()
	{
		Orders.Clear();

		IEnumerable<StockOrderModel> filtered = _allOrders;

		if ( !ShowClosedOrders )
			filtered = filtered.Where( o => !o.Closed );

		if ( EnableSupplierOrderFilter && EditableOrder.SupplierId > 0 )
			filtered = filtered.Where( o => o.SupplierId == EditableOrder.SupplierId );

		foreach ( var order in filtered )
		{
			Orders.Add( order );
		}
	}
}
