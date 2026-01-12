using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Interfaces;

public interface IBrandService
{
	Task<List<BrandModel>> GetAllBrandsAsync();
	Task<int> InsertNewBrandAsync( Dictionary<string, object?> queryParameters );
	Task UpdateBrandAsync( Dictionary<string, object?> queryParameters );
	Task DeleteBrandAsync( int brandId );
	Task<bool> IsBrandUsedAsync( int brandId );
	Task<bool> NameExistsAsync( string? brandName );
}
