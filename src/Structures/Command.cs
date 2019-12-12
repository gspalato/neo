using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles;

namespace Arpa.Structures
{
	public interface ICommand
	{
		void SetContext(CommandContext Context);
	}

	abstract public class Command : BaseDisposable, ICommand
	{
		private bool isDisposed = false;
		private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

		public CommandContext Context { get; private set; }

		public void SetContext(CommandContext Context)
		{
			this.Context = Context;
		}
	}
}