using System;
using System.Windows.Input;

namespace Modelbouwer.Helpers;

public sealed class UiBusyScope : IDisposable
{
	private readonly Cursor? _previous;

	public UiBusyScope( Cursor? cursor = null )
	{
		_previous = Mouse.OverrideCursor;
		Mouse.OverrideCursor = cursor ?? Cursors.Wait;
	}

	public void Dispose()
	{
		Mouse.OverrideCursor = _previous;
	}
}