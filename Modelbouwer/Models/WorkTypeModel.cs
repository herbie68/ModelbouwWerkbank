namespace Modelbouwer.Models;

public partial class WorktypeModel : ObservableObject
{
	[ObservableProperty] public int _worktypeId;
	[ObservableProperty] public int? _parentId;
	[ObservableProperty] public string _worktypeName = string.Empty;

	[ObservableProperty] public ObservableCollection<WorktypeModel> _children = [ ];

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