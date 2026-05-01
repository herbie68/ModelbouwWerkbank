using Modelbouwer.Model;

namespace Modelbouwer.Services;

public class SupplierService : ISupplierService
{
	private readonly GenericDataService _dataService;
	public bool SupplierUsed { get; set; } = false;

	public SupplierService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteSupplierList = $"" +
		$"SELECT " +
		$"{ DBNames.SupplierFieldNameId}, " +
		$"{ DBNames.SupplierFieldNameCode}, " +
		$"{ DBNames.SupplierFieldNameName}, " +
		$"{ DBNames.SupplierFieldNameAddress1}, " +
		$"{ DBNames.SupplierFieldNameAddress2}, " +
		$"{ DBNames.SupplierFieldNameZip}, " +
		$"{ DBNames.SupplierFieldNameCity}, " +
		$"{ DBNames.SupplierFieldNameUrl}, " +
		$"{ DBNames.SupplierFieldNameShippingCosts}, " +
		$"{ DBNames.SupplierFieldNameMinShippingCosts}, " +
		$"{ DBNames.SupplierFieldNameOrderCosts}, " +
		$"{ DBNames.SupplierFieldNameMinOrderCosts}, " +
		$"{ DBNames.SupplierFieldNameCurrencyId}, " +
		$"{ DBNames.SupplierFieldNameCountryId}, " +
		$"{ DBNames.SupplierFieldNameGeneralEmail}, " +
		$"{ DBNames.SupplierFieldNameGeneralPhone}, " +
		$"{ DBNames.SupplierFieldNameMemo}" +
		$" FROM {DBNames.Database}.{DBNames.SupplierTable};";

