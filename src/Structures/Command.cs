using System;
using System.Threading.Tasks;

namespace Arpa.Structures
{
	public interface _ICommand
	{
		void SetContext(_CommandContext Context);
	}

	abstract public class _Command : _ICommand
	{
		public _CommandContext Context { get; private set; }

		public void SetContext(_CommandContext Context)
		{
			this.Context = Context;
		}
	}
}