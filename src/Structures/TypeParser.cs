using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Arpa.Structures;

namespace Arpa.Structures
{
	public interface ITypeParser
	{
		Task<TypeParserResult> ParseAsync(string arg, _CommandContext ctx, int position);
	}

	public abstract class TypeParser<T> : ITypeParser
	{
		public abstract Task<TypeParserResult> ParseAsync(string arg, _CommandContext ctx, int position);
		internal Task<TypeParserResult> GenerateTask(dynamic parsed)
		{
			return Task.FromResult(new TypeParserResult { Value = parsed });
		}
	}
}