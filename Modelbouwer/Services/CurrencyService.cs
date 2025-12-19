using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Services;

public class CurrencyService( GenericDataService dataService )
{
	private readonly GenericDataService _dataService = dataService;
	public bool CurrencyUsed { get; set; } = false;

	#region Database query's
	public string CompleteCurrencyList = $"" +
		$"SELECT " +
		$"{DBNames.CurrencyFieldNameId} AS {DBNames.CurrencyFieldNameId}, " +
		$"{DBNames.CurrencyFieldNameCode} AS {DBNames.CurrencyFieldNameCode}, " +
		$"{DBNames.CurrencyFieldNameSymbol} AS {DBNames.CurrencyFieldNameSymbol}, " +
		$"{DBNames.CurrencyFieldNameName} AS {DBNames.CurrencyFieldNameName} " +
		$"{DBNames.CurrencyFieldNameRate} AS {DBNames.CurrencyFieldNameRate}" +
		$"FROM {DBNames.Database}.{DBNames.CurrencyTable};";

	public string CurrencyUsedQuery = $"SELECT COUNT({DBNames.CountryFieldNameCurrencyId}) FROM {DBNames.Database}.{DBNames.CountryTable} WHERE {DBNames.CountryFieldNameCurrencyId} = @CurrencyId";
	#endregion

	public Task<List<CurrencyModel>> GetAllCurrenciesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteCurrencyList, reader =>
		{
			return new CurrencyModel
			{
				CurrencyId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CurrencyFieldNameId}" ] ),
				CurrencyCode = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameCode}" ] ),
				CurrencyName = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameName}" ] ),
				CurrencyConversionRate = DatabaseValueConverter.GetInt( reader [ $"{DBNames.CurrencyFieldNameRate}" ] ),
				CurrencySymbol = DatabaseValueConverter.GetString( reader [ $"{DBNames.CurrencyFieldNameSymbol}" ] )
			};
		} );
	}
}
