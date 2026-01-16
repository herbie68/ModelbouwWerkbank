using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Services;

    public class ContactTypeService: IContactTypeService
    {
	private readonly GenericDataService _dataService;
	public bool ContactTypeUsed { get; set; } = false;

	public ContactTypeService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	#region Database query's
	public string CompleteContactTypeList = $"" +
		$"SELECT " +
		$"{DBNames.ContactTypeFieldNameId} AS {DBNames.ContactTypeFieldNameId}, " +
		$"{DBNames.ContactTypeFieldNameName} AS {DBNames.ContactTypeFieldNameName}" +
		$" FROM {DBNames.Database}.{DBNames.ContactTypeTable};";

	public string AddNewContactTypeQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.ContactTypeTable} " +
		$"({DBNames.ContactTypeFieldNameName}) " +
		$"VALUES " +
		$"(@{DBNames.ContactTypeFieldNameName});" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateContactTypeQuery =
		$"UPDATE {DBNames.Database}.{DBNames.ContactTypeTable} " +
		$"SET " +
		$"{DBNames.ContactTypeFieldNameName} = @{DBNames.ContactTypeFieldNameName}" +
		$"WHERE {DBNames.ContactTypeFieldNameId} = @{DBNames.ContactTypeFieldNameId};";

	public string DeleteContactTypeQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.ContactTypeTable} " +
		$"WHERE {DBNames.ContactTypeFieldNameId} = @{DBNames.ContactTypeFieldNameId};";

	public string ContactTypeNameExistsQuery =
		$"SELECT COUNT({DBNames.ContactTypeFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.ContactTypeTable} " +
		$"WHERE {DBNames.ContactTypeFieldNameName} = @{DBNames.ContactTypeFieldNameName}";

	public string ContactTypeUsedQuery = $"SELECT COUNT({DBNames.SupplierContactFieldNameTypeId}) FROM {DBNames.Database}.{DBNames.SupplierTable} WHERE {DBNames.SupplierContactFieldNameTypeId} = @ContactTypeId";
	#endregion

	public Task<List<ContactTypeModel>> GetAllContactTypesAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteContactTypeList, reader =>
		{
			return new ContactTypeModel
			{
				ContactTypeId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.ContactTypeFieldNameId}" ] ),
				ContactTypeName = DatabaseValueConverter.GetString( reader [ $"{DBNames.ContactTypeFieldNameName}" ] )
			};
		} );
	}

	public async Task<int> InsertNewContactTypeAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ContactTypeFieldNameName}", queryParameters[$"@{DBNames.ContactTypeFieldNameName}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewContactTypeQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateContactTypeAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"@{DBNames.ContactTypeFieldNameId}", queryParameters[$"@{DBNames.ContactTypeFieldNameId}"] ?? DBNull.Value },
			{ $"@{DBNames.ContactTypeFieldNameName}", queryParameters[$"@{DBNames.ContactTypeFieldNameName}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateContactTypeQuery, parameters );
	}

	public async Task DeleteContactTypeAsync( int contacttypeId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.ContactTypeFieldNameId}", contacttypeId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteContactTypeQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataContactTypeDeleteError}." );
		}
	}

	public async Task<bool> IsContactTypeUsedAsync( int contacttypeId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.SupplierContactFieldNameTypeId}", contacttypeId }
		};

		var usedCount = await _dataService.ExecuteScalarAsync<int>(
			ContactTypeUsedQuery,
			parameters);

		return usedCount > 0;
	}

	public async Task<bool> NameExistsAsync( string? contacttypeName )
	{
		if ( string.IsNullOrWhiteSpace( contacttypeName ) )
			return false;

		var contacttypes = await GetAllContactTypesAsync();

		return contacttypes.Any( c =>
			string.Equals( c.ContactTypeName, contacttypeName, StringComparison.OrdinalIgnoreCase ) );
	}
}
