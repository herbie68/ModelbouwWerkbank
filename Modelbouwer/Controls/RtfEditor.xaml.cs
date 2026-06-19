namespace Modelbouwer.Controls;

public partial class RtfEditor : UserControl
{
	public RtfEditor()
	{
		InitializeComponent();

		RtbEditor.PreviewLostKeyboardFocus += ( sender, e ) =>
		{
			// Controleer of de nieuwe focus binnen hetzelfde UserControl is
			if ( e.NewFocus is DependencyObject newFocus )
			{
				// Als de nieuwe focus nog steeds binnen onze RtfEditor is, update dan niet
				var ancestor = VisualTreeHelper.GetParent(newFocus);
				while ( ancestor != null )
				{
					if ( ancestor == this )
					{
						// Focus blijft binnen de RtfEditor, dus geen update
						return;
					}
					ancestor = VisualTreeHelper.GetParent( ancestor );
				}
			}

			// Focus verlaat de RtfEditor, update de binding
			UpdateRtfTextFromEditor();
			var bindingExpression = BindingOperations.GetBindingExpression(this, RtfTextProperty);
			bindingExpression?.UpdateSource();
		};
	}

	public static readonly DependencyProperty RtfTextProperty =
		DependencyProperty.Register(
			nameof(RtfText),
			typeof(string),
			typeof(RtfEditor),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRtfTextChanged));

	public string RtfText
	{
		get => ( string ) GetValue( RtfTextProperty );
		set => SetValue( RtfTextProperty, value );
	}

	private static void OnRtfTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
	{
		if ( d is not RtfEditor editor )
			return;

		var rtb = editor.RtbEditor;
		var rtf = e.NewValue as string;

		rtb.Document.Blocks.Clear();
		if ( !string.IsNullOrEmpty( rtf ) )
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
			var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
			range.Load( stream, DataFormats.Rtf );
		}
	}

	public void UpdateRtfTextFromEditor()
	{
		var range = new TextRange(RtbEditor.Document.ContentStart, RtbEditor.Document.ContentEnd);
		using var stream = new MemoryStream();
		range.Save( stream, DataFormats.Rtf );
		RtfText = Encoding.UTF8.GetString( stream.ToArray() );
	}

	// StrikeThrough button
	private void StrikeThrough_Click( object sender, RoutedEventArgs e )
	{
		var selection = RtbEditor.Selection;
		if ( selection != null )
		{
			var current = selection.GetPropertyValue(Inline.TextDecorationsProperty);
			if ( current == DependencyProperty.UnsetValue || current != TextDecorations.Strikethrough )
				selection.ApplyPropertyValue( Inline.TextDecorationsProperty, TextDecorations.Strikethrough );
			else
				selection.ApplyPropertyValue( Inline.TextDecorationsProperty, null );
		}
	}

	public void UpdateBinding()
	{
		var bindingExpression = BindingOperations.GetBindingExpression(this, RtfTextProperty);
		bindingExpression?.UpdateSource();
	}
}