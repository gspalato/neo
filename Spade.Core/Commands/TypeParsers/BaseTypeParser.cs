using Qmmands;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Commands.TypeParsers
{
	public abstract class BaseTypeParser<T> : TypeParser<T>
	{
		public ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context, IServiceProvider provider)
			=> ParseAsync(parameter, value, (SpadeContext)context, provider);

		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context)
			=> ParseAsync(parameter, value, (SpadeContext)context, null);

		public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, SpadeContext context,
			IServiceProvider provider);
	}
}
