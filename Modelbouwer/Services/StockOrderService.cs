using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySqlTransaction = MySql.Data.MySqlClient.MySqlTransaction;

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
	{DBNames.OrderViewFieldNameHasStockLog}
FROM {DBNames.Database}.{DBNames.OrderView}
ORDER BY {DBNames.OrderViewFieldNameOrderDate} DESC, {DBNames.OrderViewFieldNameId} DESC;";

	public string OrderLinesQuery = $@"
SELECT
	ol.{DBNames.OrderLineFieldNameId} AS {DBNames.OrderLineFieldNameId},
	ol.{DBNames.OrderLineFieldNameSupplierOrderId} AS {DBNames.OrderLineViewFieldNameOrderId},
	o.{DBNames.OrderFieldNameSupplierId} AS {DBNames.OrderLineViewFieldNameSupplierId},
	p.{DBNames.ProductFieldNameId} AS {DBNames.OrderLineViewFieldNameProductId},
	p.{DBNames.ProductFieldNameCode} AS {DBNames.OrderLineViewFieldNameProductCode},
	p.{DBNames.ProductFieldNameName} AS {DBNames.OrderLineViewFieldNameProductName},
	ps.{DBNames.ProductSupplierFieldNameProductNumber} AS {DBNames.ProductSupplierFieldNameProductNumber},
	ol.{DBNames.OrderLineFieldNameSupplierProductName} AS {DBNames.OrderLineFieldNameSupplierProductName},
	ol.{DBNames.OrderLineFieldNameAmount} AS {DBNames.OrderLineFieldNameAmount},
	ol.{DBNames.OrderLineFieldNameOpenAmount} AS {DBNames.OrderLineFieldNameOpenAmount},
	ol.{DBNames.OrderLineFieldNamePrice} AS {DBNames.OrderLineFieldNamePrice},
	ol.{DBNames.OrderLineFieldNameRealRowTotal} AS {DBNames.OrderLineFieldNameRealRowTotal},
	(ol.{DBNames.OrderLineFieldNameAmount} - ol.{DBNames.OrderLineFieldNameOpenAmount}) AS {DBNames.OrderLineViewFieldNameReceived},
	ol.{DBNames.OrderLineFieldNameOpenAmount} AS {DBNames.OrderLineViewFieldNameExpected},
	ol.{DBNames.OrderLineFieldNameClosed} AS {DBNames.OrderLineViewFieldNameClosed},
	ol.{DBNames.OrderLineFieldNameClosedDate} AS {DBNames.OrderLineViewFieldNameClosedDate}
FROM {DBNames.Database}.{DBNames.OrderLineTable} ol
INNER JOIN {DBNames.Database}.{DBNames.OrderTable} o ON ol.{DBNames.OrderLineFieldNameSupplierOrderId} = o.{DBNames.OrderFieldNameId}
INNER JOIN {DBNames.Database}.{DBNames.ProductTable} p ON ol.{DBNames.OrderLineFieldNameProductId} = p.{DBNames.ProductFieldNameId}
LEFT JOIN {DBNames.Database}.{DBNames.ProductSupplierTable} ps ON ps.{DBNames.ProductSupplierFieldNameSupplierId} = o.{DBNames.OrderFieldNameSupplierId}
	AND ps.{DBNames.ProductSupplierFieldNameProductId} = p.{DBNames.ProductFieldNameId}
WHERE ol.{DBNames.OrderLineFieldNameSupplierOrderId} = @OrderId
ORDER BY ol.{DBNames.OrderLineFieldNameId};";

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

	public string InsertStocklogCorrectionQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.StocklogTable} (
	{DBNames.StocklogFieldNameProductId},
	{DBNames.StocklogFieldNameSupplyOrderId},
	{DBNames.StocklogFieldNameSupplyOrderlineId},
	{DBNames.StocklogFieldNameAmountCorrection},
	{DBNames.StocklogFieldNameLogDate}
) VALUES (
	@{DBNames.StocklogFieldNameProductId},
	@{DBNames.StocklogFieldNameSupplyOrderId},
	@{DBNames.StocklogFieldNameSupplyOrderlineId},
	@{DBNames.StocklogFieldNameAmountCorrection},
	{DBNames.SqlCurrentDate}
);";

	public string InsertStocklogReceiptQuery = $@"
