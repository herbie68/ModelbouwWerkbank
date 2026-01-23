namespace Modelbouwer.Models;

public class WorktypeModel
{
	public int WorktypeId { get; set; }
	public int? ParentId { get; set; }
	public string WorktypeName { get; set; } = string.Empty;

	public ObservableCollection<WorktypeModel> Children { get; set; } = [ ];

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(WorktypeId)] = [ "ID" ],
		[nameof(ParentId)] = [ "Parent" ],

		[nameof(WorktypeName)] = [
			"Werksoort",
			"WorkType",
			"Arbeitsart" ]
	};
}
