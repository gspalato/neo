using Axion.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Kernel.Structures.TypeParsers
{
	public abstract class BaseTypeParser<T> : Qmmands.TypeParser<T>
	{
		public ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context, IServiceProvider provider)
			=> ParseAsync(parameter, value, (AxionContext)context, provider);

		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context)
			=> ParseAsync(parameter, value, (AxionContext)context, null);

		public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, AxionContext context,
			IServiceProvider provider);
	}
}
