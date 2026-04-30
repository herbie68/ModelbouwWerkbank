namespace Modelbouwer.Interfaces;

public interface IStockOrderService
{
	Task<List<StockOrderModel>> GetAllOrdersAsync();
	Task<List<StockOrderLineModel>> GetOrderLinesAsync( int orderId );
	Task<int> InsertOrderAsync( StockOrderModel order );
	Task UpdateOrderAsync( StockOrderModel order );
	Task DeleteOrderAsync( int orderId );
	Task<int> InsertOrderLineAsync( StockOrderLineModel line );
	Task UpdateOrderLineAsync( StockOrderLineModel line );
	Task DeleteOrderLineAsync( int lineId );
}
