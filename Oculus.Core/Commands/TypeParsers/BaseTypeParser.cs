using Qmmands;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.TypeParsers
{
	public abstract class BaseTypeParser<T> : TypeParser<T>
	{
		public ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context, IServiceProvider provider)
			=> ParseAsync(parameter, value, (OculusContext)context, provider);

		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context)
			=> ParseAsync(parameter, value, (OculusContext)context, null);

		public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, OculusContext context,
			IServiceProvider provider);
	}
}
