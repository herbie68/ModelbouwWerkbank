namespace Modelbouwer.Interfaces;

public interface ITimeRegistrationService
{
	Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId );
	Task<int> InsertTimeEntryAsync( TimeEntryModel entry );
	Task UpdateTimeEntryAsync( TimeEntryModel entry );
	Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId );
	Task<int> InsertMaterialUsageAsync( int projectId, ProductModel product, double amount, DateTime usageDate, string? comment );
	Task<double> GetHourRateAsync();
}
