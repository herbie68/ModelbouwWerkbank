namespace Modelbouwer.Interfaces;

public interface ITimeRegistrationService
{
	Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId );
	Task<List<TimeEntryModel>> GetTimeEntriesByProjectAsync( int projectId, CancellationToken cancellationToken );
	Task<int> InsertTimeEntryAsync( TimeEntryModel entry );
	Task<int> InsertTimeEntryAsync( TimeEntryModel entry, CancellationToken cancellationToken );
	Task UpdateTimeEntryAsync( TimeEntryModel entry );
	Task UpdateTimeEntryAsync( TimeEntryModel entry, CancellationToken cancellationToken );
	Task DeleteTimeEntryAsync( int timeEntryId );
	Task DeleteTimeEntryAsync( int timeEntryId, CancellationToken cancellationToken );
	Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId );
	Task<List<MaterialUsageModel>> GetMaterialUsageByProjectAsync( int projectId, CancellationToken cancellationToken );
	Task<int> InsertMaterialUsageAsync( MaterialUsageModel usage );
	Task<int> InsertMaterialUsageAsync( MaterialUsageModel usage, CancellationToken cancellationToken );
	Task UpdateMaterialUsageAsync( MaterialUsageModel usage );
	Task UpdateMaterialUsageAsync( MaterialUsageModel usage, CancellationToken cancellationToken );
	Task DeleteMaterialUsageAsync( int materialUsageId );
	Task DeleteMaterialUsageAsync( int materialUsageId, CancellationToken cancellationToken );
	Task<double> GetHourRateAsync();
	Task<double> GetHourRateAsync( CancellationToken cancellationToken );
	Task<CultureInfo> GetCultureAsync();
	Task<CultureInfo> GetCultureAsync( CancellationToken cancellationToken );
	Task<List<TimeReportItemModel>> GetWorkedHoursByWeekdayAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByWeekdayAsync( int projectId, CancellationToken cancellationToken );
	Task<List<TimeReportItemModel>> GetWorkedHoursByMonthAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByMonthAsync( int projectId, CancellationToken cancellationToken );
	Task<List<TimeReportItemModel>> GetWorkedHoursByYearAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByYearAsync( int projectId, CancellationToken cancellationToken );
	Task<List<TimeReportItemModel>> GetWorkedHoursByMonthYearAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByMonthYearAsync( int projectId, CancellationToken cancellationToken );
	Task<List<TimeReportItemModel>> GetWorkedHoursByWorktypeAsync( int projectId );
	Task<List<TimeReportItemModel>> GetWorkedHoursByWorktypeAsync( int projectId, CancellationToken cancellationToken );
	Task<List<CostAllocationReportItemModel>> GetCostAllocationByWorktypeAsync( int projectId, bool includeHoursInCosts, double hourRate );
	Task<List<CostAllocationReportItemModel>> GetCostAllocationByWorktypeAsync( int projectId, bool includeHoursInCosts, double hourRate, CancellationToken cancellationToken );
	Task<List<CostDeclarationReportItemModel>> GetCostDeclarationsAsync( int projectId );
	Task<List<CostDeclarationReportItemModel>> GetCostDeclarationsAsync( int projectId, CancellationToken cancellationToken );
	Task<List<CostReportItemModel>> GetCostDeclarationSummaryAsync( int projectId, bool includeHoursInCosts, double hourRate );
	Task<List<CostReportItemModel>> GetCostDeclarationSummaryAsync( int projectId, bool includeHoursInCosts, double hourRate, CancellationToken cancellationToken );
	Task<ProjectReportsDataModel> GetProjectReportsAsync( int projectId, bool includeHoursInCosts, double hourRate );
	Task<ProjectReportsDataModel> GetProjectReportsAsync( int projectId, bool includeHoursInCosts, double hourRate, CancellationToken cancellationToken );
}