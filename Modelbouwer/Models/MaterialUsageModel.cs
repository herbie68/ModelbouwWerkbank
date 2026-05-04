namespace Modelbouwer.Models;

public partial class MaterialUsageModel : ObservableObject
{
	[ObservableProperty] private int _productUsageId;
	[ObservableProperty] private int _projectId;
	[ObservableProperty] private string? _projectName;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private string? _productName;
	[ObservableProperty] private int _categoryId;
	[ObservableProperty] private string? _categoryName;
	[ObservableProperty] private DateTime _usageDate = DateTime.Today;
	[ObservableProperty] private double _amount;
	[ObservableProperty] private double _price;
	[ObservableProperty] private double _costs;
	[ObservableProperty] private string? _comment;
}
