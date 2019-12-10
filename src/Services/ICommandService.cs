using System;
using System.Reflection;

using Arpa.Structures;

namespace Arpa.Services
{
	public interface ICommandService
	{
		void AddCommand(CommandInfo info, Command cmd);
		void AddModules();
		void AddTypeParser(Type type, dynamic parser);
		void Execute(CommandContext ctx);
	}
}