namespace Modelbouwer.Services;

public class ContactService : IContactService
{
	private readonly GenericDataService _dataService;
	public bool ContactUsed { get; set; } = false;

	public ContactService( GenericDataService dataService )
	{
		_dataService = dataService;
	}

	public string CompleteContactList = $"" +
		$"SELECT " +
		$"{ DBNames.SupplierContactFieldNameId}, " +
		$"{ DBNames.SupplierContactFieldNameSupplierId}, " +
		$"{ DBNames.SupplierContactFieldNameTypeId}, " +
		$"{ DBNames.SupplierContactFieldNameName}, " +
		$"{ DBNames.SupplierContactFieldNameMail}, " +
		$"{ DBNames.SupplierContactFieldNamePhone}" +
		$" FROM {DBNames.Database}.{DBNames.SupplierContactTable};";

	public string AddNewContactQuery =
		$"INSERT INTO {DBNames.Database}.{DBNames.SupplierContactTable} ( " +
		$"{ DBNames.SupplierContactFieldNameTypeId}, " +
		$"{ DBNames.SupplierContactFieldNameName}, " +
		$"{ DBNames.SupplierContactFieldNameMail}, " +
		$"{ DBNames.SupplierContactFieldNamePhone}" +
		$"VALUES ( " +
		$"{DBNames.SupplierContactFieldNameTypeId}, " +
		$"{DBNames.SupplierContactFieldNameName}, " +
		$"{DBNames.SupplierContactFieldNameMail}, " +
		$"{DBNames.SupplierContactFieldNamePhone} );" +
		$"{DBNames.SqlSelectLastId}";

	public string UpdateContactQuery =
		$"UPDATE {DBNames.Database}.{DBNames.SupplierContactTable} " +
		$"SET " +
		$"{DBNames.SupplierContactFieldNameTypeId} = @{DBNames.SupplierContactFieldNameTypeId}, " +
		$"{DBNames.SupplierContactFieldNameName} = @{DBNames.SupplierContactFieldNameName}, " +
		$"{DBNames.SupplierContactFieldNameMail} = @{DBNames.SupplierContactFieldNameMail}, " +
		$"{DBNames.SupplierContactFieldNamePhone} = @{DBNames.SupplierContactFieldNamePhone} " +
		$"WHERE {DBNames.SupplierFieldNameId} = @{DBNames.SupplierFieldNameId};";

	public string DeleteContactQuery =
		$"DELETE FROM {DBNames.Database}.{DBNames.SupplierContactTable} " +
		$"WHERE {DBNames.SupplierContactFieldNameId} = @{DBNames.SupplierContactFieldNameId};";

	public string ContactNameExistsQuery =
		$"SELECT COUNT({DBNames.SupplierContactFieldNameId}) " +
		$"FROM {DBNames.Database}.{DBNames.SupplierContactTable} " +
		$"WHERE {DBNames.SupplierContactFieldNameName} = @{DBNames.SupplierContactFieldNameName}";

	public Task<List<SupplierContactModel>> GetAllContactsAsync()
	{
		return _dataService.ExecuteQueryAsync( CompleteContactList, reader =>
		{
			return new SupplierContactModel
			{
				SupplierContactId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierContactFieldNameId}" ] ),
				SupplierId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierContactFieldNameSupplierId}" ] ),
				ContactTypeId = DatabaseValueConverter.GetInt( reader [ $"{DBNames.SupplierContactFieldNameTypeId}" ] ),
				Name = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierContactFieldNameName}" ] ),
				Mail = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierContactFieldNameMail}" ] ),
				Phone = DatabaseValueConverter.GetString( reader [ $"{DBNames.SupplierContactFieldNamePhone}" ] ),
			};
		} );
	}

	public async Task<int> InsertNewContactAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"{DBNames.SupplierContactFieldNameSupplierId}", queryParameters[$"@{DBNames.SupplierContactFieldNameSupplierId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNameTypeId}", queryParameters[$"@{DBNames.SupplierContactFieldNameTypeId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNameName}", queryParameters[$"@{DBNames.SupplierContactFieldNameName}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNameMail}", queryParameters[$"@{DBNames.SupplierContactFieldNameMail}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNamePhone}", queryParameters[$"@{DBNames.SupplierContactFieldNamePhone}"] ?? DBNull.Value }
		};

		uint newId = await _dataService.ExecuteScalarAsync<uint>( AddNewContactQuery, parameters );

		return ( int ) newId;
	}

	public async Task UpdateContactAsync( Dictionary<string, object?> queryParameters )
	{
		Dictionary<string, object> parameters = new()
		{
			{ $"{DBNames.SupplierContactFieldNameId}", queryParameters[$"@{DBNames.SupplierContactFieldNameId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNameSupplierId}", queryParameters[$"@{DBNames.SupplierContactFieldNameSupplierId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNameTypeId}", queryParameters[$"@{DBNames.SupplierContactFieldNameTypeId}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierContactFieldNameName}", queryParameters[$"@{DBNames.SupplierContactFieldNameName}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameAddress1}", queryParameters[$"@{DBNames.SupplierContactFieldNameMail}"] ?? DBNull.Value },
			{ $"{DBNames.SupplierFieldNameAddress2}", queryParameters[$"@{DBNames.SupplierContactFieldNamePhone}"] ?? DBNull.Value }
		};

		await _dataService.ExecuteScalarAsync<uint>( UpdateContactQuery, parameters );
	}

	public async Task DeleteContactAsync( int contactId )
	{
		var parameters = new Dictionary<string, object>
		{
			{ $"@{DBNames.SupplierContactFieldNameId}", contactId }
		};

		try
		{
			await _dataService.ExecuteScalarAsync<uint>( DeleteContactQuery, parameters );
		}
		catch ( MySqlException ex ) when ( ex.Number == 1451 )
		{
			throw new EntityInUseException(
				$"{Lang.metadataSupplierContactDeleteError}." );
		}
	}

	public async Task<bool> NameExistsAsync( string? contactName )
	{
		if ( string.IsNullOrWhiteSpace( contactName ) )
			return false;

		var contacts = await GetAllContactsAsync();

		return contacts.Any( c =>
			string.Equals( c.Name, contactName, StringComparison.OrdinalIgnoreCase ) );
	}

}
