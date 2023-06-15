namespace Oculus.Common.Configurations
{
    public class BaseConfiguration
    {
        public bool DebugMode { get; set; } = default!;

        public DatabaseSection Database { get; set; } = default!;
        public class DatabaseSection
        {
            public string Url { get; set; } = default!;
            public string Name { get; set; } = default!;
        }
    }
}