	public string CompleteProductSupplierList = $"" +
		$"SELECT " +
		$"ps.{DBNames.ProductSupplierFieldNameId}, " +
		$"ps.{DBNames.ProductSupplierFieldNameProductId}, " +
		$"ps.{DBNames.ProductSupplierFieldNameSupplierId}, " +
		$"s.{DBNames.SupplierFieldNameName}, " +
		$"s.{DBNames.SupplierFieldNameCurrencyId} AS {DBNames.SupplierFieldNameCurrencyId}, " +
		$"c.{DBNames.CurrencyFieldNameSymbol}, " +
		$"ps.{DBNames.ProductSupplierFieldNameProductNumber}, " +
		$"(CASE ps.{DBNames.ProductSupplierFieldNameProductName} WHEN '' THEN p.{DBNames.ProductFieldNameName} ELSE ps.{DBNames.ProductSupplierFieldNameProductName} END) AS ProductName, " +
		$"ps.{DBNames.ProductSupplierFieldNamePrice}, " +
		$"ps.{DBNames.ProductSupplierFieldNameProductUrl} " +
		$"FROM {DBNames.Database}.{DBNames.ProductSupplierTable} ps " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.ProductTable} p ON ps.{DBNames.ProductSupplierFieldNameProductId} = p.{DBNames.ProductFieldNameId} " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.SupplierTable} s ON ps.{DBNames.ProductSupplierFieldNameSupplierId} = s.{DBNames.SupplierFieldNameId} " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.CurrencyTable} c ON s.{DBNames.SupplierFieldNameCurrencyId} = c.{DBNames.CurrencyFieldNameId};";

	public string ProductSupplierBySupplierAndProductQuery = $"" +
		$"SELECT " +
		$"ps.{DBNames.ProductSupplierFieldNameId}, " +
		$"ps.{DBNames.ProductSupplierFieldNameProductId}, " +
		$"ps.{DBNames.ProductSupplierFieldNameSupplierId}, " +
		$"s.{DBNames.SupplierFieldNameName}, " +
		$"s.{DBNames.SupplierFieldNameCurrencyId} AS {DBNames.SupplierFieldNameCurrencyId}, " +
		$"c.{DBNames.CurrencyFieldNameSymbol}, " +
		$"ps.{DBNames.ProductSupplierFieldNameProductNumber}, " +
		$"(CASE ps.{DBNames.ProductSupplierFieldNameProductName} WHEN '' THEN p.{DBNames.ProductFieldNameName} ELSE ps.{DBNames.ProductSupplierFieldNameProductName} END) AS ProductName, " +
		$"ps.{DBNames.ProductSupplierFieldNamePrice}, " +
		$"ps.{DBNames.ProductSupplierFieldNameProductUrl} " +
		$"FROM {DBNames.Database}.{DBNames.ProductSupplierTable} ps " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.ProductTable} p ON ps.{DBNames.ProductSupplierFieldNameProductId} = p.{DBNames.ProductFieldNameId} " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.SupplierTable} s ON ps.{DBNames.ProductSupplierFieldNameSupplierId} = s.{DBNames.SupplierFieldNameId} " +
		$"LEFT JOIN {DBNames.Database}.{DBNames.CurrencyTable} c ON s.{DBNames.SupplierFieldNameCurrencyId} = c.{DBNames.CurrencyFieldNameId} " +
		$"WHERE ps.{DBNames.ProductSupplierFieldNameSupplierId} = @SupplierId " +
		$"AND ps.{DBNames.ProductSupplierFieldNameProductId} = @ProductId " +
		$"LIMIT 1;";

	public string InsertProductSupplierQuery = $"" +
		$"INSERT INTO {DBNames.Database}.{DBNames.ProductSupplierTable} ( " +
		$"{DBNames.ProductSupplierFieldNameProductId}, " +
		$"{DBNames.ProductSupplierFieldNameSupplierId}, " +
		$"{DBNames.ProductSupplierFieldNameProductNumber}, " +
		$"{DBNames.ProductSupplierFieldNameProductName}, " +
		$"{DBNames.ProductSupplierFieldNamePrice}, " +
		$"{DBNames.ProductSupplierFieldNameProductUrl} ) " +
		$"VALUES ( " +
		$"@{DBNames.ProductSupplierFieldNameProductId}, " +
		$"@{DBNames.ProductSupplierFieldNameSupplierId}, " +
		$"@{DBNames.ProductSupplierFieldNameProductNumber}, " +
		$"@{DBNames.ProductSupplierFieldNameProductName}, " +
		$"@{DBNames.ProductSupplierFieldNamePrice}, " +
		$"@{DBNames.ProductSupplierFieldNameProductUrl} );" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateProductSupplierQuery = $"" +
		$"UPDATE {DBNames.Database}.{DBNames.ProductSupplierTable} SET " +
		$"{DBNames.ProductSupplierFieldNameProductId} = @{DBNames.ProductSupplierFieldNameProductId}, " +
		$"{DBNames.ProductSupplierFieldNameSupplierId} = @{DBNames.ProductSupplierFieldNameSupplierId}, " +
		$"{DBNames.ProductSupplierFieldNameProductNumber} = @{DBNames.ProductSupplierFieldNameProductNumber}, " +
		$"{DBNames.ProductSupplierFieldNameProductName} = @{DBNames.ProductSupplierFieldNameProductName}, " +
		$"{DBNames.ProductSupplierFieldNamePrice} = @{DBNames.ProductSupplierFieldNamePrice}, " +
		$"{DBNames.ProductSupplierFieldNameProductUrl} = @{DBNames.ProductSupplierFieldNameProductUrl} " +
		$"WHERE {DBNames.ProductSupplierFieldNameId} = @{DBNames.ProductSupplierFieldNameId};";

	public string CompleteCountryList =
		$"SELECT " +
		$"{DBNames.CountryFieldNameId} AS {DBNames.CountryFieldNameId}, " +
		$"{DBNames.CountryFieldNameCode} AS {DBNames.CountryFieldNameCode}, " +
		$"{DBNames.CountryFieldNameName} AS {DBNames.CountryFieldNameName}, " +
		$"{DBNames.CountryFieldNameCurrencyId} AS {DBNames.CountryFieldNameCurrencyId}, " +
		$"{DBNames.CountryFieldNameCurrencySymbol} AS {DBNames.CountryFieldNameCurrencySymbol} " +
		$"FROM {DBNames.Database}.{DBNames.CountryTable};";

	public string CompleteCurrencyList = $"" +
		$"SELECT " +
		$"{DBNames.CurrencyFieldNameId} AS {DBNames.CurrencyFieldNameId}, " +
		$"{DBNames.CurrencyFieldNameCode} AS {DBNames.CurrencyFieldNameCode}, " +
		$"{DBNames.CurrencyFieldNameSymbol} AS {DBNames.CurrencyFieldNameSymbol}, " +
		$"{DBNames.CurrencyFieldNameName} AS {DBNames.CurrencyFieldNameName}, " +
		$"{DBNames.CurrencyFieldNameRate} AS {DBNames.CurrencyFieldNameRate}" +
		$" FROM {DBNames.Database}.{DBNames.CurrencyTable};";

	public string AddNewSupplierQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.SupplierTable} ( " +
		$"{DBNames.SupplierFieldNameCode}, " +
		$"{DBNames.SupplierFieldNameName}, " +
		$"{DBNames.SupplierFieldNameAddress1}, " +
		$"{DBNames.SupplierFieldNameAddress2}, " +
		$"{DBNames.SupplierFieldNameZip}, " +
		$"{DBNames.SupplierFieldNameCity}, " +
		$"{DBNames.SupplierFieldNameUrl}, " +
		$"{DBNames.SupplierFieldNameShippingCosts}, " +
		$"{DBNames.SupplierFieldNameMinShippingCosts}, " +
		$"{DBNames.SupplierFieldNameOrderCosts}, " +
		$"{DBNames.SupplierFieldNameMinOrderCosts}, " +
		$"{DBNames.SupplierFieldNameCurrencyId}, " +
		$"{DBNames.SupplierFieldNameCountryId}, " +
		$"{DBNames.SupplierFieldNameGeneralEmail}, " +
		$"{DBNames.SupplierFieldNameGeneralPhone}, " +
		$"{DBNames.SupplierFieldNameMemo} ) " +
		$"VALUES ( " +
		$"@{DBNames.SupplierFieldNameCode}, " +
		$"@{DBNames.SupplierFieldNameName}, " +
		$"@{DBNames.SupplierFieldNameAddress1}, " +
		$"@{DBNames.SupplierFieldNameAddress2}, " +
		$"@{DBNames.SupplierFieldNameZip}, " +
		$"@{DBNames.SupplierFieldNameCity}, " +
		$"@{DBNames.SupplierFieldNameUrl}, " +
		$"@{DBNames.SupplierFieldNameShippingCosts}, " +
		$"@{DBNames.SupplierFieldNameMinShippingCosts}, " +
		$"@{DBNames.SupplierFieldNameOrderCosts}, " +
		$"@{DBNames.SupplierFieldNameMinOrderCosts}, " +
		$"@{DBNames.SupplierFieldNameCurrencyId}, " +
		$"@{DBNames.SupplierFieldNameCountryId}, " +
		$"@{DBNames.SupplierFieldNameGeneralEmail}, " +
		$"@{DBNames.SupplierFieldNameGeneralPhone}, " +
		$"@{DBNames.SupplierFieldNameMemo} );" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateSupplierQuery =
		$"UPDATE {DBNames.Database}.{DBNames.SupplierTable} " +
		$"SET " +
		$"{DBNames.SupplierFieldNameCode} = @{DBNames.SupplierFieldNameCode}, " +
		$"{DBNames.SupplierFieldNameName} = @{DBNames.SupplierFieldNameName}, " +
		$"{DBNames.SupplierFieldNameAddress1} = @{DBNames.SupplierFieldNameAddress1}, " +
		$"{DBNames.SupplierFieldNameAddress2} = @{DBNames.SupplierFieldNameAddress2}, " +
		$"{DBNames.SupplierFieldNameZip} = @{DBNames.SupplierFieldNameZip}, " +
		$"{DBNames.SupplierFieldNameCity} = @{DBNames.SupplierFieldNameCity}, " +
		$"{DBNames.SupplierFieldNameUrl} = @{DBNames.SupplierFieldNameUrl}, " +
		$"{DBNames.SupplierFieldNameShippingCosts} = @{DBNames.SupplierFieldNameShippingCosts}, " +
		$"{DBNames.SupplierFieldNameMinShippingCosts} = @{DBNames.SupplierFieldNameMinShippingCosts}, " +
		$"{DBNames.SupplierFieldNameOrderCosts} = @{DBNames.SupplierFieldNameOrderCosts}, " +
		$"{DBNames.SupplierFieldNameMinOrderCosts} = @{DBNames.SupplierFieldNameMinOrderCosts}, " +
		$"{DBNames.SupplierFieldNameCurrencyId} = @{DBNames.SupplierFieldNameCurrencyId}, " +
		$"{DBNames.SupplierFieldNameCountryId} = @{DBNames.SupplierFieldNameCountryId}, " +
		$"{DBNames.SupplierFieldNameGeneralEmail} = @{DBNames.SupplierFieldNameGeneralEmail}, " +
		$"{DBNames.SupplierFieldNameGeneralPhone} = @{DBNames.SupplierFieldNameGeneralPhone}, " +
		$"{DBNames.SupplierFieldNameMemo} = @{DBNames.SupplierFieldNameMemo} " +
		$"WHERE {DBNames.SupplierFieldNameId} = @{DBNames.SupplierFieldNameId};";

	public string DeleteSupplierQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.SupplierTable} " +
		$"WHERE {DBNames.SupplierFieldNameId} = @{DBNames.SupplierFieldNameId};";

	public string SupplierNameExistsQuery =
		$"SELECT COUNT({DBNames.SupplierFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.SupplierTable} " +
		$"WHERE {DBNames.SupplierFieldNameName} = @{DBNames.SupplierFieldNameName}";

	public string SupplierUsedQuery = $"" +
		$"SELECT" +
		$"EXISTS( " +
		$"SELECT 1 " +
		$"FROM {DBNames.Database}.{DBNames.ProductSupplierTable} " +
		$"WHERE {DBNames.ProductSupplierFieldNameSupplierId} = @SupplierId " +
		$"LIMIT 1 ) " +
		$"OR EXISTS( " +
		$"SELECT 1 " +
		$"FROM {DBNames.Database}.{DBNames.OrderTable} " +
		$"WHERE {DBNames.OrderFieldNameSupplierId} = @SupplierId " +
		$"LIMIT 1 ) " +
		$"AS SupplierInUse;";
	#endregion

	public Task<List<SupplierModel>> GetAllSuppliersAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteSupplierList, reader =>
		{
			return new SupplierModel
			{
				Id = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierFieldNameId}" ] ),
				Code = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameCode}" ] ),
				Name = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameName}" ] ),
				Address1 = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameAddress1}" ] ),
				Address2 = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameAddress2}" ] ),
				Zip = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameZip}" ] ),
				City = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameCity}" ] ),
				Url = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameUrl}" ] ),
				ShippingCosts = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.SupplierFieldNameShippingCosts}" ] ),
				MinShippingCosts = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.SupplierFieldNameMinShippingCosts}" ] ),
				OrderCosts = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.SupplierFieldNameOrderCosts}" ] ),
				MinOrderCosts = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.SupplierFieldNameMinOrderCosts}" ] ),
				CurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierFieldNameCurrencyId}" ] ),
				CountryId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierFieldNameCountryId}" ] ),
				Memo = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameMemo}" ] )
			};
		} );
	}

	public Task<List<ProductSupplierModel>> GetAllProductSuppliersAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteProductSupplierList, MapProductSupplier );
	}

	public async Task<ProductSupplierModel?> GetProductSupplierAsync( int supplierId, int productId )
	{
		var results = await _dataService.ExecuteQueryAsync(
			ProductSupplierBySupplierAndProductQuery,
			MapProductSupplier,
			new Dictionary<string, object>
			{
				{ "@SupplierId", supplierId },
				{ "@ProductId", productId }
			} );

		return results.FirstOrDefault();
	}

	public async Task<int> UpsertProductSupplierAsync( ProductSupplierModel productSupplier )
	{
		Dictionary<string, object> parameters = new()
		{
			{ DBNames.ProductSupplierFieldNameProductId, productSupplier.ProductId },
			{ DBNames.ProductSupplierFieldNameSupplierId, productSupplier.SupplierId },
			{ DBNames.ProductSupplierFieldNameProductNumber, productSupplier.ProductNumber ?? string.Empty },
			{ DBNames.ProductSupplierFieldNameProductName, productSupplier.ProductName ?? string.Empty },
			{ DBNames.ProductSupplierFieldNamePrice, productSupplier.Price },
			{ DBNames.ProductSupplierFieldNameProductUrl, productSupplier.URL ?? string.Empty }
		};

		if ( productSupplier.ProductSupplierId > 0 )
		{
			parameters.Add( DBNames.ProductSupplierFieldNameId, productSupplier.ProductSupplierId );
			await _dataService.ExecuteScalarAsync<uint>( UpdateProductSupplierQuery, parameters );
			return productSupplier.ProductSupplierId;
		}

		uint newId = await _dataService.ExecuteScalarAsync<uint>( InsertProductSupplierQuery, parameters );
		return ( int ) newId;
	}

	public Task<List<CountryModel>> GetAllCountriesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteCountryList, reader =>
		{
			return new CountryModel
			{
				CountryId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CountryFieldNameId}" ] ),
				CountryCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.CountryFieldNameCode}" ] ),
				CountryName = DatabaseValueConverter.GetString( reader [ $"{DBNames.CountryFieldNameName}" ] ),
				CountryCurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CountryFieldNameCurrencyId}" ] ),
				CountryCurrencySymbol = DatabaseValueConverter.GetString( reader [ $"{DBNames.CountryFieldNameCurrencySymbol}" ] )
			};
		} );
	}

	public Task<List<CurrencyModel>> GetAllCurrenciesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteCurrencyList, reader =>
		{
			return new CurrencyModel
			{
				CurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CurrencyFieldNameId}" ] ),
				CurrencyCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameCode}" ] ),
				CurrencyName = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameName}" ] ),
				CurrencySymbol = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameSymbol}" ] ),
				CurrencyConversionRate = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.CurrencyFieldNameRate}" ] )
			};
		} );
	}

	public async Task<int> InsertNewSupplierAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"{DBNames.SupplierFieldNameCode}", queryParameters[$"@{DBNames.SupplierFieldNameCode}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameName}", queryParameters[$"@{DBNames.SupplierFieldNameName}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameAddress1}", queryParameters[$"@{DBNames.SupplierFieldNameAddress1}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameAddress2}", queryParameters[$"@{DBNames.SupplierFieldNameAddress2}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameZip}", queryParameters[$"@{DBNames.SupplierFieldNameZip}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameCity}", queryParameters[$"@{DBNames.SupplierFieldNameCity}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameUrl}", queryParameters[$"@{DBNames.SupplierFieldNameUrl}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameShippingCosts}", queryParameters[$"@{DBNames.SupplierFieldNameShippingCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameMinShippingCosts}", queryParameters[$"@{DBNames.SupplierFieldNameMinShippingCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameOrderCosts}" , queryParameters[$"@{DBNames.SupplierFieldNameOrderCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameMinOrderCosts}", queryParameters[$"@{DBNames.SupplierFieldNameMinOrderCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameCurrencyId}", queryParameters[$"@{DBNames.SupplierFieldNameCurrencyId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameCountryId}", queryParameters[$"@{DBNames.SupplierFieldNameCountryId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameGeneralEmail}", queryParameters[$"@{DBNames.SupplierFieldNameGeneralEmail}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameGeneralPhone}", queryParameters[$"@{DBNames.SupplierFieldNameGeneralPhone}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameMemo}", queryParameters[$"@{DBNames.SupplierFieldNameMemo}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewSupplierQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateSupplierAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"{DBNames.SupplierFieldNameId}", queryParameters[$"@{DBNames.SupplierFieldNameId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameName}", queryParameters[$"@{DBNames.SupplierFieldNameName}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameAddress1}", queryParameters[$"@{DBNames.SupplierFieldNameAddress1}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameAddress2}", queryParameters[$"@{DBNames.SupplierFieldNameAddress1}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameZip}", queryParameters[$"@{DBNames.SupplierFieldNameZip}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameCity}", queryParameters[$"@{DBNames.SupplierFieldNameCity}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameUrl}", queryParameters[$"@{DBNames.SupplierFieldNameUrl}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameShippingCosts}", queryParameters[$"@{DBNames.SupplierFieldNameShippingCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameMinShippingCosts}", queryParameters[$"@{DBNames.SupplierFieldNameMinShippingCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameOrderCosts}" , queryParameters[$"@{DBNames.SupplierFieldNameOrderCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameMinOrderCosts}", queryParameters[$"@{DBNames.SupplierFieldNameMinOrderCosts}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameCurrencyId}", queryParameters[$"@{DBNames.SupplierFieldNameCurrencyId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameCountryId}", queryParameters[$"@{DBNames.SupplierFieldNameCountryId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameGeneralEmail}", queryParameters[$"@{DBNames.SupplierFieldNameGeneralEmail}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameGeneralPhone}", queryParameters[$"@{DBNames.SupplierFieldNameGeneralPhone}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameMemo}", queryParameters[$"@{DBNames.SupplierFieldNameMemo}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateSupplierQuery, parameters );
	}

	public async Task DeleteSupplierAsync( int supplierId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.SupplierFieldNameId}", supplierId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteSupplierQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataSupplierDeleteError}." );
		}
	}

	public async Task<bool> IsSupplierUsedAsync( int supplierId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@SupplierId", supplierId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			SupplierUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? supplierName )
	{
		if ( string.IsNullOrWhiteSpace( supplierName ) )
			return false;

		var suppliers = await GetAllSuppliersAsync();

		return suppliers.Any( c =>
			string.Equals( c.Name, supplierName, StringComparison.OrdinalIgnoreCase ) );
	}

	private static ProductSupplierModel MapProductSupplier( System.Data.Common.DbDataReader reader )
	{
		return new ProductSupplierModel
		{
			ProductSupplierId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductSupplierFieldNameId}" ] ),
			ProductId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductSupplierFieldNameProductId}" ] ),
			SupplierId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ProductSupplierFieldNameSupplierId}" ] ),
			SupplierName = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierFieldNameName}" ] ),
			CurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierFieldNameCurrencyId}" ] ),
			CurrencySymbol = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameSymbol}" ] ),
			ProductNumber = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductSupplierFieldNameProductNumber}" ] ),
			ProductName = DatabaseValueConverter.GetString( reader [ "ProductName" ] ),
			Price = DatabaseValueConverter.GetDouble( reader [ $"{DBNames.ProductSupplierFieldNamePrice}" ] ),
			URL = DatabaseValueConverter.GetString( reader [ $"{DBNames.ProductSupplierFieldNameProductUrl}" ] ),
			DefaultSupplier = false
		};
	}

}
