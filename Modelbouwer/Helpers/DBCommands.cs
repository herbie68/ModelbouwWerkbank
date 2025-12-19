namespace Modelbouwer.Helpers;

public class DBCommands
{
	#region GetData
	#region GetData unsorted
	public static DataTable GetData( string _table )
	{
		string selectQuery = $"" +
			$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}";

		return GetQuery( selectQuery );
	}
	#endregion

	#region GetData Sorted
	public static DataTable GetData( string _table, string _orderByFieldName )
	{
		string selectQuery =  _orderByFieldName.Equals( "nosort", StringComparison.CurrentCultureIgnoreCase )
			?  $"" +
				$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}"
			:  $"" +
				$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}" +
				$"{DBNames.SqlOrder}{_orderByFieldName}" ;
		return GetQuery( selectQuery );
	}
	#endregion

	#region GetData Sorted and filtered
	public static DataTable GetData( string _table, string _orderByFieldName, string _whereFieldName, string _whereFieldValue )
	{
		string selectQuery =  _orderByFieldName.Equals( "nosort", StringComparison.CurrentCultureIgnoreCase )
			?  $"" +
				$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}" +
				$"{DBNames.SqlWhere}{_whereFieldName} = '{_whereFieldValue}';"
			:  $"" +
				$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}" +
				$"{DBNames.SqlWhere}{_whereFieldName} = '{_whereFieldValue}'" +
				$"{DBNames.SqlOrder}{_orderByFieldName};" ;
		return GetQuery( selectQuery );
	}
	#endregion

	#region Get data sorted, and filtered on two criteria
	public static DataTable GetData( string _table, string _orderByFieldName, string _whereFieldName, string _whereFieldValue, string _andWhereFieldName, string _andWhereFieldValue )
	{
		string selectQuery =  _orderByFieldName.Equals( "nosort", StringComparison.CurrentCultureIgnoreCase )
			?  $"" +
				$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}" +
				$"{DBNames.SqlWhere}{_whereFieldName} = '{_whereFieldValue}'" +
				$"{DBNames.SqlAnd}{_andWhereFieldName} = '{_andWhereFieldValue}';"
			:  $"" +
				$"{DBNames.SqlSelectAll}{DBNames.SqlFrom}{DBNames.Database}.{_table}" +
				$"{DBNames.SqlWhere}{_whereFieldName} = '{_whereFieldValue}'" +
				$"{DBNames.SqlAnd}{_andWhereFieldName} = '{_andWhereFieldValue}'" +
				$"{DBNames.SqlOrder}{_orderByFieldName};" ;
		MySqlConnection connection = new(DBConnect.ConnectionString);
		connection.Open();
		DataTable table = new();
		MySqlDataAdapter adapter = new(selectQuery, connection);
		_ = adapter.Fill( table );
		connection.Close();
		return table;
	}
	#endregion

	#region Get the data table based on the select query
	private static DataTable GetQuery( string _sqlQuery )
	{
		MySqlConnection connection = new(DBConnect.ConnectionString);
		connection.Open();
		DataTable table = new();
		MySqlDataAdapter adapter = new(_sqlQuery, connection);
		_ = adapter.Fill( table );
		connection.Close();

		return table;
	}
	#endregion

	#region Get the latet inserted record Id from a specified table
	public static string GetLatestIdFromTable( string _table )
	{
		// There is an Id or String available for each condition, so one of them has a value the other one is 0 or ""
		string sqlQuery = $"" +
			$"{DBNames.SqlSelect}{DBNames.SqlMax}Id)" +
			$"{DBNames.SqlFrom}{DBNames.Database}.{_table.ToLower()}";

		MySqlConnection connection = new(DBConnect.ConnectionString);

		connection.Open();

		MySqlCommand cmd = new(sqlQuery, connection);

		string resultString = ((int)cmd.ExecuteScalar()).ToString();

		return resultString;
	}
	#endregion
	#endregion

	#region Fill lists
	#region CurrencyList
	public static ObservableCollection<CurrencyModel> GetCurrencyList( ObservableCollection<CurrencyModel>? _list = null )
	{
		_list ??= [ ];
		DataTable? _dt = GetData( DBNames.CurrencyTable, DBNames.CurrencyFieldNameName );

		for ( int i = 0; i < _dt.Rows.Count; i++ )
		{
			_list.Add( new CurrencyModel
			{
				CurrencyId = DatabaseValueConverter.GetInt( _dt.Rows [ i ] [ 0 ] ),
				CurrencyCode = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 1 ] ),
				CurrencySymbol = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 2 ] ),
				CurrencyName = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 3 ] ),
				CurrencyConversionRate = DatabaseValueConverter.GetDouble( _dt.Rows [ i ] [ 4 ] )
			} );
		}
		return _list;
	}
	#endregion CurrencyList

	#endregion

	#region Country table
	

	#endregion
}
