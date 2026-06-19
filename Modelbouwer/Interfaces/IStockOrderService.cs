namespace Modelbouwer.Interfaces;

public interface IStockOrderService
{
	Task<List<StockOrderModel>> GetAllOrdersAsync();
	Task<List<StockOrderModel>> GetAllOrdersAsync( CancellationToken cancellationToken );
	Task<List<StockOrderLineModel>> GetOrderLinesAsync( int orderId );
	Task<List<StockOrderLineModel>> GetOrderLinesAsync( int orderId, CancellationToken cancellationToken );
	Task<int> InsertOrderAsync( StockOrderModel order );
	Task<int> InsertOrderAsync( StockOrderModel order, CancellationToken cancellationToken );
	Task<int> InsertOrderWithLinesAsync( StockOrderModel order, IEnumerable<StockOrderLineModel> lines );
	Task<int> InsertOrderWithLinesAsync( StockOrderModel order, IEnumerable<StockOrderLineModel> lines, CancellationToken cancellationToken );
	Task UpdateOrderAsync( StockOrderModel order );
	Task UpdateOrderAsync( StockOrderModel order, CancellationToken cancellationToken );
	Task DeleteOrderAsync( int orderId );
	Task DeleteOrderAsync( int orderId, CancellationToken cancellationToken );
	Task DeleteOrderWithLinesAsync( int orderId, IEnumerable<StockOrderLineModel> lines );
	Task DeleteOrderWithLinesAsync( int orderId, IEnumerable<StockOrderLineModel> lines, CancellationToken cancellationToken );
	Task<int> InsertOrderLineAsync( StockOrderLineModel line );
	Task<int> InsertOrderLineAsync( StockOrderLineModel line, CancellationToken cancellationToken );
	Task<int> InsertOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection );
	Task<int> InsertOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection, CancellationToken cancellationToken );
	Task UpdateOrderLineAsync( StockOrderLineModel line );
	Task UpdateOrderLineAsync( StockOrderLineModel line, CancellationToken cancellationToken );
	Task UpdateOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection );
	Task UpdateOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection, CancellationToken cancellationToken );
	Task RegisterReceiptAsync( StockOrderLineModel line, double receivedAmount, DateTime? deliveryDate );
	Task RegisterReceiptAsync( StockOrderLineModel line, double receivedAmount, DateTime? deliveryDate, CancellationToken cancellationToken );
	Task DeleteOrderLineAsync( int lineId );
	Task DeleteOrderLineAsync( int lineId, CancellationToken cancellationToken );
	Task DeleteOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection );
	Task DeleteOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection, CancellationToken cancellationToken );
}