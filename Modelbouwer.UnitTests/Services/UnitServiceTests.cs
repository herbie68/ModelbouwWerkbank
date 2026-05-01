using Moq;

namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class UnitServiceTests
{
	private Mock<GenericDataService> _mockDataService = null!;
	private UnitService _unitService = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockDataService = new Mock<GenericDataService>();
		_unitService = new UnitService( _mockDataService.Object );
	}

	[TestMethod]
	public async Task GetAllUnitsAsync_ReturnsUnitList()
	{
		// Arrange
		var expectedUnits = new List<UnitModel>
		{
			new UnitModel { UnitId = 1, UnitName = "Piece" },
			new UnitModel { UnitId = 2, UnitName = "Meter" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, UnitModel>>() ) )
			.ReturnsAsync( expectedUnits );

		// Act
		var result = await _unitService.GetAllUnitsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.HasCount( 2, result );
		Assert.AreEqual( 1, result [ 0 ].UnitId );
		Assert.AreEqual( "Piece", result [ 0 ].UnitName );
		Assert.AreEqual( "Meter", result [ 1 ].UnitName );
	}

	[TestMethod]
	public async Task GetAllUnitsAsync_WithEmptyDatabase_ReturnsEmptyList()
	{
		// Arrange
		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, UnitModel>>() ) )
			.ReturnsAsync( new List<UnitModel>() );

		// Act
		var result = await _unitService.GetAllUnitsAsync();

		// Assert
		Assert.IsNotNull( result );
		Assert.IsEmpty( result );
	}

	[TestMethod]
	public async Task InsertNewUnitAsync_ReturnsNewUnitId()
	{
		// Arrange
		var parameters = CreateUnitParameters( unitName: "Box" );

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 42u );

		// Act
		var result = await _unitService.InsertNewUnitAsync( parameters );

		// Assert
		Assert.AreEqual( 42, result );
	}

	[TestMethod]
	public async Task InsertNewUnitAsync_PassesCorrectParameters()
	{
		// Arrange
		var parameters = CreateUnitParameters( unitName: "Box" );

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 1u );

		// Act
		await _unitService.InsertNewUnitAsync( parameters );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.UnitFieldNameUnitName}" ) &&
				Equals( d [ $"@{DBNames.UnitFieldNameUnitName}" ], "Box" ) ) ), Times.Once );
	}

	[TestMethod]
	public async Task UpdateUnitAsync_CallsDataServiceWithCorrectParameters()
	{
		// Arrange
		var parameters = CreateUnitParameters( 7, "Updated box" );

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		// Act
		await _unitService.UpdateUnitAsync( parameters );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.UnitFieldNameUnitId}" ) &&
				Equals( d [ $"@{DBNames.UnitFieldNameUnitId}" ], 7 ) &&
				d.ContainsKey( $"@{DBNames.UnitFieldNameUnitName}" ) &&
				Equals( d [ $"@{DBNames.UnitFieldNameUnitName}" ], "Updated box" ) ) ), Times.Once );
	}

	[TestMethod]
	public async Task DeleteUnitAsync_CallsDataServiceWithCorrectId()
	{
		// Arrange
		const int unitId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0u );

		// Act
		await _unitService.DeleteUnitAsync( unitId );

		// Assert
		_mockDataService.Verify( s => s.ExecuteScalarAsync<uint>(
			It.IsAny<string>(),
			It.Is<Dictionary<string, object>>( d =>
				d.ContainsKey( $"@{DBNames.UnitFieldNameUnitId}" ) &&
				Equals( d [ $"@{DBNames.UnitFieldNameUnitId}" ], unitId ) ) ), Times.Once );
	}

	[TestMethod]
	public async Task DeleteUnitAsync_WithConstraintViolation_ThrowsEntityInUseException()
	{
		// Arrange
		const int unitId = 123;

		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<uint>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ThrowsAsync( new TestMySqlException( 1451, "Foreign key constraint" ) );

		// Act / Assert
		try
		{
			await _unitService.DeleteUnitAsync( unitId );
			Assert.Fail( "Expected EntityInUseException was not thrown." );
		}
		catch ( EntityInUseException )
		{
			// expected
		}
	}

	[TestMethod]
	public async Task IsUnitUsedAsync_WhenUnitIsUsed_ReturnsTrue()
	{
		// Arrange
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 2 );

		// Act
		var result = await _unitService.IsUnitUsedAsync( 4 );

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public async Task IsUnitUsedAsync_WhenUnitIsNotUsed_ReturnsFalse()
	{
		// Arrange
		_mockDataService
			.Setup( s => s.ExecuteScalarAsync<int>( It.IsAny<string>(), It.IsAny<Dictionary<string, object>>() ) )
			.ReturnsAsync( 0 );

		// Act
		var result = await _unitService.IsUnitUsedAsync( 4 );

		// Assert
		Assert.IsFalse( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WithNullOrWhitespace_ReturnsFalse()
	{
		Assert.IsFalse( await _unitService.NameExistsAsync( null ) );
		Assert.IsFalse( await _unitService.NameExistsAsync( string.Empty ) );
		Assert.IsFalse( await _unitService.NameExistsAsync( "   " ) );
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenUnitNameExists_IgnoresCase()
	{
		// Arrange
		var units = new List<UnitModel>
		{
			new UnitModel { UnitId = 1, UnitName = "Piece" },
			new UnitModel { UnitId = 2, UnitName = "Meter" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, UnitModel>>() ) )
			.ReturnsAsync( units );

		// Act
		var result = await _unitService.NameExistsAsync( "piece" );

		// Assert
		Assert.IsTrue( result );
	}

	[TestMethod]
	public async Task NameExistsAsync_WhenUnitNameDoesNotExist_ReturnsFalse()
	{
		// Arrange
		var units = new List<UnitModel>
		{
			new UnitModel { UnitId = 1, UnitName = "Piece" }
		};

		_mockDataService
			.Setup( s => s.ExecuteQueryAsync( It.IsAny<string>(), It.IsAny<Func<System.Data.Common.DbDataReader, UnitModel>>() ) )
			.ReturnsAsync( units );

		// Act
		var result = await _unitService.NameExistsAsync( "Meter" );

		// Assert
		Assert.IsFalse( result );
	}

	private static Dictionary<string, object?> CreateUnitParameters( int unitId = 1, string unitName = "Piece" )
	{
		return new Dictionary<string, object?>
		{
			{ $"@{DBNames.UnitFieldNameUnitId}", unitId },
			{ $"@{DBNames.UnitFieldNameUnitName}", unitName }
		};
	}

	private class TestMySqlException : Exception
	{
		public int Number { get; }

		public TestMySqlException( int number, string message ) : base( message )
		{
			Number = number;
		}
	}
}
