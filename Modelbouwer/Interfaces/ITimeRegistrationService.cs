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
	Task<List<TimeReportItemModel>> GetWorkedHoursByWeekdayAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByMonthAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByYearAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByMonthYearAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByWorktypeAsync( int projectId );
}
