using System.Reflection;

namespace Modelbouwer.Services;

public class CsvImportResult
{
	public int TotalRows { get; set; }
	public int Imported { get; set; }
	public int Skipped { get; set; }
	public int Updated { get; set; }
}

public static class CsvImportService
{
	/// <summary>
	/// Generic CSV import for a model type T
	/// </summary>
	/// <typeparam name="T">Type of the model</typeparam>
	/// <param name="filePath">Path to CSV file</param>
	/// <param name="existingRecords">List of existing records (will be modified)</param>
	/// <param name="columnMappings">Mapping of UI column names to model property names</param>
	/// <param name="uniqueProperty">Property name used to check uniqueness (e.g., "CountryName")</param>
	public static CsvImportResult ImportCsv<T>(
		string filePath,
		List<T> existingRecords,
		Dictionary<string, string [ ]> columnMappings,
		string uniqueProperty,
		bool showMessageBox = false ) where T : class, new()
	{
		var result = new CsvImportResult();
		if ( !File.Exists( filePath ) )
			return result;

		var lines = File.ReadAllLines(filePath, Encoding.UTF8)
						.Where(l => !string.IsNullOrWhiteSpace(l))
						.ToList();
		if ( lines.Count < 2 ) // geen data
			return result;

		result.TotalRows = lines.Count - 1;

		// HEADER
		var headers = lines[0].Split(';').Select(h => h.Trim()).ToList();

		// Map headers naar propertynames
		var headerToPropertyMap = columnMappings
			.SelectMany(m => m.Value.Select(alias => new
			{
				Alias = alias,
				Property = m.Key
			}))
			.ToDictionary(x => x.Alias, x => x.Property, StringComparer.OrdinalIgnoreCase);

		var headerToProperty = headers
			.Select(h => headerToPropertyMap.TryGetValue(h, out var prop)
				? prop
				: null)
			.ToList();


		// Ignore first column (Id)
		int idIndex = 0;

		var uniquePropInfo = typeof(T).GetProperty(uniqueProperty, BindingFlags.Public | BindingFlags.Instance);
		if ( uniquePropInfo == null )
			throw new Exception( $"Unique property '{uniqueProperty}' not found on type {typeof( T ).Name}" );

		// Iterate over data rows
		for ( int i = 1; i < lines.Count; i++ )
		{
			var row = lines[i].Split(';');
			var record = new T();

			// Fill properties
			for ( int col = 0; col < headers.Count; col++ )
			{
				if ( col == idIndex )
					continue; // Skip ID

				var propName = headerToProperty[col];
				if ( propName == null )
					continue;

				var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
				if ( prop == null )
					continue;

				try
				{
					object value = Convert.ChangeType(row[col], prop.PropertyType);
					prop.SetValue( record, value );
				}
				catch
				{
					prop.SetValue( record, row [ col ] ); // fallback as string
				}
			}

			// Check for existing record by unique property
			var uniqueValue = uniquePropInfo.GetValue(record);
			var existing = existingRecords.FirstOrDefault(r =>
				uniquePropInfo.GetValue(r)?.Equals(uniqueValue) == true);

			if ( existing == null )
			{
				existingRecords.Add( record );
				result.Imported++;
			}
			else
			{
				// Compare all properties
				bool isEqual = true;
				foreach ( var prop in typeof( T ).GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
				{
					var newVal = prop.GetValue(record);
					var oldVal = prop.GetValue(existing);
					if ( !Equals( newVal, oldVal ) )
					{
						isEqual = false;
						break;
					}
				}

				if ( isEqual )
				{
					result.Skipped++;
				}
				else
				{
					// Update existing record
					foreach ( var prop in typeof( T ).GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
					{
						var newVal = prop.GetValue(record);
						prop.SetValue( existing, newVal );
					}
					result.Updated++;
				}
			}
		}

		if ( showMessageBox )
		{
			MessageBox.Show(
					$"{Lang.ImportMessagboxCompletedRead}: {result.TotalRows}\n" +
					$"{Lang.ImportMessagboxCompletedImported}: {result.Imported}\n" +
					$"{Lang.ImportMessagboxCompletedSkipped}: {result.Skipped}\n" +
					$"{Lang.ImportMessagboxCompletedModified}: {result.Updated}",
					$"{Lang.ImportMessagboxCompletedTitle}",
					MessageBoxButton.OK,
					MessageBoxImage.Information
				);
		}
		return result;
	}
}