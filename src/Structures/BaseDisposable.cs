using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles;

namespace Arpa.Structures
{
	abstract public class BaseDisposable : IDisposable
	{
		private bool isDisposed = false;
		private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (isDisposed)
				return;

			if (disposing)
				handle.Dispose();

			isDisposed = true;
		}
	}
}