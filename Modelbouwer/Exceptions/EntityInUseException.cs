using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Exceptions;

public class EntityInUseException : Exception
{
	public EntityInUseException( string message )
		: base( message )
	{
	}
}