namespace Modelbouwer.Interfaces;

public interface ITimeRegistrationService
{
	Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId );
	Task<int> InsertTimeEntryAsync( TimeEntryModel entry );
	Task UpdateTimeEntryAsync( TimeEntryModel entry );
	Task DeleteTimeEntryAsync( int timeEntryId );
	Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId );
	Task<int> InsertMaterialUsageAsync( MaterialUsageModel usage );
	Task UpdateMaterialUsageAsync( MaterialUsageModel usage );
	Task DeleteMaterialUsageAsync( int materialUsageId );
	Task<double> GetHourRateAsync();
	Task<CultureInfo> GetCultureAsync();
}
