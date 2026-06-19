using CommunityToolkit.Mvvm.ComponentModel;

namespace Modelbouwer.Mobile.Models;

public partial class MobileProject : ObservableObject
{
	[ObservableProperty] private int id;
	[ObservableProperty] private string code = string.Empty;
	[ObservableProperty] private string name = string.Empty;
	[ObservableProperty] private DateTime startDate = DateTime.Today;
	[ObservableProperty] private DateTime? endDate;
	[ObservableProperty] private bool isClosed;
}

public partial class MobileProduct : ObservableObject
{
	[ObservableProperty] private int id;
	[ObservableProperty] private int categoryId;
	[ObservableProperty] private int unitId;
	[ObservableProperty] private string code = string.Empty;
	[ObservableProperty] private string name = string.Empty;
	[ObservableProperty] private string category = string.Empty;
	[ObservableProperty] private string unit = "st";
	[ObservableProperty] private double currentInventory;
	[ObservableProperty] private double minimalStock;
	[ObservableProperty] private double price;
}

public partial class MobileTimeEntry : ObservableObject
{
	[ObservableProperty] private int id;
	[ObservableProperty] private MobileProject? project;
	[ObservableProperty] private MobileWorkType? workTypeItem;
	[ObservableProperty] private DateTime workDate = DateTime.Today;
	[ObservableProperty] private TimeSpan startTime = new(9, 0, 0);
	[ObservableProperty] private TimeSpan endTime = new(10, 0, 0);
	[ObservableProperty] private string workType = "Bouwen";
	[ObservableProperty] private string comment = string.Empty;

	public double WorkedHours => EndTime > StartTime ? Math.Round( ( EndTime - StartTime ).TotalHours, 2 ) : 0;
}

public partial class MobileTimerSession : ObservableObject
{
	[ObservableProperty] private MobileProject? project;
	[ObservableProperty] private MobileWorkType? workTypeItem;
	[ObservableProperty] private DateTime workDate = DateTime.Today;
	[ObservableProperty] private TimeSpan startTime = DateTime.Now.TimeOfDay;
	[ObservableProperty] private string comment = string.Empty;
}

public sealed class MobileWorkType
{
	public int Id { get; set; }
	public int? ParentId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string DisplayName { get; set; } = string.Empty;
}

public sealed class MobileCategory
{
	public int Id { get; set; }
	public int? ParentId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string DisplayName { get; set; } = string.Empty;
}

public sealed class MobileUnit
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}

public partial class MobileMaterialEntry : ObservableObject
{
	[ObservableProperty] private int id;
	[ObservableProperty] private MobileProject? project;
	[ObservableProperty] private MobileProduct? product;
	[ObservableProperty] private DateTime usageDate = DateTime.Today;
	[ObservableProperty] private double amount = 1;
	[ObservableProperty] private double price;
	[ObservableProperty] private string comment = string.Empty;

	public double Costs => Math.Round( Amount * Price, 2 );
}