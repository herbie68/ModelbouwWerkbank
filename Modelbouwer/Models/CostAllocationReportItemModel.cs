namespace Modelbouwer.Models;

public partial class CostAllocationReportItemModel : ObservableObject
{
	[ObservableProperty] private string _name = string.Empty;
	[ObservableProperty] private string _worktypeGroupName = string.Empty;
	[ObservableProperty] private string _worktypeName = string.Empty;
	[ObservableProperty] private double _hours;
	[ObservableProperty] private double _percentage;
	[ObservableProperty] private double _worktypePercentage;
	[ObservableProperty] private double _materialCosts;
	[ObservableProperty] private double _timeCosts;
	[ObservableProperty] private double _totalCosts;
}