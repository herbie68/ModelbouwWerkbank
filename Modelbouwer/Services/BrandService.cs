using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Services;

public class BrandService : IBrandService
{
	private readonly GenericDataService _dataService;
	public bool BrandUsed { get; set; } = false;

	public BrandService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteBrandList = $"" +
		$"SELECT " +
		$"{DBNames.BrandFieldNameId} AS {DBNames.BrandFieldNameId}, " +
		$"{DBNames.BrandFieldNameName} AS {DBNames.BrandFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.BrandTable};";

	public string AddNewBrandQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.BrandTable} " +
		$"({DBNames.BrandFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.BrandFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateBrandQuery =
		$"UPDATE {DBNames.Database}.{DBNames.BrandTable} " +
		$"SET " +
		$"{DBNames.BrandFieldNameName} = @{DBNames.BrandFieldNameName}" +
		$"WHERE {DBNames.BrandFieldNameId} = @{DBNames.BrandFieldNameId};";

	public string DeleteBrandQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.BrandTable} " +
		$"WHERE {DBNames.BrandFieldNameId} = @{DBNames.BrandFieldNameId};";

	public string BrandNameExistsQuery =
		$"SELECT COUNT({DBNames.BrandFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.BrandTable} " +
		$"WHERE {DBNames.BrandFieldNameName} = @{DBNames.BrandFieldNameName}";

	public string BrandUsedQuery = $"SELECT COUNT({DBNames.ProductFieldNameBrandId}) FROM {DBNames.Database}.{DBNames.ProductTable} WHERE {DBNames.ProductFieldNameBrandId} = @BrandId";
	#endregion

	public Task<List<BrandModel>> GetAllBrandsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteBrandList, reader =>
		{
			return new BrandModel
			{
				BrandId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.BrandFieldNameId}" ] ),
				BrandName = DatabaseValueConverter.GetString( reader [ $"{DBNames.BrandFieldNameName}" ] )
			};
		} );
	}

	public async Task<int> InsertNewBrandAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.BrandFieldNameName}", queryParameters[$"@{DBNames.BrandFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewBrandQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateBrandAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.BrandFieldNameId}", queryParameters[$"@{DBNames.BrandFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.BrandFieldNameName}", queryParameters[$"@{DBNames.BrandFieldNameName}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateBrandQuery, parameters );
	}

	public async Task DeleteBrandAsync( int brandId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.BrandFieldNameId}", brandId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteBrandQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataBrandDeleteError}." );
		}
	}

	public async Task<bool> IsBrandUsedAsync( int brandId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ProductFieldNameBrandId}", brandId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			BrandUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? brandName )
	{
		if ( string.IsNullOrWhiteSpace( brandName ) )
			return false;

		var brands = await GetAllBrandsAsync();

		return brands.Any( c =>
			string.Equals( c.BrandName, brandName, StringComparison.OrdinalIgnoreCase ) );
	}
}
