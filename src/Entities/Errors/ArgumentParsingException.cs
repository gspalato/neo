using System;

using Arpa;

namespace Arpa.Errors
{
	public class ArgumentParsingException : Exception
	{
		public ArgumentParsingException(string name) : base($"String can't be parsed into {name}.") { }
	}
}