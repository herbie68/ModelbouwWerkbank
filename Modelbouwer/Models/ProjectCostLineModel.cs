namespace Modelbouwer.Models;

public partial class ProjectCostLineModel : ObservableObject
{
	[ObservableProperty] private string? _description;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _unitPrice;
	[ObservableProperty] private double _totalCosts;
	[ObservableProperty] private string? _groupName;
}