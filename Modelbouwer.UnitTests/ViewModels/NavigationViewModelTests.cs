namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class NavigationViewModelTests
{
	private Mock<IServiceProvider> _mockServiceProvider;
	private NavigationViewModel _viewModel;

	[TestInitialize]
	public void Setup()
	{
		_mockServiceProvider = new Mock<IServiceProvider>();

		// Setup default service returns to avoid null reference exceptions
		_mockServiceProvider
			.Setup(x => x.GetService(It.IsAny<Type>()))
			.Returns((Type t) => Activator.CreateInstance(t));
	}

	[TestMethod]
	public void Constructor_ShouldInitializeNavigationItems()
	{
		// Arrange & Act
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.IsNotNull(_viewModel.NavigationItems);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Constructor_WhenServiceProviderIsNull_ShouldThrowArgumentNullException()
	{
		// Arrange & Act
		_viewModel = new NavigationViewModel(null!);

		// Assert - ExpectedException
	}

	[TestMethod]
	public void CurrentView_ShouldBeSettable()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		var testView = new object();

		// Act
		_viewModel.CurrentView = testView;

		// Assert
		Assert.AreSame(testView, _viewModel.CurrentView);
	}

	[TestMethod]
	public void CurrentView_WhenChanged_ShouldRaisePropertyChanged()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		bool eventRaised = false;
		_viewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(_viewModel.CurrentView))
				eventRaised = true;
		};

		// Act
		_viewModel.CurrentView = new object();

		// Assert
		Assert.IsTrue(eventRaised);
	}

	[TestMethod]
	public void IsNavigationLoaded_DefaultValue_ShouldBeFalse()
	{
		// Arrange & Act
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.IsFalse(_viewModel.IsNavigationLoaded);
	}

	[TestMethod]
	public void IsNavigationLoaded_WhenChanged_ShouldRaisePropertyChanged()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		bool eventRaised = false;
		_viewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(_viewModel.IsNavigationLoaded))
				eventRaised = true;
		};

		// Act
		_viewModel.IsNavigationLoaded = true;

		// Assert
		Assert.IsTrue(eventRaised);
		Assert.IsTrue(_viewModel.IsNavigationLoaded);
	}

	[TestMethod]
	public void IsNavigationLoaded_WhenSetToSameValue_ShouldNotRaisePropertyChanged()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		_viewModel.IsNavigationLoaded = false;
		int eventCount = 0;
		_viewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(_viewModel.IsNavigationLoaded))
				eventCount++;
		};

		// Act
		_viewModel.IsNavigationLoaded = false;

		// Assert
		Assert.AreEqual(0, eventCount);
	}

	[TestMethod]
	public void AppVersion_ShouldReturnVersionString()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Act
		var version = _viewModel.AppVersion;

		// Assert
		Assert.IsNotNull(version);
		Assert.IsTrue(version.StartsWith("Modelbouwer v"));
	}

	[TestMethod]
	public void AppVersion_ShouldContainVersionNumber()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Act
		var version = _viewModel.AppVersion;

		// Assert
		Assert.IsTrue(version.Contains("26.2") || version.Contains("Unknown"));
	}

	[TestMethod]
	public void NavigationItems_ShouldBeObservableCollection()
	{
		// Arrange & Act
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.IsInstanceOfType(_viewModel.NavigationItems, typeof(ObservableCollection<NavigationModel>));
	}

	[TestMethod]
	public void PropertyChanged_ShouldBeRaisedForCurrentView()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		string? propertyName = null;
		_viewModel.PropertyChanged += (s, e) =>
		{
			propertyName = e.PropertyName;
		};

		// Act
		_viewModel.CurrentView = new object();

		// Assert
		Assert.AreEqual(nameof(_viewModel.CurrentView), propertyName);
	}

	[TestMethod]
	public void CurrentView_CanBeSetToNull()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		_viewModel.CurrentView = new object();

		// Act
		_viewModel.CurrentView = null;

		// Assert
		Assert.IsNull(_viewModel.CurrentView);
	}

	[TestMethod]
	public void NavigationViewModel_ShouldImplementINotifyPropertyChanged()
	{
		// Arrange & Act
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.IsInstanceOfType(_viewModel, typeof(INotifyPropertyChanged));
	}

	[TestMethod]
	public void Constructor_ShouldInitializeEmptyNavigationItems()
	{
		// Arrange & Act
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.AreEqual(0, _viewModel.NavigationItems.Count);
	}

	[TestMethod]
	public void IsNavigationLoaded_CanBeToggled()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		_viewModel.IsNavigationLoaded = false;

		// Act
		_viewModel.IsNavigationLoaded = true;
		var firstState = _viewModel.IsNavigationLoaded;

		_viewModel.IsNavigationLoaded = false;
		var secondState = _viewModel.IsNavigationLoaded;

		// Assert
		Assert.IsTrue(firstState);
		Assert.IsFalse(secondState);
	}

	[TestMethod]
	public void PropertyChanged_ShouldNotBeNull()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		bool hasSubscribers = false;

		// Act
		_viewModel.PropertyChanged += (s, e) => { hasSubscribers = true; };
		_viewModel.CurrentView = new object();

		// Assert
		Assert.IsTrue(hasSubscribers);
	}

	[TestMethod]
	public void AppVersion_ShouldNotBeNullOrEmpty()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Act
		var version = _viewModel.AppVersion;

		// Assert
		Assert.IsFalse(string.IsNullOrEmpty(version));
	}

	[TestMethod]
	public void CurrentView_MultipleChanges_ShouldUpdateCorrectly()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		var view1 = new object();
		var view2 = new object();
		var view3 = new object();

		// Act
		_viewModel.CurrentView = view1;
		Assert.AreSame(view1, _viewModel.CurrentView);

		_viewModel.CurrentView = view2;
		Assert.AreSame(view2, _viewModel.CurrentView);

		_viewModel.CurrentView = view3;
		Assert.AreSame(view3, _viewModel.CurrentView);
	}

	[TestMethod]
	public void PropertyChanged_ShouldBeRaisedForIsNavigationLoaded()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		string? propertyName = null;
		_viewModel.PropertyChanged += (s, e) =>
		{
			propertyName = e.PropertyName;
		};

		// Act
		_viewModel.IsNavigationLoaded = true;

		// Assert
		Assert.AreEqual(nameof(_viewModel.IsNavigationLoaded), propertyName);
	}

	[TestMethod]
	public void NavigationItems_ShouldAllowAddingItems()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		var navItem = new NavigationModel
		{
			NavigationItem = "Test Item"
		};

		// Act
		_viewModel.NavigationItems.Add(navItem);

		// Assert
		Assert.AreEqual(1, _viewModel.NavigationItems.Count);
		Assert.AreSame(navItem, _viewModel.NavigationItems[0]);
	}

	[TestMethod]
	public void NavigationItems_ShouldAllowRemovingItems()
	{
		// Arrange
		_viewModel = new NavigationViewModel(_mockServiceProvider.Object);
		var navItem = new NavigationModel
		{
			NavigationItem = "Test Item"
		};
		_viewModel.NavigationItems.Add(navItem);

		// Act
		_viewModel.NavigationItems.Remove(navItem);

		// Assert
		Assert.AreEqual(0, _viewModel.NavigationItems.Count);
	}
}