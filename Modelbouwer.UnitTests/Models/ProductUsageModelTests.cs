namespace Modelbouwer.UnitTests.Models;

[TestClass]
public class ProductUsageModelTests
{
	[TestMethod]
	public void ProductUsageModel_DefaultConstructor_ShouldInitializeWithUnchangedState()
	{
		// Arrange & Act
		var model = new ProductUsageModel();

		// Assert
		Assert.AreEqual(ProductUsageModel.RecordState.Unchanged, model.State);
	}

	[TestMethod]
	public void StatusMarker_WhenStateIsUnchanged_ShouldReturnEmptyString()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Unchanged
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual(string.Empty, marker);
	}

	[TestMethod]
	public void StatusMarker_WhenStateIsModified_ShouldReturnAsterisk()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Modified
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual("*", marker);
	}

	[TestMethod]
	public void StatusMarker_WhenStateIsAdded_ShouldReturnAsterisk()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Added
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual("*", marker);
	}

	[TestMethod]
	public void StatusMarker_WhenStateIsDeleted_ShouldReturnAsterisk()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Deleted
		};

		// Act
		var marker = model.StatusMarker;

		// Assert
		Assert.AreEqual("*", marker);
	}

	[TestMethod]
	public void SetProperty_WhenPropertyChanges_ShouldMarkAsModified()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Unchanged
		};

		// Act - Use reflection to call SetProperty through a property setter
		model.ProductUsageAmount = 10.0;

		// Assert
		Assert.AreEqual(ProductUsageModel.RecordState.Modified, model.State);
	}

	[TestMethod]
	public void State_CanBeSetDirectly_WithoutTriggering()
	{
		// Arrange
		var model = new ProductUsageModel();

		// Act
		model.State = ProductUsageModel.RecordState.Added;

		// Assert
		Assert.AreEqual(ProductUsageModel.RecordState.Added, model.State);
	}

	[TestMethod]
	public void PropertyChanged_ShouldBeRaisedWhenPropertyChanges()
	{
		// Arrange
		var model = new ProductUsageModel();
		bool eventRaised = false;
		model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(model.ProductUsageAmount))
				eventRaised = true;
		};

		// Act
		model.ProductUsageAmount = 15.0;

		// Assert
		Assert.IsTrue(eventRaised);
	}

	[TestMethod]
	public void AllProperties_ShouldBeSettable()
	{
		// Arrange & Act
		var model = new ProductUsageModel
		{
			ProductUsageId = 1,
			ProductUsageProjectId = 2,
			ProductUsageProjectName = "Test Project",
			ProductUsageProductId = 3,
			ProductUsageProductName = "Test Product",
			ProductUsageUsageDate = "2024-01-01",
			ProductUsageCategoryId = 4,
			ProductUsageCategoryName = "Test Category",
			ProductUsageAmount = 10.5,
			ProductUsageProductPrice = 25.0,
			ProductUsageCosts = 262.5,
			ProductUsageComment = "Test comment"
		};

		// Assert
		Assert.AreEqual(1, model.ProductUsageId);
		Assert.AreEqual(2, model.ProductUsageProjectId);
		Assert.AreEqual("Test Project", model.ProductUsageProjectName);
		Assert.AreEqual(3, model.ProductUsageProductId);
		Assert.AreEqual("Test Product", model.ProductUsageProductName);
		Assert.AreEqual("2024-01-01", model.ProductUsageUsageDate);
		Assert.AreEqual(4, model.ProductUsageCategoryId);
		Assert.AreEqual("Test Category", model.ProductUsageCategoryName);
		Assert.AreEqual(10.5, model.ProductUsageAmount);
		Assert.AreEqual(25.0, model.ProductUsageProductPrice);
		Assert.AreEqual(262.5, model.ProductUsageCosts);
		Assert.AreEqual("Test comment", model.ProductUsageComment);
	}

	[TestMethod]
	public void HeaderToPropertyMap_ShouldNotBeNull()
	{
		// Arrange & Act
		var map = ProductUsageModel.HeaderToPropertyMap;

		// Assert
		Assert.IsNotNull(map);
		Assert.IsTrue(map.Count > 0);
	}

	[TestMethod]
	public void RecordState_AllEnumValues_ShouldBeAccessible()
	{
		// Arrange & Act & Assert
		var unchanged = ProductUsageModel.RecordState.Unchanged;
		var added = ProductUsageModel.RecordState.Added;
		var modified = ProductUsageModel.RecordState.Modified;
		var deleted = ProductUsageModel.RecordState.Deleted;

		Assert.IsNotNull(unchanged);
		Assert.IsNotNull(added);
		Assert.IsNotNull(modified);
		Assert.IsNotNull(deleted);
	}

	[TestMethod]
	public void SetProperty_WhenValueIsTheSame_ShouldNotChangeState()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Unchanged,
			ProductUsageAmount = 10.0
		};
		model.State = ProductUsageModel.RecordState.Unchanged; // Reset state

		// Act
		model.ProductUsageAmount = 10.0; // Set to same value

		// Assert
		Assert.AreEqual(ProductUsageModel.RecordState.Unchanged, model.State);
	}

	[TestMethod]
	public void MultiplePropertyChanges_ShouldMaintainModifiedState()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Unchanged
		};

		// Act
		model.ProductUsageAmount = 10.0;
		model.ProductUsageProductPrice = 5.0;
		model.ProductUsageCosts = 50.0;

		// Assert
		Assert.AreEqual(ProductUsageModel.RecordState.Modified, model.State);
	}

	[TestMethod]
	public void State_ChangingFromModified_ShouldNotResetToUnchanged()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Modified
		};

		// Act
		model.ProductUsageAmount = 15.0;

		// Assert
		Assert.AreEqual(ProductUsageModel.RecordState.Modified, model.State);
	}

	[TestMethod]
	public void PropertyChanged_ShouldBeRaisedForStatusMarker_WhenStateChanges()
	{
		// Arrange
		var model = new ProductUsageModel
		{
			State = ProductUsageModel.RecordState.Unchanged
		};
		bool statusMarkerEventRaised = false;
		model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(model.StatusMarker))
				statusMarkerEventRaised = true;
		};

		// Act
		model.ProductUsageAmount = 10.0;

		// Assert
		Assert.IsTrue(statusMarkerEventRaised);
	}
}