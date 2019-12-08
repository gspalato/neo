using System;

using Arpa;

namespace Arpa.Errors
{
	public class NonCommandException : Exception
	{
		public NonCommandException() : base("Provided class is not a valid command.")
		{

		}
	}
}