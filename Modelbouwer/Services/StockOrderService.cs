namespace Modelbouwer.Services;

public class StockOrderService : IStockOrderService
{
	private readonly GenericDataService _dataService;

	public StockOrderService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	public string CompleteOrderListQuery = $@"
SELECT
	{DBNames.OrderViewFieldNameId},
	{DBNames.OrderViewFieldNameSupplierId},
	{DBNames.OrderViewFieldNameSupplierName},
	{DBNames.OrderViewFieldNameCurrencyId},
	{DBNames.OrderViewFieldNameCurrencySymbol},
	{DBNames.OrderViewFieldNameOrderNumber},
	{DBNames.OrderViewFieldNameOrderDate},
	{DBNames.OrderViewFieldNameOrderShippingCosts},
	{DBNames.OrderViewFieldNameOrderOrderCosts},
	{DBNames.OrderViewFieldNameClosed},
	{DBNames.OrderViewFieldNameClosedDate},
	{DBNames.OrderViewFieldNameOrderMemo},
	{DBNames.OrderViewFieldNameHasStackLog}
FROM {DBNames.Database}.{DBNames.OrderView}
ORDER BY {DBNames.OrderViewFieldNameOrderDate} DESC, {DBNames.OrderViewFieldNameId} DESC;";

	public string OrderLinesQuery = $@"
SELECT
	{DBNames.OrderLineFieldNameId},
	{DBNames.OrderLineViewFieldNameOrderId},
	{DBNames.OrderLineFieldNameSupplierId},
	{DBNames.OrderLineViewFieldNameProductId},
	{DBNames.OrderLineViewFieldNameProductCode},
	{DBNames.OrderLineViewFieldNameProductName},
	{DBNames.OrderLineFieldNameSupplierProductName},
	{DBNames.OrderLineFieldNameAmount},
	{DBNames.OrderLineFieldNameOpenAmount},
	{DBNames.OrderLineFieldNamePrice},
	{DBNames.OrderLineFieldNameRealRowTotal},
	{DBNames.OrderLineViewFieldNameReceived},
	{DBNames.OrderLineViewFieldNameExpected},
	{DBNames.OrderLineViewFieldNameClosed},
	{DBNames.OrderLineViewFieldNameClosedDate}
FROM {DBNames.Database}.{DBNames.OrderLineView}
WHERE {DBNames.OrderLineViewFieldNameOrderId} = @OrderId
ORDER BY {DBNames.OrderLineFieldNameId};";

	public string InsertOrderQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.OrderTable} (
	{DBNames.OrderFieldNameSupplierId},
	{DBNames.OrderFieldNameCurrencyId},
	{DBNames.OrderFieldNameOrderNumber},
	{DBNames.OrderFieldNameOrderDate},
	{DBNames.OrderFieldNameShippingCosts},
	{DBNames.OrderFieldNameOrderCosts},
	{DBNames.OrderFieldNameOrderMemo},
	{DBNames.OrderFieldNameClosed},
	{DBNames.OrderFieldNameClosedDate}
) VALUES (
	@{DBNames.OrderFieldNameSupplierId},
	@{DBNames.OrderFieldNameCurrencyId},
	@{DBNames.OrderFieldNameOrderNumber},
	@{DBNames.OrderFieldNameOrderDate},
	@{DBNames.OrderFieldNameShippingCosts},
	@{DBNames.OrderFieldNameOrderCosts},
	@{DBNames.OrderFieldNameOrderMemo},
	@{DBNames.OrderFieldNameClosed},
	@{DBNames.OrderFieldNameClosedDate}
);
{DBNames.SqlSelectLastId}";

	public string UpdateOrderQuery = $@"
