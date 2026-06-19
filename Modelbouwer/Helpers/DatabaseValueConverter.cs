namespace Modelbouwer.Helpers;

public static class DatabaseValueConverter
{
	public static string GetString( object value ) =>
		value == null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;

	public static byte [ ]? GetBytes( object value )
	{
		if ( value == null || value == DBNull.Value )
			return null;

		if ( value is byte [ ] bytes )
			return bytes;

		if ( value is ReadOnlyMemory<byte> readOnlyMemory )
			return readOnlyMemory.ToArray();

		if ( value is Memory<byte> memory )
			return memory.ToArray();

		if ( value is Stream stream )
		{
			using MemoryStream memoryStream = new();
			stream.CopyTo( memoryStream );
			return memoryStream.ToArray();
		}

		if ( value is string text )
		{
			if ( string.IsNullOrWhiteSpace( text ) )
				return null;

			if ( text.StartsWith( "0x", StringComparison.OrdinalIgnoreCase ) )
				text = text [ 2.. ];

			if ( text.Length % 2 == 0 && text.All( Uri.IsHexDigit ) )
				return Convert.FromHexString( text );

			try
			{
				return Convert.FromBase64String( text );
			}
			catch ( FormatException )
			{
				return null;
			}
		}

		return null;
	}

	public static DateOnly GetDateOnly( object value ) =>
		value == null || value == DBNull.Value
			? DateOnly.MinValue
			: DateOnly.FromDateTime( Convert.ToDateTime( value ) );

	public static TimeOnly GetTimeOnly( object value ) =>
		value == null || value == DBNull.Value
		? TimeOnly.MinValue
		: TimeOnly.FromDateTime( Convert.ToDateTime( value ) );

	public static int GetInt( object value ) =>
		value == null || value == DBNull.Value ? 0 : Convert.ToInt32( value );

	public static double GetDouble( object value ) =>
		value == null || value == DBNull.Value ? 0.0 : Convert.ToDouble( value );

	public static decimal GetDecimal( object value ) =>
	value == null || value == DBNull.Value ? 0.000000M : Convert.ToDecimal( value );


	public static float GetFloat( object value ) =>
		value == null || value == DBNull.Value ? 0.0f : Convert.ToSingle( value );

	public static sbyte GetSByte( object value ) =>
		value == null || value == DBNull.Value ? ( sbyte ) 0 : Convert.ToSByte( value );
}