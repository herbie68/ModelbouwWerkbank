namespace Modelbouwer.Interfaces;

public interface IStockService
{
	Task<List<StockManagementModel>> GetCompleteInventoryAsync();
	Task<int> InsertCorrectionAsync( Dictionary<string, object?> queryParameters );
}