UPDATE {DBNames.Database}.{DBNames.OrderTable}
SET
	{DBNames.OrderFieldNameSupplierId} = @{DBNames.OrderFieldNameSupplierId},
	{DBNames.OrderFieldNameCurrencyId} = @{DBNames.OrderFieldNameCurrencyId},
	{DBNames.OrderFieldNameOrderNumber} = @{DBNames.OrderFieldNameOrderNumber},
	{DBNames.OrderFieldNameOrderDate} = @{DBNames.OrderFieldNameOrderDate},
	{DBNames.OrderFieldNameShippingCosts} = @{DBNames.OrderFieldNameShippingCosts},
	{DBNames.OrderFieldNameOrderCosts} = @{DBNames.OrderFieldNameOrderCosts},
	{DBNames.OrderFieldNameOrderMemo} = @{DBNames.OrderFieldNameOrderMemo},
	{DBNames.OrderFieldNameClosed} = @{DBNames.OrderFieldNameClosed},
	{DBNames.OrderFieldNameClosedDate} = @{DBNames.OrderFieldNameClosedDate}
WHERE {DBNames.OrderFieldNameId} = @{DBNames.OrderFieldNameId};";

	public string DeleteOrderQuery = $@"
DELETE FROM {DBNames.Database}.{DBNames.OrderTable}
WHERE {DBNames.OrderFieldNameId} = @{DBNames.OrderFieldNameId};";

	public string InsertOrderLineQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.OrderLineTable} (
	{DBNames.OrderLineFieldNameSupplierOrderId},
	{DBNames.OrderLineFieldNameSupplierId},
	{DBNames.OrderLineFieldNameProductId},
	{DBNames.OrderLineFieldNameSupplierProductName},
	{DBNames.OrderLineFieldNameAmount},
	{DBNames.OrderLineFieldNameOpenAmount},
	{DBNames.OrderLineFieldNamePrice},
	{DBNames.OrderLineFieldNameRealRowTotal},
	{DBNames.OrderLineFieldNameClosed},
	{DBNames.OrderLineFieldNameClosedDate}
) VALUES (
	@{DBNames.OrderLineFieldNameSupplierOrderId},
	@{DBNames.OrderLineFieldNameSupplierId},
	@{DBNames.OrderLineFieldNameProductId},
	@{DBNames.OrderLineFieldNameSupplierProductName},
	@{DBNames.OrderLineFieldNameAmount},
	@{DBNames.OrderLineFieldNameOpenAmount},
	@{DBNames.OrderLineFieldNamePrice},
	@{DBNames.OrderLineFieldNameRealRowTotal},
	@{DBNames.OrderLineFieldNameClosed},
	@{DBNames.OrderLineFieldNameClosedDate}
);
{DBNames.SqlSelectLastId}";

	public string UpdateOrderLineQuery = $@"
UPDATE {DBNames.Database}.{DBNames.OrderLineTable}
SET
	{DBNames.OrderLineFieldNameSupplierOrderId} = @{DBNames.OrderLineFieldNameSupplierOrderId},
	{DBNames.OrderLineFieldNameSupplierId} = @{DBNames.OrderLineFieldNameSupplierId},
	{DBNames.OrderLineFieldNameProductId} = @{DBNames.OrderLineFieldNameProductId},
	{DBNames.OrderLineFieldNameSupplierProductName} = @{DBNames.OrderLineFieldNameSupplierProductName},
	{DBNames.OrderLineFieldNameAmount} = @{DBNames.OrderLineFieldNameAmount},
	{DBNames.OrderLineFieldNameOpenAmount} = @{DBNames.OrderLineFieldNameOpenAmount},
	{DBNames.OrderLineFieldNamePrice} = @{DBNames.OrderLineFieldNamePrice},
	{DBNames.OrderLineFieldNameRealRowTotal} = @{DBNames.OrderLineFieldNameRealRowTotal},
	{DBNames.OrderLineFieldNameClosed} = @{DBNames.OrderLineFieldNameClosed},
	{DBNames.OrderLineFieldNameClosedDate} = @{DBNames.OrderLineFieldNameClosedDate}
WHERE {DBNames.OrderLineFieldNameId} = @{DBNames.OrderLineFieldNameId};";

	public string DeleteOrderLineQuery = $@"
