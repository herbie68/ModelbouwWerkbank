using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Modelbouwer.Helpers;

public static class TextBlockHelper
{
	public static readonly DependencyProperty HighlightTextProperty =
		DependencyProperty.RegisterAttached(
			"HighlightText", typeof(string), typeof(TextBlockHelper),
			new PropertyMetadata("", OnHighlightTextChanged));

	public static string GetHighlightText( TextBlock tb ) => ( string ) tb.GetValue( HighlightTextProperty );
	public static void SetHighlightText( TextBlock tb, string value ) => tb.SetValue( HighlightTextProperty, value );

	private static void OnHighlightTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
	{
		if ( d is not TextBlock tb )
			return;
		var search = e.NewValue?.ToString() ?? "";
		var text = tb.Text ?? "";

		tb.Inlines.Clear();

		if ( string.IsNullOrEmpty( search ) )
		{
			tb.Inlines.Add( new Run( text ) );
			return;
		}

		int index = 0;
		int searchLength = search.Length;
		while ( index < text.Length )
		{
			int foundIndex = text.IndexOf(search, index, StringComparison.OrdinalIgnoreCase);
			if ( foundIndex < 0 )
			{
				tb.Inlines.Add( new Run( text.Substring( index ) ) );
				break;
			}

			if ( foundIndex > index )
				tb.Inlines.Add( new Run( text.Substring( index, foundIndex - index ) ) );

			tb.Inlines.Add( new Run( text.Substring( foundIndex, searchLength ) ) { Background = Brushes.Yellow } );

			index = foundIndex + searchLength;
		}
	}
}
