using System;
using System.Threading.Tasks;

namespace Arpa.Structures
{
	public interface ICommand
	{
		void SetContext(CommandContext Context);
	}

	abstract public class Command : ICommand
	{
		public CommandContext Context { get; private set; }

		public void SetContext(CommandContext Context)
		{
			this.Context = Context;
		}
	}
}