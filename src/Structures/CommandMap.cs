using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arpa.Structures
{
	public interface _ICommandMap
	{
		List<Command> FindCommands(Func<CommandInfo, bool> predicate);
		void AddCommand(string id, Command command);
	}

	public class CommandMap : _ICommandMap
	{
		public Dictionary<string, Command> commands = new Dictionary<string, Command>();

		public List<Command> FindCommands(string name) =>
			FindCommands((CommandInfo info) => info.Id == name);

		public List<Command> FindCommands(Func<CommandInfo, bool> predicate)
		{
			List<Command> found = new List<Command>();

			foreach (string key in this.commands.Keys)
			{
				Command cmd;
				if (this.commands.TryGetValue(key, out cmd))
				{
					CommandInfo info = new CommandInfo
					{
						Id = key
					};

					if (predicate(info))
					{
						found.Add(cmd);
					}
				}
			}

			return found;
		}

		public void AddCommand(string id, Command command)
		{
			this.commands.Add(id, command);
		}
	}
}