namespace Modelbouwer.Helpers;

public class DBNames
{
	#region Sql commands
	public static readonly string SqlAnd = " AND ";
	public static readonly string SqlAsc = " ASC ";
	public static readonly string SqlAs = " AS ";
	public static readonly string SqlBetween = " BETWEEN ";
	public static readonly string SqlCall = "CALL ";
	public static readonly string SqlCast = "CAST( ";
	public static readonly string SqlConcat = "CONCAT( ";
	public static readonly string SqlCount = " COUNT( ";
	public static readonly string SqlCountAll = " COUNT(*) ";
	public static readonly string SqlCountUnique = " COUNT(DISTINCT ";
	public static readonly string SqlCurrentDate = " CURDATE() ";
	public static readonly string SqlCurrentDateTime = " NOW() ";
	public static readonly string SqlCurrentTime = " CURTIME() ";
	public static readonly string SqlDelete = "DELETE ";
	public static readonly string SqlDeleteFrom = "DELETE FROM ";
	public static readonly string SqlDesc = " DESC ";
	public static readonly string SqlFrom = " FROM ";
	public static readonly string SqlIn = "IN ";
	public static readonly string SqlInnerJoin = "INNER JOIN ";
	public static readonly string SqlInsert = "INSERT INTO ";
	public static readonly string SqlIsNull = " IS NULL ";
	public static readonly string SqlLimit = " LIMIT ";
	public static readonly string SqlLimit1 = " LIMIT 1 ";
	public static readonly string SqlLike = " LIKE ";
	public static readonly string SqlLower = " LOWER ( ";
	public static readonly string SqlMax = "MAX( ";
	public static readonly string SqlMin = "MIN( ";
	public static readonly string SqlOn = " ON ";
	public static readonly string SqlOr = " OR ";
	public static readonly string SqlOrder = " ORDER BY ";
	public static readonly string SqlOrderBy = " ORDER BY ";
	public static readonly string SqlSelect = "SELECT ";
	public static readonly string SqlSelectAll = "SELECT *";
	public static readonly string SqlSelectDistinct = "SELECT DISTINCT ";
	public static readonly string SqlSelectLastId = "SELECT LAST_INSERT_ID();";
	public static readonly string SqlSet = " SET ";
	public static readonly string SqlSum = " SUM( ";
	public static readonly string SqlSubString = " SUBSTRING( ";
	public static readonly string SqlUnsigned = " as UNSIGNED) ";
	public static readonly string SqlUnionAll = "UNION ALL";
	public static readonly string SqlUpdate = "UPDATE ";
	public static readonly string SqlValues = " VALUES ";
	public static readonly string SqlWhere = " WHERE ";
	public static readonly string SqlWithRecursive = "WITH RECURSIVE ";
	#endregion

	#region Stored Procedures
	public static readonly string SPDeleteProductId = "DeleteProductId";
	public static readonly string SPDeleteProductIdInputParameter = "p_ProductId";
	public static readonly string SPDeleteProductIdOutputParameter = "result";

	public static readonly string SPGetLatestAddedRecord = "GetlatestAddedRecord";
	public static readonly string SPGetLatestAddedRecordInputParameterTable = "?tableName";
	public static readonly string SPGetLatestAddedRecordInputParameterId = "?IdColumn";

	public static readonly string SPGeProjectEndDate = "GetProjectEndDate";
	public static readonly string SPProjectEndDateInputParameter = "p_ProductId";

	public static readonly string SPResetDefaultSupplier = "ResetDefaultSupplier";
	public static readonly string SPResetDefaultSupplierInputParameterProductId = "p_ProductId";
	public static readonly string SPResetDefaultSupplierInputParameterSupplierId = "p_SupplierId";

	public static readonly string SPSetDefaultSupplier = "SetDefaultSupplier";
	public static readonly string SPSetDefaultSupplierInputParameterProductId = "p_ProductId";
	public static readonly string SPSetDefaultSupplierInputParameterSupplierId = "p_SupplierId";

	public static readonly string SPGeProjectMaterialCosts = "GetTotalCostByProjectId";
	public static readonly string SPProjectMaterialCostsInputParameter = "p_ProjectId";

	public static readonly string SPGeProjectTimeSpent = "GetTotalMinutesByProjectId";
	public static readonly string SPProjectTimeSpentParameter = "p_ProjectId";

	public static readonly string SPGetShortListProductsBySupplier = "GetShortListProductsBySupplier";
	public static readonly string SPGetShortListProductsBySupplierInputParameter = "?SelectedSupplierId"; // The ? is added to work correctly

	public static readonly string SPGetProductsUsageByProject = "GetProductsByProject";
	public static readonly string SPGetProductsUsageByProjectInputParameter = "?SelectedProjectId"; // The ? is added to work correctly


	public static readonly string SPGetAllProductsUsage = "GetProductsUsage";

	public static readonly string SPGetOrderHistoryBySupplier = "GetOrderHistoryBySupplier";
	public static readonly string SPGetOrderHistoryBySupplierInputParameter = "?SupplierId"; // The ? is added to work correctly

	public static readonly string SPGetStockOverviewByProduct = "GetProductStockMovements";
	public static readonly string SPGetStockOverviewByProductInputParameter = "?SelectedProductId"; // The ? is added to work correctly

	public static readonly string SPChartsWorkedHoursPerDayModel = "GetDaylyWorkedHours";
	public static readonly string SPChartsWorkedHoursPerDayModelInputParameter = "?SelectedProjectId";

	public static readonly string SPChartsWorkedHoursPerMonthModel = "GetMonthlyWorkedHours";
	public static readonly string SPChartsWorkedHoursPerMonthModelInputParameter = "?SelectedProjectId";

	public static readonly string SPChartsWorkedHoursPerYearModel = "GetYearlyWorkedHours";
	public static readonly string SPChartsWorkedHoursPerYearModelInputParameter = "?SelectedProjectId";

	public static readonly string SPChartsWorkedHoursPerWorktype = "GetWorkedHoursByWorktype";
	public static readonly string SPChartsWorkedHoursPerWorktypeInputParameter = "?SelectedProjectId";
	#endregion

	#region Database
	public static readonly string Database = "modelbuilder";
	#endregion

	#region Settings table

	public static readonly string SettingsTable = "settings";
	public static readonly string SettingsFieldNameCulture = "Culture";
	public static readonly string SettingsFieldNameLanguage = "Language";
	public static readonly string SettingsFieldNameHourRate = "HourRate";

	public static readonly string SettingsFieldTypeCulture = "string";
	public static readonly string SettingsFieldTypeLanguage = "string";
	public static readonly string SettingsFieldTypeHourRate = "double";

	#endregion Settings table

	#region Brand table

