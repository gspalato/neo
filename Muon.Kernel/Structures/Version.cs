namespace Muon.Kernel
{
	public static class Version
	{
		private static int Major = 0;
		private static int Minor = 2;
		private static int Patch = 0;
		private static int Hotfix = 2;

		public static ReleaseType ReleaseType = ReleaseType.Development;

		public static System.Version DotnetVersion = new System.Version(Major, Minor, Patch, Hotfix);
		public static string FullVersion = $"{Major}.{Minor}.{Patch}.{Hotfix}{(ReleaseType == ReleaseType.Development ? "DEV" : "PROD")}";
	}

	public enum ReleaseType
	{
		Development,
		Release
	}
}