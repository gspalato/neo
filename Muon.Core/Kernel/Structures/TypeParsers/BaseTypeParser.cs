using System;
using System.Threading.Tasks;
using Muon.Commands;
using Qmmands;

namespace Muon.Kernel.Structures.TypeParsers
{
	public abstract class BaseTypeParser<T> : Qmmands.TypeParser<T>
	{
		public ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context, IServiceProvider provider)
			=> ParseAsync(parameter, value, (MuonContext)context, provider);

		public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
			CommandContext context)
			=> ParseAsync(parameter, value, (MuonContext)context, null);

		public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, MuonContext context,
			IServiceProvider provider);
	}
}
