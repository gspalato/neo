using System.Diagnostics;

namespace Oculus.Docker
{
	internal static class Program
	{
		private static void Main()
		{
			var processInfo = new ProcessStartInfo("powershell.exe",
				"-File " + "ps-oculus.ps1")
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