namespace Modelbouwer.Models;

public sealed class ProjectReportsDataModel
{
	public List<TimeReportItemModel> WeekdayHours { get; init; } = [];
	public List<TimeReportItemModel> MonthHours { get; init; } = [];
	public List<TimeReportItemModel> YearHours { get; init; } = [];
	public List<TimeReportItemModel> MonthYearHours { get; init; } = [];
	public List<TimeReportItemModel> WorktypeHours { get; init; } = [];
	public List<CostAllocationReportItemModel> CostAllocationLines { get; init; } = [];
	public List<CostDeclarationReportItemModel> CostDeclarationLines { get; init; } = [];
	public List<CostReportItemModel> CostDeclarationSummary { get; init; } = [];
}
