namespace Modelbouwer.Services;

public static class HeaderMappingService
{
	// Stores mappings per model type
	private static readonly Dictionary<Type, Dictionary<string, string>> _headerToProperty
		= new();

	private static readonly Dictionary<Type, Dictionary<string, string>> _propertyToHeader
		= new();

	/// <summary>
	/// Registers a header-to-property mapping for a specific model type.
	/// Multiple headers (e.g. translated versions) may map to the same property.
	/// </summary>
	public static void RegisterMapping<T>( string header, string propertyName )
	{
		Type type = typeof(T);

		if ( !_headerToProperty.ContainsKey( type ) )
			_headerToProperty [ type ] = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

		if ( !_propertyToHeader.ContainsKey( type ) )
			_propertyToHeader [ type ] = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

		// Map UI header → PropertyName
		_headerToProperty [ type ] [ header ] = propertyName;

		// Map PropertyName → UI Header (last registered wins)
		_propertyToHeader [ type ] [ propertyName ] = header;
	}

	/// <summary>
	/// Returns the property name for a UI header.
	/// </summary>
	public static string? GetPropertyForHeader<T>( string header )
	{
		Type type = typeof(T);

		if ( _headerToProperty.ContainsKey( type ) &&
			_headerToProperty [ type ].TryGetValue( header, out var property ) )
		{
			return property;
		}

		return null;
	}

	/// <summary>
	/// Returns the UI header for a property (useful for exporting).
	/// </summary>
	public static string? GetHeaderForProperty<T>( string propertyName )
	{
		Type type = typeof(T);

		if ( _propertyToHeader.ContainsKey( type ) &&
			_propertyToHeader [ type ].TryGetValue( propertyName, out var header ) )
		{
			return header;
		}

		return null;
	}

	/// <summary>
	/// Returns all registered property names for this model type.
	/// </summary>
	public static IEnumerable<string> GetAllPropertiesFor<T>() =>
		_propertyToHeader.TryGetValue( typeof( T ), out var dict )
			? dict.Keys
			: [ ];
}