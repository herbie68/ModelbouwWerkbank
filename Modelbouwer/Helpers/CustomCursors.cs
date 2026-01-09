using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Helpers;

public static class CustomCursors
{
	public static Cursor Exporting =>
		new(
			Application.GetResourceStream(
				new Uri( "pack://application:,,,/Resources/Cursors/exporting.ani" )
			)!.Stream
		);
}