INSERT INTO {DBNames.Database}.{DBNames.StocklogTable} (
	{DBNames.StocklogFieldNameProductId},
	{DBNames.StocklogFieldNameSupplyOrderId},
	{DBNames.StocklogFieldNameSupplyOrderlineId},
	{DBNames.StocklogFieldNameAmountReceived},
	{DBNames.StocklogFieldNameLogDate}
) VALUES (
	@{DBNames.StocklogFieldNameProductId},
	@{DBNames.StocklogFieldNameSupplyOrderId},
	@{DBNames.StocklogFieldNameSupplyOrderlineId},
	@{DBNames.StocklogFieldNameAmountReceived},
	@{DBNames.StocklogFieldNameLogDate}
);";

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
				HasStockLog = DatabaseValueConverter.GetSByte( reader[DBNames.OrderViewFieldNameHasStockLog] ) == 1
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
				SupplierId = DatabaseValueConverter.GetInt( reader[DBNames.OrderLineViewFieldNameSupplierId] ),
				ProductId = DatabaseValueConverter.GetInt( reader[DBNames.OrderLineViewFieldNameProductId] ),
				ProductCode = DatabaseValueConverter.GetString( reader[DBNames.OrderLineViewFieldNameProductCode] ),
				ProductName = DatabaseValueConverter.GetString( reader[DBNames.OrderLineViewFieldNameProductName] ),
				SupplierProductNumber = DatabaseValueConverter.GetString( reader[DBNames.ProductSupplierFieldNameProductNumber] ),
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

	public Task<int> InsertOrderWithLinesAsync( StockOrderModel order, IEnumerable<StockOrderLineModel> lines )
	{
		List<StockOrderLineModel> lineList = lines.ToList();

		return _dataService.ExecuteInTransactionAsync<int>( async ( connection, transaction ) =>
		{
			uint orderId = await ExecuteScalarInTransactionAsync<uint>( connection, transaction, InsertOrderQuery, BuildOrderParameters( order, includeId: false ) );

			foreach ( var line in lineList )
			{
				line.SupplyOrderId = ( int ) orderId;
				uint lineId = await ExecuteScalarInTransactionAsync<uint>( connection, transaction, InsertOrderLineQuery, BuildOrderLineParameters( line, includeId: false ) );
				line.Id = ( int ) lineId;
				await InsertStocklogCorrectionAsync( connection, transaction, line.ProductId, ( int ) orderId, ( int ) lineId, line.Amount );
			}

			return ( int ) orderId;
		} );
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

	public Task DeleteOrderWithLinesAsync( int orderId, IEnumerable<StockOrderLineModel> lines )
	{
		List<StockOrderLineModel> lineList = lines.ToList();

		return _dataService.ExecuteInTransactionAsync( async ( connection, transaction ) =>
		{
			foreach ( var line in lineList )
			{
				if ( line.Id > 0 )
				{
					await ExecuteNonQueryInTransactionAsync( connection, transaction, DeleteOrderLineQuery, new Dictionary<string, object>
					{
						{ $"@{DBNames.OrderLineFieldNameId}", line.Id }
					} );
				}

				await InsertStocklogCorrectionAsync( connection, transaction, line.ProductId, orderId, line.Id, -line.Amount );
			}

			await ExecuteNonQueryInTransactionAsync( connection, transaction, DeleteOrderQuery, new Dictionary<string, object>
			{
				{ $"@{DBNames.OrderFieldNameId}", orderId }
			} );
		} );
	}

	public async Task<int> InsertOrderLineAsync( StockOrderLineModel line )
	{
		uint newId = await _dataService.ExecuteScalarAsync<uint>( InsertOrderLineQuery, BuildOrderLineParameters( line, includeId: false ) );
		return ( int ) newId;
	}

	public Task<int> InsertOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection )
	{
		return _dataService.ExecuteInTransactionAsync<int>( async ( connection, transaction ) =>
		{
			uint lineId = await ExecuteScalarInTransactionAsync<uint>( connection, transaction, InsertOrderLineQuery, BuildOrderLineParameters( line, includeId: false ) );
			line.Id = ( int ) lineId;
			await InsertStocklogCorrectionAsync( connection, transaction, line.ProductId, line.SupplyOrderId, ( int ) lineId, stockCorrection );
			return ( int ) lineId;
		} );
	}

	public Task UpdateOrderLineAsync( StockOrderLineModel line )
	{
		return _dataService.ExecuteScalarAsync<uint>( UpdateOrderLineQuery, BuildOrderLineParameters( line, includeId: true ) );
	}

	public Task UpdateOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection )
	{
		return _dataService.ExecuteInTransactionAsync( async ( connection, transaction ) =>
		{
			await ExecuteNonQueryInTransactionAsync( connection, transaction, UpdateOrderLineQuery, BuildOrderLineParameters( line, includeId: true ) );
			await InsertStocklogCorrectionAsync( connection, transaction, line.ProductId, line.SupplyOrderId, line.Id, stockCorrection );
		} );
	}

	public Task RegisterReceiptAsync( StockOrderLineModel line, double receivedAmount, DateTime? deliveryDate )
	{
		return _dataService.ExecuteInTransactionAsync( async ( connection, transaction ) =>
		{
			await ExecuteNonQueryInTransactionAsync( connection, transaction, UpdateOrderLineQuery, BuildOrderLineParameters( line, includeId: true ) );
			await InsertStocklogReceiptAsync( connection, transaction, line.ProductId, line.SupplyOrderId, line.Id, receivedAmount, deliveryDate );
		} );
	}

	public Task DeleteOrderLineAsync( int lineId )
	{
		return _dataService.ExecuteScalarAsync<uint>( DeleteOrderLineQuery, new Dictionary<string, object>
		{
			{ $"@{DBNames.OrderLineFieldNameId}", lineId }
		} );
	}

	public Task DeleteOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection )
	{
		return _dataService.ExecuteInTransactionAsync( async ( connection, transaction ) =>
		{
			await ExecuteNonQueryInTransactionAsync( connection, transaction, DeleteOrderLineQuery, new Dictionary<string, object>
			{
				{ $"@{DBNames.OrderLineFieldNameId}", line.Id }
			} );
			await InsertStocklogCorrectionAsync( connection, transaction, line.ProductId, line.SupplyOrderId, line.Id, stockCorrection );
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
			{ $"@{DBNames.OrderLineFieldNameProductId}", line.ProductId },
			{ $"@{DBNames.OrderLineFieldNameSupplierProductName}", line.SupplierProductName ?? string.Empty },
			{ $"@{DBNames.OrderLineFieldNameAmount}", line.Amount },
			{ $"@{DBNames.OrderLineFieldNameOpenAmount}", line.OpenAmount > 0 || line.Closed ? line.OpenAmount : line.Amount },
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

	private static async Task<T> ExecuteScalarInTransactionAsync<T>( MySqlConnection connection, MySqlTransaction transaction, string query, Dictionary<string, object> parameters )
	{
		await using MySqlCommand command = new(query, connection, transaction);

		foreach ( var parameter in parameters )
		{
			command.Parameters.AddWithValue( parameter.Key, parameter.Value ?? DBNull.Value );
		}

		object? result = await command.ExecuteScalarAsync();

		if ( result == null || result == DBNull.Value )
			return default!;

		Type targetType = Nullable.GetUnderlyingType( typeof( T ) ) ?? typeof( T );
		object converted = Convert.ChangeType( result, targetType );
		return ( T ) converted;
	}

	private static async Task ExecuteNonQueryInTransactionAsync( MySqlConnection connection, MySqlTransaction transaction, string query, Dictionary<string, object> parameters )
	{
		await using MySqlCommand command = new(query, connection, transaction);

		foreach ( var parameter in parameters )
		{
			command.Parameters.AddWithValue( parameter.Key, parameter.Value ?? DBNull.Value );
		}

		await command.ExecuteNonQueryAsync();
	}

	private Task InsertStocklogCorrectionAsync( MySqlConnection connection, MySqlTransaction transaction, int productId, int supplyOrderId, int supplyOrderLineId, double correctionAmount )
	{
		if ( productId <= 0 || correctionAmount == 0d )
			return Task.CompletedTask;

		return ExecuteNonQueryInTransactionAsync( connection, transaction, InsertStocklogCorrectionQuery, new Dictionary<string, object>
		{
			{ $"@{DBNames.StocklogFieldNameProductId}", productId },
			{ $"@{DBNames.StocklogFieldNameSupplyOrderId}", supplyOrderId > 0 ? supplyOrderId : DBNull.Value },
			{ $"@{DBNames.StocklogFieldNameSupplyOrderlineId}", supplyOrderLineId > 0 ? supplyOrderLineId : DBNull.Value },
			{ $"@{DBNames.StocklogFieldNameAmountCorrection}", correctionAmount }
		} );
	}

	private Task InsertStocklogReceiptAsync( MySqlConnection connection, MySqlTransaction transaction, int productId, int supplyOrderId, int supplyOrderLineId, double receivedAmount, DateTime? deliveryDate )
	{
		if ( productId <= 0 || receivedAmount == 0d )
			return Task.CompletedTask;

		return ExecuteNonQueryInTransactionAsync( connection, transaction, InsertStocklogReceiptQuery, new Dictionary<string, object>
		{
			{ $"@{DBNames.StocklogFieldNameProductId}", productId },
			{ $"@{DBNames.StocklogFieldNameSupplyOrderId}", supplyOrderId > 0 ? supplyOrderId : DBNull.Value },
			{ $"@{DBNames.StocklogFieldNameSupplyOrderlineId}", supplyOrderLineId > 0 ? supplyOrderLineId : DBNull.Value },
			{ $"@{DBNames.StocklogFieldNameAmountReceived}", receivedAmount },
			{ $"@{DBNames.StocklogFieldNameLogDate}", deliveryDate is DateTime date ? date : DateTime.Today }
		} );
	}
}
