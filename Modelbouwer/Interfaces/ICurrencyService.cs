namespace Modelbouwer.Interfaces;

public interface ICurrencyService
{
	Task<List<CurrencyModel>> GetAllCurrenciesAsync();
	Task<int> InsertNewCurrencyAsync( Dictionary<string, object?> queryParameters );
	Task UpdateCurrencyAsync( Dictionary<string, object?> queryParameters );
	Task DeleteCurrencyAsync( int currencyId );
	Task<bool> IsCurrencyUsedAsync( int currencyId );
	Task<bool> CodeExistsAsync( string? currencyCode );
	Task<bool> NameExistsAsync( string? currencyName );
}
