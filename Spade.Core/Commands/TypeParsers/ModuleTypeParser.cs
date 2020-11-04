using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spade.Core.Commands.TypeParsers
{
	public class ModuleTypeParser : BaseTypeParser<Module>
	{
		public static readonly ModuleTypeParser Instance = new();

		private ModuleTypeParser() { }

		public override ValueTask<TypeParserResult<Module>> ParseAsync(Parameter parameter, string value,
			SpadeContext context, IServiceProvider provider)
		{
			var commandService = context.GetService<ICommandService>();

			var modules = commandService.GetAllModules();
			foreach (var module in modules)
			{
				var attribute = (GroupAttribute)module.Attributes.FirstOrDefault(a => a is GroupAttribute);
				if (attribute is null)
				{
					Console.WriteLine("no group attribute");
					return TypeParserResult<Module>.Unsuccessful("Couldn't find command.");
				}

				if (attribute.Aliases.Contains(value))
					return TypeParserResult<Module>.Successful(module);
			}

			return TypeParserResult<Module>.Unsuccessful("Couldn't find command.");
		}
	}
}