	public static readonly string BrandTable = "brand";
	public static readonly string BrandFieldNameId = "Id";
	public static readonly string BrandFieldNameName = "Name";

	public static readonly string BrandFieldTypeId = "int";
	public static readonly string BrandFieldTypeName = "string";

	#endregion Brand table

	#region Category table

	public static readonly string CategoryTable = "category";
	public static readonly string CategoryFieldNameId = "Id";
	public static readonly string CategoryFieldNameParentId = "ParentId";
	public static readonly string CategoryFieldNameName = "Name";
	public static readonly string CategoryFieldTypeId = "int";
	public static readonly string CategoryFieldTypeParentId = "int";
	public static readonly string CategoryFieldTypeName = "string";

	#endregion Category table

	#region Contacttype table

	public static readonly string ContactTypeTable = "contacttype";
	public static readonly string ContactTypeFieldNameId = "Id";
	public static readonly string ContactTypeFieldTypeId = "Int";
	public static readonly string ContactTypeFieldNameName = "Name";
	public static readonly string ContactTypeFieldTypeName = "string";

	#endregion Contacttype table

	#region Currency table

	public static readonly string CurrencyTable = "currency";
	public static readonly string CurrencyFieldNameId = "Id";
	public static readonly string CurrencyFieldTypeId = "int";
	public static readonly string CurrencyFieldNameCode = "Code";
	public static readonly string CurrencyFieldTypeCode = "string";
	public static readonly string CurrencyFieldNameSymbol = "Symbol";
	public static readonly string CurrencyFieldTypeSymbol = "string";
	public static readonly string CurrencyFieldNameName = "Name";
	public static readonly string CurrencyFieldTypeName = "string";
	public static readonly string CurrencyFieldNameRate = "ConversionRate";
	public static readonly string CurrencyFieldTypeRate = "double";

	#endregion Currency table

	#region Country table

	public static readonly string CountryTable = "country";
	public static readonly string CountryView = "view_country";
	public static readonly string CountryFieldNameId = "Id";
	public static readonly string CountryFieldTypeId = "int";
	public static readonly string CountryFieldNameCode = "Code";
	public static readonly string CountryFieldTypeCode = "string";
	public static readonly string CountryFieldNameCurrencySymbol = "Defaultcurrency_Symbol";
	public static readonly string CountryFieldTypeCurrencySymbol = "string";
	public static readonly string CountryFieldNameName = "Name";
	public static readonly string CountryFieldTypeName = "string";
	public static readonly string CountryFieldNameCurrencyId = "Defaultcurrency_Id";
	public static readonly string CountryFieldTypeCurrencyId = "int";

	#endregion Country table

	#region Product table
	public static readonly string ProductFieldNameBrandId = "Brand_Id";
	public static readonly string ProductFieldNameCategoryId = "Category_Id";
	public static readonly string ProductFieldNameCode = "Code";
	public static readonly string ProductFieldNameDimensions = "Dimensions";
	public static readonly string ProductFieldNameHide = "Hide";
	public static readonly string ProductFieldNameId = "Id";
	public static readonly string ProductFieldNameImage = "Image";
	public static readonly string ProductFieldNameImageRotationAngle = "ImageRotationAngle";
	public static readonly string ProductFieldNameMemo = "Memo";
	public static readonly string ProductFieldNameMinimalStock = "MinimalStock";
	public static readonly string ProductFieldNameName = "Name";
	public static readonly string ProductFieldNamePrice = "Price";
	public static readonly string ProductFieldNameProjectCosts = "ProjectCosts";
	public static readonly string ProductFieldNameStandardOrderQuantity = "StandardOrderQuantity";
	public static readonly string ProductFieldNameStorageId = "Storage_Id";
	public static readonly string ProductFieldNameUnitId = "Unit_Id";
	public static readonly string ProductFieldTypeBrandId = "int";
	public static readonly string ProductFieldTypeCategoryId = "int";
	public static readonly string ProductFieldTypeCode = "string";
	public static readonly string ProductFieldTypeDimensions = "string";
	public static readonly string ProductFieldTypeHide = "int";
	public static readonly string ProductFieldTypeId = "int";
	public static readonly string ProductFieldTypeImage = "blob";
	public static readonly string ProductFieldTypeImageRotationAngle = "string";
	public static readonly string ProductFieldTypeMemo = "longtext";
	public static readonly string ProductFieldTypeMinimalStock = "double";
	public static readonly string ProductFieldTypeName = "string";
	public static readonly string ProductFieldTypePrice = "double";
	public static readonly string ProductFieldTypeProjectCosts = "int";
	public static readonly string ProductFieldTypeStandardOrderQuantity = "double";
	public static readonly string ProductFieldTypeStorageId = "int";
	public static readonly string ProductFieldTypeUnitId = "int";
	public static readonly string ProductTable = "product";
	public static readonly string ProductView = "view_product";

	#endregion Product table

	#region Product Inventory table and view
	public static readonly string ProductInventoryView = "view_productinventory";
	public static readonly string ProductInventoryViewFieldNameProductId = "Product_Id";
	public static readonly string ProductInventoryViewFieldTypeProductId = "int";
	public static readonly string ProductInventoryViewFieldNameProductCode = "Code";
	public static readonly string ProductInventoryViewFieldTypeProductCode = "string";
	public static readonly string ProductInventoryViewFieldNameProductName = "Name";
	public static readonly string ProductInventoryViewFieldTypeProductName = "string";
	public static readonly string ProductInventoryViewFieldNameProductPrice = "Price";
	public static readonly string ProductInventoryViewFieldTypeProductPrice = "string";
	public static readonly string ProductInventoryViewFieldNameProductMinimalStock = "MinimalStock";
	public static readonly string ProductInventoryViewFieldTypeProductMinimalStock = "double";
	public static readonly string ProductInventoryViewFieldNameProductCategory = "Category";
	public static readonly string ProductInventoryViewFieldTypeProductCategory = "string";
	public static readonly string ProductInventoryViewFieldNameProductStorage = "Location";
	public static readonly string ProductInventoryViewFieldTypeProductStorage = "string";
	public static readonly string ProductInventoryViewFieldNameProductMinimalInventory = "InventoryOrder";
	public static readonly string ProductInventoryViewFieldTypeProductMinimalInventory = "double";
	public static readonly string ProductInventoryViewFieldNameProductMinimalValue = "Value";
	public static readonly string ProductInventoryViewFieldTypeProductMinimalValue = "string";
	public static readonly string ProductInventoryViewFieldNameProductMinimalOrder = "InOrder";
	public static readonly string ProductInventoryViewFieldTypeProductMinimalOrder = "double";
	public static readonly string ProductInventoryViewFieldNameProductMinimalVirtualInventory = "VirtualInventory";
	public static readonly string ProductInventoryViewFieldTypeProductMinimalVirtualInventory = "double";
	public static readonly string ProductInventoryViewFieldNameProductMinimalVirtualValue = "VirtualValue";
	public static readonly string ProductInventoryViewFieldTypeProductMinimalVirtualValue = "string";
	public static readonly string ProductInventoryViewFieldNameProductShort = "Short";
	public static readonly string ProductInventoryViewFieldTypeProductShort = "double";
	public static readonly string ProductInventoryViewFieldNameProductTempShort = "TempShort";
	public static readonly string ProductInventoryViewFieldTypeProductTempShort = "double";

