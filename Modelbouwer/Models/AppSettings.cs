namespace Modelbouwer.Models;

public class AppSettings
{
	public string Culture { get; set; } = "nl-NL";
	public string Language { get; set; } = "NL";
	public double HourRate { get; set; } = 15.00;
	public string ExportFolder { get; set; } = "C:\\";
	public string StockManagementGridLayout { get; set; } = "";

}