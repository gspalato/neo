using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Arpa.Entities;

namespace Arpa.Structures
{
	public class StringParser : ITypeParser<string>
	{
		public Task<string> ParseAsync(string arg, CommandContext ctx, int position)
		{
			return Task.FromResult(arg);
		}
	}
}