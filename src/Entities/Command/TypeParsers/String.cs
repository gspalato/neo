using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Arpa.Structures;

namespace Arpa.Structures
{
	public class StringParser : TypeParser<string>
	{
		public override Task<TypeParserResult> ParseAsync(string arg, _CommandContext ctx, int position)
		{
			return GenerateTask(arg);
		}
	}
}