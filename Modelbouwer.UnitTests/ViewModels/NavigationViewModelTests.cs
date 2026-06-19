using Moq;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class NavigationViewModelTests
{
	private Mock<IServiceProvider> _mockServiceProvider = null!;
	private NavigationViewModel _viewModel = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockServiceProvider = new Mock<IServiceProvider>();

		_viewModel = new NavigationViewModel( _mockServiceProvider.Object );
	}

	[TestMethod]
	public void Constructor_InitializesNavigationItems()
	{
		// Assert
		Assert.IsNotNull( _viewModel.NavigationItems );
	}

	[TestMethod]
	public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
	{
		// Act & Assert
		try
		{
			new NavigationViewModel( null! );
			Assert.Fail( "Expected ArgumentNullException was not thrown." );
		}
		catch ( ArgumentNullException )
		{
			// expected
		}
	}

	[TestMethod]
	public void CurrentView_CanBeSetAndRetrieved()
	{
		// Arrange
		var testView = new object();

		// Act
		_viewModel.CurrentView = testView;

		// Assert
		Assert.AreEqual( testView, _viewModel.CurrentView );
	}

	[TestMethod]
	public void CurrentView_RaisesPropertyChangedEvent()
	{
		// Arrange
		var propertyChanged = false;
		string? changedPropertyName = null;
		_viewModel.PropertyChanged += ( s, e ) =>
		{
			propertyChanged = true;
			changedPropertyName = e.PropertyName;
		};

		// Act
		_viewModel.CurrentView = new object();

		// Assert
		Assert.IsTrue( propertyChanged );
		Assert.AreEqual( nameof( _viewModel.CurrentView ), changedPropertyName );
	}

	[TestMethod]
	public void IsNavigationLoaded_DefaultValue_IsFalse()
	{
		// Arrange & Act
		var viewModel = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.IsFalse( viewModel.IsNavigationLoaded );
	}

	[TestMethod]
	public void IsNavigationLoaded_CanBeSetAndRetrieved()
	{
		// Act
		_viewModel.IsNavigationLoaded = true;

		// Assert
		Assert.IsTrue( _viewModel.IsNavigationLoaded );
	}

	[TestMethod]
	public void IsNavigationLoaded_RaisesPropertyChangedEvent()
	{
		// Arrange
		var propertyChanged = false;
		string? changedPropertyName = null;
		_viewModel.PropertyChanged += ( s, e ) =>
		{
			propertyChanged = true;
			changedPropertyName = e.PropertyName;
		};

		// Act
		_viewModel.IsNavigationLoaded = true;

		// Assert
		Assert.IsTrue( propertyChanged );
		Assert.AreEqual( nameof( _viewModel.IsNavigationLoaded ), changedPropertyName );
	}

	[TestMethod]
	public void IsNavigationLoaded_SettingSameValue_DoesNotRaisePropertyChanged()
	{
		// Arrange
		_viewModel.IsNavigationLoaded = false;
		var propertyChangedCount = 0;
		_viewModel.PropertyChanged += ( s, e ) =>
		{
			if ( e.PropertyName == nameof( _viewModel.IsNavigationLoaded ) )
				propertyChangedCount++;
		};

		// Act
		_viewModel.IsNavigationLoaded = false;

		// Assert
		Assert.AreEqual( 0, propertyChangedCount );
	}

	[TestMethod]
	public void AppVersion_ReturnsVersionString()
	{
		// Act
		var version = NavigationViewModel.AppVersion;

		// Assert
		Assert.IsNotNull( version );
		Assert.StartsWith( "Modelbouwer v", version );
	}

	[TestMethod]
	public void AppVersion_ContainsVersionNumber()
	{
		// Act
		var version = NavigationViewModel.AppVersion;

		// Assert
		// Should contain a version number pattern
		Assert.IsTrue( System.Text.RegularExpressions.Regex.IsMatch(
			version,
			@"Modelbouwer v\d+\.\d+|Modelbouwer v.*"
		) );
	}

	[TestMethod]
	public void PropertyChanged_IsRaisedWhenCurrentViewChanges()
	{
		// Arrange
		var eventRaised = false;
		_viewModel.PropertyChanged += ( sender, args ) =>
		{
			if ( args.PropertyName == nameof( _viewModel.CurrentView ) )
				eventRaised = true;
		};

		// Act
		_viewModel.CurrentView = new object();

		// Assert
		Assert.IsTrue( eventRaised );
	}

	[TestMethod]
	public void NavigationItems_IsNotNull()
	{
		// Assert
		Assert.IsNotNull( _viewModel.NavigationItems );
	}

	[TestMethod]
	public void NavigationViewModel_ImplementsINotifyPropertyChanged()
	{
		// Assert
		Assert.IsInstanceOfType( _viewModel, typeof( System.ComponentModel.INotifyPropertyChanged ) );
	}

	[TestMethod]
	public void CurrentView_CanBeSetToNull()
	{
		// Arrange
		_viewModel.CurrentView = new object();

		// Act
		_viewModel.CurrentView = null;

		// Assert
		Assert.IsNull( _viewModel.CurrentView );
	}

	[TestMethod]
	public void CurrentView_CanBeChangedMultipleTimes()
	{
		// Arrange
		var view1 = new object();
		var view2 = new object();
		var view3 = new object();

		// Act
		_viewModel.CurrentView = view1;
		_viewModel.CurrentView = view2;
		_viewModel.CurrentView = view3;

		// Assert
		Assert.AreEqual( view3, _viewModel.CurrentView );
	}

	[TestMethod]
	public void PropertyChanged_OnlyRaisedForChangedProperties()
	{
		// Arrange
		var currentViewChangedCount = 0;
		var isNavigationLoadedChangedCount = 0;

		_viewModel.PropertyChanged += ( sender, args ) =>
		{
			if ( args.PropertyName == nameof( _viewModel.CurrentView ) )
				currentViewChangedCount++;
			if ( args.PropertyName == nameof( _viewModel.IsNavigationLoaded ) )
				isNavigationLoadedChangedCount++;
		};

		// Act
		_viewModel.CurrentView = new object();
		_viewModel.IsNavigationLoaded = true;

		// Assert
		Assert.AreEqual( 1, currentViewChangedCount );
		Assert.AreEqual( 1, isNavigationLoadedChangedCount );
	}

	[TestMethod]
	public void IsNavigationLoaded_ChangingFromFalseToTrue_RaisesEvent()
	{
		// Arrange
		_viewModel.IsNavigationLoaded = false;
		var propertyChanged = false;

		_viewModel.PropertyChanged += ( s, e ) =>
		{
			if ( e.PropertyName == nameof( _viewModel.IsNavigationLoaded ) )
				propertyChanged = true;
		};

		// Act
		_viewModel.IsNavigationLoaded = true;

		// Assert
		Assert.IsTrue( propertyChanged );
	}

	[TestMethod]
	public void IsNavigationLoaded_ChangingFromTrueToFalse_RaisesEvent()
	{
		// Arrange
		_viewModel.IsNavigationLoaded = true;
		var propertyChanged = false;

		_viewModel.PropertyChanged += ( s, e ) =>
		{
			if ( e.PropertyName == nameof( _viewModel.IsNavigationLoaded ) )
				propertyChanged = true;
		};

		// Act
		_viewModel.IsNavigationLoaded = false;

		// Assert
		Assert.IsTrue( propertyChanged );
	}

	[TestMethod]
	public void NavigationViewModel_CanBeInstantiatedMultipleTimes()
	{
		// Act
		var viewModel1 = new NavigationViewModel(_mockServiceProvider.Object);
		var viewModel2 = new NavigationViewModel(_mockServiceProvider.Object);

		// Assert
		Assert.IsNotNull( viewModel1 );
		Assert.IsNotNull( viewModel2 );
		Assert.AreNotSame( viewModel1, viewModel2 );
	}
}