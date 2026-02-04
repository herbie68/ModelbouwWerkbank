namespace Modelbouwer.Models;

public partial class ProjectModel : ObservableObject
{
	private bool _projectClosed;
	public bool ProjectClosed
	{
		get => _projectClosed;
		set
		{
			if ( _projectClosed == value )
				return;

			_projectClosed = value;
			OnPropertyChanged();

			// Notify dependent properties
			OnPropertyChanged( nameof( ProjectDisplayEndDate ) );
			OnPropertyChanged( nameof( ProjectEndDateDisplay ) );
		}
	}

	[ObservableProperty]
	private int _projectId;

	[ObservableProperty]
	private string? _projectCode;

	[ObservableProperty]
	public string? _projectAverageHoursPerDay;

	[ObservableProperty]
	public string? _projectAverageHoursPerDayLong;

	[ObservableProperty]
	public string? _projectBuildDays;

	[ObservableProperty]
	public string? _projectCreated;

	public DateOnly? _projectEndDate;
	public DateOnly? ProjectEndDate
	{
		get => _projectEndDate;
		set
		{
			if ( _projectEndDate == value )
				return;

			_projectEndDate = value;
			OnPropertyChanged();
			OnPropertyChanged( nameof( ProjectDisplayEndDate ) );
			OnPropertyChanged( nameof( ProjectEndDateDisplay ) );
		}
	}

	[ObservableProperty]
	public int? _projectExpectedTime;

	[ObservableProperty]
	public DateOnly? _projectExpectedEndDate;

	[ObservableProperty]
	public string? _projectExpectedWorkdays;

	[ObservableProperty]
	public string? _projectExpectedWorkdaysText;

	[ObservableProperty]
	public string? _projectLongestWorkday;

	[ObservableProperty]
	public string? _projectLongestWorkdayHours;

	[ObservableProperty]
	public string? _projectMaterialCosts;

	[ObservableProperty]
	public string? _projectModified;

	[ObservableProperty]
	public string? _projectName;

	[ObservableProperty]
	public string? _projectSearchField;

	[ObservableProperty]
	public string? _projectShortestWorkday;

	[ObservableProperty]
	public string? _projectShortestWorkdayHours;

	[ObservableProperty]
	public DateOnly? _projectStartDate;

	[ObservableProperty]
	public string? _projectTimeCosts;

	[ObservableProperty]
	public string? _projectTodoTime;

	[ObservableProperty]
	public string? _projectTodoWorkdays;

	[ObservableProperty]
	public string? _projectTodoWorkdaysText;

	[ObservableProperty]
	public string? _projectTotalCosts;

	[ObservableProperty]
	public string? _projectTotalTimeInHours;

	[ObservableProperty]
	public string? _projectTotalTimeInText;

	[ObservableProperty]
	private double _projectImageRotationAngle;

	[ObservableProperty]
	private byte[]? _projectImage;

	[ObservableProperty]
	private string? _projectMemo;

	// Define the property that you want to use in TLists (for example in the errorList
	public string? Name => ProjectName;

	public DateOnly? ProjectDisplayEndDate
	{
		get
		{
			if ( ProjectClosed )
				return ProjectEndDate;

			return ProjectExpectedEndDate;
		}
	}

	public string? ProjectStartDateDisplay => ProjectStartDate?.ToString( "dd-MM-yyyy", CultureInfo.CurrentCulture );

	public string? ProjectEndDateDisplay => ProjectDisplayEndDate?.ToString( "dd-MM-yyyy", CultureInfo.CurrentCulture );

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(ProjectId)] = [ "ID" ],

		[nameof(ProjectCode)] =
		[
			"Zoeknaam",
			"Search name",
			"Suchname" ],

		[nameof(ProjectName)] = [
			"Projectnaam",
			"Project name",
			"Projektname" ],

		[nameof(ProjectStartDate)] = [
			"Start datum",
			"Start date",
			"Startdatum" ],

		[nameof(ProjectEndDate)] = [
			"Eind datum",
			"End date",
			"Endedatum" ],

		[nameof(ProjectClosed)] = [
			"Voltooid",
			"Completed",
			"Vollendet" ]
	};

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
		{
		{ DBNames.ProjectFieldNameId, "ProjectId" },
		{ DBNames.ProjectFieldNameCode, "ProjectCode"},
		{ DBNames.ProjectFieldNameName, "ProjectName"},
		{ DBNames.ProjectFieldNameStartDate, "ProjectStartDate"},
		{ DBNames.ProjectFieldNameEndDate, "ProjectEndDate"},
		{ DBNames.ProjectFieldNameExpectedTime, "ProjectExpectedTime"},
		{ DBNames.ProjectFieldNameImage, "ProjectImage" },
		{ DBNames.ProjectFieldNameImageRotationAngle, "ProjectImageRotationAngle" },
		{ DBNames.ProjectFieldNameMemo, "ProjectMemo" },
		{ DBNames.ProjectFieldNameClosed, "ProjectClosed"},
		};
}