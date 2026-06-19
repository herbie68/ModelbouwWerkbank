using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Interfaces;

public interface IUnitService
{
	Task<List<UnitModel>> GetAllUnitsAsync();
	Task<int> InsertNewUnitAsync( Dictionary<string, object?> queryParameters );
	Task UpdateUnitAsync( Dictionary<string, object?> queryParameters );
	Task DeleteUnitAsync( int unitId );
	Task<bool> IsUnitUsedAsync( int unitId );
	Task<bool> NameExistsAsync( string? unitName );
}