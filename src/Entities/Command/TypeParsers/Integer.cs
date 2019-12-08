using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Arpa.Structures;

namespace Arpa.Structures
{
	public class IntegerParser : TypeParser<int>
	{
		public override Task<TypeParserResult> ParseAsync(string arg, _CommandContext ctx, int position)
		{
			if (!(Int32.TryParse(arg, out int n)))
				return GenerateTask(n);
			else
				return null;
		}
	}
}