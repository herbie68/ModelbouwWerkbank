using System.Windows.Controls;

namespace Modelbouwer.Helpers;

public static class RichTextBoxRtfBehavior
{
	private static bool _isUpdating = false;
	private static bool _enterPressed = false;

	public static readonly DependencyProperty RtfTextProperty =
		DependencyProperty.RegisterAttached(
			"RtfText",
			typeof(string),
			typeof(RichTextBoxRtfBehavior),
			new FrameworkPropertyMetadata(
				string.Empty,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnRtfTextChanged));

	public static string GetRtfText( DependencyObject obj )
		=> ( string ) obj.GetValue( RtfTextProperty );

	public static void SetRtfText( DependencyObject obj, string value )
		=> obj.SetValue( RtfTextProperty, value );

	private static void OnRtfTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
	{
		if ( d is not RichTextBox rtb )
			return;

		// Alleen initial load vanaf VM (geen caret reset)
		if ( !rtb.IsLoaded )
			return;

		var rtf = e.NewValue as string;
		if ( !string.IsNullOrEmpty( rtf ) )
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
			rtb.Document.Blocks.Clear();
			rtb.Selection.Load( stream, DataFormats.Rtf );
		}
	}

	private static void RichTextBox_TextChanged( object sender, TextChangedEventArgs e )
	{
		if ( sender is not RichTextBox rtb )
			return;

		using var stream = new MemoryStream();
		rtb.Document.Save( stream, DataFormats.Rtf );
		var rtf = Encoding.UTF8.GetString(stream.ToArray());
		SetRtfText( rtb, rtf );
	}

	private static void RichTextBox_PreviewKeyDown( object sender, KeyEventArgs e )
	{
		if ( sender is not RichTextBox rtb )
			return;

		if ( e.Key == Key.Enter && !Keyboard.IsKeyDown( Key.LeftShift ) && !Keyboard.IsKeyDown( Key.RightShift ) )
		{
			e.Handled = true;
			rtb.CaretPosition.InsertParagraphBreak();
			_enterPressed = true; // flag dat we Enter hebben gedaan
		}
		else if ( e.Key == Key.Enter )
		{
			e.Handled = true;
			rtb.CaretPosition.InsertLineBreak();
			_enterPressed = true;
		}
	}

	public static void Attach( RichTextBox rtb )
	{
		if ( rtb == null )
			return;

		rtb.TextChanged -= RichTextBox_TextChanged;
		rtb.PreviewKeyDown -= RichTextBox_PreviewKeyDown;

		rtb.TextChanged += RichTextBox_TextChanged;
		rtb.PreviewKeyDown += RichTextBox_PreviewKeyDown;
	}

	public static int GetCaretOffset( RichTextBox rtb ) => new TextRange( rtb.Document.ContentStart, rtb.CaretPosition ).Text.Length;

	public static TextPointer? GetTextPointerAtOffset( TextPointer start, int offset )
	{
		var navigator = start;
		var charsRemaining = offset;

		while ( navigator != null )
		{
			if ( navigator.GetPointerContext( LogicalDirection.Forward ) == TextPointerContext.Text )
			{
				var textRun = navigator.GetTextInRun(LogicalDirection.Forward);
				if ( textRun.Length >= charsRemaining )
					return navigator.GetPositionAtOffset( charsRemaining );

				charsRemaining -= textRun.Length;
			}

			navigator = navigator.GetNextContextPosition( LogicalDirection.Forward );
		}

		return start;
	}
}
