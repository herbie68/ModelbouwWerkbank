using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Helpers;

public class DBNames
{
	public static readonly string Database = "modelbuilder";

	#region Sql commands
	public static readonly string SqlAnd = " AND ";
	public static readonly string SqlAsc = " ASC ";
	public static readonly string SqlAs = " AS ";
	public static readonly string SqlBetween = " BETWEEN ";
	public static readonly string SqlCall = "CALL ";
	public static readonly string SqlCast = "CAST( ";
	public static readonly string SqlConcat = "CONCAT( ";
	public static readonly string SqlCount = " COUNT( ";
	public static readonly string SqlCountAll = " COUNT(*) ";
	public static readonly string SqlCountUnique = " COUNT(DISTINCT ";
	public static readonly string SqlDelete = "DELETE ";
	public static readonly string SqlDeleteFrom = "DELETE FROM ";
	public static readonly string SqlDesc = " DESC ";
	public static readonly string SqlFrom = " FROM ";
	public static readonly string SqlIn = "IN ";
	public static readonly string SqlInnerJoin = "INNER JOIN ";
	public static readonly string SqlInsert = "INSERT INTO ";
	public static readonly string SqlIsNull = " IS NULL ";
	public static readonly string SqlLimit = " LIMIT ";
	public static readonly string SqlLimit1 = " LIMIT 1 ";
	public static readonly string SqlLike = " LIKE ";
	public static readonly string SqlLower = " LOWER ( ";
	public static readonly string SqlMax = "MAX( ";
	public static readonly string SqlMin = "MIN( ";
	public static readonly string SqlOn = " ON ";
	public static readonly string SqlOr = " OR ";
	public static readonly string SqlOrder = " ORDER BY ";
	public static readonly string SqlOrderBy = " ORDER BY ";
	public static readonly string SqlSelect = "SELECT ";
	public static readonly string SqlSelectAll = "SELECT *";
	public static readonly string SqlSelectDistinct = "SELECT DISTINCT ";
	public static readonly string SqlSet = " SET ";
	public static readonly string SqlSum = " SUM( ";
	public static readonly string SqlSubString = " SUBSTRING( ";
	public static readonly string SqlUnsigned = " as UNSIGNED) ";
	public static readonly string SqlUnionAll = "UNION ALL";
	public static readonly string SqlUpdate = "UPDATE ";
	public static readonly string SqlValues = " VALUES ";
	public static readonly string SqlWhere = " WHERE ";
	public static readonly string SqlWithRecursive = "WITH RECURSIVE ";
	#endregion Sql commands

	#region Settings table
	public static readonly string SettingsTable = "settings";
	public static readonly string SettingsFieldNameCulture = "Culture";
	public static readonly string SettingsFieldNameLanguage = "Language";
	public static readonly string SettingsFieldNameHourRate = "HourRate";

	public static readonly string SettingsFieldTypeCulture = "string";
	public static readonly string SettingsFieldTypeLanguage = "string";
	public static readonly string SettingsFieldTypeHourRate = "double";
	#endregion Settings table

	#region Country table
	public static readonly string CountryTable = "country";
	public static readonly string CountryView = "view_country";
	public static readonly string CountryFieldNameId = "Id";
	public static readonly string CountryFieldTypeId = "int";
	public static readonly string CountryFieldNameCode = "Code";
	public static readonly string CountryFieldTypeCode = "string";
	public static readonly string CountryFieldNameCurrencySymbol = "Symbol";
	public static readonly string CountryFieldTypeCurrencySymbol = "string";
	public static readonly string CountryFieldNameName = "Name";
	public static readonly string CountryFieldTypeName = "string";
	public static readonly string CountryFieldNameCurrencyId = "Defaultcurrency_Id";
	public static readonly string CountryFieldTypeCurrencyId = "int";
	#endregion Country table

	#region Currency table
	public static readonly string CurrencyTable = "currency";
	public static readonly string CurrencyFieldNameId = "Id";
	public static readonly string CurrencyFieldTypeId = "int";
	public static readonly string CurrencyFieldNameCode = "Code";
	public static readonly string CurrencyFieldTypeCode = "string";
	public static readonly string CurrencyFieldNameSymbol = "Symbol";
	public static readonly string CurrencyFieldTypeSymbol = "string";
	public static readonly string CurrencyFieldNameName = "Name";
	public static readonly string CurrencyFieldTypeName = "string";
	public static readonly string CurrencyFieldNameRate = "ConversionRate";
	public static readonly string CurrencyFieldTypeRate = "double";
	#endregion


}
