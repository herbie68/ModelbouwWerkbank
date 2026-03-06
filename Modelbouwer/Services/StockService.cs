namespace Modelbouwer.Services;

public class StockService : IStockService
{
	private readonly GenericDataService _dataService;

	#region Database queries
	public string CompleteInventoryList = $"" +
		$"SELECT " +
		$"	p.{DBNames.ProductFieldNameId} AS Product_Id, " +
		$"	p.{DBNames.ProductFieldNameCode} AS Code, " +
		$"	p.{DBNames.ProductFieldNameName} AS Name, " +
		$"	p.{DBNames.ProductFieldNamePrice} AS Price, " +
		$"	p.{DBNames.ProductFieldNameMinimalStock} AS MinimalStock, " +
		$"	{DBNames.Database}.{DBNames.GetCategoryFunction}(p.{DBNames.ProductFieldNameCategoryId}) AS Category, " +
		$"	{DBNames.Database}.{DBNames.GetStockLocationFunction}(s.{DBNames.StocklogFieldNameId}) AS Location, " +
		$"	IFNULL(sl.InventoryAmount, 0) AS InventoryAmount, " +
		$"	p.{DBNames.ProductFieldNamePrice} * IFNULL(sl.InventoryAmount, 0) AS InventoryValue, " +
		$"	IFNULL(so.InOrder, 0) AS InOrder, " +
		$"	(IFNULL(sl.InventoryAmount, 0) + IFNULL(so.InOrder, 0)) AS VirtualInventoryAmount, " +
		$"	p.{DBNames.ProductFieldNamePrice} * (IFNULL(sl.InventoryAmount, 0) + IFNULL(so.InOrder, 0)) AS VirtualInventoryValue, " +
		$"	GREATEST(p.{DBNames.ProductFieldNameMinimalStock} - (IFNULL(sl.InventoryAmount, 0) + IFNULL(so.InOrder, 0)), 0) AS Short, " +
		$"  CASE " +
		$"		WHEN GREATEST( p.{DBNames.ProductFieldNameMinimalStock} - (IFNULL(sl.InventoryAmount, 0) + IFNULL(so.InOrder, 0)), 0) > 0 THEN 0 ELSE GREATEST(p.{DBNames.ProductFieldNameMinimalStock} - IFNULL(sl.InventoryAmount,0), 0) END AS TempShort " +
		$"FROM {DBNames.Database}.{DBNames.ProductTable} p " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.StorageTable} s ON p.{DBNames.ProductFieldNameStorageId} = s.{DBNames.StorageFieldNameId} " +
		$"LEFT JOIN ( " +
		$"	SELECT" +
		$"		{DBNames.StocklogFieldNameProductId}, " +
		$"		SUM(({DBNames.StocklogFieldNameAmountReceived} - {DBNames.StocklogFieldNameAmountUsed}) + {DBNames.StocklogFieldNameAmountCorrection}) AS InventoryAmount " +
		$"	FROM {DBNames.Database}.{DBNames.StocklogTable} " +
		$"	GROUP BY {DBNames.StocklogFieldNameProductId}" +
		$") sl ON sl.{DBNames.StocklogFieldNameProductId} = p.{DBNames.ProductFieldNameId} " +
		$"LEFT JOIN ( " +
		$"	SELECT " +
		$"		{DBNames.OrderLineFieldNameProductId}, " +
		$"		SUM({DBNames.OrderLineFieldNameOpenAmount}) AS InOrder " +
		$"	FROM {DBNames.Database}.{DBNames.OrderLineTable} " +
		$"	GROUP BY {DBNames.OrderLineFieldNameProductId}" +
		$") so ON so.{DBNames.OrderLineFieldNameProductId} = p.{DBNames.ProductFieldNameId} " +
		$"WHERE p.{DBNames.ProductFieldNameHide} = 0;";

	public string InsertInventoryCorrection = $"" +
		$"INSERT INTO {DBNames.Database}.{DBNames.StocklogTable} ( " +
		$"{DBNames.StocklogFieldNameProductId}, " +
		$"{DBNames.StocklogFieldNameAmountCorrection}, " +
		$"{DBNames.StocklogFieldNameLogDate} " +
		$") VALUES ( " +
		$"@{DBNames.StocklogFieldNameProductId}, " +
		$"@{DBNames.StocklogFieldNameAmountCorrection}, " +
		$"{DBNames.SqlCurrentDate} " +
		$");";

	public string UpdateProductCorrection = $"" +
		$"UPDATE {DBNames.Database}.{DBNames.ProductTable} " +
		$"SET " +
		$"{DBNames.ProductFieldNamePrice} = @{DBNames.ProductFieldNamePrice}, " +
		$"{DBNames.ProductFieldNameMinimalStock} = @{DBNames.ProductFieldNameMinimalStock} " +
		$"WHERE {DBNames.ProductFieldNameId} = @{DBNames.ProductFieldNameId};";
	#endregion

	#region All Database mutations for inventory management
	#region Get the complete inventory list
	public Task<List<StockManagementModel>> GetCompleteInventoryAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteInventoryList, reader =>
		{
			return new StockManagementModel
			{
				ProductId = DatabaseValueConverter.GetInt( reader [ "Product_Id" ] ),
				ProductCode = DatabaseValueConverter.GetString( reader [ "Code" ] ),
				ProductName = DatabaseValueConverter.GetString( reader [ "Name" ] ),
				ProductPrice = DatabaseValueConverter.GetDouble( reader [ "Price" ] ),
				ProductMinimalStock = DatabaseValueConverter.GetDouble( reader [ "MinimalStock" ] ),
				ProductCategory = DatabaseValueConverter.GetString( reader [ "Category" ] ),
				ProductStorageLocation = DatabaseValueConverter.GetString( reader [ "Location" ] ),
				ProductInventory = DatabaseValueConverter.GetDouble( reader [ "InventoryAmount" ] ),
				ProductOriginalInventory = DatabaseValueConverter.GetDouble( reader [ "InventoryAmount" ] ), // Is used to determine the inventory correction that has to be written to the database
				ProductInventoryValue = DatabaseValueConverter.GetDouble( reader [ "InventoryValue" ] ),
				ProductInOrder = DatabaseValueConverter.GetDouble( reader [ "InOrder" ] ),
				ProductVirtualInventory = DatabaseValueConverter.GetDouble( reader [ "VirtualInventoryAmount" ] ),
				ProductVirtualInventoryValue = DatabaseValueConverter.GetDouble( reader [ "VirtualInventoryValue" ] ),
				ProductShortInventory = DatabaseValueConverter.GetDouble( reader [ "Short" ] ),
				ProductTempShortInventory = DatabaseValueConverter.GetDouble( reader [ "TempShort" ] )
			};
		} );
	}
	#endregion

	#region Insert a inventory correction for the given product
	public async Task<int> InsertCorrectionAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.StocklogFieldNameProductId}", queryParameters[$"@{DBNames.StocklogFieldNameProductId}"] ?? DBNull.Value },
			{ $"@{DBNames.StocklogFieldNameAmountCorrection}", queryParameters[$"@{DBNames.StocklogFieldNameAmountCorrection}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( InsertInventoryCorrection, parameters );

		return ( int ) newId;
	}
	#endregion

	#region Update Minimal stock and/or price for the given product
	public async Task<int> UpdateProductCorrectionAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ProductFieldNameId}", queryParameters[$"@{DBNames.ProductFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.ProductFieldNameMinimalStock}", queryParameters[$"@{DBNames.ProductFieldNameMinimalStock}"] ?? DBNull.Value },
			{ $"@{DBNames.ProductFieldNamePrice}", queryParameters[$"@{DBNames.ProductFieldNamePrice}"] ?? 0 }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( UpdateProductCorrection, parameters );

		return ( int ) newId;
	}
	#endregion
	#endregion

	public StockService( GenericDataService dataService )
	{
		_dataService = dataService;
	}
}
