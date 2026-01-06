using System;
using System.Globalization;
using System.Windows;
using Modelbouwer.Converters;
using Xunit;

namespace Modelbouwer.Tests;

public sealed class BooleanToVisibilityConverterTests
{
	[Fact]
	public void Convert_BoolTrue_ReturnsVisible()
	{
		var conv = new BooleanToVisibilityConverter();
		var result = conv.Convert( true, typeof( Visibility ), null, CultureInfo.InvariantCulture );
		Assert.Equal( Visibility.Visible, result );
	}

	[Fact]
	public void Convert_BoolFalse_ReturnsCollapsed()
	{
		var conv = new BooleanToVisibilityConverter();
		var result = conv.Convert( false, typeof( Visibility ), null, CultureInfo.InvariantCulture );
		Assert.Equal( Visibility.Collapsed, result );
	}

	[Fact]
	public void Convert_StringTrue_ReturnsVisible()
	{
		var conv = new BooleanToVisibilityConverter();
		var result = conv.Convert( "true", typeof( Visibility ), null, CultureInfo.InvariantCulture );
		Assert.Equal( Visibility.Visible, result );
	}

	[Fact]
	public void Convert_IntOne_ReturnsVisible()
	{
		var conv = new BooleanToVisibilityConverter();
		var result = conv.Convert( 1, typeof( Visibility ), null, CultureInfo.InvariantCulture );
		Assert.Equal( Visibility.Visible, result );
	}

	[Fact]
	public void Convert_InvertedParameter_ReturnsOpposite()
	{
		var conv = new BooleanToVisibilityConverter();
		var result = conv.Convert( true, typeof( Visibility ), "Invert", CultureInfo.InvariantCulture );
		Assert.Equal( Visibility.Collapsed, result );
	}

	[Fact]
	public void ConvertBack_FromVisible_ReturnsTrue()
	{
		var conv = new BooleanToVisibilityConverter();
		var result = conv.ConvertBack( Visibility.Visible, typeof( bool ), null, CultureInfo.InvariantCulture );
		Assert.True( (bool)result );
	}

	[Fact]
	public void ConvertBack_InvertedParameter_ReturnsFalse()
	{
		var conv = new BooleanToVisibilityConverter { IsInverted = false };
		var result = conv.ConvertBack( Visibility.Visible, typeof( bool ), "Invert", CultureInfo.InvariantCulture );
		Assert.False( (bool)result );
	}
}