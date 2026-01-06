using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Models;

public class ValidationResult
{
	public bool IsValid => !Errors.Any();
	public List<string> Errors { get; } = new();
}
