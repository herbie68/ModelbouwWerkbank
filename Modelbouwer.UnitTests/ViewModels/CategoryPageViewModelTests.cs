using System.Reflection;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class CategoryPageViewModelTests
{
	[TestMethod]
	public void UpdateParameters_UsesDistinctKeysForIdAndParentId()
	{
		// Arrange
		var category = new CategoryModel
		{
			CategoryId = 42,
			ParentId = 7,
			CategoryName = "Subcategory"
		};

		var method = typeof( CategoryPageViewModel ).GetMethod(
			"UpdateParameters",
			BindingFlags.NonPublic | BindingFlags.Static );

		// Act
		var parameters = method?.Invoke( null, [ category ] ) as Dictionary<string, object?>;

		// Assert
		Assert.IsNotNull( parameters );
		Assert.AreEqual( 3, parameters.Count );
		Assert.AreEqual( 42, parameters [ $"@{DBNames.CategoryFieldNameId}" ] );
		Assert.AreEqual( 7, parameters [ $"@{DBNames.CategoryFieldNameParentId}" ] );
		Assert.AreEqual( "Subcategory", parameters [ $"@{DBNames.CategoryFieldNameName}" ] );
	}
}
