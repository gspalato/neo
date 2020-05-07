using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Core.Commands.TypeParsers
{
    public class CommandTypeParser : BaseTypeParser<Command>
    {
        public static readonly CommandTypeParser Instance = new CommandTypeParser();

        private CommandTypeParser() { }

        public override ValueTask<TypeParserResult<Command>> ParseAsync(Parameter parameter, string value,
            AxionContext context, IServiceProvider provider)
        {
            var commandService = context.GetService<ICommandService>();

            Command command = commandService.FindCommands(value).First().Command
                ?? commandService.GetAllCommands().First(c => c.Name.ToLower() == value.ToLower());

            if (command is null)
                return TypeParserResult<Command>.Unsuccessful("Couldn't find command.");
            else
                return TypeParserResult<Command>.Successful(command);
        }
    }
}
