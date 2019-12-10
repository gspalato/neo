using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Arpa.Utilities;

namespace Arpa.Structures
{
	public interface ICommandMap
	{
		List<Command> FindCommands(string name);
		List<Command> FindCommands(Func<CommandInfo, bool> predicate);
		void AddCommand(string id, Type command);
	}

	public class CommandMap : ICommandMap
	{
		public Dictionary<string, Type> commands = new Dictionary<string, Type>();

		public List<Command> FindCommands(string name) =>
			FindCommands((CommandInfo info) => info.Id == name);

		public List<Command> FindCommands(Func<CommandInfo, bool> predicate)
		{
			List<Command> found = new List<Command>();

			foreach (string key in this.commands.Keys)
			{
				if (this.commands.TryGetValue(key, out Type t))
				{
					CommandInfo info = new CommandInfo
					{
						Id = key
					};

					if (predicate(info))
					{
						found.Add(ClassUtilities.Instantiate<Command>(t));
					}
				}
			}

			return found;
		}

		public Command GetCommand(string name) =>
			this.FindCommands(name).First();

		public void AddCommand(string id, Type command)
		{
			this.commands.Add(id, command);
		}
	}
}