	public static readonly string ProductInventoryOrderProcedure = "GetProductsBySupplier";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductId = "Product_Id";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductId = "int";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductCode = "Code";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductCode = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductName = "Name";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductName = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductPrice = "Price";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductPrice = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductMinimalStock = "MinimalStock";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductMinimalStock = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductCategory = "Category";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductCategory = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductMinimalInventory = "InventoryOrder";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductMinimalInventory = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductMinimalValue = "Value";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductMinimalValue = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductMinimalOrder = "InOrder";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductMinimalOrder = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductOrderPer = "OrderPer";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductOrderPer = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductToOrder = "ToOrder";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductToOrder = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductShort = "Short";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductShort = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameSupplierProductNumber = "SupplierProductNumber";
	public static readonly string ProductInventoryOrderProcedureFieldTypeSupplierProductNumber = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameSupplierProductName = "SupplierProductName";
	public static readonly string ProductInventoryOrderProcedureFieldTypeSupplierProductName = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameSupplierPrice = "SupplierPrice";
	public static readonly string ProductInventoryOrderProcedureFieldTypeSupplierPrice = "double";
	public static readonly string ProductInventoryOrderProcedureFieldNameSupplierCurrencyId = "SupplierCurrencyId";
	public static readonly string ProductInventoryOrderProcedureFieldTypeSupplierCurrencyId = "int";
	public static readonly string ProductInventoryOrderProcedureFieldNameSupplierCurrencySymbol = "SupplierCurrencySymbol";
	public static readonly string ProductInventoryOrderProcedureFieldTypeSupplierCurrencySymbol = "string";
	public static readonly string ProductInventoryOrderProcedureFieldNameProductFromSupplier = "ProductFromSupplier";
	public static readonly string ProductInventoryOrderProcedureFieldTypeProductFromSupplier = "string";

	public static readonly string ProductInventoryReceiptsProcedure = "GetSupplyOrderDetails";
	public static readonly string ProductInventoryReceiptsView = "view_supplyopenorderdetails";
	public static readonly string ProductInventoryReceiptsProcedureFieldNameOrderId = "Order_Id";
	public static readonly string ProductInventoryReceiptsProcedureFieldTypeOrderId = "int";
	public static readonly string ProductInventoryReceiptsProcedureFieldNameOrderLineId = "OrderLine_Id";
	public static readonly string ProductInventoryReceiptsProcedureFieldTypeOrderLineId = "int";
	public static readonly string ProductInventoryReceiptsProcedureFieldNameProductId = "Product_Id";
	public static readonly string ProductInventoryReceiptsProcedureFieldTypeProductId = "int";
	public static readonly string ProductInventoryReceiptProcedureFieldNameProductNumber = "SupplierProductNumber";
	public static readonly string ProductInventoryReceiptProcedureFieldTypeProductNumber = "string";
	public static readonly string ProductInventoryReceiptProcedureFieldNameProductName = "SupplierProductName";
	public static readonly string ProductInventoryReceiptProcedureFieldTypeProductName = "string";
	public static readonly string ProductInventoryReceiptProcedureFieldNameOrdered = "Ordered";
	public static readonly string ProductInventoryReceiptProcedureFieldTypeOrdered = "double";
	public static readonly string ProductInventoryReceiptProcedureFieldNameWaitFor = "WaitFor";
	public static readonly string ProductInventoryReceiptProcedureFieldTypeWaitFor = "double";
	public static readonly string ProductInventoryReceiptProcedureFieldNameStockLogRecieved = "StockLogRecieved";
	public static readonly string ProductInventoryReceiptProcedureFieldTypeStockLogRecieved = "double";
	public static readonly string ProductInventoryReceiptProcedureFieldNameStockInStock = "InStock";
	public static readonly string ProductInventoryReceiptProcedureFieldTypeStockInStock = "double";
	#endregion

	#region ProductSupplier table

	public static readonly string ProductSupplierTable = "productsupplier";
	public static readonly string ProductSupplierView = "view_productsupplier";
	public static readonly string ProductSupplierFieldNameId = "Id";
	public static readonly string ProductSupplierFieldTypeId = "int";
	public static readonly string ProductSupplierFieldNameProductId = "Product_Id";
	public static readonly string ProductSupplierFieldTypeProductId = "int";
	public static readonly string ProductSupplierFieldNameProductCode = "Code";          // Not in table only needed for searching the Product table on import
	public static readonly string ProductSupplierFieldTypeProductCode = "string";        // Not in table only needed for searching the Product table on import
	public static readonly string ProductSupplierFieldNameSupplierId = "Supplier_Id";
	public static readonly string ProductSupplierFieldTypeSupplierId = "int";
	public static readonly string ProductSupplierFieldNameCurrencyId = "Currency_Id";
	public static readonly string ProductSupplierFieldTypeCurrencyId = "int";
	public static readonly string ProductSupplierFieldNameProductNumber = "ProductNumber";
	public static readonly string ProductSupplierFieldTypeProductNumber = "string";
	public static readonly string ProductSupplierFieldNameProductName = "ProductName";
	public static readonly string ProductSupplierFieldTypeProductName = "string";
	public static readonly string ProductSupplierFieldNamePrice = "Price";
	public static readonly string ProductSupplierFieldTypePrice = "double";
	public static readonly string ProductSupplierFieldNameProductUrl = "Url";
	public static readonly string ProductSupplierFieldTypeProductUrl = "string";
	public static readonly string ProductSupplierFieldNameDefaultSupplier = "DefaultSupplier";
	public static readonly string ProductSupplierFieldTypeDefaultSupplier = "string";
	public static readonly string ProductSupplierFieldNameSupplierName = "SupplierName";
	public static readonly string ProductSupplierFieldTypeSupplierName = "string";

	#endregion ProductSupplier table

