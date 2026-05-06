namespace Modelbouwer.Models;

public partial class CostReportItemModel : ObservableObject
{
	[ObservableProperty] private string _name = string.Empty;
	[ObservableProperty] private double _totalCosts;
	[ObservableProperty] private double _percentage;
}
