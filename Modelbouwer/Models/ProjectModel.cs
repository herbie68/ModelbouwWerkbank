namespace Modelbouwer.Models;

public class ProjectModel : ObservableObject
{
	public int ProjectId { get; set; }
	public string? ProjectCode { get; set; }
	public string? ProjectName { get; set; }
	public string? ProjectStartDate { get; set; }
	public string? ProjectStartDateStr { get; set; }
	public string? ProjectEndDate { get; set; }
	public string? ProjectEndDateStr { get; set; }
	public string? ProjectExpectedTime { get; set; }
	public byte [ ]? ProjectImage { get; set; }
	public string? ProjectImageRotationAngle { get; set; }
	public bool ProjectClosed { get; set; }
	public string? ProjectMemo { get; set; }
	public string? ProjectExpectedWorkdays { get; set; }
	public string? ProjectExpectedWorkdaysText { get; set; }
	public string? ProjectTodoTime { get; set; }
	public string? ProjectTodoWorkdays { get; set; }
	public string? ProjectTodoWorkdaysText { get; set; }
	public string? ProjectCreated { get; set; }
	public string? ProjectModified { get; set; }
	public string? ProjectSearchField { get; set; }
	public string? ProjectTotalTimeInHours { get; set; }
	public string? ProjectTotalTimeInText { get; set; }
	public string? ProjectShortestWorkday { get; set; }
	public string? ProjectShortestWorkdayHours { get; set; }
	public string? ProjectLongestWorkday { get; set; }
	public string? ProjectLongestWorkdayHours { get; set; }
	public string? ProjectAverageHoursPerDay { get; set; }
	public string? ProjectAverageHoursPerDayLong { get; set; }
	public string? ProjectBuildDays { get; set; }
	public string? ProjectMaterialCosts { get; set; }
	public string? ProjectTimeCosts { get; set; }
	public string? ProjectTotalCosts { get; set; }

	// Define the property that you want to use in TLists (for example in the errorList
	public string Name => ProjectName;

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ProjectFieldNameId, "ProjectId" },
		{ DBNames.ProjectFieldNameCode, "ProjectCode"},
		{ DBNames.ProjectFieldNameName, "ProjectName"},
		{ DBNames.ProjectFieldNameStartDate, "ProjectStartDate"},
		{ DBNames.ProjectFieldNameEndDate, "ProjectEndDate"},
		{ DBNames.ProjectFieldNameExpectedTime, "ProjectExpectedTime"},
		{ DBNames.ProjectFieldNameClosed, "ProjectClosed"},
		{DBNames.ProjectTotalssViewFieldNameAverageHoursPerDay, "ProjectAverageHoursPerDay" }
	};
}