	#region Project table
	public static readonly string ProjectTable = "project";
	public static readonly string ProjectFieldNameId = "Id";
	public static readonly string ProjectFieldTypeId = "int";
	public static readonly string ProjectFieldNameName = "Name";
	public static readonly string ProjectFieldTypeName = "string";
	public static readonly string ProjectFieldNameCode = "Code";
	public static readonly string ProjectFieldTypeCode = "string";
	public static readonly string ProjectFieldNameStartDate = "StartDate";
	public static readonly string ProjectFieldTypeStartDate = "date";
	public static readonly string ProjectFieldNameEndDate = "EndDate";
	public static readonly string ProjectFieldTypeEndDate = "date";
	public static readonly string ProjectFieldNameClosed = "Closed";
	public static readonly string ProjectFieldTypeClosed = "int";
	public static readonly string ProjectFieldNameExpectedTime = "ExpectedTime";
	public static readonly string ProjectFieldTypeExpectedTime = "int";
	public static readonly string ProjectFieldNameMemo = "Memo";
	public static readonly string ProjectFieldTypeMemo = "longtext";
	public static readonly string ProjectFieldNameImageRotationAngle = "ImageRotationAngle";
	public static readonly string ProjectFieldTypeImageRotationAngle = "string";
	public static readonly string ProjectFieldNameImage = "Image";
	public static readonly string ProjectFieldTypeImage = "blob";

	public static readonly string ProjectCostsView = "view_projectcosts";
	public static readonly string ProjectCostsViewFieldNameProjectName = "ProjectName";
	public static readonly string ProjectCostsViewFieldTypeProjectName = "string";
	public static readonly string ProjectCostsViewFieldNameCategoryName = "Category";
	public static readonly string ProjectCostsViewFieldTypeCategoryName = "string";
	public static readonly string ProjectCostsViewFieldNameProductName = "_productName";
	public static readonly string ProjectCostsViewFieldTypeProductName = "string";
	public static readonly string ProjectCostsViewFieldNameAmountUsed = "AmountUsed";
	public static readonly string ProjectCostsViewFieldTypeAmountUsed = "int";
	public static readonly string ProjectCostsViewFieldNamePrice = "Price";
	public static readonly string ProjectCostsViewFieldTypePrice = "double";
	public static readonly string ProjectCostsViewFieldNameTotal = "Total";
	public static readonly string ProjectCostsViewFieldTypeTotal = "double";

	public static readonly string ProjectExportView = "view_projectexport";

	public static readonly string ProjectTotalsView = "view_projecttotals";
	public static readonly string ProjectTotalssViewFieldNameId = "Id";
	public static readonly string ProjectTotalssViewFieldNameCode = "Code";
	public static readonly string ProjectTotalssViewFieldNameName = "Name";
	public static readonly string ProjectTotalssViewFieldNameStartDate = "StartDate";
	public static readonly string ProjectTotalssViewFieldNameEndDate = "EndDate";
	public static readonly string ProjectTotalssViewFieldNameExpectedTime = "ExpectedTime";
	public static readonly string ProjectTotalssViewFieldNameImage = "Image";
	public static readonly string ProjectTotalssViewFieldNameImageRotationAngle = "ImageRotationAngle";
	public static readonly string ProjectTotalssViewFieldNameMemo = "Memo";
	public static readonly string ProjectTotalssViewFieldNameTotalTimeInHours = "TotalTimeInHours";
	public static readonly string ProjectTotalssViewFieldNameShortestWorkday = "ShortestWorkday";
	public static readonly string ProjectTotalssViewFieldNameAverageHoursPerDay = "AverageHoursPerDay";
	public static readonly string ProjectTotalssViewFieldNameHoursShortestWorkday = "HoursShortestWorkday";
	public static readonly string ProjectTotalssViewFieldNameLongestWorkday = "LongestWorkday";
	public static readonly string ProjectTotalssViewFieldNameHoursLongestWorkday = "HoursLongestWorkday";
	public static readonly string ProjectTotalssViewFieldNameWorkingDays = "WorkingDays";
	public static readonly string ProjectTotalssViewFieldNameIsClosed = "IsClosed";

	public static readonly string SPProjectExpectedEndDate = "GetProjectEndDate";
	#endregion Project table

	#region Products in stock
	#region Stocklog table
	public static readonly string StocklogTable = "stocklog";
	public static readonly string GetCategoryFunction = "GetCategoryPath";
	public static readonly string GetStockLocationFunction = "GetStockLocationPath";
	public static readonly string StocklogFieldNameId = "Id";
	public static readonly string StocklogFieldTypeId = "int";
	public static readonly string StocklogFieldNameProductId = "product_Id";
	public static readonly string StocklogFieldTypeProductId = "int";
	public static readonly string StocklogFieldNameStorageId = "storage_Id";
	public static readonly string StocklogFieldTypeStorageId = "int";
	public static readonly string StocklogFieldNameSupplyOrderId = "supplyorder_Id";
	public static readonly string StocklogFieldTypeSupplyOrderId = "int";
	public static readonly string StocklogFieldNameProductUsageId = "productusage_Id";
	public static readonly string StocklogFieldTypeProductUsageId = "int";
	public static readonly string StocklogFieldNameSupplyOrderlineId = "supplyorderline_Id";
	public static readonly string StocklogFieldTypeSupplyOrderlineId = "int";
	public static readonly string StocklogFieldNameAmountReceived = "AmountReceived";
	public static readonly string StocklogFieldTypeAmountReceived = "double";
	public static readonly string StocklogFieldNameAmountUsed = "AmountUsed";
	public static readonly string StocklogFieldTypeAmountUsed = "double";
	public static readonly string StocklogFieldNameAmountCorrection = "AmountCorrection";
	public static readonly string StocklogFieldTypeAmountCorrection = "double";
	public static readonly string StocklogFieldNameLogDate = "LogDate";
	public static readonly string StocklogFieldTypeLogDate = "date";
	#endregion Stocklog table

	#region Products in stock
	public static readonly string ProductsInStockView = "view_instock";
	public static readonly string ProductsInStockFieldNameProduct_Id = "Product_Id";
	public static readonly string ProductsInStockFieldTypeProduct_Id = "int";
	public static readonly string ProductsInStockFieldNameReceived = "AmountReceived";
	public static readonly string ProductsInStockFieldTypeReceived = "double";
	public static readonly string ProductsInStockFieldNameUsed = "AmountUsed";
	public static readonly string ProductsInStockFieldTypeUsed = "double";
	public static readonly string ProductsInStockFieldNameCorrection = "AmountCorrection";
	public static readonly string ProductsInStockFieldTypeCorrection = "double";
	public static readonly string ProductsInStockFieldNameAmount = "Amount";
	public static readonly string ProductsInStockFieldTypeAmount = "double";
	#endregion
	#endregion

	#region Storage table

	public static readonly string StorageTable = "storage";
	public static readonly string StorageFieldNameId = "Id";
	public static readonly string StorageFieldTypeId = "int";
	public static readonly string StorageFieldNameParentId = "ParentId";
	public static readonly string StorageFieldTypeParentId = "int";
	public static readonly string StorageFieldNameName = "Name";
	public static readonly string StorageFieldTypeName = "string";
	public static readonly string StorageFieldNameFullpath = "Fullpath";
	public static readonly string StorageFieldTypeFullpath = "string";

