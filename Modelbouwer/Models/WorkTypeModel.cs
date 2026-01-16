using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Models;

public class WorkTypeModel
{
	public int WorkTypeId { get; set; }
	public int? ParentId { get; set; }
	public string WorkTypeName { get; set; } = string.Empty;

	public ObservableCollection<WorkTypeModel> Children { get; set; } = [ ];

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(WorkTypeId)] = [ "ID" ],
		[nameof(ParentId)] = [ "Parent" ],

		[nameof(WorkTypeName)] = [
			"Worksoort",
			"WorkType",
			"Arbeitsart" ]
	};
}
