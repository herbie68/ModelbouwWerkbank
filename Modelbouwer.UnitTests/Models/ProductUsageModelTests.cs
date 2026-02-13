using Modelbouwer.Model;

namespace Modelbouwer.UnitTests.Models;

[TestClass]
public class ProductUsageModelTests
{
	[TestMethod]
	public void ProductUsageModel_DefaultState_IsUnchanged()
	{
		// Arrange & Act
		var model = new ProductUsageModel();

		// Assert
		Assert.AreEqual( ProductUsageModel.RecordState.Unchanged, model.State );
	}

	[TestMethod]
	public void StatusMarker_WhenUnchanged_ReturnsEmptyString()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Unchanged
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual( string.Empty, marker );
	}

	[TestMethod]
	public void StatusMarker_WhenAdded_ReturnsAsterisk()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Added
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual( "*", marker );
	}

	[TestMethod]
	public void StatusMarker_WhenModified_ReturnsAsterisk()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Modified
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual( "*", marker );
	}

	[TestMethod]
	public void StatusMarker_WhenDeleted_ReturnsAsterisk()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Deleted
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual( "*", marker );
	}

	[TestMethod]
	public void SetProperty_ChangingValueFromUnchanged_MarksAsModified()
	{
		// Arrange
		var model = new ProductUsageModel();
		var propertyChangedRaised = false;
		model.PropertyChanged += ( s, e ) =>
		{
			if ( e.PropertyName == nameof( model.State ) )
				propertyChangedRaised = true;
		};

		// Act
		model.ProductUsageAmount = 10.5;

		// Assert
		Assert.AreEqual( ProductUsageModel.RecordState.Modified, model.State );
		Assert.IsTrue( propertyChangedRaised );
	}

	[TestMethod]
	public void SetProperty_ChangingSameValue_DoesNotChangeState()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			ProductUsageAmount = 10.5
		};
		model.State = ProductUsageModel.RecordState.Unchanged;

		// Act
		model.ProductUsageAmount = 10.5;

		// Assert
		Assert.AreEqual( ProductUsageModel.RecordState.Unchanged, model.State );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedEvent()
	{
		// Arrange
		var model = new ProductUsageModel();
		var propertyChangedRaised = false;
		string? changedPropertyName = null;
		model.PropertyChanged += ( s, e ) =>
		{
			propertyChangedRaised = true;
			changedPropertyName = e.PropertyName;
		};

		// Act
		model.ProductUsageAmount = 15.0;

		// Assert
		Assert.IsTrue( propertyChangedRaised );
		Assert.IsNotNull( changedPropertyName );
	}

	[TestMethod]
	public void SetProperty_RaisesStatusMarkerPropertyChanged()
	{
		// Arrange
		var model = new ProductUsageModel();
		var statusMarkerChangedRaised = false;
		model.PropertyChanged += ( s, e ) =>
		{
			if ( e.PropertyName == nameof( model.StatusMarker ) )
				statusMarkerChangedRaised = true;
		};

		// Act
		model.ProductUsageAmount = 20.0;

		// Assert
		Assert.IsTrue( statusMarkerChangedRaised );
	}

	// New tests verifying both the changed property and StatusMarker fire for other properties
	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForProjectName()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageProjectName = "Test Project";

		Assert.Contains( nameof( model.ProductUsageProjectName ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForProductName()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageProductName = "Test Product";

		Assert.Contains( nameof( model.ProductUsageProductName ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForUsageDate()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageUsageDate = "2024-01-01";

		Assert.Contains( nameof( model.ProductUsageUsageDate ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForCategoryName()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageCategoryName = "Test Category";

		Assert.Contains( nameof( model.ProductUsageCategoryName ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForProductPrice()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageProductPrice = 12.34;

		Assert.Contains( nameof( model.ProductUsageProductPrice ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForCosts()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageCosts = 99.99;

		Assert.Contains( nameof( model.ProductUsageCosts ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void SetProperty_RaisesPropertyChangedAndStatusMarker_ForComment()
	{
		var model = new ProductUsageModel();
		var raised = new List<string>();
		model.PropertyChanged += ( s, e ) => raised.Add( e.PropertyName ?? string.Empty );

		model.ProductUsageComment = "Test Comment";

		Assert.Contains( nameof( model.ProductUsageComment ), raised );
		Assert.Contains( nameof( model.StatusMarker ), raised );
	}

	[TestMethod]
	public void State_CanBeSetDirectly_WithoutMarkingAsModified()
	{
		// Arrange
		var model = new ProductUsageModel();

		// Act
		model.State = ProductUsageModel.RecordState.Added;

		// Assert
		Assert.AreEqual( ProductUsageModel.RecordState.Added, model.State );
	}

	[TestMethod]
	public void HeaderToPropertyMap_ContainsExpectedMappings()
	{
		// Assert
		Assert.IsTrue( ProductUsageModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductUsageViewFieldNameProductName ) );
		Assert.IsTrue( ProductUsageModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductUsageViewFieldTypeCategoryName ) );
		Assert.IsTrue( ProductUsageModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductUsageViewFieldNameAmountUsed ) );
		Assert.IsTrue( ProductUsageModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductUsageViewFieldNamePrice ) );
		Assert.IsTrue( ProductUsageModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductUsageViewFieldNameTotalCosts ) );
		Assert.IsTrue( ProductUsageModel.HeaderToPropertyMap.ContainsKey( DBNames.ProductUsageViewFieldNameComment ) );
	}

	[TestMethod]
	public void ProductUsageModel_AllProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var model = new ProductUsageModel();

		// Act
		model.ProductUsageId = 1;
		model.ProductUsageProjectId = 2;
		model.ProductUsageProjectName = "Test Project";
		model.ProductUsageProductId = 3;
		model.ProductUsageProductName = "Test Product";
		model.ProductUsageUsageDate = "2024-01-01";
		model.ProductUsageCategoryId = 4;
		model.ProductUsageCategoryName = "Test Category";
		model.ProductUsageAmount = 5.5;
		model.ProductUsageProductPrice = 10.0;
		model.ProductUsageCosts = 55.0;
		model.ProductUsageComment = "Test Comment";

		// Assert
		Assert.AreEqual( 1, model.ProductUsageId );
		Assert.AreEqual( 2, model.ProductUsageProjectId );
		Assert.AreEqual( "Test Project", model.ProductUsageProjectName );
		Assert.AreEqual( 3, model.ProductUsageProductId );
		Assert.AreEqual( "Test Product", model.ProductUsageProductName );
		Assert.AreEqual( "2024-01-01", model.ProductUsageUsageDate );
		Assert.AreEqual( 4, model.ProductUsageCategoryId );
		Assert.AreEqual( "Test Category", model.ProductUsageCategoryName );
		Assert.AreEqual( 5.5, model.ProductUsageAmount );
		Assert.AreEqual( 10.0, model.ProductUsageProductPrice );
		Assert.AreEqual( 55.0, model.ProductUsageCosts );
		Assert.AreEqual( "Test Comment", model.ProductUsageComment );
	}

	[TestMethod]
	public void RecordState_Enum_HasAllExpectedValues()
	{
		// Assert
		Assert.IsTrue( Enum.IsDefined( typeof( ProductUsageModel.RecordState ), ProductUsageModel.RecordState.Unchanged ) );
		Assert.IsTrue( Enum.IsDefined( typeof( ProductUsageModel.RecordState ), ProductUsageModel.RecordState.Added ) );
		Assert.IsTrue( Enum.IsDefined( typeof( ProductUsageModel.RecordState ), ProductUsageModel.RecordState.Modified ) );
		Assert.IsTrue( Enum.IsDefined( typeof( ProductUsageModel.RecordState ), ProductUsageModel.RecordState.Deleted ) );
	}

	[TestMethod]
	public void SetProperty_ChangingStateProperty_DoesNotTriggerModified()
	{
		// Arrange
		var model = new ProductUsageModel();

		// Act
		model.State = ProductUsageModel.RecordState.Added;

		// Assert - State should be Added, not Modified
		Assert.AreEqual( ProductUsageModel.RecordState.Added, model.State );
	}
}