	#endregion Storage table

	#region Supplier tables
	#region Supplier table

	public static readonly string SupplierTable = "supplier";
	public static readonly string SupplierView = "view_supplier";
	public static readonly string SupplierFieldNameId = "Id";
	public static readonly string SupplierFieldTypeId = "int";
	public static readonly string SupplierFieldNameCode = "Code";
	public static readonly string SupplierFieldTypeCode = "string";
	public static readonly string SupplierFieldNameName = "Name";
	public static readonly string SupplierFieldTypeName = "string";
	public static readonly string SupplierFieldNameAddress1 = "Address1";
	public static readonly string SupplierFieldTypeAddress1 = "string";
	public static readonly string SupplierFieldNameAddress2 = "Address2";
	public static readonly string SupplierFieldTypeAddress2 = "string";
	public static readonly string SupplierFieldNameZip = "Zip";
	public static readonly string SupplierFieldTypeZip = "string";
	public static readonly string SupplierFieldNameCity = "City";
	public static readonly string SupplierFieldTypeCity = "string";
	public static readonly string SupplierFieldNameUrl = "Url";
	public static readonly string SupplierFieldTypeUrl = "string";
	public static readonly string SupplierFieldNameMemo = "Memo";
	public static readonly string SupplierFieldTypeMemo = "longtext";
	public static readonly string SupplierFieldNameCountryId = "Country_Id";
	public static readonly string SupplierFieldTypeCountryId = "int";
	public static readonly string SupplierFieldNameCurrencyId = "Currency_Id";
	public static readonly string SupplierFieldTypeCurrencyId = "int";
	public static readonly string SupplierFieldNameShippingCosts = "ShippingCosts";
	public static readonly string SupplierFieldTypeShippingCosts = "double";
	public static readonly string SupplierFieldNameMinShippingCosts = "MinShippingCosts";
	public static readonly string SupplierFieldTypeMinShippingCosts = "double";
	public static readonly string SupplierFieldNameOrderCosts = "OrderCosts";
	public static readonly string SupplierFieldTypeOrderCosts = "double";
	public static readonly string SupplierFieldNameMinOrderCosts = "MinOrderCosts";
	public static readonly string SupplierFieldTypeMinOrderCosts = "double";
	public static readonly string SupplierFieldNameGeneralEmail = "GeneralEmail";
	public static readonly string SupplierFieldTypeGeneralEmail = "string";
	public static readonly string SupplierFieldNameGeneralPhone = "GeneralPhone";
	public static readonly string SupplierFieldTypeGeneralPhone = "string";
	#endregion Supplier table

	#region SupplierContact Table
	public static readonly string SupplierContactTable = "suppliercontact";
	public static readonly string SupplierContactView = "view_suppliercontact";
	public static readonly string SupplierContactFieldNameId = "Id";
	public static readonly string SupplierContactFieldTypeId = "int";
	public static readonly string SupplierContactFieldNameSupplierId = "Supplier_Id";
	public static readonly string SupplierContactFieldTypeSupplierId = "int";
	public static readonly string SupplierContactFieldNameName = "Name";
	public static readonly string SupplierContactFieldTypeName = "string";
	public static readonly string SupplierContactFieldNameTypeId = "Contacttype_Id";
	public static readonly string SupplierContactFieldTypeTypeId = "int";
	public static readonly string SupplierContactFieldNameTypeName = "ContactType";
	public static readonly string SupplierContactFieldTypeTypeName = "string";
	public static readonly string SupplierContactFieldNamePhone = "Phone";
	public static readonly string SupplierContactFieldTypePhone = "string";
	public static readonly string SupplierContactFieldNameMail = "Mail";
	public static readonly string SupplierContactFieldTypeMail = "string";
	#endregion SupplierContact Table

	#region Supplier Order History
	public static readonly string SupplierOrderHistorySupplierId = "SupplierId";
	public static readonly string SupplierOrderHistoryOrderId = "OrderId";
	public static readonly string SupplierOrderHistoryOrderNumber= "OrderNumber";
	public static readonly string SupplierOrderHistoryOrderDate= "OrderDate";
	public static readonly string SupplierOrderHistoryOrderCosts= "OrderCosts";
	public static readonly string SupplierOrderHistoryShippingCosts= "ShippingCosts";
	public static readonly string SupplierOrderHistoryCurrencyConversionRate= "CurrencyConversionRate";
	public static readonly string SupplierOrderHistoryReceived= "Received";
	public static readonly string SupplierOrderHistoryProductId= "Product_Id";
	public static readonly string SupplierOrderHistoryProductNumber= "ProductNumber";
	public static readonly string SupplierOrderHistoryProductDescription= "Description";
	public static readonly string SupplierOrderHistoryPrice= "Price";
	public static readonly string SupplierOrderHistoryAmount= "Amount";
	public static readonly string SupplierOrderHistoryRowTotal= "RowTotal";
	public static readonly string SupplierOrderHistoryOrderTotal= "OrderTotal";
	#endregion
	#endregion Supplier tables

	#region Time

	public static readonly string TimeTable = "time";
	public static readonly string TimeFieldNameId = "Id";
	public static readonly string TimeFieldTypeId = "int";
	public static readonly string TimeFieldNameProjectId = "project_Id";
	public static readonly string TimeFieldTypeProjectId = "int";
	public static readonly string TimeFieldNameWorktypeId = "Worktype_Id";
	public static readonly string TimeFieldTypeWorktypeId = "int";
	public static readonly string TimeFieldNameWorkDate = "WorkDate";
	public static readonly string TimeFieldTypeWorkDate = "date";
	public static readonly string TimeFieldNameStartTime = "StartTime";
	public static readonly string TimeFieldTypeStartTime = "time";
	public static readonly string TimeFieldNameEndTime = "EndTime";
	public static readonly string TimeFieldTypeEndTime = "time";
	public static readonly string TimeFieldNameComment = "Comment";
	public static readonly string TimeFieldTypeComment = "string";

