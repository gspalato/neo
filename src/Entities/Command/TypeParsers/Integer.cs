using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Arpa.Entities;

namespace Arpa.Structures
{
	public class IntegerParser : ITypeParser<int>
	{
		public Task<int> ParseAsync(string arg, CommandContext ctx, int position)
		{
			if (!(Int32.TryParse(arg, out int n)))
				return Task.FromResult(n);
			else
				return null;
		}
	}
}