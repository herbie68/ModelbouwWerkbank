namespace Modelbouwer.Models;

public partial class ProjectWorkStats : ObservableObject
{
	[ObservableProperty] public DateTime _startDate;

	[ObservableProperty] public double _totalHours;
}
