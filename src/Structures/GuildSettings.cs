using System;

namespace Arpa.Structures
{
	public interface IGuildSettings
	{
		ulong guild_id { get; set; }
		string prefix { get; set; }
	}

	public class GuildSettings : IGuildSettings
	{
		public ulong guild_id { get; set; }
		public string prefix { get; set; }
	}
}