	public static readonly string TimeView = "view_time";
	public static readonly string TimeViewFieldNameId = "Id";
	public static readonly string TimeViewFieldTypeId = "int";
	public static readonly string TimeViewFieldNameProjectId = "ProjectId";
	public static readonly string TimeViewFieldTypeProjectId = "int";
	public static readonly string TimeViewFieldNameProjectName = "ProjectName";
	public static readonly string TimeViewFieldTypeProjectName = "string";
	public static readonly string TimeViewFieldNameWorktypeId = "WorktypeId";
	public static readonly string TimeViewFieldTypeWorktypeId = "int";
	public static readonly string TimeViewFieldNameWorktypeName = "WorktypeName";
	public static readonly string TimeViewFieldTypeWorktypeName = "string";
	public static readonly string TimeViewFieldNameWorkDate = "WorkDate";
	public static readonly string TimeViewFieldTypeWorkDate = "date";
	public static readonly string TimeViewFieldNameStartTime = "StartTime";
	public static readonly string TimeViewFieldTypeStartTime = "string";
	public static readonly string TimeViewFieldNameEndTime = "EndTime";
	public static readonly string TimeViewFieldTypeEndTime = "string";
	public static readonly string TimeViewFieldNameElapsedTime = "ELapsedTime";
	public static readonly string TimeViewFieldTypeElapsedTime = "string";
	public static readonly string TimeViewFieldNameElapsedMinutes = "ELapsedMinutes";
	public static readonly string TimeViewFieldTypeElapsedMinutes = "double";
	public static readonly string TimeViewFieldNameComment = "Comment";
	public static readonly string TimeViewFieldTypeComment = "string";
	public static readonly string TimeViewFieldNameYear = "Year";
	public static readonly string TimeViewFieldTypeYear = "int";
	public static readonly string TimeViewFieldNameMonth = "Month";
	public static readonly string TimeViewFieldTypeMonth = "int";
	public static readonly string TimeViewFieldNameWorkday = "Workday";
	public static readonly string TimeViewFieldTypeWorkday = "int";
	public static readonly string TimeViewFieldNameYearMonth = "YearMonth";
	public static readonly string TimeViewFieldTypeYearMonth = "string";
	public static readonly string TimeViewFieldNameYearWorkday = "YearWorkday";
	public static readonly string TimeViewFieldTypeYearWorkday = "string";
	public static readonly string TimeViewFieldNameSortIndex = "SortIndex";
	public static readonly string TimeViewFieldTypeSortIndex = "string";

	public static readonly string TimeReportView = "view_timereport";
	public static readonly string TimeViewFieldNameWorkedMinutes = "WorkedMinutes";
	public static readonly string TimeViewFieldTypeWorkedMinutes = "int";

	public static readonly string TimeExportView = "view_timeexport";

	#endregion Time

	#region ProductUsage

	public static readonly string ProductUsageTable = "productusage";
	public static readonly string ProductUsageFieldNameId = "Id";
	public static readonly string ProductUsageFieldTypeId = "int";
	public static readonly string ProductUsageFieldNameProjectId = "project_Id";
	public static readonly string ProductUsageFieldTypeProjectId = "int";
	public static readonly string ProductUsageFieldNameProductId = "product_Id";
	public static readonly string ProductUsageFieldTypeProductId = "int";
	public static readonly string ProductUsageFieldNameStorageId = "storage_Id";
	public static readonly string ProductUsageFieldTypeStorageId = "int";
	public static readonly string ProductUsageFieldNameAmountUsed = "AmountUsed";
	public static readonly string ProductUsageFieldTypeAmountUsed = "double";
	public static readonly string ProductUsageFieldNameUsageDate = "UsageDate";
	public static readonly string ProductUsageFieldTypeUsageDate = "date";
	public static readonly string ProductUsageFieldNameComment = "Comment";
	public static readonly string ProductUsageFieldTypeComment = "string";

	public static readonly string ProductUsageView = "view_productusage";
	public static readonly string ProductUsageViewFieldNameId = "Id";
	public static readonly string ProductUsageViewFieldTypeId = "int";
	public static readonly string ProductUsageViewFieldNameProjectId = "ProjectId";
	public static readonly string ProductUsageViewFieldTypeProjectId = "int";
	public static readonly string ProductUsageViewFieldNameProductId = "_productId";
	public static readonly string ProductUsageViewFieldTypeProductId = "int";
	public static readonly string ProductUsageViewFieldNameStorageId = "StorageId";
	public static readonly string ProductUsageViewFieldTypeStorageId = "int";
	public static readonly string ProductUsageViewFieldNameCategoryId = "CategoryId";
	public static readonly string ProductUsageViewFieldTypeCategoryId = "int";
	public static readonly string ProductUsageViewFieldNameProjectName = "ProjectName";
	public static readonly string ProductUsageViewFieldTypeProjectName = "string";
	public static readonly string ProductUsageViewFieldNameProductName = "_productName";
	public static readonly string ProductUsageViewFieldTypeProductName = "string";
	public static readonly string ProductUsageViewFieldNameStorageName = "StorageName";
	public static readonly string ProductUsageViewFieldTypeStorageName = "string";
	public static readonly string ProductUsageViewFieldNameCategoryName = "CategoryName";
	public static readonly string ProductUsageViewFieldTypeCategoryName = "string";
	public static readonly string ProductUsageViewFieldNameAmountUsed = "AmountUsed";
	public static readonly string ProductUsageViewFieldTypeAmountUsed = "double";
	public static readonly string ProductUsageViewFieldNameUsageDate = "UsageDate";
	public static readonly string ProductUsageViewFieldTypeUsageDate = "date";
	public static readonly string ProductUsageViewFieldNamePrice = "Price";
	public static readonly string ProductUsageViewFieldTypePrice = "double";
	public static readonly string ProductUsageViewFieldNameTotalCosts = "TotalCost";
	public static readonly string ProductUsageViewFieldTypeTotalCosts = "double";
	public static readonly string ProductUsageViewFieldNameComment = "Comment";
	public static readonly string ProductUsageViewFieldTypeComment = "string";

	#endregion ProductUsage

	#region Order table and view
	public static readonly string OrderTable = "supplyorder";
	public static readonly string OpenOrderView = "view_supplyopenorder";
	public static readonly string OrderFieldNameId = "Id";
	public static readonly string OrderFieldTypeId = "int";
	public static readonly string OrderFieldNameClosed = "Closed";
	public static readonly string OrderFieldTypeClosed = "int";
	public static readonly string OrderFieldNameClosedDate = "ClosedDate";
	public static readonly string OrderFieldTypeClosedDate = "date";
	public static readonly string OrderFieldNameSupplierId = "Supplier_Id";
	public static readonly string OrderFieldTypeSupplierId = "int";
	public static readonly string OrderFieldNameCurrencyId = "Currency_Id";
	public static readonly string OrderFieldTypeCurrencyId = "int";
	public static readonly string OrderFieldNameOrderNumber = "OrderNumber";
	public static readonly string OrderFieldTypeOrderNumber = "string";
	public static readonly string OrderFieldNameOrderDate = "OrderDate";
	public static readonly string OrderFieldTypeOrderDate = "date";
	public static readonly string OrderFieldNameCurrencySymbol = "CurrencySymbol";
	public static readonly string OrderFieldTypeCurrencySymbol = "string";
	public static readonly string OrderFieldNameConversionRate = "CurrencyConversionRate";
	public static readonly string OrderFieldTypeConversionRate = "double";
	public static readonly string OrderFieldNameShippingCosts = "ShippingCosts";
	public static readonly string OrderFieldTypeShippingCosts = "double";
	public static readonly string OrderFieldNameOrderCosts = "OrderCosts";
	public static readonly string OrderFieldTypeOrderCosts = "double";
	public static readonly string OrderFieldNameOrderMemo = "Memo";
	public static readonly string OrderFieldTypeOrderMemo = "longtext";

