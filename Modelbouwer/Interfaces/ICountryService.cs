namespace Modelbouwer.Interfaces;

public interface ICountryService
{
	Task<bool> CodeExistsAsync( string? countryCode );
	Task<bool> NameExistsAsync( string? countryName );

	Task<List<CountryModel>> GetAllCountriesAsync();
	Task<bool> IsCountryUsedAsync( int countryId );
	Task<int> InsertNewCountryAsync( Dictionary<string, object?> parameters );
	Task DeleteCountryAsync( int countryId );
	Task UpdateCountryAsync( Dictionary<string, object?> parameters );
}
