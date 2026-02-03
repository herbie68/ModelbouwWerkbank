namespace Modelbouwer.Controls;

/// <summary>
/// Interaction logic for RtfEditor.xaml
/// </summary>
public partial class RtfEditor : UserControl
{
	public RtfEditor()
	{
		InitializeComponent();
		RichTextBoxRtfBehavior.Attach( RtbEditor );
	}

	public static readonly DependencyProperty RtfTextProperty =
		DependencyProperty.Register(
			nameof(RtfText),
			typeof(string),
			typeof(RtfEditor),
			new FrameworkPropertyMetadata(
				string.Empty,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public string RtfText
	{
		get => ( string ) GetValue( RtfTextProperty );
		set => SetValue( RtfTextProperty, value );
	}

	// StrikeThrough workaround
	private void StrikeThrough_Click( object sender, RoutedEventArgs e )
	{
		var selection = RtbEditor.Selection;
		if ( selection != null )
		{
			var current = selection.GetPropertyValue(Inline.TextDecorationsProperty);
			if ( current == DependencyProperty.UnsetValue || current != TextDecorations.Strikethrough )
			{
				selection.ApplyPropertyValue( Inline.TextDecorationsProperty, TextDecorations.Strikethrough );
			}
			else
			{
				selection.ApplyPropertyValue( Inline.TextDecorationsProperty, null );
			}
		}
	}
}
