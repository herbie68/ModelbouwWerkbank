using System.Text;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class CsvImportServiceTests
{
	[TestMethod]
	public void ImportCsv_WithShortDataRow_ImportsRecordWithMissingValuesInsteadOfThrowing()
	{
		// Arrange
		var filePath = Path.Combine( Path.GetTempPath(), $"{Guid.NewGuid()}.csv" );
		File.WriteAllLines(
			filePath,
			[
				"ID;Code;Name",
				"1;ABC"
			],
			Encoding.UTF8 );

		var records = new List<ImportTestModel>();

		try
		{
			// Act
			var result = CsvImportService.ImportCsv(
				filePath,
				records,
				ImportTestModel.ColumnMappings,
				nameof( ImportTestModel.Code ) );

			// Assert
			Assert.AreEqual( 1, result.TotalRows );
			Assert.AreEqual( 1, result.Imported );
			Assert.AreEqual( "ABC", records [ 0 ].Code );
			Assert.AreEqual( string.Empty, records [ 0 ].Name );
		}
		finally
		{
			File.Delete( filePath );
		}
	}

	[TestMethod]
	public void ImportCsv_WithExistingChangedRecord_UpdatesRecordAndKeepsLookupStable()
	{
		// Arrange
		var filePath = Path.Combine( Path.GetTempPath(), $"{Guid.NewGuid()}.csv" );
		File.WriteAllLines(
			filePath,
			[
				"ID;Code;Name",
				"1;ABC;Updated"
			],
			Encoding.UTF8 );

		var records = new List<ImportTestModel>
		{
			new() { Code = "ABC", Name = "Old" }
		};

		try
		{
			// Act
			var result = CsvImportService.ImportCsv(
				filePath,
				records,
				ImportTestModel.ColumnMappings,
				nameof( ImportTestModel.Code ) );

			// Assert
			Assert.AreEqual( 0, result.Imported );
			Assert.AreEqual( 1, result.Updated );
			Assert.AreEqual( 1, records.Count );
			Assert.AreEqual( "Updated", records [ 0 ].Name );
		}
		finally
		{
			File.Delete( filePath );
		}
	}

	[TestMethod]
	public void ImportCsv_WithInvalidNumericValue_LeavesDefaultValueInsteadOfThrowing()
	{
		// Arrange
		var filePath = Path.Combine( Path.GetTempPath(), $"{Guid.NewGuid()}.csv" );
		File.WriteAllLines(
			filePath,
			[
				"ID;Code;Quantity",
				"1;ABC;not-a-number"
			],
			Encoding.UTF8 );

		var records = new List<ImportTestModel>();

		try
		{
			// Act
			var result = CsvImportService.ImportCsv(
				filePath,
				records,
				ImportTestModel.ColumnMappings,
				nameof( ImportTestModel.Code ) );

			// Assert
			Assert.AreEqual( 1, result.Imported );
			Assert.AreEqual( 0, records [ 0 ].Quantity );
		}
		finally
		{
			File.Delete( filePath );
		}
	}

	private sealed class ImportTestModel
	{
		public string? Code { get; set; }
		public string? Name { get; set; }
		public int Quantity { get; set; }

		public static readonly Dictionary<string, string[]> ColumnMappings = new()
		{
			[nameof( Code )] = [ "Code" ],
			[nameof( Name )] = [ "Name" ],
			[nameof( Quantity )] = [ "Quantity" ]
		};
	}
}