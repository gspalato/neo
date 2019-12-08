using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arpa.Structures
{
	public interface _ICommandMap
	{
		List<_Command> FindCommands(Func<_CommandInfo, bool> predicate);
		void AddCommand(string id, _Command command);
	}

	public class _CommandMap : _ICommandMap
	{
		public Dictionary<string, _Command> commands = new Dictionary<string, _Command>();

		public List<_Command> FindCommands(string name) =>
			FindCommands((_CommandInfo info) => info.Id == name);

		public List<_Command> FindCommands(Func<_CommandInfo, bool> predicate)
		{
			List<_Command> found = new List<_Command>();

			foreach (string key in this.commands.Keys)
			{
				_Command cmd;
				if (this.commands.TryGetValue(key, out cmd))
				{
					_CommandInfo info = new _CommandInfo
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

		public void AddCommand(string id, _Command command)
		{
			this.commands.Add(id, command);
		}
	}
}