namespace Modelbouwer.Models;

public class ReleaseCommitModel
{
	public string Sha { get; set; } = string.Empty;
	public DateTimeOffset Date { get; set; }
	public string DateText { get; set; } = string.Empty;
	public string Author { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string? FullMessage { get; set; }
}