	public static readonly string OrderView = "view_supplyorder";
	public static readonly string OrderViewFieldNameId = "Id";
	public static readonly string OrderViewFieldTypeId = "int";
	public static readonly string OrderViewFieldNameSupplierId = "Supplier_Id";
	public static readonly string OrderViewFieldTypeSupplierId = "int";
	public static readonly string OrderViewFieldNameSupplierName = "SupplierName";
	public static readonly string OrderViewFieldTypeSupplierName = "string";
	public static readonly string OrderViewFieldNameShippingCosts = "ShippingCosts";
	public static readonly string OrderViewFieldTypeShippingCosts = "double";
	public static readonly string OrderViewFieldNameMinShippingCosts = "MinShippingCosts";
	public static readonly string OrderViewFieldTypeMinShippingCosts = "double";
	public static readonly string OrderViewFieldNameOrderCosts = "OrderCosts";
	public static readonly string OrderViewFieldTypeOrderCosts = "double";
	public static readonly string OrderViewFieldNameDefSupplierCurrencyId = "DefaultCurrencyIdSupplier";
	public static readonly string OrderViewFieldTypeDefSupplierCurrencyId = "int";
	public static readonly string OrderViewFieldNameDefSupplierCurrencySymbol = "DefaultCurrencySymboldSupplier";
	public static readonly string OrderViewFieldTypeDefSupplierCurrencySymbol = "string";
	public static readonly string OrderViewFieldNameDefSupplierConversionRatel = "DefaultConversionRateSupplier";
	public static readonly string OrderViewFieldTypeDefSupplierConversionRatel = "Double";
	public static readonly string OrderViewFieldNameCurrencyId = "Currency_Id";
	public static readonly string OrderViewFieldTypeCurrencyId = "int";
	public static readonly string OrderViewFieldNameDefOrderCurrencySymbol = "DefaultCurrencySymboldOrder";
	public static readonly string OrderViewFieldTypeDefOrderCurrencySymbol = "string";
	public static readonly string OrderViewFieldNameDefOrderConversionRatel = "DefaultConversionRateOrder";
	public static readonly string OrderViewFieldTypeDefOrderConversionRatel = "Double";
	public static readonly string OrderViewFieldNameOrderNumber = "OrderNumber";
	public static readonly string OrderViewFieldTypeOrderNumber = "string";
	public static readonly string OrderViewFieldNameOrderDate = "OrderDate";
	public static readonly string OrderViewFieldTypeOrderDate = "date";
	public static readonly string OrderViewFieldNameCurrencySymbol = "OrderCurrencySymbol";
	public static readonly string OrderViewFieldTypeCurrencySymbol = "string";
	public static readonly string OrderViewFieldNameConversionRate = "OrderCurrencyConversionRate";
	public static readonly string OrderViewFieldTypeConversionRate = "double";
	public static readonly string OrderViewFieldNameOrderShippingCosts = "OrderShippingCosts";
	public static readonly string OrderViewFieldTypeOrderShippingCosts = "double";
	public static readonly string OrderViewFieldNameOrderOrderCosts = "OrderOrderCosts";
	public static readonly string OrderViewFieldTypeOrderOrderCosts = "double";
	public static readonly string OrderViewFieldNameClosed = "Closed";
	public static readonly string OrderViewFieldTypeClosed = "int";
	public static readonly string OrderViewFieldNameClosedDate = "ClosedDate";
	public static readonly string OrderViewFieldTypeClosedDate = "date";
	public static readonly string OrderViewFieldNameOrderMemo = "Memo";
	public static readonly string OrderViewFieldTypeOrderMemo = "longtext";
	public static readonly string OrderViewFieldNameHasStackLog = "HasStackLog";
	public static readonly string OrderViewFieldTypeHasStackLog = "int";

	public static readonly string OrderLineTable = "supplyorderline";
	public static readonly string OpenOrderLineView = "view_supplyopenorderline";
	public static readonly string OrderLineFieldNameId = "Id";
	public static readonly string OrderLineFieldTypeId = "int";
	public static readonly string OrderLineFieldNameSupplierOrderId = "SupplyOrder_Id";
	public static readonly string OrderLineFieldTypeSupplierOrderId = "int";
	public static readonly string OrderLineFieldNameSupplierId = "Supplier_Id";
	public static readonly string OrderLineFieldTypeSupplierId = "int";
	public static readonly string OrderLineFieldNameProductId = "Product_Id";
	public static readonly string OrderLineFieldTypeProductId = "int";
	public static readonly string OrderLineFieldNameProjectId = "Project_Id";
	public static readonly string OrderLineFieldTypeProjectId = "int";
	public static readonly string OrderLineFieldNameCategoryId = "Category_Id";
	public static readonly string OrderLineFieldTypeCategoryId = "int";
	public static readonly string OrderLineFieldNameSupplierProductName = "SupplierProductName";
	public static readonly string OrderLineFieldTypeSupplierProductName = "string";
	public static readonly string OrderLineFieldNameOrderId = "supplyorder_Id";
	public static readonly string OrderLineFieldTypeOrderId = "int";
	public static readonly string OrderLineFieldNameAmount = "Amount";
	public static readonly string OrderLineFieldTypeAmount = "double";
	public static readonly string OrderLineFieldNameOpenAmount = "OpenAmount";
	public static readonly string OrderLineFieldTypeOpenAmount = "double";
	public static readonly string OrderLineFieldNamePrice = "Price";
	public static readonly string OrderLineFieldTypePrice = "double";
	public static readonly string OrderLineFieldNameRealRowTotal = "RealRowTotal";
	public static readonly string OrderLineFieldTypeRealRowTotal = "double";
	public static readonly string OrderLineFieldNameClosed = "Closed";
	public static readonly string OrderLineFieldTypeClosed = "int";
	public static readonly string OrderLineFieldNameClosedDate = "ClosedDate";
	public static readonly string OrderLineFieldTypeClosedDate = "date";

	public static readonly string OpenOrderLineFieldNameSupplyOrderId = "Supplyorder_Id";
	public static readonly string OpenOrderLineFieldTypeSupplyOrderId = "int";

