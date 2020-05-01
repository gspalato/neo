using System.Diagnostics;

namespace Axion.Docker
{
	internal static class AxionDocker
	{
		private static void Main()
		{
			var processInfo = new ProcessStartInfo("powershell.exe",
				"-File " + "ps-axion.ps1")
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