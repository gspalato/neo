using System.Diagnostics;

namespace Spade.Docker
{
	internal static class Program
	{
		private static void Main()
		{
			var processInfo = new ProcessStartInfo("powershell.exe",
				"-File " + "ps-spade.ps1")
			{
				CreateNoWindow = false,
				UseShellExecute = false
			};

			var process = Process.Start(processInfo);
			process?.WaitForExit();
			process?.Close();
		}
	}
}