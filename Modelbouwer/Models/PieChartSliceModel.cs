namespace Modelbouwer.Models;

public partial class PieChartSliceModel : ObservableObject
{
	[ObservableProperty] private string _name = string.Empty;
	[ObservableProperty] private double _hours;
	[ObservableProperty] private double _percentage;
	[ObservableProperty] private Geometry? _sliceGeometry;
	[ObservableProperty] private Geometry? _shadowGeometry;
	[ObservableProperty] private Brush? _fill;
	[ObservableProperty] private Brush? _shadowFill;
}
