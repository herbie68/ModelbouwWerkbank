namespace Modelbouwer.Interfaces;

public interface IStockOrderService
{
	Task<List<StockOrderModel>> GetAllOrdersAsync();
	Task<List<StockOrderLineModel>> GetOrderLinesAsync( int orderId );
	Task<int> InsertOrderAsync( StockOrderModel order );
	Task<int> InsertOrderWithLinesAsync( StockOrderModel order, IEnumerable<StockOrderLineModel> lines );
	Task UpdateOrderAsync( StockOrderModel order );
	Task DeleteOrderAsync( int orderId );
	Task DeleteOrderWithLinesAsync( int orderId, IEnumerable<StockOrderLineModel> lines );
	Task<int> InsertOrderLineAsync( StockOrderLineModel line );
	Task<int> InsertOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection );
	Task UpdateOrderLineAsync( StockOrderLineModel line );
	Task UpdateOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection );
	Task DeleteOrderLineAsync( int lineId );
	Task DeleteOrderLineWithStockCorrectionAsync( StockOrderLineModel line, double stockCorrection );
}
