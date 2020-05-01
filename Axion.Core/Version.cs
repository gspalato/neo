namespace Axion
{
	public static class Version
	{
		private const int Major = 0;
		private const int Minor = 4;
		private const int Patch = 1;
		private const int Hotfix = 2;

#if (DEBUG)
		public static ReleaseType ReleaseType = ReleaseType.Development;
#elif (RELEASE)
		public static ReleaseType ReleaseType = ReleaseType.Production;
#endif

		public static System.Version DotnetVersion = new System.Version(Major, Minor, Patch, Hotfix);
		public static string FullVersion => $"{Major}.{Minor}.{Patch}.{Hotfix}{(ReleaseType == ReleaseType.Development ? "DEV" : "PROD")}";
	}

	public enum ReleaseType
	{
		Development,
		Production
	}
}