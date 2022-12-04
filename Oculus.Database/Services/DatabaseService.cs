using Oculus.Database.Models;

namespace Oculus.Database.Services
{
    public class DatabaseService
    {
        internal Supabase.Client? _client { get; private set; } = null;

        public DatabaseService()
        {

        }

        public Task InitializeAsync(string url, string key)
        {
            if (_client is null)
                _client = new Supabase.Client(url, key,
                    new Supabase.SupabaseOptions { AutoConnectRealtime = true });

            return _client.InitializeAsync();
        }

        public async Task<GuildSettings?> GetGuildSettings(ulong guildId)
        {
            if (_client is null)
                throw new Exception("Not yet connected.");

            var response = await _client.Postgrest.Table<GuildSettings>()
                .Match(
                new Dictionary<string, string> {
                    { "guild_id", guildId.ToString() }
                })
                .Get();
            var matches = response.Models;
            if (!matches.Any())
            {
                return null;
            }

            return matches.FirstOrDefault();
        }

        // Returns GuildSettings if successful, returns null if failed.
        public async Task<GuildSettings?> CreateGuildSettings(ulong guildId)
        {
            if (_client is null)
                throw new Exception("Not yet connected.");

            var settings = new GuildSettings
            {
                GuildId = guildId.ToString(),
                isInRadioMode = false,
                RadioChannelId = ""
            };

            var response = await _client.From<GuildSettings>().Insert(settings);
            Console.WriteLine(response.Content);
            if (response.ResponseMessage is not null && response.ResponseMessage.IsSuccessStatusCode)
                return settings;
            else
                return null;
        }
    }
}