	public static readonly string OrderLineShortView = "view_supplyorderlineshort";
	public static readonly string OrderLineShortFieldNameId = "Id";
	public static readonly string OrderLineShortFieldTypeId = "int";
	public static readonly string OrderLineShortFieldNameProductId = "Product_Id";
	public static readonly string OrderLineShortFieldTypeProductId = "int";
	public static readonly string OrderLineShortFieldNameOrderId = "supplyorder_Id";
	public static readonly string OrderLineShortFieldTypeOrderId = "int";
	public static readonly string OrderLineShortFieldNameAmount = "Amount";
	public static readonly string OrderLineShortFieldTypeAmount = "double";
	public static readonly string OrderLineShortFieldNamePrice = "Price";
	public static readonly string OrderLineShortFieldTypePrice = "double";

	public static readonly string OrderLineView = "view_supplyorderline";
	public static readonly string OrderLineViewFieldNameOrderId = "Order_Id";
	public static readonly string OrderLineViewFieldTypeOrderId = "int";
	public static readonly string OrderLineViewFieldNameOrderNumber = "Ordernumber";
	public static readonly string OrderLineViewFieldTypeOrderNumber = "string";
	public static readonly string OrderLineViewFieldNameOrderDate = "OrderDate";
	public static readonly string OrderLineViewFieldTypeOrderDate = "date";
	public static readonly string OrderLineViewFieldNameSupplierId = "SupplierId";
	public static readonly string OrderLineViewFieldTypeSupplierId = "int";
	public static readonly string OrderLineViewFieldNameSupplierName = "Supplier";
	public static readonly string OrderLineViewFieldTypeSupplierName = "string";
	public static readonly string OrderLineViewFieldNameProductId = "ProductId";
	public static readonly string OrderLineViewFieldTypeProductId = "int";
	public static readonly string OrderLineViewFieldNameProductCode = "ProductCode";
	public static readonly string OrderLineViewFieldTypeProductCode = "string";
	public static readonly string OrderLineViewFieldNameProductName = "ProductName";
	public static readonly string OrderLineViewFieldTypeProductName = "string";
	public static readonly string OrderLineViewFieldNameUnitPrice = "UnitPrice";
	public static readonly string OrderLineViewFieldTypeUnitPrice = "double";
	public static readonly string OrderLineViewFieldNameOrdered = "Ordered";
	public static readonly string OrderLineViewFieldTypeOrdered = "double";
	public static readonly string OrderLineViewFieldNameReceived = "Received";
	public static readonly string OrderLineViewFieldTypeReceived = "double";
	public static readonly string OrderLineViewFieldNameExpected = "Expect";
	public static readonly string OrderLineViewFieldTypeExpected = "double";
	public static readonly string OrderLineViewFieldNameClosed = "Closed";
	public static readonly string OrderLineViewFieldTypeClosed = "int";
	public static readonly string OrderLineViewFieldNameClosedDate = "ClosedDate";
	public static readonly string OrderLineViewFieldTypeClosedDate = "date";

	public static readonly string ReceiptsReportView = "view_receiptsreport";
	public static readonly string ReceiptsReportFieldNameOrderDate = "OrderDate";
	public static readonly string ReceiptsReportFieldTypeOrderDate = "date";
	public static readonly string ReceiptsReportFieldNameOrderNumber = "OrderNumber";
	public static readonly string ReceiptsReportFieldTypeOrderNumber = "string";
	public static readonly string ReceiptsReportFieldNameSupplier = "Supplier";
	public static readonly string ReceiptsReportFieldTypeSupplier = "string";
	public static readonly string ReceiptsReportFieldNameShortName = "Shortname";
	public static readonly string ReceiptsReportFieldTypeShortName = "string";
	public static readonly string ReceiptsReportFieldNameDescription = "Description";
	public static readonly string ReceiptsReportFieldTypeDescription = "string";
	public static readonly string ReceiptsReportFieldNameOrderedAmount = "Ordered";
	public static readonly string ReceiptsReportFieldTypeOrderedAmount = "double";
	public static readonly string ReceiptsReportFieldNameReceivedDate = "ReceivedDate";
	public static readonly string ReceiptsReportFieldTypeReceivedDate = "date";
	public static readonly string ReceiptsReportFieldNameReceivedAmount = "Received";
	public static readonly string ReceiptsReportFieldTypeReceivedAmount = "double";
	public static readonly string ReceiptsReportFieldNameIsOrderLine = "IsOrderLine";
	public static readonly string ReceiptsReportFieldTypeIsOrderLine = "int";
	#endregion

	#region Unit Table

	public static readonly string UnitTable = "unit";
	public static readonly string UnitFieldNameUnitId = "Id";
	public static readonly string UnitFieldTypeUnitId = "int";
	public static readonly string UnitFieldNameUnitName = "Name";
	public static readonly string UnitFieldTypeUnitName = "string";

	#endregion Unit Table

	#region Worktype Table and view
	public static readonly string WorktypeTable = "worktype";
	public static readonly string WorktypeFieldNameId = "Id";
	public static readonly string WorktypeFieldTypeId = "int";
	public static readonly string WorktypeFieldNameParentId = "ParentId";
	public static readonly string WorktypeFieldTypeParentId = "int";
	public static readonly string WorktypeFieldNameName = "Name";
	public static readonly string WorktypeFieldTypeName = "string";
	public static readonly string WorktypeFieldNameFullpath = "FullPath";
	public static readonly string WorktypeFieldTypeFullpath = "string";

	public static readonly string WorktypeGroupTotalsView = "view_groupedtimetotals";
	public static readonly string WorktypeGroupTotalsNameProjectId = "ProjectId";
	public static readonly string WorktypeGroupTotalsTypeProjectId = "int";
	public static readonly string WorktypeGroupTotalsNameProjectName = "ProjectName";
	public static readonly string WorktypeGroupTotalsTypeProjectName = "string";
	public static readonly string WorktypeGroupTotalsNameWorktypeParentId = "WorktypeParentId";
	public static readonly string WorktypeGroupTotalsTypeWorktypeParentId = "int";
	public static readonly string WorktypeGroupTotalsNameWorktypeId = "WorktypeId";
	public static readonly string WorktypeGroupTotalsTypeWorktypeId = "int";
	public static readonly string WorktypeGroupTotalsNameWorktypeName = "WorktypeName";
	public static readonly string WorktypeGroupTotalsTypeWorktypeName = "string";
	public static readonly string WorktypeGroupTotalsNameMinutes = "TotalElapsedMinutes";
	public static readonly string WorktypeGroupTotalsTypeMinutes = "decimal";
	public static readonly string WorktypeGroupTotalsNameHours = "TotalElapsedHours";
	public static readonly string WorktypeGroupTotalsTypeHours = "time";
	public static readonly string WorktypeGroupTotalsNameSortOrder = "SortOrder";
	public static readonly string WorktypeGroupTotalsTypeSortOrder = "bigint";
	#endregion
}