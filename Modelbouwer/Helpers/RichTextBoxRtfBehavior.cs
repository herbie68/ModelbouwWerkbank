using System.Windows.Controls;

namespace Modelbouwer.Helpers;

public static class RichTextBoxRtfBehavior
{
	private static bool _isUpdatingFromSource;

	public static readonly DependencyProperty RtfTextProperty =
		DependencyProperty.RegisterAttached(
			"RtfText",
			typeof(string),
			typeof(RichTextBoxRtfBehavior),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnRtfTextChanged));

	public static string? GetRtfText( DependencyObject obj )
		=> ( string? ) obj.GetValue( RtfTextProperty );

	public static void SetRtfText( DependencyObject obj, string? value )
		=> obj.SetValue( RtfTextProperty, value );

	private static void OnRtfTextChanged(
		DependencyObject d,
		DependencyPropertyChangedEventArgs e )
	{
		if ( _isUpdatingFromSource )
			return;

		if ( d is not RichTextBox rtb )
			return;

		rtb.TextChanged -= RichTextBox_TextChanged;

		rtb.Document.Blocks.Clear();

		if ( e.NewValue is string rtf && !string.IsNullOrWhiteSpace( rtf ) )
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
			var range = new TextRange(
				rtb.Document.ContentStart,
				rtb.Document.ContentEnd);

			range.Load( stream, DataFormats.Rtf );
		}

		rtb.CaretPosition = rtb.Document.ContentEnd;
		rtb.TextChanged += RichTextBox_TextChanged;
	}

	private static void RichTextBox_TextChanged(
		object sender,
		TextChangedEventArgs e )
	{
		if ( _isUpdatingFromSource )
			return;

		if ( sender is not RichTextBox rtb )
			return;

		_isUpdatingFromSource = true;

		var range = new TextRange(
			rtb.Document.ContentStart,
			rtb.Document.ContentEnd);

		using var stream = new MemoryStream();
		range.Save( stream, DataFormats.Rtf );

		SetRtfText( rtb, Encoding.UTF8.GetString( stream.ToArray() ) );

		_isUpdatingFromSource = false;
	}
}
