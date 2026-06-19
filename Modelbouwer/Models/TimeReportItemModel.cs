namespace Modelbouwer.Models;

public partial class TimeReportItemModel : ObservableObject
{
	[ObservableProperty] private string _name = string.Empty;
	[ObservableProperty] private string _worktypeGroupName = string.Empty;
	[ObservableProperty] private string _worktypeName = string.Empty;
	[ObservableProperty] private double _hours;
	[ObservableProperty] private double _percentage;
	[ObservableProperty] private int _sortOrder;
}