namespace Modelbouwer.Models;

public partial class CostDeclarationReportItemModel : ObservableObject
{
	[ObservableProperty] private DateTime _usageDate;
	[ObservableProperty] private string _productName = string.Empty;
	[ObservableProperty] private string _categoryName = string.Empty;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _unitPrice;
	[ObservableProperty] private double _totalCosts;
	[ObservableProperty] private string? _comment;
}