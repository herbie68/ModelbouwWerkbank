using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Interfaces;

public interface IWorkTypeService
{
	Task<List<WorkTypeModel>> GetAllWorkTypesAsync();
	Task<int> InsertNewWorkTypeAsync( Dictionary<string, object?> queryParameters );
	Task UpdateWorkTypeAsync( Dictionary<string, object?> queryParameters );
	Task DeleteWorkTypeAsync( int worktypeId );
	Task<bool> IsWorkTypeUsedAsync( int worktypeId );
	Task<bool> NameExistsAsync( string? worktypeName, int? parentId );
}