DELETE FROM {DBNames.Database}.{DBNames.OrderLineTable}
WHERE {DBNames.OrderLineFieldNameId} = @{DBNames.OrderLineFieldNameId};";

	public Task<List<StockOrderModel>> GetAllOrdersAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteOrderListQuery, reader =>
		{
			return new StockOrderModel
			{
				Id = DatabaseValueConverter.GetInt( reader[DBNames.OrderViewFieldNameId] ),
				SupplierId = DatabaseValueConverter.GetInt( reader[DBNames.OrderViewFieldNameSupplierId] ),
				SupplierName = DatabaseValueConverter.GetString( reader[DBNames.OrderViewFieldNameSupplierName] ),
				CurrencyId = DatabaseValueConverter.GetInt( reader[DBNames.OrderViewFieldNameCurrencyId] ),
				CurrencySymbol = DatabaseValueConverter.GetString( reader[DBNames.OrderViewFieldNameCurrencySymbol] ),
				OrderNumber = DatabaseValueConverter.GetString( reader[DBNames.OrderViewFieldNameOrderNumber] ),
				OrderDate = GetNullableDateTime( reader[DBNames.OrderViewFieldNameOrderDate] ),
				ShippingCosts = DatabaseValueConverter.GetDouble( reader[DBNames.OrderViewFieldNameOrderShippingCosts] ),
				OrderCosts = DatabaseValueConverter.GetDouble( reader[DBNames.OrderViewFieldNameOrderOrderCosts] ),
				Closed = DatabaseValueConverter.GetSByte( reader[DBNames.OrderViewFieldNameClosed] ) == 1,
				ClosedDate = GetNullableDateTime( reader[DBNames.OrderViewFieldNameClosedDate] ),
				Memo = DatabaseValueConverter.GetString( reader[DBNames.OrderViewFieldNameOrderMemo] ),
				HasStockLog = DatabaseValueConverter.GetSByte( reader[DBNames.OrderViewFieldNameHasStackLog] ) == 1
			};
		}, null );
	}

	public Task<List<StockOrderLineModel>> GetOrderLinesAsync( int orderId )
	{
		return _dataService.ExecuteQueryAsync(
			OrderLinesQuery,
			reader => new StockOrderLineModel
			{
				Id = DatabaseValueConverter.GetInt( reader[DBNames.OrderLineFieldNameId] ),
				SupplyOrderId = DatabaseValueConverter.GetInt( reader[DBNames.OrderLineViewFieldNameOrderId] ),
				SupplierId = DatabaseValueConverter.GetInt( reader[DBNames.OrderLineFieldNameSupplierId] ),
				ProductId = DatabaseValueConverter.GetInt( reader[DBNames.OrderLineViewFieldNameProductId] ),
				ProductCode = DatabaseValueConverter.GetString( reader[DBNames.OrderLineViewFieldNameProductCode] ),
				ProductName = DatabaseValueConverter.GetString( reader[DBNames.OrderLineViewFieldNameProductName] ),
				SupplierProductName = DatabaseValueConverter.GetString( reader[DBNames.OrderLineFieldNameSupplierProductName] ),
				Amount = DatabaseValueConverter.GetDouble( reader[DBNames.OrderLineFieldNameAmount] ),
				OpenAmount = DatabaseValueConverter.GetDouble( reader[DBNames.OrderLineFieldNameOpenAmount] ),
				Price = DatabaseValueConverter.GetDouble( reader[DBNames.OrderLineFieldNamePrice] ),
				RealRowTotal = DatabaseValueConverter.GetDouble( reader[DBNames.OrderLineFieldNameRealRowTotal] ),
				Received = DatabaseValueConverter.GetDouble( reader[DBNames.OrderLineViewFieldNameReceived] ),
				Expected = DatabaseValueConverter.GetDouble( reader[DBNames.OrderLineViewFieldNameExpected] ),
				Closed = DatabaseValueConverter.GetSByte( reader[DBNames.OrderLineViewFieldNameClosed] ) == 1,
				ClosedDate = GetNullableDateTime( reader[DBNames.OrderLineViewFieldNameClosedDate] )
			},
			new Dictionary<string, object> { { "@OrderId", orderId } } );
	}

	public async Task<int> InsertOrderAsync( StockOrderModel order )
	{
		uint newId = await _dataService.ExecuteScalarAsync<uint>( InsertOrderQuery, BuildOrderParameters( order, includeId: false ) );
		return ( int ) newId;
	}

	public Task UpdateOrderAsync( StockOrderModel order )
	{
		return _dataService.ExecuteScalarAsync<uint>( UpdateOrderQuery, BuildOrderParameters( order, includeId: true ) );
	}

	public Task DeleteOrderAsync( int orderId )
	{
		return _dataService.ExecuteScalarAsync<uint>( DeleteOrderQuery, new Dictionary<string, object>
		{
			{ $"@{DBNames.OrderFieldNameId}", orderId }
		} );
	}

	public async Task<int> InsertOrderLineAsync( StockOrderLineModel line )
	{
		uint newId = await _dataService.ExecuteScalarAsync<uint>( InsertOrderLineQuery, BuildOrderLineParameters( line, includeId: false ) );
		return ( int ) newId;
	}

	public Task UpdateOrderLineAsync( StockOrderLineModel line )
	{
		return _dataService.ExecuteScalarAsync<uint>( UpdateOrderLineQuery, BuildOrderLineParameters( line, includeId: true ) );
	}

	public Task DeleteOrderLineAsync( int lineId )
	{
		return _dataService.ExecuteScalarAsync<uint>( DeleteOrderLineQuery, new Dictionary<string, object>
		{
			{ $"@{DBNames.OrderLineFieldNameId}", lineId }
		} );
	}

	private Dictionary<string, object> BuildOrderParameters( StockOrderModel order, bool includeId )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.OrderFieldNameSupplierId}", order.SupplierId },
			{ $"@{DBNames.OrderFieldNameCurrencyId}", order.CurrencyId },
			{ $"@{DBNames.OrderFieldNameOrderNumber}", order.OrderNumber ?? string.Empty },
			{ $"@{DBNames.OrderFieldNameOrderDate}", order.OrderDate is DateTime orderDate ? orderDate : DBNull.Value },
			{ $"@{DBNames.OrderFieldNameShippingCosts}", order.ShippingCosts },
			{ $"@{DBNames.OrderFieldNameOrderCosts}", order.OrderCosts },
			{ $"@{DBNames.OrderFieldNameOrderMemo}", order.Memo ?? string.Empty },
			{ $"@{DBNames.OrderFieldNameClosed}", order.Closed },
			{ $"@{DBNames.OrderFieldNameClosedDate}", order.ClosedDate is DateTime closedDate ? closedDate : DBNull.Value }
		};

		if ( includeId )
		{
			parameters.Add( $"@{DBNames.OrderFieldNameId}", order.Id );
		}

		return parameters;
	}

	private Dictionary<string, object> BuildOrderLineParameters( StockOrderLineModel line, bool includeId )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.OrderLineFieldNameSupplierOrderId}", line.SupplyOrderId },
			{ $"@{DBNames.OrderLineFieldNameSupplierId}", line.SupplierId },
			{ $"@{DBNames.OrderLineFieldNameProductId}", line.ProductId },
			{ $"@{DBNames.OrderLineFieldNameSupplierProductName}", line.SupplierProductName ?? string.Empty },
			{ $"@{DBNames.OrderLineFieldNameAmount}", line.Amount },
			{ $"@{DBNames.OrderLineFieldNameOpenAmount}", line.OpenAmount > 0 ? line.OpenAmount : line.Amount },
			{ $"@{DBNames.OrderLineFieldNamePrice}", line.Price },
			{ $"@{DBNames.OrderLineFieldNameRealRowTotal}", line.RealRowTotal },
			{ $"@{DBNames.OrderLineFieldNameClosed}", line.Closed },
			{ $"@{DBNames.OrderLineFieldNameClosedDate}", line.ClosedDate is DateTime closedDate ? closedDate : DBNull.Value }
		};

		if ( includeId )
		{
			parameters.Add( $"@{DBNames.OrderLineFieldNameId}", line.Id );
		}

		return parameters;
	}

	private static DateTime? GetNullableDateTime( object value )
	{
		return value == null || value == DBNull.Value
			? null
			: Convert.ToDateTime( value );
	}
}
