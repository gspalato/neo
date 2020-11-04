using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spade.Core.Commands.TypeParsers
{
	public class CommandTypeParser : BaseTypeParser<Command>
	{
		public static readonly CommandTypeParser Instance = new();

		private CommandTypeParser() { }

		public override ValueTask<TypeParserResult<Command>> ParseAsync(Parameter parameter, string value,
			SpadeContext context, IServiceProvider provider)
		{
			var commandService = context.GetService<ICommandService>();

			var command = commandService.FindCommands(value).First().Command
				?? commandService.GetAllCommands().First(c => c.Name == value);

			return command is null
				? TypeParserResult<Command>.Unsuccessful("Couldn't find command.")
				: TypeParserResult<Command>.Successful(command);
		}
	}
}
