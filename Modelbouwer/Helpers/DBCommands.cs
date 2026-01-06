using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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
	public static async Task<int> GetLatestIdFromTableAsync( string table )
	{
		string sqlQuery = $@"
        SELECT MAX(Id)
        FROM {DBNames.Database}.{table.ToLower()}";

		await using var connection = new MySqlConnection(DBConnect.ConnectionString);
		await connection.OpenAsync();

		await using var cmd = new MySqlCommand(sqlQuery, connection);
		var result = await cmd.ExecuteScalarAsync();

		return Convert.ToInt32( result );
	}
	#endregion
	#endregion

	#region Fill lists
	#region CountryList
	public static ObservableCollection<CountryModel> GetCountryList( ObservableCollection<CountryModel>? countryList = null )
	{
		countryList ??= [ ];
		DataTable? _dt = GetData( DBNames.CountryView, DBNames.CountryFieldNameName );

		for ( int i = 0; i < _dt.Rows.Count; i++ )
		{
			countryList.Add( new CountryModel
			{
				CountryId = DatabaseValueConverter.GetInt( _dt.Rows [ i ] [ 0 ] ),
				CountryCode = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 1 ] ),
				CountryName = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 2 ] ),
				CountryCurrencyId = DatabaseValueConverter.GetInt( _dt.Rows [ i ] [ 3 ] ),
				CountryCurrencySymbol = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 4 ] )
			} );
		}
		return countryList;
	}
	//public static ObservableCollection<CountryViewModel> GetCountryViewList( ObservableCollection<CountryViewModel>? countryList = null )
	//{
	//	countryList ??= [ ];
	//	DataTable? _dt = GetData( DBNames.CountryView, DBNames.CountryFieldNameName );

	//	for ( int i = 0; i < _dt.Rows.Count; i++ )
	//	{
	//		countryList.Add( new CountryViewModel
	//		{
	//			CountryId = DatabaseValueConverter.GetInt( _dt.Rows [ i ] [ 0 ] ),
	//			CountryCode = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 1 ] ),
	//			CountryName = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 2 ] ),
	//			CountryCurrencyId = DatabaseValueConverter.GetInt( _dt.Rows [ i ] [ 3 ] ),
	//			CountryCurrencySymbol = DatabaseValueConverter.GetString( _dt.Rows [ i ] [ 4 ] )
	//		} );
	//	}
	//	return countryList;
	//}
	#endregion CountryList

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
}
